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
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;

[ScriptPath("res://src/Core/Nodes/Screens/CardSelection/NDeckTransformSelectScreen.cs")]
public sealed class NDeckTransformSelectScreen : NCardGridSelectionScreen
{
	public new class MethodName : NCardGridSelectionScreen.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName RefreshConfirmButtonVisibility = "RefreshConfirmButtonVisibility";

		public static readonly StringName CloseSelection = "CloseSelection";

		public static readonly StringName CancelSelection = "CancelSelection";

		public static readonly StringName ConfirmSelection = "ConfirmSelection";

		public static readonly StringName OpenPreviewScreen = "OpenPreviewScreen";

		public static readonly StringName CompleteSelection = "CompleteSelection";

		public static readonly StringName ToggleShowUpgrades = "ToggleShowUpgrades";

		public static readonly StringName OnControllerStateUpdated = "OnControllerStateUpdated";
	}

	public new class PropertyName : NCardGridSelectionScreen.PropertyName
	{
		public new static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public new static readonly StringName FocusedControlFromTopBar = "FocusedControlFromTopBar";

		public static readonly StringName _previewContainer = "_previewContainer";

		public static readonly StringName _transformPreview = "_transformPreview";

		public static readonly StringName _confirmButton = "_confirmButton";

		public static readonly StringName _previewCancelButton = "_previewCancelButton";

		public static readonly StringName _previewConfirmButton = "_previewConfirmButton";

		public static readonly StringName _bottomTextContainer = "_bottomTextContainer";

		public static readonly StringName _infoLabel = "_infoLabel";

		public static readonly StringName _viewUpgrades = "_viewUpgrades";

		public static readonly StringName _closeButton = "_closeButton";
	}

	public new class SignalName : NCardGridSelectionScreen.SignalName
	{
	}

	private readonly HashSet<CardModel> _selectedCards = new HashSet<CardModel>();

	private Func<CardModel, CardTransformation> _cardToTransformation;

	private CardSelectorPrefs _prefs;

	private Control _previewContainer;

	private NTransformPreview _transformPreview;

	private NConfirmButton _confirmButton;

	private NBackButton _previewCancelButton;

	private NConfirmButton _previewConfirmButton;

	private Control _bottomTextContainer;

	private MegaRichTextLabel _infoLabel;

	private NTickbox _viewUpgrades;

	private NBackButton _closeButton;

	private static string ScenePath => SceneHelper.GetScenePath("screens/card_selection/deck_transform_select_screen");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	protected override IEnumerable<Control> PeekButtonTargets => new global::_003C_003Ez__ReadOnlyArray<Control>(new Control[3] { _previewContainer, _closeButton, _bottomTextContainer });

	public override Control? DefaultFocusedControl
	{
		get
		{
			if (_previewContainer.Visible)
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
			if (_previewContainer.Visible)
			{
				return null;
			}
			return _grid.FocusedControlFromTopBar;
		}
	}

	public override void _Ready()
	{
		ConnectSignalsAndInitGrid();
		_confirmButton = GetNode<NConfirmButton>("Confirm");
		_previewContainer = GetNode<Control>("%PreviewContainer");
		_transformPreview = _previewContainer.GetNode<NTransformPreview>("TransformPreview");
		_previewCancelButton = _previewContainer.GetNode<NBackButton>("Cancel");
		_previewConfirmButton = _previewContainer.GetNode<NConfirmButton>("Confirm");
		_closeButton = GetNode<NBackButton>("%Close");
		_previewCancelButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(CancelSelection));
		_previewConfirmButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(CompleteSelection));
		_closeButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(CloseSelection));
		_confirmButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(ConfirmSelection));
		if (_prefs.Cancelable)
		{
			_closeButton.Enable();
		}
		else
		{
			_closeButton.Disable();
		}
		RefreshConfirmButtonVisibility();
		_previewCancelButton.Disable();
		_previewConfirmButton.Disable();
		_bottomTextContainer = GetNode<Control>("%BottomText");
		_infoLabel = _bottomTextContainer.GetNode<MegaRichTextLabel>("%BottomLabel");
		_infoLabel.Text = _prefs.Prompt.GetFormattedText();
		_viewUpgrades = GetNode<NTickbox>("%Upgrades");
		_viewUpgrades.IsTicked = false;
		_viewUpgrades.Connect(NTickbox.SignalName.Toggled, Callable.From<NTickbox>(ToggleShowUpgrades));
		OnControllerStateUpdated();
		NControllerManager.Instance.Connect(NControllerManager.SignalName.MouseDetected, Callable.From(OnControllerStateUpdated));
		NControllerManager.Instance.Connect(NControllerManager.SignalName.ControllerDetected, Callable.From(OnControllerStateUpdated));
		NInputManager.Instance.Connect(NInputManager.SignalName.InputRebound, Callable.From(OnControllerStateUpdated));
		GetNode<MegaLabel>("%ViewUpgradesLabel").SetTextAutoSize(new LocString("card_selection", "VIEW_UPGRADES").GetFormattedText());
	}

	public static NDeckTransformSelectScreen ShowScreen(IReadOnlyList<CardModel> cards, Func<CardModel, CardTransformation> cardToTransformation, CardSelectorPrefs prefs)
	{
		NDeckTransformSelectScreen nDeckTransformSelectScreen = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NDeckTransformSelectScreen>(PackedScene.GenEditState.Disabled);
		nDeckTransformSelectScreen.Name = "NDeckTransformSelectScreen";
		nDeckTransformSelectScreen._cards = cards;
		nDeckTransformSelectScreen._cardToTransformation = cardToTransformation;
		nDeckTransformSelectScreen._prefs = prefs;
		NOverlayStack.Instance.Push(nDeckTransformSelectScreen);
		return nDeckTransformSelectScreen;
	}

	private void RefreshConfirmButtonVisibility()
	{
		if (_prefs.MinSelect != _prefs.MaxSelect && _selectedCards.Count >= _prefs.MinSelect)
		{
			_confirmButton.Enable();
		}
		else
		{
			_confirmButton.Disable();
		}
	}

	protected override void OnCardClicked(CardModel card)
	{
		if (_selectedCards.Add(card))
		{
			_grid.HighlightCard(card);
			if (_prefs.MaxSelect == _selectedCards.Count)
			{
				OpenPreviewScreen();
			}
		}
		else
		{
			_selectedCards.Remove(card);
			_grid.UnhighlightCard(card);
		}
		RefreshConfirmButtonVisibility();
	}

	private void CloseSelection(NButton _)
	{
		_completionSource.SetResult(Array.Empty<CardModel>());
		_previewCancelButton.Disable();
		_previewConfirmButton.Disable();
		NOverlayStack.Instance.Remove(this);
	}

	private void CancelSelection(NButton _)
	{
		_previewContainer.Visible = false;
		_previewContainer.MouseFilter = MouseFilterEnum.Ignore;
		_transformPreview.Uninitialize();
		_previewCancelButton.Disable();
		_previewConfirmButton.Disable();
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
		if (_selectedCards.Count >= _prefs.MinSelect)
		{
			if (_prefs.RequireManualConfirmation)
			{
				OpenPreviewScreen();
			}
			else
			{
				CompleteSelection(_);
			}
		}
	}

	private void OpenPreviewScreen()
	{
		GetViewport().GuiReleaseFocus();
		_previewContainer.Visible = true;
		_previewContainer.MouseFilter = MouseFilterEnum.Stop;
		_previewCancelButton.Enable();
		_previewConfirmButton.Enable();
		foreach (CardModel selectedCard in _selectedCards)
		{
			_grid.UnhighlightCard(selectedCard);
		}
		_transformPreview.Initialize(_selectedCards.Select(_cardToTransformation));
		_closeButton.Disable();
	}

	private void CompleteSelection(NButton _)
	{
		_completionSource.SetResult(_selectedCards);
		NOverlayStack.Instance.Remove(this);
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
		List<MethodInfo> list = new List<MethodInfo>(9);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshConfirmButtonVisibility, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		list.Add(new MethodInfo(MethodName.OpenPreviewScreen, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CompleteSelection, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
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
		if (method == MethodName.RefreshConfirmButtonVisibility && args.Count == 0)
		{
			RefreshConfirmButtonVisibility();
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
		if (method == MethodName.OpenPreviewScreen && args.Count == 0)
		{
			OpenPreviewScreen();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CompleteSelection && args.Count == 1)
		{
			CompleteSelection(VariantUtils.ConvertTo<NButton>(in args[0]));
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
		if (method == MethodName.RefreshConfirmButtonVisibility)
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
		if (method == MethodName.OpenPreviewScreen)
		{
			return true;
		}
		if (method == MethodName.CompleteSelection)
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
		if (name == PropertyName._previewContainer)
		{
			_previewContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._transformPreview)
		{
			_transformPreview = VariantUtils.ConvertTo<NTransformPreview>(in value);
			return true;
		}
		if (name == PropertyName._confirmButton)
		{
			_confirmButton = VariantUtils.ConvertTo<NConfirmButton>(in value);
			return true;
		}
		if (name == PropertyName._previewCancelButton)
		{
			_previewCancelButton = VariantUtils.ConvertTo<NBackButton>(in value);
			return true;
		}
		if (name == PropertyName._previewConfirmButton)
		{
			_previewConfirmButton = VariantUtils.ConvertTo<NConfirmButton>(in value);
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
		if (name == PropertyName._viewUpgrades)
		{
			_viewUpgrades = VariantUtils.ConvertTo<NTickbox>(in value);
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
		if (name == PropertyName._previewContainer)
		{
			value = VariantUtils.CreateFrom(in _previewContainer);
			return true;
		}
		if (name == PropertyName._transformPreview)
		{
			value = VariantUtils.CreateFrom(in _transformPreview);
			return true;
		}
		if (name == PropertyName._confirmButton)
		{
			value = VariantUtils.CreateFrom(in _confirmButton);
			return true;
		}
		if (name == PropertyName._previewCancelButton)
		{
			value = VariantUtils.CreateFrom(in _previewCancelButton);
			return true;
		}
		if (name == PropertyName._previewConfirmButton)
		{
			value = VariantUtils.CreateFrom(in _previewConfirmButton);
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
		if (name == PropertyName._viewUpgrades)
		{
			value = VariantUtils.CreateFrom(in _viewUpgrades);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._previewContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._transformPreview, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._confirmButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._previewCancelButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._previewConfirmButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bottomTextContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._infoLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._viewUpgrades, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._closeButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.FocusedControlFromTopBar, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._previewContainer, Variant.From(in _previewContainer));
		info.AddProperty(PropertyName._transformPreview, Variant.From(in _transformPreview));
		info.AddProperty(PropertyName._confirmButton, Variant.From(in _confirmButton));
		info.AddProperty(PropertyName._previewCancelButton, Variant.From(in _previewCancelButton));
		info.AddProperty(PropertyName._previewConfirmButton, Variant.From(in _previewConfirmButton));
		info.AddProperty(PropertyName._bottomTextContainer, Variant.From(in _bottomTextContainer));
		info.AddProperty(PropertyName._infoLabel, Variant.From(in _infoLabel));
		info.AddProperty(PropertyName._viewUpgrades, Variant.From(in _viewUpgrades));
		info.AddProperty(PropertyName._closeButton, Variant.From(in _closeButton));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._previewContainer, out var value))
		{
			_previewContainer = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._transformPreview, out var value2))
		{
			_transformPreview = value2.As<NTransformPreview>();
		}
		if (info.TryGetProperty(PropertyName._confirmButton, out var value3))
		{
			_confirmButton = value3.As<NConfirmButton>();
		}
		if (info.TryGetProperty(PropertyName._previewCancelButton, out var value4))
		{
			_previewCancelButton = value4.As<NBackButton>();
		}
		if (info.TryGetProperty(PropertyName._previewConfirmButton, out var value5))
		{
			_previewConfirmButton = value5.As<NConfirmButton>();
		}
		if (info.TryGetProperty(PropertyName._bottomTextContainer, out var value6))
		{
			_bottomTextContainer = value6.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._infoLabel, out var value7))
		{
			_infoLabel = value7.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._viewUpgrades, out var value8))
		{
			_viewUpgrades = value8.As<NTickbox>();
		}
		if (info.TryGetProperty(PropertyName._closeButton, out var value9))
		{
			_closeButton = value9.As<NBackButton>();
		}
	}
}
