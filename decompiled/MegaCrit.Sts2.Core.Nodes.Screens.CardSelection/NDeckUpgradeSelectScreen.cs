using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;

[ScriptPath("res://src/Core/Nodes/Screens/CardSelection/NDeckUpgradeSelectScreen.cs")]
public sealed class NDeckUpgradeSelectScreen : NCardGridSelectionScreen
{
	public new class MethodName : NCardGridSelectionScreen.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName CloseSelection = "CloseSelection";

		public static readonly StringName CancelSelection = "CancelSelection";

		public static readonly StringName ConfirmSelection = "ConfirmSelection";

		public static readonly StringName CheckIfSelectionComplete = "CheckIfSelectionComplete";

		public static readonly StringName ToggleShowUpgrades = "ToggleShowUpgrades";

		public static readonly StringName OnControllerStateUpdated = "OnControllerStateUpdated";
	}

	public new class PropertyName : NCardGridSelectionScreen.PropertyName
	{
		public static readonly StringName UseSingleSelection = "UseSingleSelection";

		public new static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public new static readonly StringName FocusedControlFromTopBar = "FocusedControlFromTopBar";

		public static readonly StringName _upgradeSinglePreviewContainer = "_upgradeSinglePreviewContainer";

		public static readonly StringName _singlePreview = "_singlePreview";

		public static readonly StringName _singlePreviewCancelButton = "_singlePreviewCancelButton";

		public static readonly StringName _singlePreviewConfirmButton = "_singlePreviewConfirmButton";

		public static readonly StringName _viewUpgrades = "_viewUpgrades";

		public static readonly StringName _bottomTextContainer = "_bottomTextContainer";

		public static readonly StringName _infoLabel = "_infoLabel";

		public static readonly StringName _upgradeMultiPreviewContainer = "_upgradeMultiPreviewContainer";

		public static readonly StringName _multiPreview = "_multiPreview";

		public static readonly StringName _multiPreviewCancelButton = "_multiPreviewCancelButton";

		public static readonly StringName _multiPreviewConfirmButton = "_multiPreviewConfirmButton";

		public static readonly StringName _closeButton = "_closeButton";
	}

	public new class SignalName : NCardGridSelectionScreen.SignalName
	{
	}

	private readonly HashSet<CardModel> _selectedCards = new HashSet<CardModel>();

	private CardSelectorPrefs _prefs;

	private IRunState _runState;

	private Control _upgradeSinglePreviewContainer;

	private NUpgradePreview _singlePreview;

	private NBackButton _singlePreviewCancelButton;

	private NConfirmButton _singlePreviewConfirmButton;

	private NTickbox _viewUpgrades;

	private Control _bottomTextContainer;

	private MegaRichTextLabel _infoLabel;

	private Control _upgradeMultiPreviewContainer;

	private Control _multiPreview;

	private NBackButton _multiPreviewCancelButton;

	private NConfirmButton _multiPreviewConfirmButton;

	private NBackButton _closeButton;

	private static string ScenePath => SceneHelper.GetScenePath("screens/card_selection/deck_upgrade_select_screen");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	private bool UseSingleSelection => _prefs.MaxSelect == 1;

	protected override IEnumerable<Control> PeekButtonTargets => new global::_003C_003Ez__ReadOnlyArray<Control>(new Control[3] { _upgradeSinglePreviewContainer, _upgradeMultiPreviewContainer, _closeButton });

	public override Control? DefaultFocusedControl
	{
		get
		{
			if (_upgradeSinglePreviewContainer.Visible || _upgradeMultiPreviewContainer.Visible)
			{
				return null;
			}
			return _grid.DefaultFocusedControl;
		}
	}

	public override Control? FocusedControlFromTopBar
	{
		get
		{
			if (_upgradeSinglePreviewContainer.Visible || _upgradeMultiPreviewContainer.Visible)
			{
				return null;
			}
			return _grid.FocusedControlFromTopBar;
		}
	}

	public override void _Ready()
	{
		ConnectSignalsAndInitGrid();
		_upgradeSinglePreviewContainer = GetNode<Control>("%UpgradeSinglePreviewContainer");
		_singlePreview = _upgradeSinglePreviewContainer.GetNode<NUpgradePreview>("UpgradePreview");
		_singlePreviewCancelButton = _upgradeSinglePreviewContainer.GetNode<NBackButton>("Cancel");
		_singlePreviewConfirmButton = _upgradeSinglePreviewContainer.GetNode<NConfirmButton>("Confirm");
		_upgradeMultiPreviewContainer = GetNode<Control>("%UpgradeMultiPreviewContainer");
		_multiPreview = _upgradeMultiPreviewContainer.GetNode<Control>("Cards");
		_multiPreviewCancelButton = _upgradeMultiPreviewContainer.GetNode<NBackButton>("Cancel");
		_multiPreviewConfirmButton = _upgradeMultiPreviewContainer.GetNode<NConfirmButton>("Confirm");
		_closeButton = GetNode<NBackButton>("%Close");
		_bottomTextContainer = GetNode<Control>("%BottomText");
		_infoLabel = _bottomTextContainer.GetNode<MegaRichTextLabel>("%BottomLabel");
		_infoLabel.Text = _prefs.Prompt.GetFormattedText();
		_singlePreviewCancelButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(CancelSelection));
		_singlePreviewConfirmButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(ConfirmSelection));
		_multiPreviewCancelButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(CancelSelection));
		_multiPreviewConfirmButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(ConfirmSelection));
		_closeButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(CloseSelection));
		if (_prefs.Cancelable)
		{
			_closeButton.Enable();
		}
		else
		{
			_closeButton.Disable();
		}
		_upgradeSinglePreviewContainer.Visible = false;
		_upgradeSinglePreviewContainer.MouseFilter = MouseFilterEnum.Ignore;
		_upgradeMultiPreviewContainer.Visible = false;
		_upgradeMultiPreviewContainer.MouseFilter = MouseFilterEnum.Ignore;
		_singlePreviewCancelButton.Disable();
		_singlePreviewConfirmButton.Disable();
		_multiPreviewCancelButton.Disable();
		_multiPreviewConfirmButton.Disable();
		_viewUpgrades = GetNode<NTickbox>("%Upgrades");
		_viewUpgrades.IsTicked = false;
		_viewUpgrades.Connect(NTickbox.SignalName.Toggled, Callable.From<NTickbox>(ToggleShowUpgrades));
		OnControllerStateUpdated();
		NControllerManager.Instance.Connect(NControllerManager.SignalName.MouseDetected, Callable.From(OnControllerStateUpdated));
		NControllerManager.Instance.Connect(NControllerManager.SignalName.ControllerDetected, Callable.From(OnControllerStateUpdated));
		NInputManager.Instance.Connect(NInputManager.SignalName.InputRebound, Callable.From(OnControllerStateUpdated));
		GetNode<MegaLabel>("%ViewUpgradesLabel").SetTextAutoSize(new LocString("card_selection", "VIEW_UPGRADES").GetFormattedText());
	}

	public static NDeckUpgradeSelectScreen ShowScreen(IReadOnlyList<CardModel> cards, CardSelectorPrefs prefs, IRunState runState)
	{
		NDeckUpgradeSelectScreen nDeckUpgradeSelectScreen = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NDeckUpgradeSelectScreen>(PackedScene.GenEditState.Disabled);
		nDeckUpgradeSelectScreen.Name = "NDeckUpgradeSelectScreen";
		nDeckUpgradeSelectScreen._cards = cards;
		nDeckUpgradeSelectScreen._prefs = prefs;
		nDeckUpgradeSelectScreen._runState = runState;
		NOverlayStack.Instance.Push(nDeckUpgradeSelectScreen);
		return nDeckUpgradeSelectScreen;
	}

	protected override void OnCardClicked(CardModel card)
	{
		if (_selectedCards.Add(card))
		{
			_grid.HighlightCard(card);
			if (UseSingleSelection)
			{
				GetViewport().GuiReleaseFocus();
				_upgradeSinglePreviewContainer.Visible = true;
				_upgradeSinglePreviewContainer.MouseFilter = MouseFilterEnum.Stop;
				_singlePreview.Card = card;
				_singlePreviewCancelButton.Enable();
				_singlePreviewConfirmButton.Enable();
				_grid.SetCanScroll(canScroll: false);
				_closeButton.Disable();
			}
			else
			{
				if (_prefs.MaxSelect != _selectedCards.Count)
				{
					return;
				}
				GetViewport().GuiReleaseFocus();
				_upgradeMultiPreviewContainer.Visible = true;
				_upgradeMultiPreviewContainer.MouseFilter = MouseFilterEnum.Stop;
				_multiPreviewCancelButton.Enable();
				_multiPreviewConfirmButton.Enable();
				foreach (CardModel selectedCard in _selectedCards)
				{
					_grid.UnhighlightCard(selectedCard);
					CardModel cardModel = _runState.CloneCard(selectedCard);
					cardModel.UpgradeInternal();
					cardModel.UpgradePreviewType = CardUpgradePreviewType.Deck;
					NCard nCard = NCard.Create(cardModel);
					_multiPreview.AddChildSafely(NPreviewCardHolder.Create(nCard, showHoverTips: true, scaleOnHover: false));
					nCard.ShowUpgradePreview();
					_grid.SetCanScroll(canScroll: false);
					_closeButton.Disable();
				}
			}
		}
		else
		{
			_selectedCards.Remove(card);
			_grid.UnhighlightCard(card);
		}
	}

	private void CloseSelection(NButton _)
	{
		_completionSource.SetResult(Array.Empty<CardModel>());
		_singlePreviewCancelButton.Disable();
		_singlePreviewConfirmButton.Disable();
		_multiPreviewCancelButton.Disable();
		_multiPreviewConfirmButton.Disable();
		NOverlayStack.Instance.Remove(this);
	}

	private void CancelSelection(NButton _)
	{
		if (UseSingleSelection)
		{
			_upgradeSinglePreviewContainer.Visible = false;
			_upgradeSinglePreviewContainer.MouseFilter = MouseFilterEnum.Ignore;
			_singlePreviewCancelButton.Disable();
			_singlePreviewConfirmButton.Disable();
		}
		else
		{
			_upgradeMultiPreviewContainer.Visible = false;
			_upgradeMultiPreviewContainer.MouseFilter = MouseFilterEnum.Ignore;
			for (int i = 0; i < _multiPreview.GetChildCount(); i++)
			{
				_multiPreview.GetChild(i).QueueFreeSafely();
			}
			_multiPreviewCancelButton.Disable();
			_multiPreviewConfirmButton.Disable();
		}
		_grid.SetCanScroll(canScroll: true);
		if (_prefs.Cancelable)
		{
			_closeButton.Enable();
		}
		foreach (CardModel selectedCard in _selectedCards)
		{
			_grid.UnhighlightCard(selectedCard);
		}
		_grid.GetCardHolder(_selectedCards.Last())?.TryGrabFocus();
		_selectedCards.Clear();
	}

	private void ConfirmSelection(NButton _)
	{
		if (_selectedCards.Count != 0)
		{
			CheckIfSelectionComplete();
		}
	}

	private void CheckIfSelectionComplete()
	{
		_singlePreviewCancelButton.Enable();
		_singlePreviewConfirmButton.Enable();
		if (_selectedCards.Count >= _prefs.MaxSelect)
		{
			_completionSource.SetResult(_selectedCards);
			NOverlayStack.Instance.Remove(this);
		}
	}

	private void ToggleShowUpgrades(NTickbox tickbox)
	{
		_grid.IsShowingUpgrades = tickbox.IsTicked;
	}

	private void OnControllerStateUpdated()
	{
		_viewUpgrades.Visible = !NControllerManager.Instance.IsUsingController;
		if (NControllerManager.Instance.IsUsingController)
		{
			_viewUpgrades.IsTicked = false;
			ToggleShowUpgrades(_viewUpgrades);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(7);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CloseSelection, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.CancelSelection, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ConfirmSelection, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.CheckIfSelectionComplete, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ToggleShowUpgrades, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "tickbox", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnControllerStateUpdated, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CloseSelection && args.Count == 1)
		{
			CloseSelection(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CancelSelection && args.Count == 1)
		{
			CancelSelection(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ConfirmSelection && args.Count == 1)
		{
			ConfirmSelection(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CheckIfSelectionComplete && args.Count == 0)
		{
			CheckIfSelectionComplete();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ToggleShowUpgrades && args.Count == 1)
		{
			ToggleShowUpgrades(VariantUtils.ConvertTo<NTickbox>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnControllerStateUpdated && args.Count == 0)
		{
			OnControllerStateUpdated();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.CloseSelection)
		{
			return true;
		}
		if (method == MethodName.CancelSelection)
		{
			return true;
		}
		if (method == MethodName.ConfirmSelection)
		{
			return true;
		}
		if (method == MethodName.CheckIfSelectionComplete)
		{
			return true;
		}
		if (method == MethodName.ToggleShowUpgrades)
		{
			return true;
		}
		if (method == MethodName.OnControllerStateUpdated)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._upgradeSinglePreviewContainer)
		{
			_upgradeSinglePreviewContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._singlePreview)
		{
			_singlePreview = VariantUtils.ConvertTo<NUpgradePreview>(in value);
			return true;
		}
		if (name == PropertyName._singlePreviewCancelButton)
		{
			_singlePreviewCancelButton = VariantUtils.ConvertTo<NBackButton>(in value);
			return true;
		}
		if (name == PropertyName._singlePreviewConfirmButton)
		{
			_singlePreviewConfirmButton = VariantUtils.ConvertTo<NConfirmButton>(in value);
			return true;
		}
		if (name == PropertyName._viewUpgrades)
		{
			_viewUpgrades = VariantUtils.ConvertTo<NTickbox>(in value);
			return true;
		}
		if (name == PropertyName._bottomTextContainer)
		{
			_bottomTextContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._infoLabel)
		{
			_infoLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._upgradeMultiPreviewContainer)
		{
			_upgradeMultiPreviewContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._multiPreview)
		{
			_multiPreview = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._multiPreviewCancelButton)
		{
			_multiPreviewCancelButton = VariantUtils.ConvertTo<NBackButton>(in value);
			return true;
		}
		if (name == PropertyName._multiPreviewConfirmButton)
		{
			_multiPreviewConfirmButton = VariantUtils.ConvertTo<NConfirmButton>(in value);
			return true;
		}
		if (name == PropertyName._closeButton)
		{
			_closeButton = VariantUtils.ConvertTo<NBackButton>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.UseSingleSelection)
		{
			value = VariantUtils.CreateFrom<bool>(UseSingleSelection);
			return true;
		}
		Control from;
		if (name == PropertyName.DefaultFocusedControl)
		{
			from = DefaultFocusedControl;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.FocusedControlFromTopBar)
		{
			from = FocusedControlFromTopBar;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName._upgradeSinglePreviewContainer)
		{
			value = VariantUtils.CreateFrom(in _upgradeSinglePreviewContainer);
			return true;
		}
		if (name == PropertyName._singlePreview)
		{
			value = VariantUtils.CreateFrom(in _singlePreview);
			return true;
		}
		if (name == PropertyName._singlePreviewCancelButton)
		{
			value = VariantUtils.CreateFrom(in _singlePreviewCancelButton);
			return true;
		}
		if (name == PropertyName._singlePreviewConfirmButton)
		{
			value = VariantUtils.CreateFrom(in _singlePreviewConfirmButton);
			return true;
		}
		if (name == PropertyName._viewUpgrades)
		{
			value = VariantUtils.CreateFrom(in _viewUpgrades);
			return true;
		}
		if (name == PropertyName._bottomTextContainer)
		{
			value = VariantUtils.CreateFrom(in _bottomTextContainer);
			return true;
		}
		if (name == PropertyName._infoLabel)
		{
			value = VariantUtils.CreateFrom(in _infoLabel);
			return true;
		}
		if (name == PropertyName._upgradeMultiPreviewContainer)
		{
			value = VariantUtils.CreateFrom(in _upgradeMultiPreviewContainer);
			return true;
		}
		if (name == PropertyName._multiPreview)
		{
			value = VariantUtils.CreateFrom(in _multiPreview);
			return true;
		}
		if (name == PropertyName._multiPreviewCancelButton)
		{
			value = VariantUtils.CreateFrom(in _multiPreviewCancelButton);
			return true;
		}
		if (name == PropertyName._multiPreviewConfirmButton)
		{
			value = VariantUtils.CreateFrom(in _multiPreviewConfirmButton);
			return true;
		}
		if (name == PropertyName._closeButton)
		{
			value = VariantUtils.CreateFrom(in _closeButton);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.UseSingleSelection, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._upgradeSinglePreviewContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._singlePreview, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._singlePreviewCancelButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._singlePreviewConfirmButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._viewUpgrades, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bottomTextContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._infoLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._upgradeMultiPreviewContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._multiPreview, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._multiPreviewCancelButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._multiPreviewConfirmButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._closeButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.FocusedControlFromTopBar, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._upgradeSinglePreviewContainer, Variant.From(in _upgradeSinglePreviewContainer));
		info.AddProperty(PropertyName._singlePreview, Variant.From(in _singlePreview));
		info.AddProperty(PropertyName._singlePreviewCancelButton, Variant.From(in _singlePreviewCancelButton));
		info.AddProperty(PropertyName._singlePreviewConfirmButton, Variant.From(in _singlePreviewConfirmButton));
		info.AddProperty(PropertyName._viewUpgrades, Variant.From(in _viewUpgrades));
		info.AddProperty(PropertyName._bottomTextContainer, Variant.From(in _bottomTextContainer));
		info.AddProperty(PropertyName._infoLabel, Variant.From(in _infoLabel));
		info.AddProperty(PropertyName._upgradeMultiPreviewContainer, Variant.From(in _upgradeMultiPreviewContainer));
		info.AddProperty(PropertyName._multiPreview, Variant.From(in _multiPreview));
		info.AddProperty(PropertyName._multiPreviewCancelButton, Variant.From(in _multiPreviewCancelButton));
		info.AddProperty(PropertyName._multiPreviewConfirmButton, Variant.From(in _multiPreviewConfirmButton));
		info.AddProperty(PropertyName._closeButton, Variant.From(in _closeButton));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._upgradeSinglePreviewContainer, out var value))
		{
			_upgradeSinglePreviewContainer = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._singlePreview, out var value2))
		{
			_singlePreview = value2.As<NUpgradePreview>();
		}
		if (info.TryGetProperty(PropertyName._singlePreviewCancelButton, out var value3))
		{
			_singlePreviewCancelButton = value3.As<NBackButton>();
		}
		if (info.TryGetProperty(PropertyName._singlePreviewConfirmButton, out var value4))
		{
			_singlePreviewConfirmButton = value4.As<NConfirmButton>();
		}
		if (info.TryGetProperty(PropertyName._viewUpgrades, out var value5))
		{
			_viewUpgrades = value5.As<NTickbox>();
		}
		if (info.TryGetProperty(PropertyName._bottomTextContainer, out var value6))
		{
			_bottomTextContainer = value6.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._infoLabel, out var value7))
		{
			_infoLabel = value7.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._upgradeMultiPreviewContainer, out var value8))
		{
			_upgradeMultiPreviewContainer = value8.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._multiPreview, out var value9))
		{
			_multiPreview = value9.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._multiPreviewCancelButton, out var value10))
		{
			_multiPreviewCancelButton = value10.As<NBackButton>();
		}
		if (info.TryGetProperty(PropertyName._multiPreviewConfirmButton, out var value11))
		{
			_multiPreviewConfirmButton = value11.As<NConfirmButton>();
		}
		if (info.TryGetProperty(PropertyName._closeButton, out var value12))
		{
			_closeButton = value12.As<NBackButton>();
		}
	}
}
