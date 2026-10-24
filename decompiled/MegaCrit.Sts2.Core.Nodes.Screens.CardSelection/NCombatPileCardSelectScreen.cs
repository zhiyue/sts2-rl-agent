using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
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
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;

[ScriptPath("res://src/Core/Nodes/Screens/CardSelection/NCombatPileCardSelectScreen.cs")]
public sealed class NCombatPileCardSelectScreen : NCardGridSelectionScreen
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : NCardGridSelectionScreen.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the '_EnterTree' method.
		/// </summary>
		public new static readonly StringName _EnterTree = "_EnterTree";

		/// <summary>
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";

		/// <summary>
		/// Cached name for the 'ConnectSignalsAndInitGrid' method.
		/// </summary>
		public new static readonly StringName ConnectSignalsAndInitGrid = "ConnectSignalsAndInitGrid";

		/// <summary>
		/// Cached name for the 'AfterOverlayOpened' method.
		/// </summary>
		public new static readonly StringName AfterOverlayOpened = "AfterOverlayOpened";

		/// <summary>
		/// Cached name for the 'UpdateConfirmButton' method.
		/// </summary>
		public static readonly StringName UpdateConfirmButton = "UpdateConfirmButton";

		/// <summary>
		/// Cached name for the 'CheckIfSelectionComplete' method.
		/// </summary>
		public static readonly StringName CheckIfSelectionComplete = "CheckIfSelectionComplete";

		/// <summary>
		/// Cached name for the 'CompleteSelection' method.
		/// </summary>
		public static readonly StringName CompleteSelection = "CompleteSelection";

		/// <summary>
		/// Cached name for the 'UnsubscribeFromPile' method.
		/// </summary>
		public static readonly StringName UnsubscribeFromPile = "UnsubscribeFromPile";

		/// <summary>
		/// Cached name for the 'UpdatePileContents' method.
		/// </summary>
		public static readonly StringName UpdatePileContents = "UpdatePileContents";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : NCardGridSelectionScreen.PropertyName
	{
		/// <summary>
		/// Cached name for the '_bottomTextContainer' field.
		/// </summary>
		public static readonly StringName _bottomTextContainer = "_bottomTextContainer";

		/// <summary>
		/// Cached name for the '_infoLabel' field.
		/// </summary>
		public static readonly StringName _infoLabel = "_infoLabel";

		/// <summary>
		/// Cached name for the '_confirmButton' field.
		/// </summary>
		public static readonly StringName _confirmButton = "_confirmButton";

		/// <summary>
		/// Cached name for the '_combatPiles' field.
		/// </summary>
		public static readonly StringName _combatPiles = "_combatPiles";

		/// <summary>
		/// Cached name for the '_isSubscribedToPile' field.
		/// </summary>
		public static readonly StringName _isSubscribedToPile = "_isSubscribedToPile";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NCardGridSelectionScreen.SignalName
	{
	}

	private Control _bottomTextContainer;

	private MegaRichTextLabel _infoLabel;

	private NConfirmButton _confirmButton;

	private NCombatPilesContainer _combatPiles;

	private CancellationTokenSource _cts = new CancellationTokenSource();

	private HashSet<CardModel> _selectedCards = new HashSet<CardModel>();

	private CardSelectorPrefs _prefs;

	private CardPile _pile;

	private Func<CardModel, bool> _filter;

	private List<CardCreationResult>? _cardResults;

	private bool _isSubscribedToPile;

	private static string ScenePath => SceneHelper.GetScenePath("screens/card_selection/combat_pile_card_select_screen");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	protected override IEnumerable<Control> PeekButtonTargets => new global::_003C_003Ez__ReadOnlySingleElementList<Control>(_bottomTextContainer);

	public static NCombatPileCardSelectScreen Create(CardPile pile, CardSelectorPrefs prefs, Func<CardModel, bool> filter)
	{
		NCombatPileCardSelectScreen nCombatPileCardSelectScreen = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NCombatPileCardSelectScreen>(PackedScene.GenEditState.Disabled);
		nCombatPileCardSelectScreen.Name = "NCombatPileCardSelectScreen";
		nCombatPileCardSelectScreen._cardResults = null;
		nCombatPileCardSelectScreen._prefs = prefs;
		nCombatPileCardSelectScreen._cards = Array.Empty<CardModel>();
		nCombatPileCardSelectScreen._pile = pile;
		nCombatPileCardSelectScreen._filter = filter;
		return nCombatPileCardSelectScreen;
	}

	public override void _Ready()
	{
		ConnectSignalsAndInitGrid();
		_confirmButton = GetNode<NConfirmButton>("%Confirm");
		_bottomTextContainer = GetNode<Control>("%BottomText");
		_infoLabel = _bottomTextContainer.GetNode<MegaRichTextLabel>("%BottomLabel");
		_infoLabel.Text = _prefs.Prompt.GetFormattedText();
		UpdatePileContents();
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

	public override void _EnterTree()
	{
		_cts = new CancellationTokenSource();
		_pile.ContentsChanged += UpdatePileContents;
		_isSubscribedToPile = true;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		_cts.Cancel();
		UnsubscribeFromPile();
	}

	protected override void ConnectSignalsAndInitGrid()
	{
		base.ConnectSignalsAndInitGrid();
		_combatPiles = GetNode<NCombatPilesContainer>("%CombatPiles");
		if (CombatManager.Instance.IsInProgress)
		{
			_combatPiles.Initialize(_pile.Cards.First().Owner);
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
		await this.AwaitProcessFrame(_cts.Token);
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
		UpdateConfirmButton();
	}

	private void UpdateConfirmButton()
	{
		int num = Mathf.Min(_prefs.MinSelect, _grid.CurrentlyDisplayedCards.Count());
		if (_selectedCards.Count >= num && _prefs.RequireManualConfirmation)
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
		int num = Mathf.Min(_prefs.MaxSelect, _grid.CurrentlyDisplayedCards.Count());
		if (_selectedCards.Count >= num)
		{
			CompleteSelection();
		}
	}

	private void CompleteSelection()
	{
		UnsubscribeFromPile();
		_completionSource.SetResult(_selectedCards);
		NOverlayStack.Instance.Remove(this);
	}

	private void UnsubscribeFromPile()
	{
		if (_isSubscribedToPile)
		{
			_pile.ContentsChanged -= UpdatePileContents;
			_isSubscribedToPile = false;
		}
	}

	private void UpdatePileContents()
	{
		List<CardModel> validPileCards = _pile.Cards.Where(_filter).ToList();
		_selectedCards = _selectedCards.Where((CardModel c) => validPileCards.Contains(c)).ToHashSet();
		if (validPileCards.Count == 0)
		{
			_selectedCards.Clear();
			CompleteSelection();
			return;
		}
		if (validPileCards.Count == _selectedCards.Count)
		{
			CompleteSelection();
			return;
		}
		List<SortingOrders> list;
		if (_pile.Type != PileType.Draw)
		{
			int num = 1;
			list = new List<SortingOrders>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<SortingOrders> span = CollectionsMarshal.AsSpan(list);
			int index = 0;
			span[index] = SortingOrders.Ascending;
		}
		else
		{
			int index = 2;
			list = new List<SortingOrders>(index);
			CollectionsMarshal.SetCount(list, index);
			Span<SortingOrders> span = CollectionsMarshal.AsSpan(list);
			int num = 0;
			span[num] = SortingOrders.RarityAscending;
			num++;
			span[num] = SortingOrders.AlphabetAscending;
		}
		List<SortingOrders> sortingPriority = list;
		_grid.SetCards(validPileCards, _pile.Type, sortingPriority);
		UpdateConfirmButton();
		foreach (CardModel selectedCard in _selectedCards)
		{
			_grid.HighlightCard(selectedCard);
		}
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(10);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ConnectSignalsAndInitGrid, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AfterOverlayOpened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateConfirmButton, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CheckIfSelectionComplete, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CompleteSelection, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UnsubscribeFromPile, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdatePileContents, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._EnterTree && args.Count == 0)
		{
			_EnterTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
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
		if (method == MethodName.UpdateConfirmButton && args.Count == 0)
		{
			UpdateConfirmButton();
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
		if (method == MethodName.UnsubscribeFromPile && args.Count == 0)
		{
			UnsubscribeFromPile();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdatePileContents && args.Count == 0)
		{
			UpdatePileContents();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
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
		if (method == MethodName.UpdateConfirmButton)
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
		if (method == MethodName.UnsubscribeFromPile)
		{
			return true;
		}
		if (method == MethodName.UpdatePileContents)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
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
		if (name == PropertyName._isSubscribedToPile)
		{
			_isSubscribedToPile = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
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
		if (name == PropertyName._isSubscribedToPile)
		{
			value = VariantUtils.CreateFrom(in _isSubscribedToPile);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	/// <summary>
	/// Get the property information for all the properties declared in this class.
	/// This method is used by Godot to register the available properties in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bottomTextContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._infoLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._confirmButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._combatPiles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isSubscribedToPile, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._bottomTextContainer, Variant.From(in _bottomTextContainer));
		info.AddProperty(PropertyName._infoLabel, Variant.From(in _infoLabel));
		info.AddProperty(PropertyName._confirmButton, Variant.From(in _confirmButton));
		info.AddProperty(PropertyName._combatPiles, Variant.From(in _combatPiles));
		info.AddProperty(PropertyName._isSubscribedToPile, Variant.From(in _isSubscribedToPile));
	}

	/// <inheritdoc />
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
		if (info.TryGetProperty(PropertyName._isSubscribedToPile, out var value5))
		{
			_isSubscribedToPile = value5.As<bool>();
		}
	}
}
