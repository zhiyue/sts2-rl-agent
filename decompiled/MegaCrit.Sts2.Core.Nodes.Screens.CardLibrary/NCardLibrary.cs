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
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.Unlocks;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;

[ScriptPath("res://src/Core/Nodes/Screens/CardLibrary/NCardLibrary.cs")]
public sealed class NCardLibrary : NSubmenu
{
	public new class MethodName : NSubmenu.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnCardTypeSort = "OnCardTypeSort";

		public static readonly StringName OnRaritySort = "OnRaritySort";

		public static readonly StringName OnCostSort = "OnCostSort";

		public static readonly StringName OnAlphabetSort = "OnAlphabetSort";

		public new static readonly StringName OnSubmenuOpened = "OnSubmenuOpened";

		public new static readonly StringName OnSubmenuClosed = "OnSubmenuClosed";

		public static readonly StringName ToggleShowStats = "ToggleShowStats";

		public static readonly StringName ToggleShowUpgrades = "ToggleShowUpgrades";

		public static readonly StringName ToggleFilterMultiplayerCards = "ToggleFilterMultiplayerCards";

		public static readonly StringName UpdateCardPoolFilter = "UpdateCardPoolFilter";

		public static readonly StringName UpdateTypeFilter = "UpdateTypeFilter";

		public static readonly StringName UpdateRarityFilter = "UpdateRarityFilter";

		public static readonly StringName UpdateCostFilter = "UpdateCostFilter";

		public static readonly StringName SearchBarQueryChanged = "SearchBarQueryChanged";

		public static readonly StringName SearchBarQuerySubmitted = "SearchBarQuerySubmitted";

		public static readonly StringName UpdateFilter = "UpdateFilter";

