using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;

[ScriptPath("res://src/Core/Nodes/Screens/CardSelection/NSimpleCardSelectScreen.cs")]
public sealed class NSimpleCardSelectScreen : NCardGridSelectionScreen
{
	public new class MethodName : NCardGridSelectionScreen.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName ConnectSignalsAndInitGrid = "ConnectSignalsAndInitGrid";

		public new static readonly StringName AfterOverlayOpened = "AfterOverlayOpened";

		public static readonly StringName CheckIfSelectionComplete = "CheckIfSelectionComplete";

		public static readonly StringName CompleteSelection = "CompleteSelection";
	}

	public new class PropertyName : NCardGridSelectionScreen.PropertyName
	{
		public static readonly StringName _bottomTextContainer = "_bottomTextContainer";

		public static readonly StringName _infoLabel = "_infoLabel";

		public static readonly StringName _confirmButton = "_confirmButton";

		public static readonly StringName _combatPiles = "_combatPiles";
	}

	public new class SignalName : NCardGridSelectionScreen.SignalName
	{
	}

	private Control _bottomTextContainer;

	private MegaRichTextLabel _infoLabel;

	private NConfirmButton _confirmButton;

	private NCombatPilesContainer _combatPiles;

	private readonly HashSet<CardModel> _selectedCards = new HashSet<CardModel>();

	private CardSelectorPrefs _prefs;

	private List<CardCreationResult>? _cardResults;

	private static string ScenePath => SceneHelper.GetScenePath("screens/card_selection/simple_card_select_screen");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	protected override IEnumerable<Control> PeekButtonTargets => new global::_003C_003Ez__ReadOnlySingleElementList<Control>(_bottomTextContainer);

	public static NSimpleCardSelectScreen Create(IReadOnlyList<CardModel> cards, CardSelectorPrefs prefs)
	{
		NSimpleCardSelectScreen nSimpleCardSelectScreen = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NSimpleCardSelectScreen>(PackedScene.GenEditState.Disabled);
		nSimpleCardSelectScreen.Name = "NSimpleCardSelectScreen";
		nSimpleCardSelectScreen._cards = cards.ToList();
		nSimpleCardSelectScreen._cardResults = null;
		nSimpleCardSelectScreen._prefs = prefs;
		return nSimpleCardSelectScreen;
	}

	public static NSimpleCardSelectScreen Create(IReadOnlyList<CardCreationResult> cards, CardSelectorPrefs prefs)
	{
		NSimpleCardSelectScreen nSimpleCardSelectScreen = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NSimpleCardSelectScreen>(PackedScene.GenEditState.Disabled);
		nSimpleCardSelectScreen.Name = "NSimpleCardSelectScreen";
		nSimpleCardSelectScreen._cards = cards.Select((CardCreationResult r) => r.Card).ToList();
		nSimpleCardSelectScreen._cardResults = cards.ToList();
		nSimpleCardSelectScreen._prefs = prefs;
		return nSimpleCardSelectScreen;
	}

	public override void _Ready()
	{
		ConnectSignalsAndInitGrid();
		_confirmButton = GetNode<NConfirmButton>("%Confirm");
		_bottomTextContainer = GetNode<Control>("%BottomText");
		_infoLabel = _bottomTextContainer.GetNode<MegaRichTextLabel>("%BottomLabel");
		_infoLabel.Text = _prefs.Prompt.GetFormattedText();
		if (_prefs.MinSelect == 0)
		{
			_confirmButton.Enable();
		}
		else
		{
			_confirmButton.Disable();
		}
		_confirmButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(delegate
		{
			CompleteSelection();
		}));
	}

	protected override void ConnectSignalsAndInitGrid()
	{
		base.ConnectSignalsAndInitGrid();
		_combatPiles = GetNode<NCombatPilesContainer>("%CombatPiles");
		if (CombatManager.Instance.IsInProgress)
		{
			_combatPiles.Initialize(_cards.First().Owner);
		}
		_combatPiles.Disable();
		_combatPiles.SetVisible(visible: false);
		_peekButton.Connect(NPeekButton.SignalName.Toggled, Callable.From<NPeekButton>(delegate
		{
			if (_peekButton.IsPeeking)
			{
				_combatPiles.Enable();
				_combatPiles.SetVisible(visible: true);
			}
			else
			{
				_combatPiles.Disable();
				_combatPiles.SetVisible(visible: false);
			}
		}));
	}

	public override void AfterOverlayOpened()
	{
		base.AfterOverlayOpened();
		TaskHelper.RunSafely(FlashRelicsOnModifiedCards());
	}

	private async Task FlashRelicsOnModifiedCards()
	{
		if (_cardResults == null)
		{
			return;
		}
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		foreach (CardCreationResult result in _cardResults)
		{
			NGridCardHolder nGridCardHolder = _grid.CurrentlyDisplayedCardHolders.FirstOrDefault((NGridCardHolder h) => h.CardModel == result.Card);
			if (nGridCardHolder == null || !result.HasBeenModified)
			{
				continue;
			}
			foreach (RelicModel modifyingRelic in result.ModifyingRelics)
			{
				modifyingRelic.Flash();
				nGridCardHolder.CardNode?.FlashRelicOnCard(modifyingRelic);
			}
		}
	}

	protected override void OnCardClicked(CardModel card)
	{
		if (_selectedCards.Contains(card))
		{
			_grid.UnhighlightCard(card);
			_selectedCards.Remove(card);
		}
		else
		{
			if (_selectedCards.Count < _prefs.MaxSelect)
			{
				_grid.HighlightCard(card);
				_selectedCards.Add(card);
			}
			if (!_prefs.RequireManualConfirmation)
			{
				CheckIfSelectionComplete();
			}
		}
		if (_selectedCards.Count >= _prefs.MinSelect && _prefs.RequireManualConfirmation)
		{
			_confirmButton.Enable();
		}
		else
		{
			_confirmButton.Disable();
		}
	}

	private void CheckIfSelectionComplete()
	{
		if (_selectedCards.Count >= _prefs.MaxSelect)
		{
			CompleteSelection();
		}
	}

	private void CompleteSelection()
	{
		_completionSource.SetResult(_selectedCards);
		NOverlayStack.Instance.Remove(this);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ConnectSignalsAndInitGrid, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AfterOverlayOpened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CheckIfSelectionComplete, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CompleteSelection, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.ConnectSignalsAndInitGrid && args.Count == 0)
		{
			ConnectSignalsAndInitGrid();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AfterOverlayOpened && args.Count == 0)
		{
			AfterOverlayOpened();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CheckIfSelectionComplete && args.Count == 0)
		{
			CheckIfSelectionComplete();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CompleteSelection && args.Count == 0)
		{
			CompleteSelection();
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
		if (method == MethodName.ConnectSignalsAndInitGrid)
		{
			return true;
		}
		if (method == MethodName.AfterOverlayOpened)
		{
			return true;
		}
		if (method == MethodName.CheckIfSelectionComplete)
		{
			return true;
		}
		if (method == MethodName.CompleteSelection)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
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
		if (name == PropertyName._confirmButton)
		{
			_confirmButton = VariantUtils.ConvertTo<NConfirmButton>(in value);
			return true;
		}
		if (name == PropertyName._combatPiles)
		{
			_combatPiles = VariantUtils.ConvertTo<NCombatPilesContainer>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
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
		if (name == PropertyName._confirmButton)
		{
			value = VariantUtils.CreateFrom(in _confirmButton);
			return true;
		}
		if (name == PropertyName._combatPiles)
		{
			value = VariantUtils.CreateFrom(in _combatPiles);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bottomTextContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._infoLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._confirmButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._combatPiles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._bottomTextContainer, Variant.From(in _bottomTextContainer));
		info.AddProperty(PropertyName._infoLabel, Variant.From(in _infoLabel));
		info.AddProperty(PropertyName._confirmButton, Variant.From(in _confirmButton));
		info.AddProperty(PropertyName._combatPiles, Variant.From(in _combatPiles));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._bottomTextContainer, out var value))
		{
			_bottomTextContainer = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._infoLabel, out var value2))
		{
			_infoLabel = value2.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._confirmButton, out var value3))
		{
			_confirmButton = value3.As<NConfirmButton>();
		}
		if (info.TryGetProperty(PropertyName._combatPiles, out var value4))
		{
			_combatPiles = value4.As<NCombatPilesContainer>();
		}
	}
}
