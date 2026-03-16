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
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;

[ScriptPath("res://src/Core/Nodes/Screens/CardSelection/NDeckEnchantSelectScreen.cs")]
public sealed class NDeckEnchantSelectScreen : NCardGridSelectionScreen
{
	public new class MethodName : NCardGridSelectionScreen.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName RefreshConfirmButtonVisibility = "RefreshConfirmButtonVisibility";

		public static readonly StringName CloseSelection = "CloseSelection";

		public static readonly StringName CancelSelection = "CancelSelection";

		public static readonly StringName PreviewSelection = "PreviewSelection";

		public static readonly StringName ConfirmSelection = "ConfirmSelection";

		public static readonly StringName CheckIfSelectionComplete = "CheckIfSelectionComplete";
	}

	public new class PropertyName : NCardGridSelectionScreen.PropertyName
	{
		public static readonly StringName UseSingleSelection = "UseSingleSelection";

		public new static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public new static readonly StringName FocusedControlFromTopBar = "FocusedControlFromTopBar";

		public static readonly StringName _enchantmentAmount = "_enchantmentAmount";

		public static readonly StringName _enchantSinglePreviewContainer = "_enchantSinglePreviewContainer";

		public static readonly StringName _singlePreview = "_singlePreview";

		public static readonly StringName _singlePreviewCancelButton = "_singlePreviewCancelButton";

		public static readonly StringName _singlePreviewConfirmButton = "_singlePreviewConfirmButton";

		public static readonly StringName _confirmButton = "_confirmButton";

		public static readonly StringName _enchantMultiPreviewContainer = "_enchantMultiPreviewContainer";

		public static readonly StringName _multiPreview = "_multiPreview";

		public static readonly StringName _multiPreviewCancelButton = "_multiPreviewCancelButton";

		public static readonly StringName _multiPreviewConfirmButton = "_multiPreviewConfirmButton";

		public static readonly StringName _enchantmentDescriptionContainer = "_enchantmentDescriptionContainer";

		public static readonly StringName _enchantmentTitle = "_enchantmentTitle";

		public static readonly StringName _enchantmentDescription = "_enchantmentDescription";

		public static readonly StringName _enchantmentIcon = "_enchantmentIcon";

		public static readonly StringName _bottomTextContainer = "_bottomTextContainer";

		public static readonly StringName _infoLabel = "_infoLabel";

		public static readonly StringName _closeButton = "_closeButton";
	}

	public new class SignalName : NCardGridSelectionScreen.SignalName
	{
	}

	private readonly HashSet<CardModel> _selectedCards = new HashSet<CardModel>();

	private CardSelectorPrefs _prefs;

	private EnchantmentModel _enchantment;

	private int _enchantmentAmount;

	private Control _enchantSinglePreviewContainer;

	private NEnchantPreview _singlePreview;

	private NBackButton _singlePreviewCancelButton;

	private NConfirmButton _singlePreviewConfirmButton;

	private NConfirmButton _confirmButton;

	private Control _enchantMultiPreviewContainer;

	private Control _multiPreview;

	private NBackButton _multiPreviewCancelButton;

	private NConfirmButton _multiPreviewConfirmButton;

	private Control _enchantmentDescriptionContainer;

	private MegaLabel _enchantmentTitle;

	private MegaRichTextLabel _enchantmentDescription;

	private TextureRect _enchantmentIcon;

	private Control _bottomTextContainer;

	private MegaRichTextLabel _infoLabel;

	private NBackButton _closeButton;

	private static string ScenePath => SceneHelper.GetScenePath("screens/card_selection/deck_enchant_select_screen");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	private bool UseSingleSelection => _prefs.MaxSelect == 1;

	protected override IEnumerable<Control> PeekButtonTargets => new global::_003C_003Ez__ReadOnlyArray<Control>(new Control[5] { _enchantSinglePreviewContainer, _enchantMultiPreviewContainer, _enchantmentDescriptionContainer, _closeButton, _bottomTextContainer });

	public override Control? DefaultFocusedControl
	{
		get
		{
			if (_enchantSinglePreviewContainer.Visible || _enchantMultiPreviewContainer.Visible)
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
			if (_enchantSinglePreviewContainer.Visible || _enchantMultiPreviewContainer.Visible)
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
		_enchantSinglePreviewContainer = GetNode<Control>("%EnchantSinglePreviewContainer");
		_singlePreview = _enchantSinglePreviewContainer.GetNode<NEnchantPreview>("EnchantPreview");
		_singlePreviewCancelButton = _enchantSinglePreviewContainer.GetNode<NBackButton>("Cancel");
		_singlePreviewConfirmButton = _enchantSinglePreviewContainer.GetNode<NConfirmButton>("Confirm");
		_enchantMultiPreviewContainer = GetNode<Control>("%EnchantMultiPreviewContainer");
		_multiPreview = _enchantMultiPreviewContainer.GetNode<Control>("Cards");
		_multiPreviewCancelButton = _enchantMultiPreviewContainer.GetNode<NBackButton>("Cancel");
		_multiPreviewConfirmButton = _enchantMultiPreviewContainer.GetNode<NConfirmButton>("Confirm");
		_enchantmentDescriptionContainer = GetNode<Control>("%EnchantmentDescriptionContainer");
		_enchantmentIcon = _enchantmentDescriptionContainer.GetNode<TextureRect>("%EnchantmentIcon");
		_enchantmentTitle = _enchantmentDescriptionContainer.GetNode<MegaLabel>("%EnchantmentTitle");
		_enchantmentDescription = _enchantmentDescriptionContainer.GetNode<MegaRichTextLabel>("%EnchantmentDescription");
		_closeButton = GetNode<NBackButton>("%Close");
		_singlePreviewCancelButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(CancelSelection));
		_singlePreviewConfirmButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(ConfirmSelection));
		_multiPreviewCancelButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(CancelSelection));
		_multiPreviewConfirmButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(ConfirmSelection));
		_closeButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(CloseSelection));
		_confirmButton.Connect(NClickableControl.SignalName.Released, Callable.From((Action<NButton>)PreviewSelection));
		if (_prefs.Cancelable)
		{
			_closeButton.Enable();
		}
		else
		{
			_closeButton.Disable();
		}
		EnchantmentModel enchantmentModel = _enchantment.ToMutable();
		enchantmentModel.Amount = _enchantmentAmount;
		enchantmentModel.RecalculateValues();
		_enchantmentTitle.SetTextAutoSize(enchantmentModel.Title.GetFormattedText());
		_enchantmentDescription.Text = enchantmentModel.DynamicDescription.GetFormattedText();
		_enchantmentIcon.Texture = enchantmentModel.Icon;
		_enchantSinglePreviewContainer.Visible = false;
		_enchantSinglePreviewContainer.MouseFilter = MouseFilterEnum.Ignore;
		_enchantMultiPreviewContainer.Visible = false;
		_enchantMultiPreviewContainer.MouseFilter = MouseFilterEnum.Ignore;
		RefreshConfirmButtonVisibility();
		_bottomTextContainer = GetNode<Control>("%BottomText");
		_infoLabel = _bottomTextContainer.GetNode<MegaRichTextLabel>("%BottomLabel");
		_infoLabel.Text = "[center]" + _prefs.Prompt.GetFormattedText() + "[/center]";
	}

	public static NDeckEnchantSelectScreen ShowScreen(IReadOnlyList<CardModel> cards, EnchantmentModel enchantment, int amount, CardSelectorPrefs prefs)
	{
		NDeckEnchantSelectScreen nDeckEnchantSelectScreen = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NDeckEnchantSelectScreen>(PackedScene.GenEditState.Disabled);
		nDeckEnchantSelectScreen.Name = "NDeckEnchantSelectScreen";
		nDeckEnchantSelectScreen._cards = cards;
		nDeckEnchantSelectScreen._prefs = prefs;
		nDeckEnchantSelectScreen._enchantment = enchantment;
		nDeckEnchantSelectScreen._enchantmentAmount = amount;
		NOverlayStack.Instance.Push(nDeckEnchantSelectScreen);
		return nDeckEnchantSelectScreen;
	}

	protected override void OnCardClicked(CardModel card)
	{
		if (_selectedCards.Add(card))
		{
			_grid.HighlightCard(card);
			if (_prefs.MaxSelect == _selectedCards.Count)
			{
				PreviewSelection();
			}
		}
		else
		{
			_selectedCards.Remove(card);
			_grid.UnhighlightCard(card);
		}
		RefreshConfirmButtonVisibility();
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

	private void CloseSelection(NButton _)
	{
		_completionSource.SetResult(Array.Empty<CardModel>());
		NOverlayStack.Instance.Remove(this);
	}

	private void CancelSelection(NButton _)
	{
		if (UseSingleSelection)
		{
			_singlePreviewCancelButton.Disable();
			_singlePreviewConfirmButton.Disable();
			_enchantSinglePreviewContainer.Visible = false;
			_enchantSinglePreviewContainer.MouseFilter = MouseFilterEnum.Ignore;
		}
		else
		{
			_multiPreviewCancelButton.Disable();
			_multiPreviewConfirmButton.Disable();
			_enchantMultiPreviewContainer.Visible = false;
			_enchantMultiPreviewContainer.MouseFilter = MouseFilterEnum.Ignore;
			for (int i = 0; i < _multiPreview.GetChildCount(); i++)
			{
				_multiPreview.GetChild(i).QueueFreeSafely();
			}
		}
		_grid.SetCanScroll(canScroll: true);
		ActiveScreenContext.Instance.Update();
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

	private void PreviewSelection(NButton _)
	{
		PreviewSelection();
	}

	private void PreviewSelection()
	{
		if (UseSingleSelection)
		{
			_grid.SetCanScroll(canScroll: false);
			_closeButton.Disable();
			GetViewport().GuiReleaseFocus();
			_enchantSinglePreviewContainer.Visible = true;
			_enchantSinglePreviewContainer.MouseFilter = MouseFilterEnum.Stop;
			_singlePreview.Init(_selectedCards.First(), _enchantment, _enchantmentAmount);
			_singlePreviewCancelButton.Enable();
			_singlePreviewConfirmButton.Enable();
			return;
		}
		_grid.SetCanScroll(canScroll: false);
		_closeButton.Disable();
		GetViewport().GuiReleaseFocus();
		_enchantMultiPreviewContainer.Visible = true;
		_enchantMultiPreviewContainer.MouseFilter = MouseFilterEnum.Stop;
		_multiPreviewCancelButton.Enable();
		_multiPreviewConfirmButton.Enable();
		foreach (CardModel selectedCard in _selectedCards)
		{
			NCard nCard = NCard.Create(selectedCard);
			_multiPreview.AddChildSafely(NPreviewCardHolder.Create(nCard, showHoverTips: true, scaleOnHover: false));
			nCard.UpdateVisuals(selectedCard.Pile.Type, CardPreviewMode.Normal);
		}
	}

	private void ConfirmSelection(NButton inputEvent)
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
		if (_selectedCards.Count >= _prefs.MinSelect && _selectedCards.Count <= _prefs.MaxSelect)
		{
			_completionSource.SetResult(_selectedCards);
			NOverlayStack.Instance.Remove(this);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(8);
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
		list.Add(new MethodInfo(MethodName.PreviewSelection, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.PreviewSelection, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ConfirmSelection, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.CheckIfSelectionComplete, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.PreviewSelection && args.Count == 1)
		{
			PreviewSelection(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PreviewSelection && args.Count == 0)
		{
			PreviewSelection();
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
		if (method == MethodName.PreviewSelection)
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
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._enchantmentAmount)
		{
			_enchantmentAmount = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._enchantSinglePreviewContainer)
		{
			_enchantSinglePreviewContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._singlePreview)
		{
			_singlePreview = VariantUtils.ConvertTo<NEnchantPreview>(in value);
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
		if (name == PropertyName._confirmButton)
		{
			_confirmButton = VariantUtils.ConvertTo<NConfirmButton>(in value);
			return true;
		}
		if (name == PropertyName._enchantMultiPreviewContainer)
		{
			_enchantMultiPreviewContainer = VariantUtils.ConvertTo<Control>(in value);
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
		if (name == PropertyName._enchantmentDescriptionContainer)
		{
			_enchantmentDescriptionContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._enchantmentTitle)
		{
			_enchantmentTitle = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._enchantmentDescription)
		{
			_enchantmentDescription = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._enchantmentIcon)
		{
			_enchantmentIcon = VariantUtils.ConvertTo<TextureRect>(in value);
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
		if (name == PropertyName._enchantmentAmount)
		{
			value = VariantUtils.CreateFrom(in _enchantmentAmount);
			return true;
		}
		if (name == PropertyName._enchantSinglePreviewContainer)
		{
			value = VariantUtils.CreateFrom(in _enchantSinglePreviewContainer);
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
		if (name == PropertyName._confirmButton)
		{
			value = VariantUtils.CreateFrom(in _confirmButton);
			return true;
		}
		if (name == PropertyName._enchantMultiPreviewContainer)
		{
			value = VariantUtils.CreateFrom(in _enchantMultiPreviewContainer);
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
		if (name == PropertyName._enchantmentDescriptionContainer)
		{
			value = VariantUtils.CreateFrom(in _enchantmentDescriptionContainer);
			return true;
		}
		if (name == PropertyName._enchantmentTitle)
		{
			value = VariantUtils.CreateFrom(in _enchantmentTitle);
			return true;
		}
		if (name == PropertyName._enchantmentDescription)
		{
			value = VariantUtils.CreateFrom(in _enchantmentDescription);
			return true;
		}
		if (name == PropertyName._enchantmentIcon)
		{
			value = VariantUtils.CreateFrom(in _enchantmentIcon);
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
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._enchantmentAmount, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._enchantSinglePreviewContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._singlePreview, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._singlePreviewCancelButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._singlePreviewConfirmButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._confirmButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._enchantMultiPreviewContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._multiPreview, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._multiPreviewCancelButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._multiPreviewConfirmButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._enchantmentDescriptionContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._enchantmentTitle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._enchantmentDescription, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._enchantmentIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bottomTextContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._infoLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._closeButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.FocusedControlFromTopBar, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._enchantmentAmount, Variant.From(in _enchantmentAmount));
		info.AddProperty(PropertyName._enchantSinglePreviewContainer, Variant.From(in _enchantSinglePreviewContainer));
		info.AddProperty(PropertyName._singlePreview, Variant.From(in _singlePreview));
		info.AddProperty(PropertyName._singlePreviewCancelButton, Variant.From(in _singlePreviewCancelButton));
		info.AddProperty(PropertyName._singlePreviewConfirmButton, Variant.From(in _singlePreviewConfirmButton));
		info.AddProperty(PropertyName._confirmButton, Variant.From(in _confirmButton));
		info.AddProperty(PropertyName._enchantMultiPreviewContainer, Variant.From(in _enchantMultiPreviewContainer));
		info.AddProperty(PropertyName._multiPreview, Variant.From(in _multiPreview));
		info.AddProperty(PropertyName._multiPreviewCancelButton, Variant.From(in _multiPreviewCancelButton));
		info.AddProperty(PropertyName._multiPreviewConfirmButton, Variant.From(in _multiPreviewConfirmButton));
		info.AddProperty(PropertyName._enchantmentDescriptionContainer, Variant.From(in _enchantmentDescriptionContainer));
		info.AddProperty(PropertyName._enchantmentTitle, Variant.From(in _enchantmentTitle));
		info.AddProperty(PropertyName._enchantmentDescription, Variant.From(in _enchantmentDescription));
		info.AddProperty(PropertyName._enchantmentIcon, Variant.From(in _enchantmentIcon));
		info.AddProperty(PropertyName._bottomTextContainer, Variant.From(in _bottomTextContainer));
		info.AddProperty(PropertyName._infoLabel, Variant.From(in _infoLabel));
		info.AddProperty(PropertyName._closeButton, Variant.From(in _closeButton));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._enchantmentAmount, out var value))
		{
			_enchantmentAmount = value.As<int>();
		}
		if (info.TryGetProperty(PropertyName._enchantSinglePreviewContainer, out var value2))
		{
			_enchantSinglePreviewContainer = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._singlePreview, out var value3))
		{
			_singlePreview = value3.As<NEnchantPreview>();
		}
		if (info.TryGetProperty(PropertyName._singlePreviewCancelButton, out var value4))
		{
			_singlePreviewCancelButton = value4.As<NBackButton>();
		}
		if (info.TryGetProperty(PropertyName._singlePreviewConfirmButton, out var value5))
		{
			_singlePreviewConfirmButton = value5.As<NConfirmButton>();
		}
		if (info.TryGetProperty(PropertyName._confirmButton, out var value6))
		{
			_confirmButton = value6.As<NConfirmButton>();
		}
		if (info.TryGetProperty(PropertyName._enchantMultiPreviewContainer, out var value7))
		{
			_enchantMultiPreviewContainer = value7.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._multiPreview, out var value8))
		{
			_multiPreview = value8.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._multiPreviewCancelButton, out var value9))
		{
			_multiPreviewCancelButton = value9.As<NBackButton>();
		}
		if (info.TryGetProperty(PropertyName._multiPreviewConfirmButton, out var value10))
		{
			_multiPreviewConfirmButton = value10.As<NConfirmButton>();
		}
		if (info.TryGetProperty(PropertyName._enchantmentDescriptionContainer, out var value11))
		{
			_enchantmentDescriptionContainer = value11.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._enchantmentTitle, out var value12))
		{
			_enchantmentTitle = value12.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._enchantmentDescription, out var value13))
		{
			_enchantmentDescription = value13.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._enchantmentIcon, out var value14))
		{
			_enchantmentIcon = value14.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._bottomTextContainer, out var value15))
		{
			_bottomTextContainer = value15.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._infoLabel, out var value16))
		{
			_infoLabel = value16.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._closeButton, out var value17))
		{
			_closeButton = value17.As<NBackButton>();
		}
	}
}