		public static readonly StringName ShowCardDetail = "ShowCardDetail";
	}

	public new class PropertyName : NSubmenu.PropertyName
	{
		public new static readonly StringName InitialFocusedControl = "InitialFocusedControl";

		public static readonly StringName _grid = "_grid";

		public static readonly StringName _searchBar = "_searchBar";

		public static readonly StringName _ironcladFilter = "_ironcladFilter";

		public static readonly StringName _silentFilter = "_silentFilter";

		public static readonly StringName _defectFilter = "_defectFilter";

		public static readonly StringName _regentFilter = "_regentFilter";

		public static readonly StringName _necrobinderFilter = "_necrobinderFilter";

		public static readonly StringName _colorlessFilter = "_colorlessFilter";

		public static readonly StringName _ancientsFilter = "_ancientsFilter";

		public static readonly StringName _miscPoolFilter = "_miscPoolFilter";

		public static readonly StringName _typeSorter = "_typeSorter";

		public static readonly StringName _attackFilter = "_attackFilter";

		public static readonly StringName _skillFilter = "_skillFilter";

		public static readonly StringName _powerFilter = "_powerFilter";

		public static readonly StringName _otherTypeFilter = "_otherTypeFilter";

		public static readonly StringName _raritySorter = "_raritySorter";

		public static readonly StringName _commonFilter = "_commonFilter";

		public static readonly StringName _uncommonFilter = "_uncommonFilter";

		public static readonly StringName _rareFilter = "_rareFilter";

		public static readonly StringName _otherFilter = "_otherFilter";

		public static readonly StringName _costSorter = "_costSorter";

		public static readonly StringName _zeroFilter = "_zeroFilter";

		public static readonly StringName _oneFilter = "_oneFilter";

		public static readonly StringName _twoFilter = "_twoFilter";

		public static readonly StringName _threePlusFilter = "_threePlusFilter";

		public static readonly StringName _xFilter = "_xFilter";

		public static readonly StringName _alphabetSorter = "_alphabetSorter";

		public static readonly StringName _viewMultiplayerCards = "_viewMultiplayerCards";

		public static readonly StringName _viewStats = "_viewStats";

		public static readonly StringName _viewUpgrades = "_viewUpgrades";

		public static readonly StringName _cardCountLabel = "_cardCountLabel";

		public static readonly StringName _noResultsLabel = "_noResultsLabel";

		public static readonly StringName _lastHoveredControl = "_lastHoveredControl";
	}

	public new class SignalName : NSubmenu.SignalName
	{
	}

	private const int _delayAfterTextFilterChangedMsec = 250;

	private static readonly string _scenePath = SceneHelper.GetScenePath("screens/card_library/card_library");

	private readonly LocString _cardCountLocString = new LocString("card_library", "CARD_COUNT");

	private readonly LocString _noResultsLocString = new LocString("card_library", "NO_RESULTS");

	private IRunState? _runState;

	private NCardLibraryGrid _grid;

	private NSearchBar _searchBar;

	private readonly Dictionary<string, Func<CardModel, bool>> _specialSearchbarKeywords = new Dictionary<string, Func<CardModel, bool>>();

	private readonly Dictionary<CharacterModel, NCardPoolFilter> _cardPoolFilters = new Dictionary<CharacterModel, NCardPoolFilter>();

	private NCardPoolFilter _ironcladFilter;

	private NCardPoolFilter _silentFilter;

	private NCardPoolFilter _defectFilter;

	private NCardPoolFilter _regentFilter;

	private NCardPoolFilter _necrobinderFilter;

	private NCardPoolFilter _colorlessFilter;

	private NCardPoolFilter _ancientsFilter;

	private NCardPoolFilter _miscPoolFilter;

	private readonly Dictionary<NCardPoolFilter, Func<CardModel, bool>> _poolFilters = new Dictionary<NCardPoolFilter, Func<CardModel, bool>>();

	private NCardViewSortButton _typeSorter;

	private NCardTypeTickbox _attackFilter;

	private NCardTypeTickbox _skillFilter;

	private NCardTypeTickbox _powerFilter;

	private NCardTypeTickbox _otherTypeFilter;

	private readonly Dictionary<NCardTypeTickbox, Func<CardModel, bool>> _cardTypeFilters = new Dictionary<NCardTypeTickbox, Func<CardModel, bool>>();

	private NCardViewSortButton _raritySorter;

	private NCardRarityTickbox _commonFilter;

	private NCardRarityTickbox _uncommonFilter;

	private NCardRarityTickbox _rareFilter;

	private NCardRarityTickbox _otherFilter;

	private readonly Dictionary<NCardRarityTickbox, Func<CardModel, bool>> _rarityFilters = new Dictionary<NCardRarityTickbox, Func<CardModel, bool>>();

	private NCardViewSortButton _costSorter;

	private NCardCostTickbox _zeroFilter;

	private NCardCostTickbox _oneFilter;

	private NCardCostTickbox _twoFilter;

	private NCardCostTickbox _threePlusFilter;

	private NCardCostTickbox _xFilter;

	private readonly Dictionary<NCardCostTickbox, Func<CardModel, bool>> _costFilters = new Dictionary<NCardCostTickbox, Func<CardModel, bool>>();

	private NCardViewSortButton _alphabetSorter;

	private NLibraryStatTickbox _viewMultiplayerCards;

	private NLibraryStatTickbox _viewStats;

	private NLibraryStatTickbox _viewUpgrades;

	private MegaRichTextLabel _cardCountLabel;

	private MegaRichTextLabel _noResultsLabel;

	private CancellationTokenSource? _displayCardsShortDelayCancelToken;

	private readonly List<SortingOrders> _sortingPriority;

	private Func<CardModel, bool> _filter;

	private Control? _lastHoveredControl;

	public static string[] AssetPaths => new string[1] { _scenePath };

	protected override Control InitialFocusedControl => _lastHoveredControl ?? _ironcladFilter;

	public static NCardLibrary? Create()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		return PreloadManager.Cache.GetScene(_scenePath).Instantiate<NCardLibrary>(PackedScene.GenEditState.Disabled);
	}

	public override void _Ready()
	{
		ConnectSignals();
		_grid = GetNode<NCardLibraryGrid>("%CardGrid");
		_grid.Connect(NCardGrid.SignalName.HolderPressed, Callable.From<NCardHolder>(ShowCardDetail));
		_grid.Connect(NCardGrid.SignalName.HolderAltPressed, Callable.From<NCardHolder>(ShowCardDetail));
		_cardCountLabel = GetNode<MegaRichTextLabel>("%CardCountLabel");
		_noResultsLabel = GetNode<MegaRichTextLabel>("%NoResultsLabel");
		_noResultsLabel.Text = _noResultsLocString.GetFormattedText();
		_searchBar = GetNode<NSearchBar>("%SearchBar");
		_searchBar.Connect(NSearchBar.SignalName.QueryChanged, Callable.From<string>(SearchBarQueryChanged));
		_searchBar.Connect(NSearchBar.SignalName.QuerySubmitted, Callable.From<string>(SearchBarQuerySubmitted));
		_ironcladFilter = GetNode<NCardPoolFilter>("%IroncladPool");
		_silentFilter = GetNode<NCardPoolFilter>("%SilentPool");
		_defectFilter = GetNode<NCardPoolFilter>("%DefectPool");
		_regentFilter = GetNode<NCardPoolFilter>("%RegentPool");
		_necrobinderFilter = GetNode<NCardPoolFilter>("%NecrobinderPool");
		_colorlessFilter = GetNode<NCardPoolFilter>("%ColorlessPool");
		_ancientsFilter = GetNode<NCardPoolFilter>("%AncientsPool");
		_miscPoolFilter = GetNode<NCardPoolFilter>("%MiscPool");
		Callable callable = Callable.From<NCardPoolFilter>(UpdateCardPoolFilter);
		_ironcladFilter.Connect(NCardPoolFilter.SignalName.Toggled, callable);
		_silentFilter.Connect(NCardPoolFilter.SignalName.Toggled, callable);
		_defectFilter.Connect(NCardPoolFilter.SignalName.Toggled, callable);
		_regentFilter.Connect(NCardPoolFilter.SignalName.Toggled, callable);
		_necrobinderFilter.Connect(NCardPoolFilter.SignalName.Toggled, callable);
		_colorlessFilter.Connect(NCardPoolFilter.SignalName.Toggled, callable);
		_ancientsFilter.Connect(NCardPoolFilter.SignalName.Toggled, callable);
		_miscPoolFilter.Connect(NCardPoolFilter.SignalName.Toggled, callable);
		_poolFilters.Add(_ironcladFilter, (CardModel c) => c.Pool is IroncladCardPool);
		_poolFilters.Add(_silentFilter, (CardModel c) => c.Pool is SilentCardPool);
		_poolFilters.Add(_defectFilter, (CardModel c) => c.Pool is DefectCardPool);
		_poolFilters.Add(_regentFilter, (CardModel c) => c.Pool is RegentCardPool);
		_poolFilters.Add(_necrobinderFilter, (CardModel c) => c.Pool is NecrobinderCardPool);
		_poolFilters.Add(_colorlessFilter, (CardModel c) => c.Pool is ColorlessCardPool);
		_poolFilters.Add(_ancientsFilter, (CardModel c) => c.Rarity == CardRarity.Ancient);
		_poolFilters.Add(_miscPoolFilter, delegate(CardModel c)
		{
			CardRarity rarity = c.Rarity;
			return (uint)(rarity - 6) <= 4u;
		});
		_typeSorter = GetNode<NCardViewSortButton>("%CardTypeSorter");
		_typeSorter.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnCardTypeSort));
		_attackFilter = GetNode<NCardTypeTickbox>("%AttackType");
		_skillFilter = GetNode<NCardTypeTickbox>("%SkillType");
		_powerFilter = GetNode<NCardTypeTickbox>("%PowerType");
		_otherTypeFilter = GetNode<NCardTypeTickbox>("%OtherType");
		Callable callable2 = Callable.From<NCardTypeTickbox>(UpdateTypeFilter);
		_attackFilter.Connect(NCardTypeTickbox.SignalName.Toggled, callable2);
		_skillFilter.Connect(NCardTypeTickbox.SignalName.Toggled, callable2);
		_powerFilter.Connect(NCardTypeTickbox.SignalName.Toggled, callable2);
		_otherTypeFilter.Connect(NCardTypeTickbox.SignalName.Toggled, callable2);
		_cardTypeFilters.Add(_attackFilter, (CardModel c) => c.Type == CardType.Attack);
		_cardTypeFilters.Add(_skillFilter, (CardModel c) => c.Type == CardType.Skill);
		_cardTypeFilters.Add(_powerFilter, (CardModel c) => c.Type == CardType.Power);
		_cardTypeFilters.Add(_otherTypeFilter, delegate(CardModel c)
		{
			CardType type = c.Type;
			bool flag = (uint)(type - 1) <= 2u;
			return !flag;
		});
		_raritySorter = GetNode<NCardViewSortButton>("%RaritySorter");
		_raritySorter.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnRaritySort));
		_commonFilter = GetNode<NCardRarityTickbox>("%CommonRarity");
		_uncommonFilter = GetNode<NCardRarityTickbox>("%UncommonRarity");
		_rareFilter = GetNode<NCardRarityTickbox>("%RareRarity");
		_otherFilter = GetNode<NCardRarityTickbox>("%OtherRarity");
		Callable callable3 = Callable.From<NTickbox>(UpdateRarityFilter);
		_commonFilter.Connect(NTickbox.SignalName.Toggled, callable3);
		_uncommonFilter.Connect(NTickbox.SignalName.Toggled, callable3);
		_rareFilter.Connect(NTickbox.SignalName.Toggled, callable3);
		_otherFilter.Connect(NTickbox.SignalName.Toggled, callable3);
		_rarityFilters.Add(_commonFilter, (CardModel c) => c.Rarity == CardRarity.Common);
		_rarityFilters.Add(_uncommonFilter, (CardModel c) => c.Rarity == CardRarity.Uncommon);
		_rarityFilters.Add(_rareFilter, (CardModel c) => c.Rarity == CardRarity.Rare);
		_rarityFilters.Add(_otherFilter, delegate(CardModel c)
		{
			CardRarity rarity = c.Rarity;
			bool flag = (uint)(rarity - 2) <= 2u;
			return !flag;
		});
		_costSorter = GetNode<NCardViewSortButton>("%CostSorter");
		_costSorter.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnCostSort));
		_zeroFilter = GetNode<NCardCostTickbox>("%Cost0");
		_oneFilter = GetNode<NCardCostTickbox>("%Cost1");
		_twoFilter = GetNode<NCardCostTickbox>("%Cost2");
		_threePlusFilter = GetNode<NCardCostTickbox>("%Cost3+");
		_xFilter = GetNode<NCardCostTickbox>("%CostX");
		Callable callable4 = Callable.From<NCardCostTickbox>(UpdateCostFilter);
		_zeroFilter.Connect(NClickableControl.SignalName.Released, callable4);
		_oneFilter.Connect(NClickableControl.SignalName.Released, callable4);
		_twoFilter.Connect(NClickableControl.SignalName.Released, callable4);
		_threePlusFilter.Connect(NClickableControl.SignalName.Released, callable4);
		_xFilter.Connect(NClickableControl.SignalName.Released, callable4);
		_costFilters.Add(_zeroFilter, delegate(CardModel c)
		{
			CardEnergyCost energyCost = c.EnergyCost;
			return energyCost != null && energyCost.Canonical <= 0 && !energyCost.CostsX;
		});
		_costFilters.Add(_oneFilter, (CardModel c) => c.EnergyCost.Canonical == 1);
		_costFilters.Add(_twoFilter, (CardModel c) => c.EnergyCost.Canonical == 2);
		_costFilters.Add(_threePlusFilter, (CardModel c) => c.EnergyCost.Canonical >= 3);
		_costFilters.Add(_xFilter, (CardModel c) => c.EnergyCost.CostsX || c.HasStarCostX);
		_alphabetSorter = GetNode<NCardViewSortButton>("%AlphabetSorter");
		_alphabetSorter.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnAlphabetSort));
		_viewStats = GetNode<NLibraryStatTickbox>("%Stats");
		_viewUpgrades = GetNode<NLibraryStatTickbox>("%Upgrades");
		_viewMultiplayerCards = GetNode<NLibraryStatTickbox>("%MultiplayerCards");
		_viewStats.Connect(NTickbox.SignalName.Toggled, Callable.From<NTickbox>(ToggleShowStats));
		_viewUpgrades.Connect(NTickbox.SignalName.Toggled, Callable.From<NTickbox>(ToggleShowUpgrades));
		_viewMultiplayerCards.Connect(NTickbox.SignalName.Toggled, Callable.From<NTickbox>(ToggleFilterMultiplayerCards));
		_typeSorter.SetLabel(new LocString("gameplay_ui", "SORT_TYPE").GetRawText());
		_raritySorter.SetLabel(new LocString("gameplay_ui", "SORT_RARITY").GetRawText());
		_costSorter.SetLabel(new LocString("gameplay_ui", "SORT_COST").GetRawText());
		_alphabetSorter.SetLabel(new LocString("gameplay_ui", "SORT_ALPHABET").GetRawText());
		_commonFilter.SetLabel(new LocString("card_library", "RARITY_COMMON").GetRawText());
		_uncommonFilter.SetLabel(new LocString("card_library", "RARITY_UNCOMMON").GetRawText());
		_rareFilter.SetLabel(new LocString("card_library", "RARITY_RARE").GetRawText());
		_otherFilter.SetLabel(new LocString("card_library", "RARITY_OTHER").GetRawText());
		_viewStats.SetLabel(new LocString("card_library", "VIEW_STATS").GetRawText());
		_viewUpgrades.SetLabel(new LocString("card_library", "VIEW_UPGRADES").GetRawText());
		_viewMultiplayerCards.SetLabel(new LocString("card_library", "VIEW_MULTIPLAYER_CARDS").GetRawText());
		_colorlessFilter.Loc = new LocString("card_library", "POOL_COLORLESS_TIP");
		_ancientsFilter.Loc = new LocString("card_library", "POOL_ANCIENT_TIP");
		_miscPoolFilter.Loc = new LocString("card_library", "POOL_MISC_TIP");
		_attackFilter.Loc = new LocString("card_library", "TYPE_ATTACK_TIP");
		_skillFilter.Loc = new LocString("card_library", "TYPE_SKILL_TIP");
		_powerFilter.Loc = new LocString("card_library", "TYPE_POWER_TIP");
		_otherTypeFilter.Loc = new LocString("card_library", "TYPE_OTHER_TIP");
		_commonFilter.Loc = new LocString("card_library", "RARITY_COMMON_TIP");
		_uncommonFilter.Loc = new LocString("card_library", "RARITY_UNCOMMON_TIP");
		_rareFilter.Loc = new LocString("card_library", "RARITY_RARE_TIP");
		_otherFilter.Loc = new LocString("card_library", "RARITY_OTHER_TIP");
		_zeroFilter.Loc = new LocString("card_library", "COST_ZERO_TIP");
		_oneFilter.Loc = new LocString("card_library", "COST_ONE_TIP");
		_twoFilter.Loc = new LocString("card_library", "COST_TWO_TIP");
		_threePlusFilter.Loc = new LocString("card_library", "COST_THREE_TIP");
		_xFilter.Loc = new LocString("card_library", "COST_X_TIP");
		_cardPoolFilters.Add(ModelDb.Character<Ironclad>(), _ironcladFilter);
		_cardPoolFilters.Add(ModelDb.Character<Silent>(), _silentFilter);
		_cardPoolFilters.Add(ModelDb.Character<Defect>(), _defectFilter);
		_cardPoolFilters.Add(ModelDb.Character<Necrobinder>(), _necrobinderFilter);
		_cardPoolFilters.Add(ModelDb.Character<Regent>(), _regentFilter);
		UnlockState unlockState = SaveManager.Instance.GenerateUnlockStateFromProgress();
		foreach (KeyValuePair<CharacterModel, NCardPoolFilter> cardPoolFilter in _cardPoolFilters)
		{
			cardPoolFilter.Value.Visible = unlockState.Characters.Contains(cardPoolFilter.Key);
		}
		CardRarity[] values = Enum.GetValues<CardRarity>();
		for (int num = 0; num < values.Length; num++)
		{
			CardRarity keyword = values[num];
			_specialSearchbarKeywords.Add(keyword.ToString().ToLowerInvariant(), (CardModel c) => c.Rarity == keyword);
		}
		_ironcladFilter.Connect(Control.SignalName.FocusEntered, Callable.From(delegate
		{
			_lastHoveredControl = _ironcladFilter;
		}));
		_silentFilter.Connect(Control.SignalName.FocusEntered, Callable.From(delegate
		{
			_lastHoveredControl = _silentFilter;
		}));
		_defectFilter.Connect(Control.SignalName.FocusEntered, Callable.From(delegate
		{
			_lastHoveredControl = _defectFilter;
		}));
		_regentFilter.Connect(Control.SignalName.FocusEntered, Callable.From(delegate
		{
			_lastHoveredControl = _regentFilter;
		}));
		_necrobinderFilter.Connect(Control.SignalName.FocusEntered, Callable.From(delegate
		{
			_lastHoveredControl = _necrobinderFilter;
		}));
		_colorlessFilter.Connect(Control.SignalName.FocusEntered, Callable.From(delegate
		{
			_lastHoveredControl = _colorlessFilter;
		}));
		_ancientsFilter.Connect(Control.SignalName.FocusEntered, Callable.From(delegate
		{
			_lastHoveredControl = _ancientsFilter;
		}));
		_miscPoolFilter.Connect(Control.SignalName.FocusEntered, Callable.From(delegate
		{
			_lastHoveredControl = _miscPoolFilter;
		}));
		_attackFilter.Connect(Control.SignalName.FocusEntered, Callable.From(delegate
		{
			_lastHoveredControl = _attackFilter;
		}));
		_skillFilter.Connect(Control.SignalName.FocusEntered, Callable.From(delegate
		{
			_lastHoveredControl = _skillFilter;
		}));
		_powerFilter.Connect(Control.SignalName.FocusEntered, Callable.From(delegate
		{
			_lastHoveredControl = _powerFilter;
		}));
		_otherTypeFilter.Connect(Control.SignalName.FocusEntered, Callable.From(delegate
		{
			_lastHoveredControl = _otherTypeFilter;
		}));
		_commonFilter.Connect(Control.SignalName.FocusEntered, Callable.From(delegate
		{
			_lastHoveredControl = _commonFilter;
		}));
		_uncommonFilter.Connect(Control.SignalName.FocusEntered, Callable.From(delegate
		{
			_lastHoveredControl = _uncommonFilter;
		}));
		_rareFilter.Connect(Control.SignalName.FocusEntered, Callable.From(delegate
		{
			_lastHoveredControl = _rareFilter;
		}));
		_otherFilter.Connect(Control.SignalName.FocusEntered, Callable.From(delegate
		{
			_lastHoveredControl = _otherFilter;
		}));
		_zeroFilter.Connect(Control.SignalName.FocusEntered, Callable.From(delegate
		{
			_lastHoveredControl = _zeroFilter;
		}));
		_oneFilter.Connect(Control.SignalName.FocusEntered, Callable.From(delegate
		{
			_lastHoveredControl = _oneFilter;
		}));
		_twoFilter.Connect(Control.SignalName.FocusEntered, Callable.From(delegate
		{
			_lastHoveredControl = _twoFilter;
		}));
		_threePlusFilter.Connect(Control.SignalName.FocusEntered, Callable.From(delegate
		{
			_lastHoveredControl = _threePlusFilter;
		}));
		_alphabetSorter.Connect(Control.SignalName.FocusEntered, Callable.From(delegate
		{
			_lastHoveredControl = _alphabetSorter;
		}));
		_viewUpgrades.Connect(Control.SignalName.FocusEntered, Callable.From(delegate
		{
			_lastHoveredControl = _viewUpgrades;
		}));
	}

	public void Initialize(IRunState runState)
	{
		_runState = runState;
	}

	private void OnCardTypeSort(NButton button)
	{
		_sortingPriority.Remove(SortingOrders.TypeAscending);
		_sortingPriority.Remove(SortingOrders.TypeDescending);
		if (_typeSorter.IsDescending)
		{
			_sortingPriority.Insert(0, SortingOrders.TypeDescending);
		}
		else
		{
			_sortingPriority.Insert(0, SortingOrders.TypeAscending);
		}
		TaskHelper.RunSafely(DisplayCards());
	}

	private void OnRaritySort(NButton button)
	{
		_sortingPriority.Remove(SortingOrders.RarityAscending);
		_sortingPriority.Remove(SortingOrders.RarityDescending);
		if (_raritySorter.IsDescending)
		{
			_sortingPriority.Insert(0, SortingOrders.RarityAscending);
		}
		else
		{
			_sortingPriority.Insert(0, SortingOrders.RarityDescending);
		}
		TaskHelper.RunSafely(DisplayCards());
	}

	private void OnCostSort(NButton button)
	{
		_sortingPriority.Remove(SortingOrders.CostAscending);
		_sortingPriority.Remove(SortingOrders.CostDescending);
		if (_costSorter.IsDescending)
		{
			_sortingPriority.Insert(0, SortingOrders.CostDescending);
		}
		else
		{
			_sortingPriority.Insert(0, SortingOrders.CostAscending);
		}
		TaskHelper.RunSafely(DisplayCards());
	}

	private void OnAlphabetSort(NButton button)
	{
		_sortingPriority.Remove(SortingOrders.AlphabetAscending);
		_sortingPriority.Remove(SortingOrders.AlphabetDescending);
		if (_alphabetSorter.IsDescending)
		{
			_sortingPriority.Insert(0, SortingOrders.AlphabetDescending);
		}
		else
		{
			_sortingPriority.Insert(0, SortingOrders.AlphabetAscending);
		}
		TaskHelper.RunSafely(DisplayCards());
	}

	public override void OnSubmenuOpened()
	{
		_grid.RefreshVisibility();
		CharacterModel characterModel = LocalContext.GetMe(_runState)?.Character;
		_searchBar.ClearText();
		if (characterModel != null)
		{
			foreach (NCardPoolFilter key in _poolFilters.Keys)
			{
				key.IsSelected = _cardPoolFilters[characterModel] == key;
			}
		}
		else
		{
			foreach (NCardPoolFilter key2 in _poolFilters.Keys)
			{
				key2.IsSelected = key2 == _ironcladFilter;
			}
		}
		foreach (NCardTypeTickbox key3 in _cardTypeFilters.Keys)
		{
			key3.IsTicked = false;
		}
		foreach (NCardRarityTickbox key4 in _rarityFilters.Keys)
		{
			key4.IsTicked = false;
		}
		foreach (NCardCostTickbox key5 in _costFilters.Keys)
		{
			key5.IsTicked = false;
		}
		_typeSorter.IsDescending = true;
		_raritySorter.IsDescending = true;
		_costSorter.IsDescending = true;
		_alphabetSorter.IsDescending = true;
		_viewUpgrades.IsTicked = false;
		_viewStats.IsTicked = false;
		_viewMultiplayerCards.IsTicked = true;
		ToggleShowStats(_viewStats);
		ToggleShowUpgrades(_viewUpgrades);
		UpdateFilter();
	}

	public override void OnSubmenuClosed()
	{
		_grid.ClearGrid();
	}

	private async Task DisplayCardsAfterShortDelay()
	{
		if (_displayCardsShortDelayCancelToken != null)
		{
			await _displayCardsShortDelayCancelToken.CancelAsync();
		}
		if (!_grid.IsAnimatingOut)
		{
			TaskHelper.RunSafely(_grid.AnimateOut());
		}
		CancellationTokenSource cancelToken = (_displayCardsShortDelayCancelToken = new CancellationTokenSource());
		await Task.Delay(250, cancelToken.Token);
		if (!cancelToken.IsCancellationRequested)
		{
			await DisplayCards();
		}
	}

	private async Task DisplayCards()
	{
		if (_displayCardsShortDelayCancelToken != null)
		{
			await _displayCardsShortDelayCancelToken.CancelAsync();
		}
		await Task.Yield();
		_grid.FilterCards(_filter, _sortingPriority);
		_cardCountLocString.Add("Amount", _grid.VisibleCards.Count());
		_cardCountLabel.Text = "[center]" + _cardCountLocString.GetFormattedText() + "[/center]";
		_noResultsLabel.Visible = !_grid.VisibleCards.Any();
	}

	private void ToggleShowStats(NTickbox tickbox)
	{
		_grid.ShowStats = tickbox.IsTicked;
	}

	private void ToggleShowUpgrades(NTickbox tickbox)
	{
		_grid.IsShowingUpgrades = tickbox.IsTicked;
		if (!string.IsNullOrWhiteSpace(_searchBar.Text))
		{
			UpdateFilter();
		}
	}

	private void ToggleFilterMultiplayerCards(NTickbox tickbox)
	{
		UpdateFilter();
	}

	private void UpdateCardPoolFilter(NCardPoolFilter filter)
	{
		if (filter.IsSelected)
		{
			foreach (NCardPoolFilter key2 in _poolFilters.Keys)
			{
				if (key2 != filter)
				{
					key2.IsSelected = false;
				}
			}
		}
		bool flag = true;
		foreach (KeyValuePair<NCardPoolFilter, Func<CardModel, bool>> poolFilter in _poolFilters)
		{
			NCardPoolFilter key = poolFilter.Key;
			if (key.IsSelected && key != _miscPoolFilter && key != _ancientsFilter)
			{
				flag = false;
				break;
			}
		}
		foreach (NCardRarityTickbox key3 in _rarityFilters.Keys)
		{
			if (flag)
			{
				key3.Disable();
			}
			else
			{
				key3.Enable();
			}
		}
		UpdateFilter();
	}

	private void UpdateTypeFilter(NCardTypeTickbox tickbox)
	{
		UpdateFilter();
	}

	private void UpdateRarityFilter(NTickbox tickbox)
	{
		UpdateFilter();
	}

	private void UpdateCostFilter(NCardCostTickbox tickbox)
	{
		UpdateFilter();
	}

	private void SearchBarQueryChanged(string _ = "")
	{
		UpdateFilter(isTextInput: true);
	}

	private void SearchBarQuerySubmitted(string _ = "")
	{
		UpdateFilter();
	}

	private void UpdateFilter(bool isTextInput = false)
	{
		List<Func<CardModel, bool>> activeRarityFilters = new List<Func<CardModel, bool>>();
		bool flag = true;
		foreach (KeyValuePair<NCardPoolFilter, Func<CardModel, bool>> poolFilter2 in _poolFilters)
		{
			if (poolFilter2.Key.IsSelected && poolFilter2.Key != _miscPoolFilter && poolFilter2.Key != _ancientsFilter)
			{
				flag = false;
				break;
			}
		}
		Func<CardModel, bool> value;
		if (!flag)
		{
			foreach (KeyValuePair<NCardRarityTickbox, Func<CardModel, bool>> rarityFilter in _rarityFilters)
			{
				rarityFilter.Deconstruct(out var key, out value);
				NTickbox nTickbox = key;
				Func<CardModel, bool> item = value;
				if (nTickbox.IsTicked)
				{
					activeRarityFilters.Add(item);
				}
			}
		}
		if (activeRarityFilters.Count == 0)
		{
			activeRarityFilters.Add((CardModel _) => true);
		}
		List<Func<CardModel, bool>> activeCardTypeFilter = new List<Func<CardModel, bool>>();
		foreach (KeyValuePair<NCardTypeTickbox, Func<CardModel, bool>> cardTypeFilter in _cardTypeFilters)
		{
			cardTypeFilter.Deconstruct(out var key2, out value);
			NCardTypeTickbox nCardTypeTickbox = key2;
			Func<CardModel, bool> item2 = value;
			if (nCardTypeTickbox.IsTicked)
			{
				activeCardTypeFilter.Add(item2);
			}
		}
		if (activeCardTypeFilter.Count == 0)
		{
			activeCardTypeFilter.Add((CardModel _) => true);
		}
		List<Func<CardModel, bool>> poolFilter = new List<Func<CardModel, bool>>();
		foreach (KeyValuePair<NCardPoolFilter, Func<CardModel, bool>> poolFilter3 in _poolFilters)
		{
			if (poolFilter3.Key.IsSelected)
			{
				poolFilter.Add(poolFilter3.Value);
			}
		}
		List<Func<CardModel, bool>> activeCostFilter = new List<Func<CardModel, bool>>();
		foreach (KeyValuePair<NCardCostTickbox, Func<CardModel, bool>> costFilter in _costFilters)
		{
			costFilter.Deconstruct(out var key3, out value);
			NCardCostTickbox nCardCostTickbox = key3;
			Func<CardModel, bool> item3 = value;
			if (nCardCostTickbox.IsTicked)
			{
				activeCostFilter.Add(item3);
			}
		}
		if (activeCostFilter.Count == 0)
		{
			activeCostFilter.Add((CardModel _) => true);
		}
		Func<CardModel, bool> multiplayerCardFilter = (CardModel c) => true;
		if (!_viewMultiplayerCards.IsTicked)
		{
			multiplayerCardFilter = (CardModel c) => c.MultiplayerConstraint != CardMultiplayerConstraint.MultiplayerOnly;
		}
		_filter = (CardModel c) => activeCostFilter.Any((Func<CardModel, bool> filter) => filter(c)) && activeRarityFilters.Any((Func<CardModel, bool> filter) => filter(c)) && activeCardTypeFilter.Any((Func<CardModel, bool> filter) => filter(c)) && poolFilter.Any((Func<CardModel, bool> filter) => filter(c)) && TextFilter(c) && multiplayerCardFilter(c);
		Task task = ((!isTextInput) ? DisplayCards() : DisplayCardsAfterShortDelay());
		TaskHelper.RunSafely(task);
		bool TextFilter(CardModel card)
		{
			if (string.IsNullOrWhiteSpace(_searchBar.Text))
			{
				return true;
			}
			if (!SaveManager.Instance.Progress.DiscoveredCards.Contains(card.Id))
			{
				return false;
			}
			string title = card.Title;
			string text;
			if (_viewUpgrades.IsTicked && card.IsUpgradable)
			{
				CardModel cardModel = (CardModel)card.MutableClone();
				cardModel.UpgradeInternal();
				cardModel.UpdateDynamicVarPreview(CardPreviewMode.Upgrade, null, card.DynamicVars);
				text = cardModel.GetDescriptionForUpgradePreview().StripBbCode();
			}
			else
			{
				text = card.GetDescriptionForPile(PileType.None).StripBbCode();
			}
			global::_003C_003Ey__InlineArray2<string> buffer = default(global::_003C_003Ey__InlineArray2<string>);
			global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<global::_003C_003Ey__InlineArray2<string>, string>(ref buffer, 0) = title;
			global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<global::_003C_003Ey__InlineArray2<string>, string>(ref buffer, 1) = NSearchBar.RemoveHtmlTags(text);
			string text2 = string.Join(" ", global::_003CPrivateImplementationDetails_003E.InlineArrayAsReadOnlySpan<global::_003C_003Ey__InlineArray2<string>, string>(in buffer, 2));
			string text3 = NSearchBar.Normalize(text2);
			string text4 = _searchBar.Text.ToLowerInvariant();
			if (_specialSearchbarKeywords.TryGetValue(text4, out Func<CardModel, bool> value2))
			{
				if (!value2(card))
				{
					return text3.Contains(text4);
				}
				return true;
			}
			return text3.Contains(text4);
		}
	}

	private void ShowCardDetail(NCardHolder holder)
	{
		if (SaveManager.Instance.Progress.DiscoveredCards.Contains(holder.CardModel.Id))
		{
			_lastHoveredControl = holder;
			List<CardModel> list = _grid.VisibleCards.Where((CardModel c) => SaveManager.Instance.Progress.DiscoveredCards.Contains(c.Id)).ToList();
			NGame.Instance.GetInspectCardScreen().Open(list, list.IndexOf(holder.CardModel), _viewUpgrades.IsTicked);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(19);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnCardTypeSort, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "button", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnRaritySort, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "button", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnCostSort, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "button", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnAlphabetSort, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "button", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuOpened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuClosed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ToggleShowStats, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "tickbox", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ToggleShowUpgrades, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "tickbox", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ToggleFilterMultiplayerCards, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "tickbox", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateCardPoolFilter, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "filter", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateTypeFilter, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "tickbox", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateRarityFilter, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "tickbox", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateCostFilter, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "tickbox", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SearchBarQueryChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "_", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SearchBarQuerySubmitted, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "_", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateFilter, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "isTextInput", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ShowCardDetail, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "holder", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NCardLibrary>(Create());
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnCardTypeSort && args.Count == 1)
		{
			OnCardTypeSort(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnRaritySort && args.Count == 1)
		{
			OnRaritySort(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnCostSort && args.Count == 1)
		{
			OnCostSort(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnAlphabetSort && args.Count == 1)
		{
			OnAlphabetSort(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSubmenuOpened && args.Count == 0)
		{
			OnSubmenuOpened();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSubmenuClosed && args.Count == 0)
		{
			OnSubmenuClosed();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ToggleShowStats && args.Count == 1)
		{
			ToggleShowStats(VariantUtils.ConvertTo<NTickbox>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ToggleShowUpgrades && args.Count == 1)
		{
			ToggleShowUpgrades(VariantUtils.ConvertTo<NTickbox>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ToggleFilterMultiplayerCards && args.Count == 1)
		{
			ToggleFilterMultiplayerCards(VariantUtils.ConvertTo<NTickbox>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateCardPoolFilter && args.Count == 1)
		{
			UpdateCardPoolFilter(VariantUtils.ConvertTo<NCardPoolFilter>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateTypeFilter && args.Count == 1)
		{
			UpdateTypeFilter(VariantUtils.ConvertTo<NCardTypeTickbox>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateRarityFilter && args.Count == 1)
		{
			UpdateRarityFilter(VariantUtils.ConvertTo<NTickbox>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateCostFilter && args.Count == 1)
		{
			UpdateCostFilter(VariantUtils.ConvertTo<NCardCostTickbox>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SearchBarQueryChanged && args.Count == 1)
		{
			SearchBarQueryChanged(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SearchBarQuerySubmitted && args.Count == 1)
		{
			SearchBarQuerySubmitted(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateFilter && args.Count == 1)
		{
			UpdateFilter(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ShowCardDetail && args.Count == 1)
		{
			ShowCardDetail(VariantUtils.ConvertTo<NCardHolder>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NCardLibrary>(Create());
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.Create)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.OnCardTypeSort)
		{
			return true;
		}
		if (method == MethodName.OnRaritySort)
		{
			return true;
		}
		if (method == MethodName.OnCostSort)
		{
			return true;
		}
		if (method == MethodName.OnAlphabetSort)
		{
			return true;
		}
		if (method == MethodName.OnSubmenuOpened)
		{
			return true;
		}
		if (method == MethodName.OnSubmenuClosed)
		{
			return true;
		}
		if (method == MethodName.ToggleShowStats)
		{
			return true;
		}
		if (method == MethodName.ToggleShowUpgrades)
		{
			return true;
		}
		if (method == MethodName.ToggleFilterMultiplayerCards)
		{
			return true;
		}
		if (method == MethodName.UpdateCardPoolFilter)
		{
			return true;
		}
		if (method == MethodName.UpdateTypeFilter)
		{
			return true;
		}
		if (method == MethodName.UpdateRarityFilter)
		{
			return true;
		}
		if (method == MethodName.UpdateCostFilter)
		{
			return true;
		}
		if (method == MethodName.SearchBarQueryChanged)
		{
			return true;
		}
		if (method == MethodName.SearchBarQuerySubmitted)
		{
			return true;
		}
		if (method == MethodName.UpdateFilter)
		{
			return true;
		}
		if (method == MethodName.ShowCardDetail)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._grid)
		{
			_grid = VariantUtils.ConvertTo<NCardLibraryGrid>(in value);
			return true;
		}
		if (name == PropertyName._searchBar)
		{
			_searchBar = VariantUtils.ConvertTo<NSearchBar>(in value);
			return true;
		}
		if (name == PropertyName._ironcladFilter)
		{
			_ironcladFilter = VariantUtils.ConvertTo<NCardPoolFilter>(in value);
			return true;
		}
		if (name == PropertyName._silentFilter)
		{
			_silentFilter = VariantUtils.ConvertTo<NCardPoolFilter>(in value);
			return true;
		}
		if (name == PropertyName._defectFilter)
		{
			_defectFilter = VariantUtils.ConvertTo<NCardPoolFilter>(in value);
			return true;
		}
		if (name == PropertyName._regentFilter)
		{
			_regentFilter = VariantUtils.ConvertTo<NCardPoolFilter>(in value);
			return true;
		}
		if (name == PropertyName._necrobinderFilter)
		{
			_necrobinderFilter = VariantUtils.ConvertTo<NCardPoolFilter>(in value);
			return true;
		}
		if (name == PropertyName._colorlessFilter)
		{
			_colorlessFilter = VariantUtils.ConvertTo<NCardPoolFilter>(in value);
			return true;
		}
		if (name == PropertyName._ancientsFilter)
		{
			_ancientsFilter = VariantUtils.ConvertTo<NCardPoolFilter>(in value);
			return true;
		}
		if (name == PropertyName._miscPoolFilter)
		{
			_miscPoolFilter = VariantUtils.ConvertTo<NCardPoolFilter>(in value);
			return true;
		}
		if (name == PropertyName._typeSorter)
		{
			_typeSorter = VariantUtils.ConvertTo<NCardViewSortButton>(in value);
			return true;
		}
		if (name == PropertyName._attackFilter)
		{
			_attackFilter = VariantUtils.ConvertTo<NCardTypeTickbox>(in value);
			return true;
		}
		if (name == PropertyName._skillFilter)
		{
			_skillFilter = VariantUtils.ConvertTo<NCardTypeTickbox>(in value);
			return true;
		}
		if (name == PropertyName._powerFilter)
		{
			_powerFilter = VariantUtils.ConvertTo<NCardTypeTickbox>(in value);
			return true;
		}
		if (name == PropertyName._otherTypeFilter)
		{
			_otherTypeFilter = VariantUtils.ConvertTo<NCardTypeTickbox>(in value);
			return true;
		}
		if (name == PropertyName._raritySorter)
		{
			_raritySorter = VariantUtils.ConvertTo<NCardViewSortButton>(in value);
			return true;
		}
		if (name == PropertyName._commonFilter)
		{
			_commonFilter = VariantUtils.ConvertTo<NCardRarityTickbox>(in value);
			return true;
		}
		if (name == PropertyName._uncommonFilter)
		{
			_uncommonFilter = VariantUtils.ConvertTo<NCardRarityTickbox>(in value);
			return true;
		}
		if (name == PropertyName._rareFilter)
		{
			_rareFilter = VariantUtils.ConvertTo<NCardRarityTickbox>(in value);
			return true;
		}
		if (name == PropertyName._otherFilter)
		{
			_otherFilter = VariantUtils.ConvertTo<NCardRarityTickbox>(in value);
			return true;
		}
		if (name == PropertyName._costSorter)
		{
			_costSorter = VariantUtils.ConvertTo<NCardViewSortButton>(in value);
			return true;
		}
		if (name == PropertyName._zeroFilter)
		{
			_zeroFilter = VariantUtils.ConvertTo<NCardCostTickbox>(in value);
			return true;
		}
		if (name == PropertyName._oneFilter)
		{
			_oneFilter = VariantUtils.ConvertTo<NCardCostTickbox>(in value);
			return true;
		}
		if (name == PropertyName._twoFilter)
		{
			_twoFilter = VariantUtils.ConvertTo<NCardCostTickbox>(in value);
			return true;
		}
		if (name == PropertyName._threePlusFilter)
		{
			_threePlusFilter = VariantUtils.ConvertTo<NCardCostTickbox>(in value);
			return true;
		}
		if (name == PropertyName._xFilter)
		{
			_xFilter = VariantUtils.ConvertTo<NCardCostTickbox>(in value);
			return true;
		}
		if (name == PropertyName._alphabetSorter)
		{
			_alphabetSorter = VariantUtils.ConvertTo<NCardViewSortButton>(in value);
			return true;
		}
		if (name == PropertyName._viewMultiplayerCards)
		{
			_viewMultiplayerCards = VariantUtils.ConvertTo<NLibraryStatTickbox>(in value);
			return true;
		}
		if (name == PropertyName._viewStats)
		{
			_viewStats = VariantUtils.ConvertTo<NLibraryStatTickbox>(in value);
			return true;
		}
		if (name == PropertyName._viewUpgrades)
		{
			_viewUpgrades = VariantUtils.ConvertTo<NLibraryStatTickbox>(in value);
			return true;
		}
		if (name == PropertyName._cardCountLabel)
		{
			_cardCountLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._noResultsLabel)
		{
			_noResultsLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._lastHoveredControl)
		{
			_lastHoveredControl = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.InitialFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(InitialFocusedControl);
			return true;
		}
		if (name == PropertyName._grid)
		{
			value = VariantUtils.CreateFrom(in _grid);
			return true;
		}
		if (name == PropertyName._searchBar)
		{
			value = VariantUtils.CreateFrom(in _searchBar);
			return true;
		}
		if (name == PropertyName._ironcladFilter)
		{
			value = VariantUtils.CreateFrom(in _ironcladFilter);
			return true;
		}
		if (name == PropertyName._silentFilter)
		{
			value = VariantUtils.CreateFrom(in _silentFilter);
			return true;
		}
		if (name == PropertyName._defectFilter)
		{
			value = VariantUtils.CreateFrom(in _defectFilter);
			return true;
		}
		if (name == PropertyName._regentFilter)
		{
			value = VariantUtils.CreateFrom(in _regentFilter);
			return true;
		}
		if (name == PropertyName._necrobinderFilter)
		{
			value = VariantUtils.CreateFrom(in _necrobinderFilter);
			return true;
		}
		if (name == PropertyName._colorlessFilter)
		{
			value = VariantUtils.CreateFrom(in _colorlessFilter);
			return true;
		}
		if (name == PropertyName._ancientsFilter)
		{
			value = VariantUtils.CreateFrom(in _ancientsFilter);
			return true;
		}
		if (name == PropertyName._miscPoolFilter)
		{
			value = VariantUtils.CreateFrom(in _miscPoolFilter);
			return true;
		}
		if (name == PropertyName._typeSorter)
		{
			value = VariantUtils.CreateFrom(in _typeSorter);
			return true;
		}
		if (name == PropertyName._attackFilter)
		{
			value = VariantUtils.CreateFrom(in _attackFilter);
			return true;
		}
		if (name == PropertyName._skillFilter)
		{
			value = VariantUtils.CreateFrom(in _skillFilter);
			return true;
		}
		if (name == PropertyName._powerFilter)
		{
			value = VariantUtils.CreateFrom(in _powerFilter);
			return true;
		}
		if (name == PropertyName._otherTypeFilter)
		{
			value = VariantUtils.CreateFrom(in _otherTypeFilter);
			return true;
		}
		if (name == PropertyName._raritySorter)
		{
			value = VariantUtils.CreateFrom(in _raritySorter);
			return true;
		}
		if (name == PropertyName._commonFilter)
		{
			value = VariantUtils.CreateFrom(in _commonFilter);
			return true;
		}
		if (name == PropertyName._uncommonFilter)
		{
			value = VariantUtils.CreateFrom(in _uncommonFilter);
			return true;
		}
		if (name == PropertyName._rareFilter)
		{
			value = VariantUtils.CreateFrom(in _rareFilter);
			return true;
		}
		if (name == PropertyName._otherFilter)
		{
			value = VariantUtils.CreateFrom(in _otherFilter);
			return true;
		}
		if (name == PropertyName._costSorter)
		{
			value = VariantUtils.CreateFrom(in _costSorter);
			return true;
		}
		if (name == PropertyName._zeroFilter)
		{
			value = VariantUtils.CreateFrom(in _zeroFilter);
			return true;
		}
		if (name == PropertyName._oneFilter)
		{
			value = VariantUtils.CreateFrom(in _oneFilter);
			return true;
		}
		if (name == PropertyName._twoFilter)
		{
			value = VariantUtils.CreateFrom(in _twoFilter);
			return true;
		}
		if (name == PropertyName._threePlusFilter)
		{
			value = VariantUtils.CreateFrom(in _threePlusFilter);
			return true;
		}
		if (name == PropertyName._xFilter)
		{
			value = VariantUtils.CreateFrom(in _xFilter);
			return true;
		}
		if (name == PropertyName._alphabetSorter)
		{
			value = VariantUtils.CreateFrom(in _alphabetSorter);
			return true;
		}
		if (name == PropertyName._viewMultiplayerCards)
		{
			value = VariantUtils.CreateFrom(in _viewMultiplayerCards);
			return true;
		}
		if (name == PropertyName._viewStats)
		{
			value = VariantUtils.CreateFrom(in _viewStats);
			return true;
		}
		if (name == PropertyName._viewUpgrades)
		{
			value = VariantUtils.CreateFrom(in _viewUpgrades);
			return true;
		}
		if (name == PropertyName._cardCountLabel)
		{
			value = VariantUtils.CreateFrom(in _cardCountLabel);
			return true;
		}
		if (name == PropertyName._noResultsLabel)
		{
			value = VariantUtils.CreateFrom(in _noResultsLabel);
			return true;
		}
		if (name == PropertyName._lastHoveredControl)
		{
			value = VariantUtils.CreateFrom(in _lastHoveredControl);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._grid, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._searchBar, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._ironcladFilter, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._silentFilter, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._defectFilter, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._regentFilter, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._necrobinderFilter, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._colorlessFilter, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._ancientsFilter, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._miscPoolFilter, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._typeSorter, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._attackFilter, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._skillFilter, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._powerFilter, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._otherTypeFilter, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._raritySorter, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._commonFilter, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._uncommonFilter, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._rareFilter, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._otherFilter, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._costSorter, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._zeroFilter, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._oneFilter, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._twoFilter, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._threePlusFilter, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._xFilter, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._alphabetSorter, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._viewMultiplayerCards, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._viewStats, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._viewUpgrades, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardCountLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._noResultsLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._lastHoveredControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.InitialFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._grid, Variant.From(in _grid));
		info.AddProperty(PropertyName._searchBar, Variant.From(in _searchBar));
		info.AddProperty(PropertyName._ironcladFilter, Variant.From(in _ironcladFilter));
		info.AddProperty(PropertyName._silentFilter, Variant.From(in _silentFilter));
		info.AddProperty(PropertyName._defectFilter, Variant.From(in _defectFilter));
		info.AddProperty(PropertyName._regentFilter, Variant.From(in _regentFilter));
		info.AddProperty(PropertyName._necrobinderFilter, Variant.From(in _necrobinderFilter));
		info.AddProperty(PropertyName._colorlessFilter, Variant.From(in _colorlessFilter));
		info.AddProperty(PropertyName._ancientsFilter, Variant.From(in _ancientsFilter));
		info.AddProperty(PropertyName._miscPoolFilter, Variant.From(in _miscPoolFilter));
		info.AddProperty(PropertyName._typeSorter, Variant.From(in _typeSorter));
		info.AddProperty(PropertyName._attackFilter, Variant.From(in _attackFilter));
		info.AddProperty(PropertyName._skillFilter, Variant.From(in _skillFilter));
		info.AddProperty(PropertyName._powerFilter, Variant.From(in _powerFilter));
		info.AddProperty(PropertyName._otherTypeFilter, Variant.From(in _otherTypeFilter));
		info.AddProperty(PropertyName._raritySorter, Variant.From(in _raritySorter));
		info.AddProperty(PropertyName._commonFilter, Variant.From(in _commonFilter));
		info.AddProperty(PropertyName._uncommonFilter, Variant.From(in _uncommonFilter));
		info.AddProperty(PropertyName._rareFilter, Variant.From(in _rareFilter));
		info.AddProperty(PropertyName._otherFilter, Variant.From(in _otherFilter));
		info.AddProperty(PropertyName._costSorter, Variant.From(in _costSorter));
		info.AddProperty(PropertyName._zeroFilter, Variant.From(in _zeroFilter));
		info.AddProperty(PropertyName._oneFilter, Variant.From(in _oneFilter));
		info.AddProperty(PropertyName._twoFilter, Variant.From(in _twoFilter));
		info.AddProperty(PropertyName._threePlusFilter, Variant.From(in _threePlusFilter));
		info.AddProperty(PropertyName._xFilter, Variant.From(in _xFilter));
		info.AddProperty(PropertyName._alphabetSorter, Variant.From(in _alphabetSorter));
		info.AddProperty(PropertyName._viewMultiplayerCards, Variant.From(in _viewMultiplayerCards));
		info.AddProperty(PropertyName._viewStats, Variant.From(in _viewStats));
		info.AddProperty(PropertyName._viewUpgrades, Variant.From(in _viewUpgrades));
		info.AddProperty(PropertyName._cardCountLabel, Variant.From(in _cardCountLabel));
		info.AddProperty(PropertyName._noResultsLabel, Variant.From(in _noResultsLabel));
		info.AddProperty(PropertyName._lastHoveredControl, Variant.From(in _lastHoveredControl));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._grid, out var value))
		{
			_grid = value.As<NCardLibraryGrid>();
		}
		if (info.TryGetProperty(PropertyName._searchBar, out var value2))
		{
			_searchBar = value2.As<NSearchBar>();
		}
		if (info.TryGetProperty(PropertyName._ironcladFilter, out var value3))
		{
			_ironcladFilter = value3.As<NCardPoolFilter>();
		}
		if (info.TryGetProperty(PropertyName._silentFilter, out var value4))
		{
			_silentFilter = value4.As<NCardPoolFilter>();
		}
		if (info.TryGetProperty(PropertyName._defectFilter, out var value5))
		{
			_defectFilter = value5.As<NCardPoolFilter>();
		}
		if (info.TryGetProperty(PropertyName._regentFilter, out var value6))
		{
			_regentFilter = value6.As<NCardPoolFilter>();
		}
		if (info.TryGetProperty(PropertyName._necrobinderFilter, out var value7))
		{
			_necrobinderFilter = value7.As<NCardPoolFilter>();
		}
		if (info.TryGetProperty(PropertyName._colorlessFilter, out var value8))
		{
			_colorlessFilter = value8.As<NCardPoolFilter>();
		}
		if (info.TryGetProperty(PropertyName._ancientsFilter, out var value9))
		{
			_ancientsFilter = value9.As<NCardPoolFilter>();
		}
		if (info.TryGetProperty(PropertyName._miscPoolFilter, out var value10))
		{
			_miscPoolFilter = value10.As<NCardPoolFilter>();
		}
		if (info.TryGetProperty(PropertyName._typeSorter, out var value11))
		{
			_typeSorter = value11.As<NCardViewSortButton>();
		}
		if (info.TryGetProperty(PropertyName._attackFilter, out var value12))
		{
			_attackFilter = value12.As<NCardTypeTickbox>();
		}
		if (info.TryGetProperty(PropertyName._skillFilter, out var value13))
		{
			_skillFilter = value13.As<NCardTypeTickbox>();
		}
		if (info.TryGetProperty(PropertyName._powerFilter, out var value14))
		{
			_powerFilter = value14.As<NCardTypeTickbox>();
		}
		if (info.TryGetProperty(PropertyName._otherTypeFilter, out var value15))
		{
			_otherTypeFilter = value15.As<NCardTypeTickbox>();
		}
		if (info.TryGetProperty(PropertyName._raritySorter, out var value16))
		{
			_raritySorter = value16.As<NCardViewSortButton>();
		}
		if (info.TryGetProperty(PropertyName._commonFilter, out var value17))
		{
			_commonFilter = value17.As<NCardRarityTickbox>();
		}
		if (info.TryGetProperty(PropertyName._uncommonFilter, out var value18))
		{
			_uncommonFilter = value18.As<NCardRarityTickbox>();
		}
		if (info.TryGetProperty(PropertyName._rareFilter, out var value19))
		{
			_rareFilter = value19.As<NCardRarityTickbox>();
		}
		if (info.TryGetProperty(PropertyName._otherFilter, out var value20))
		{
			_otherFilter = value20.As<NCardRarityTickbox>();
		}
		if (info.TryGetProperty(PropertyName._costSorter, out var value21))
		{
			_costSorter = value21.As<NCardViewSortButton>();
		}
		if (info.TryGetProperty(PropertyName._zeroFilter, out var value22))
		{
			_zeroFilter = value22.As<NCardCostTickbox>();
		}
		if (info.TryGetProperty(PropertyName._oneFilter, out var value23))
		{
			_oneFilter = value23.As<NCardCostTickbox>();
		}
		if (info.TryGetProperty(PropertyName._twoFilter, out var value24))
		{
			_twoFilter = value24.As<NCardCostTickbox>();
		}
		if (info.TryGetProperty(PropertyName._threePlusFilter, out var value25))
		{
			_threePlusFilter = value25.As<NCardCostTickbox>();
		}
		if (info.TryGetProperty(PropertyName._xFilter, out var value26))
		{
			_xFilter = value26.As<NCardCostTickbox>();
		}
		if (info.TryGetProperty(PropertyName._alphabetSorter, out var value27))
		{
			_alphabetSorter = value27.As<NCardViewSortButton>();
		}
		if (info.TryGetProperty(PropertyName._viewMultiplayerCards, out var value28))
		{
			_viewMultiplayerCards = value28.As<NLibraryStatTickbox>();
		}
		if (info.TryGetProperty(PropertyName._viewStats, out var value29))
		{
			_viewStats = value29.As<NLibraryStatTickbox>();
		}
		if (info.TryGetProperty(PropertyName._viewUpgrades, out var value30))
		{
			_viewUpgrades = value30.As<NLibraryStatTickbox>();
		}
		if (info.TryGetProperty(PropertyName._cardCountLabel, out var value31))
		{
			_cardCountLabel = value31.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._noResultsLabel, out var value32))
		{
			_noResultsLabel = value32.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._lastHoveredControl, out var value33))
		{
			_lastHoveredControl = value33.As<Control>();
		}
	}

	public NCardLibrary()
	{
		int num = 4;
		List<SortingOrders> list = new List<SortingOrders>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<SortingOrders> span = CollectionsMarshal.AsSpan(list);
		int num2 = 0;
		span[num2] = SortingOrders.RarityAscending;
		num2++;
		span[num2] = SortingOrders.TypeAscending;
		num2++;
		span[num2] = SortingOrders.CostAscending;
		span[num2 + 1] = SortingOrders.AlphabetAscending;
		_sortingPriority = list;
		_filter = (CardModel _) => true;
		base._002Ector();
	}
}
