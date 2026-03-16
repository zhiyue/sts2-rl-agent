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
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;

[ScriptPath("res://src/Core/Nodes/Screens/CardSelection/NDeckCardSelectScreen.cs")]
public sealed class NDeckCardSelectScreen : NCardGridSelectionScreen
{
	public new class MethodName : NCardGridSelectionScreen.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName RefreshConfirmButtonVisibility = "RefreshConfirmButtonVisibility";

		public static readonly StringName PreviewSelection = "PreviewSelection";

		public static readonly StringName CloseSelection = "CloseSelection";

		public static readonly StringName CancelSelection = "CancelSelection";

		public static readonly StringName ConfirmSelection = "ConfirmSelection";

		public static readonly StringName CheckIfSelectionComplete = "CheckIfSelectionComplete";

		public new static readonly StringName AfterOverlayShown = "AfterOverlayShown";

		public new static readonly StringName AfterOverlayHidden = "AfterOverlayHidden";
	}

	public new class PropertyName : NCardGridSelectionScreen.PropertyName
	{
		public new static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public new static readonly StringName FocusedControlFromTopBar = "FocusedControlFromTopBar";

		public static readonly StringName _previewContainer = "_previewContainer";

		public static readonly StringName _previewCards = "_previewCards";

		public static readonly StringName _previewCancelButton = "_previewCancelButton";

		public static readonly StringName _previewConfirmButton = "_previewConfirmButton";

		public static readonly StringName _closeButton = "_closeButton";

		public static readonly StringName _confirmButton = "_confirmButton";

		public static readonly StringName _infoLabel = "_infoLabel";
	}

	public new class SignalName : NCardGridSelectionScreen.SignalName
	{
	}

	private readonly HashSet<CardModel> _selectedCards = new HashSet<CardModel>();

	private CardSelectorPrefs _prefs;

	private Control _previewContainer;

	private Control _previewCards;

	private NBackButton _previewCancelButton;

	private NConfirmButton _previewConfirmButton;

	private NBackButton _closeButton;

	private NConfirmButton _confirmButton;

	private MegaRichTextLabel _infoLabel;

	private static string ScenePath => SceneHelper.GetScenePath("screens/card_selection/deck_card_select_screen");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	protected override IEnumerable<Control> PeekButtonTargets => new global::_003C_003Ez__ReadOnlyArray<Control>(new Control[3] { _previewContainer, _closeButton, _confirmButton });

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
		_previewContainer = GetNode<Control>("%PreviewContainer");
		_previewCards = _previewContainer.GetNode<Control>("%Cards");
		_previewCancelButton = _previewContainer.GetNode<NBackButton>("%PreviewCancel");
		_previewConfirmButton = _previewContainer.GetNode<NConfirmButton>("%PreviewConfirm");
		_closeButton = GetNode<NBackButton>("%Close");
		_confirmButton = GetNode<NConfirmButton>("%Confirm");
		_infoLabel = GetNode<MegaRichTextLabel>("%BottomLabel");
		_previewCancelButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(CancelSelection));
		_previewConfirmButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(ConfirmSelection));
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
		RefreshConfirmButtonVisibility();
		_previewContainer.Visible = false;
		_previewContainer.MouseFilter = MouseFilterEnum.Ignore;
		_previewCancelButton.Disable();
		_previewConfirmButton.Disable();
		_infoLabel.Text = _prefs.Prompt.GetFormattedText();
	}

	public static NDeckCardSelectScreen Create(IReadOnlyList<CardModel> cards, CardSelectorPrefs prefs)
	{
		NDeckCardSelectScreen nDeckCardSelectScreen = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NDeckCardSelectScreen>(PackedScene.GenEditState.Disabled);
		nDeckCardSelectScreen.Name = "NDeckCardSelectScreen";
		nDeckCardSelectScreen._cards = cards;
		nDeckCardSelectScreen._prefs = prefs;
		return nDeckCardSelectScreen;
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

	private void PreviewSelection(NButton _)
	{
		PreviewSelection();
	}

	private void PreviewSelection()
	{
		GetViewport().GuiReleaseFocus();
		_previewContainer.Visible = true;
		_previewContainer.MouseFilter = MouseFilterEnum.Stop;
		_closeButton.Disable();
		_grid.SetCanScroll(canScroll: false);
		_previewCancelButton.Enable();
		_previewConfirmButton.Enable();
		foreach (CardModel selectedCard in _selectedCards)
		{
			_grid.UnhighlightCard(selectedCard);
			NCard nCard = NCard.Create(selectedCard);
			NPreviewCardHolder child = NPreviewCardHolder.Create(nCard, showHoverTips: true, scaleOnHover: false);
			_previewCards.AddChildSafely(child);
			nCard.UpdateVisuals(selectedCard.Pile.Type, CardPreviewMode.Normal);
		}
		Callable.From(delegate
		{
			_previewCards.PivotOffset = _previewCards.Size / 2f;
			float num = 1f;
			if (_selectedCards.Count > 6)
			{
				num = 0.55f;
			}
			else if (_selectedCards.Count > 3)
			{
				num = 0.8f;
			}
			_previewCards.Scale = Vector2.One * num;
		}).CallDeferred();
	}

	private void CloseSelection(NButton _)
	{
		_completionSource.SetResult(Array.Empty<CardModel>());
		NOverlayStack.Instance.Remove(this);
	}

	private void CancelSelection(NButton _)
	{
		_previewContainer.Visible = false;
		_previewCancelButton.Disable();
		_previewConfirmButton.Disable();
		_grid.SetCanScroll(canScroll: true);
		_previewContainer.MouseFilter = MouseFilterEnum.Ignore;
		for (int i = 0; i < _previewCards.GetChildCount(); i++)
		{
			_previewCards.GetChild(i).QueueFreeSafely();
		}
		_grid.GetCardHolder(_selectedCards.Last())?.TryGrabFocus();
		_selectedCards.Clear();
		if (_prefs.Cancelable)
		{
			_closeButton.Enable();
		}
	}

	private void ConfirmSelection(NButton _)
	{
		CheckIfSelectionComplete();
	}

	private void CheckIfSelectionComplete()
	{
		if (_selectedCards.Count >= _prefs.MinSelect)
		{
			_completionSource.SetResult(_selectedCards);
			NOverlayStack.Instance.Remove(this);
		}
	}

	public override void AfterOverlayShown()
	{
		if (_prefs.Cancelable)
		{
			_closeButton.Enable();
		}
	}

	public override void AfterOverlayHidden()
	{
		_closeButton.Disable();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(10);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshConfirmButtonVisibility, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.PreviewSelection, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.PreviewSelection, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		list.Add(new MethodInfo(MethodName.AfterOverlayShown, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AfterOverlayHidden, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.AfterOverlayShown && args.Count == 0)
		{
			AfterOverlayShown();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AfterOverlayHidden && args.Count == 0)
		{
			AfterOverlayHidden();
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
		if (method == MethodName.PreviewSelection)
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
		if (method == MethodName.AfterOverlayShown)
		{
			return true;
		}
		if (method == MethodName.AfterOverlayHidden)
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
		if (name == PropertyName._previewCards)
		{
			_previewCards = VariantUtils.ConvertTo<Control>(in value);
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
		if (name == PropertyName._closeButton)
		{
			_closeButton = VariantUtils.ConvertTo<NBackButton>(in value);
			return true;
		}
		if (name == PropertyName._confirmButton)
		{
			_confirmButton = VariantUtils.ConvertTo<NConfirmButton>(in value);
			return true;
		}
		if (name == PropertyName._infoLabel)
		{
			_infoLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
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
		if (name == PropertyName._previewCards)
		{
			value = VariantUtils.CreateFrom(in _previewCards);
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
		if (name == PropertyName._closeButton)
		{
			value = VariantUtils.CreateFrom(in _closeButton);
			return true;
		}
		if (name == PropertyName._confirmButton)
		{
			value = VariantUtils.CreateFrom(in _confirmButton);
			return true;
		}
		if (name == PropertyName._infoLabel)
		{
			value = VariantUtils.CreateFrom(in _infoLabel);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._previewContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._previewCards, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._previewCancelButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._previewConfirmButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._closeButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._confirmButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._infoLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.FocusedControlFromTopBar, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._previewContainer, Variant.From(in _previewContainer));
		info.AddProperty(PropertyName._previewCards, Variant.From(in _previewCards));
		info.AddProperty(PropertyName._previewCancelButton, Variant.From(in _previewCancelButton));
		info.AddProperty(PropertyName._previewConfirmButton, Variant.From(in _previewConfirmButton));
		info.AddProperty(PropertyName._closeButton, Variant.From(in _closeButton));
		info.AddProperty(PropertyName._confirmButton, Variant.From(in _confirmButton));
		info.AddProperty(PropertyName._infoLabel, Variant.From(in _infoLabel));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._previewContainer, out var value))
		{
			_previewContainer = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._previewCards, out var value2))
		{
			_previewCards = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._previewCancelButton, out var value3))
		{
			_previewCancelButton = value3.As<NBackButton>();
		}
		if (info.TryGetProperty(PropertyName._previewConfirmButton, out var value4))
		{
			_previewConfirmButton = value4.As<NConfirmButton>();
		}
		if (info.TryGetProperty(PropertyName._closeButton, out var value5))
		{
			_closeButton = value5.As<NBackButton>();
		}
		if (info.TryGetProperty(PropertyName._confirmButton, out var value6))
		{
			_confirmButton = value6.As<NConfirmButton>();
		}
		if (info.TryGetProperty(PropertyName._infoLabel, out var value7))
		{
			_infoLabel = value7.As<MegaRichTextLabel>();
		}
	}
}
