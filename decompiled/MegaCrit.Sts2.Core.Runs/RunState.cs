using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Singleton;
using MegaCrit.Sts2.Core.Odds;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Runs;

public class RunState : IRunState, ICardScope, IPlayerCollection
{
	private readonly List<Player> _players = new List<Player>();

	private int _currentActIndex;

	private readonly List<MapCoord> _visitedMapCoords = new List<MapCoord>();

	private readonly List<List<MapPointHistoryEntry>> _mapPointHistory = new List<List<MapPointHistoryEntry>>();

	/// <summary>
	/// A stack of all the rooms the player is currently in.
	///
	/// There will usually be exactly one room in here, but here are the reasons it may be a different number:
	/// * There may be 2+ rooms in here when a player makes a choice that spawns a new room without traveling to a new
	///   map point. For example, in the Dense Vegetation event, when the player chooses the Rest option, a new
	///   <see cref="T:MegaCrit.Sts2.Core.Rooms.CombatRoom" /> is pushed to this stack. Then, when that combat is over, we pop that room off the
	///   stack, allowing us to return to the event.
	/// * There may be 0 rooms in here during brief windows, like in the middle of traveling to a new map point.
	///
	/// Whenever we travel to a new map point, this stack is cleared and the first room for that map point is pushed
	/// onto it.
	/// </summary>
	private readonly List<AbstractRoom> _currentRooms = new List<AbstractRoom>();

	private readonly HashSet<ModelId> _visitedEventIds = new HashSet<ModelId>();

	/// <summary>
	/// All cards that have been created within this state.
	/// This allows us to keep track of "floating" cards that have not been added to a deck (like card rewards or fake
	/// cards in upgrade previews).
	/// </summary>
	private readonly List<CardModel> _allCards = new List<CardModel>();

	/// <summary>
	/// See <see cref="P:MegaCrit.Sts2.Core.Runs.IPlayerCollection.Players" />
	/// </summary>
	public IReadOnlyList<Player> Players => _players;

	/// <summary>
	/// See <see cref="P:MegaCrit.Sts2.Core.Runs.IRunState.Acts" />
	/// </summary>
	public IReadOnlyList<ActModel> Acts { get; private set; }

	/// <summary>
	/// See <see cref="P:MegaCrit.Sts2.Core.Runs.IRunState.CurrentActIndex" />
	/// </summary>
	public int CurrentActIndex
	{
		get
		{
			return _currentActIndex;
		}
		set
		{
			if (_currentActIndex != value)
			{
				_visitedMapCoords.Clear();
				ActFloor = 0;
				NextRoomId = 0;
				_currentActIndex = value;
			}
		}
	}

	/// <summary>
	/// The index of the next ID which would be returned from <see cref="M:MegaCrit.Sts2.Core.Runs.RunState.GetAndIncrementNextRoomId" />.
	/// Reset when we enter a new map location.
	/// </summary>
	public int NextRoomId { get; private set; }

	/// <summary>
	/// See <see cref="P:MegaCrit.Sts2.Core.Runs.IRunState.Act" />
	/// </summary>
	public ActModel Act => Acts[CurrentActIndex];

	/// <summary>
	/// See <see cref="P:MegaCrit.Sts2.Core.Runs.IRunState.Map" />
	/// </summary>
	public ActMap Map { get; set; } = NullActMap.Instance;

	/// <summary>
	/// The MapCoords that have been visited within the current act.
	/// </summary>
	public IReadOnlyList<MapCoord> VisitedMapCoords => _visitedMapCoords;

	/// <summary>
	/// See <see cref="P:MegaCrit.Sts2.Core.Runs.IRunState.CurrentMapCoord" />
	/// </summary>
	public MapCoord? CurrentMapCoord
	{
		get
		{
			if (_visitedMapCoords.Count != 0)
			{
				return _visitedMapCoords.Last();
			}
			return null;
		}
	}

	/// <summary>
	/// See <see cref="P:MegaCrit.Sts2.Core.Runs.IRunState.CurrentMapPoint" />
	/// </summary>
	public MapPoint? CurrentMapPoint
	{
		get
		{
			if (!CurrentMapCoord.HasValue)
			{
				return null;
			}
			return Map.GetPoint(CurrentMapCoord.Value);
		}
	}

	/// <summary>
	/// See <see cref="P:MegaCrit.Sts2.Core.Runs.IRunState.RunLocation" />
	/// </summary>
	public RunLocation RunLocation => new RunLocation(MapLocation, CurrentRoom?.Id);

	/// <summary>
	/// See <see cref="P:MegaCrit.Sts2.Core.Runs.IRunState.MapLocation" />
	/// </summary>
	public MapLocation MapLocation => new MapLocation(CurrentMapCoord, CurrentActIndex);

	/// <summary>
	/// See <see cref="P:MegaCrit.Sts2.Core.Runs.IRunState.ActFloor" />
	/// </summary>
	public int ActFloor { get; set; }

	/// <summary>
	/// See <see cref="P:MegaCrit.Sts2.Core.Runs.IRunState.TotalFloor" />
	/// </summary>
	///  NOTE: If in game we end up with an ability to skip rows (ie sts1 winged boots) we will have to rethink this approach.
	public int TotalFloor => MapPointHistory.Sum((IReadOnlyList<MapPointHistoryEntry> c) => c.Count);

	/// <summary>
	/// See <see cref="P:MegaCrit.Sts2.Core.Runs.IRunState.MapPointHistory" />
	/// </summary>
	public IReadOnlyList<IReadOnlyList<MapPointHistoryEntry>> MapPointHistory => _mapPointHistory;

	/// <summary>
	/// See <see cref="P:MegaCrit.Sts2.Core.Runs.IRunState.CurrentMapPointHistoryEntry" />
	/// </summary>
	public MapPointHistoryEntry? CurrentMapPointHistoryEntry => MapPointHistory.LastOrDefault()?.LastOrDefault();

	/// <summary>
	/// See <see cref="P:MegaCrit.Sts2.Core.Runs.IRunState.CurrentRoomCount" />
	/// </summary>
	public int CurrentRoomCount => _currentRooms.Count;

	public AbstractRoom? CurrentRoom => _currentRooms.LastOrDefault();

	/// <summary>
	/// The room at the base of the rooms stack.
	/// Usually the same as CurrentRoom because there's usually only one room in the rooms stack. However, when there
	/// are multiple rooms in the stack (like at an event where one of the options starts a combat), this will be the
	/// first room that was entered in the current map point.
	/// Usually safe to treat as non-null, but may be null during brief windows, like while we're in the middle of
	/// traveling to a new map point.
	/// </summary>
	public AbstractRoom? BaseRoom => _currentRooms.FirstOrDefault();

	/// <summary>
	/// See <see cref="P:MegaCrit.Sts2.Core.Runs.IRunState.IsGameOver" />
	/// </summary>
	public bool IsGameOver
	{
		get
		{
			if (Players.Count > 0)
			{
				return Players.All((Player p) => p.Creature.IsDead);
			}
			return false;
		}
	}

	/// <summary>
	/// See <see cref="P:MegaCrit.Sts2.Core.Runs.IRunState.GameMode" />
	/// </summary>
	public GameMode GameMode { get; init; }

	/// <summary>
	/// See <see cref="P:MegaCrit.Sts2.Core.Runs.IRunState.AscensionLevel" />
	/// </summary>
	public int AscensionLevel { get; init; }

	/// <summary>
	/// See <see cref="P:MegaCrit.Sts2.Core.Runs.IRunState.Rng" />
	/// </summary>
	public RunRngSet Rng { get; init; }

	/// <summary>
	/// See <see cref="P:MegaCrit.Sts2.Core.Runs.IRunState.Odds" />
	/// </summary>
	public RunOddsSet Odds { get; init; }

	/// <summary>
	/// See <see cref="P:MegaCrit.Sts2.Core.Runs.IRunState.SharedRelicGrabBag" />
	/// </summary>
	public RelicGrabBag SharedRelicGrabBag { get; init; }

	/// <summary>
	/// The unlock state for the run.
	/// In multiplayer, this encompasses the unlocks for _all_ players in the run. Only use this for things that are
	/// shared among players, like the shared treasure relics, or ancients. For checking player-specific unlocks, e.g.
	/// for player-specific rewards, use the unlock state on the individual players.
	/// In singleplayer, this is equivalent to the player's unlock state.
	/// </summary>
	public UnlockState UnlockState { get; init; }

	/// <summary>
	/// The IDs of all the events that have been visited during this run.
	/// This allows us to avoid visiting the same shared event twice across multiple acts.
	/// </summary>
	public IReadOnlySet<ModelId> VisitedEventIds => _visitedEventIds;

	/// <summary>
	/// List of custom modifiers applied to this combat, for daily or custom runs.
	/// </summary>
	public IReadOnlyList<ModifierModel> Modifiers { get; private set; }

	/// <summary>
	/// List of badge models.
	/// </summary>
	public IReadOnlyList<BadgeModel> BadgeModels { get; private set; }

	/// <summary>
	/// See <see cref="P:MegaCrit.Sts2.Core.Runs.IRunState.ExtraFields" />.
	/// </summary>
	public ExtraRunFields ExtraFields { get; private set; } = new ExtraRunFields();

	/// <summary>
	/// The model used to scale various things (block, power application) in multiplayer.
	/// </summary>
	public MultiplayerScalingModel MultiplayerScalingModel { get; private set; }

	/// <summary>
	/// Create a RunState for a brand new run.
	/// This will connect each player up with the RunState so their starting inventory works properly.
	/// </summary>
	/// <param name="players">The players that should be in the run.</param>
	/// <param name="acts">The mutable acts that should be in the run.</param>
	/// <param name="modifiers">The modifiers that are applied to the run.</param>
	/// <param name="gameMode">The game mode that we're playing in.</param>
	/// <param name="ascensionLevel">The ascension level that the run should be played at.</param>
	/// <param name="seed">The seed that the run's RNG should use.</param>
	public static RunState CreateForNewRun(IReadOnlyList<Player> players, IReadOnlyList<ActModel> acts, IReadOnlyList<ModifierModel> modifiers, GameMode gameMode, int ascensionLevel, string seed)
	{
		RunRngSet runRngSet = new RunRngSet(seed);
		RunOddsSet odds = new RunOddsSet(runRngSet.UnknownMapPoint);
		RunState result = CreateShared(players, acts, modifiers, gameMode, 0, runRngSet, odds, new RelicGrabBag(refreshAllowed: true), ascensionLevel);
		foreach (Player player in players)
		{
			player.InitializeSeed(seed);
			foreach (CardModel card in player.Deck.Cards)
			{
				card.AfterCreated();
			}
		}
		return result;
	}

	/// <summary>
	/// Load a serialized RunState.
	/// </summary>
	public static RunState FromSerializable(SerializableRun save)
	{
		List<SerializablePlayer> players = save.Players;
		List<Player> players2 = players.Select(Player.FromSerializable).ToList();
		RunRngSet runRngSet = RunRngSet.FromSave(save.SerializableRng);
		RunState runState = CreateShared(players2, save.Acts.Select(ActModel.FromSave).ToList(), save.Modifiers.Select(ModifierModel.FromSerializable).ToList(), save.GameMode, save.CurrentActIndex, runRngSet, RunOddsSet.FromSerializable(save.SerializableOdds, runRngSet.UnknownMapPoint), RelicGrabBag.FromSerializable(save.SerializableSharedRelicGrabBag), save.Ascension);
		runState._visitedMapCoords.AddRange(save.VisitedMapCoords);
		runState._visitedEventIds.UnionWith(save.EventsSeen);
		runState._mapPointHistory.AddRange(new global::_003C_003Ez__ReadOnlyArray<List<MapPointHistoryEntry>>(save.MapPointHistory.ToArray()));
		runState.ExtraFields = ExtraRunFields.FromSerializable(save.ExtraFields);
		return runState;
	}

	public static RunState CreateForTest(IReadOnlyList<Player>? players = null, IReadOnlyList<ActModel>? acts = null, IReadOnlyList<ModifierModel>? modifiers = null, GameMode gameMode = GameMode.Standard, int ascensionLevel = 0, string? seed = null)
	{
		if (seed == null)
		{
			seed = SeedHelper.GetRandomSeed();
		}
		RunRngSet runRngSet = new RunRngSet(seed);
		RunState runState = CreateShared(players ?? new global::_003C_003Ez__ReadOnlySingleElementList<Player>(Player.CreateForNewRun<Deprived>(MegaCrit.Sts2.Core.Unlocks.UnlockState.all, 1uL)), (acts ?? ActModel.GetDefaultList()).Select((ActModel a) => a.ToMutable()).ToList(), modifiers ?? Array.Empty<ModifierModel>(), gameMode, 0, runRngSet, new RunOddsSet(runRngSet.UnknownMapPoint), new RelicGrabBag(refreshAllowed: true), ascensionLevel);
		foreach (Player player in runState.Players)
		{
			player.InitializeSeed(seed);
		}
		return runState;
	}

	private static RunState CreateShared(IReadOnlyList<Player> players, IReadOnlyList<ActModel> acts, IReadOnlyList<ModifierModel> modifiers, GameMode gameMode, int currentActIndex, RunRngSet rng, RunOddsSet odds, RelicGrabBag sharedRelicGrabBag, int ascensionLevel)
	{
		RunState runState = new RunState(players, acts, modifiers, gameMode, currentActIndex, rng, odds, sharedRelicGrabBag, ascensionLevel);
		foreach (Player player in players)
		{
			player.RunState = runState;
			foreach (CardModel card in player.Deck.Cards)
			{
				runState.AddCard(card, player);
			}
		}
		runState.MultiplayerScalingModel = (MultiplayerScalingModel)ModelDb.Singleton<MultiplayerScalingModel>().MutableClone();
		runState.MultiplayerScalingModel.Initialize(runState);
		runState.BadgeModels = ModelDb.BadgeModels.Select((BadgeModel m) => (BadgeModel)m.MutableClone()).ToList();
		return runState;
	}

	private RunState(IReadOnlyList<Player> players, IReadOnlyList<ActModel> acts, IReadOnlyList<ModifierModel> modifiers, GameMode gameMode, int currentActIndex, RunRngSet rng, RunOddsSet odds, RelicGrabBag sharedRelicGrabBag, int ascensionLevel)
	{
		foreach (ActModel act in acts)
		{
			act.AssertMutable();
		}
		_players.AddRange(players);
		Acts = acts;
		Modifiers = modifiers;
		GameMode = gameMode;
		CurrentActIndex = currentActIndex;
		Rng = rng;
		Odds = odds;
		SharedRelicGrabBag = sharedRelicGrabBag;
		UnlockState = new UnlockState(players.Select((Player p) => p.UnlockState));
		AscensionLevel = ascensionLevel;
	}

	public int GetPlayerSlotIndex(Player player)
	{
		return Players.IndexOf(player);
	}

	public int GetPlayerSlotIndex(ulong netId)
	{
		return Players.FirstIndex((Player p) => p.NetId == netId);
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Runs.IPlayerCollection.GetPlayer(System.UInt64)" />.
	/// </summary>
	public Player? GetPlayer(ulong netId)
	{
		return Players.FirstOrDefault((Player p) => p.NetId == netId);
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Runs.ICardScope.CreateCard``1(MegaCrit.Sts2.Core.Entities.Players.Player)" />.
	/// </summary>
	public T CreateCard<T>(Player owner) where T : CardModel
	{
		return (T)CreateCard(ModelDb.Card<T>(), owner);
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Runs.ICardScope.CreateCard(MegaCrit.Sts2.Core.Models.CardModel,MegaCrit.Sts2.Core.Entities.Players.Player)" />.
	/// </summary>
	public CardModel CreateCard(CardModel canonicalCard, Player owner)
	{
		CardModel cardModel = canonicalCard.ToMutable();
		AddCard(cardModel, owner);
		cardModel.AfterCreated();
		return cardModel;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Runs.ICardScope.CloneCard(MegaCrit.Sts2.Core.Models.CardModel)" />.
	/// </summary>
	public CardModel CloneCard(CardModel mutableCard)
	{
		CardModel cardModel = (CardModel)mutableCard.ClonePreservingMutability();
		AddCard(cardModel);
		return cardModel;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Runs.ICardScope.AddCard(MegaCrit.Sts2.Core.Models.CardModel,MegaCrit.Sts2.Core.Entities.Players.Player)" />.
	/// </summary>
	public void AddCard(CardModel card, Player owner)
	{
		if (!card.HasBeenRemovedFromState)
		{
			card.Owner = owner;
		}
		AddCard(card);
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Runs.ICardScope.RemoveCard(MegaCrit.Sts2.Core.Models.CardModel)" />.
	/// </summary>
	public void RemoveCard(CardModel card)
	{
		_allCards.Remove(card);
		card.Owner = null;
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Runs.IRunState.ContainsCard(MegaCrit.Sts2.Core.Models.CardModel)" />.
	/// </summary>
	public bool ContainsCard(CardModel card)
	{
		return _allCards.Contains(card);
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Runs.IRunState.LoadCard(MegaCrit.Sts2.Core.Saves.Runs.SerializableCard,MegaCrit.Sts2.Core.Entities.Players.Player)" />
	/// </summary>
	public CardModel LoadCard(SerializableCard serializableCard, Player owner)
	{
		CardModel cardModel = CardModel.FromSerializable(serializableCard);
		AddCard(cardModel, owner);
		return cardModel;
	}

	private void AddCard(CardModel card)
	{
		card.AssertMutable();
		if (card.HasBeenRemovedFromState)
		{
			if (!ContainsCard(card))
			{
				throw new InvalidOperationException($"Tried to add card {card} to RunState that has HasBeenRemovedFromState set as true, but it does not belong to this state!");
			}
			card.HasBeenRemovedFromState = false;
		}
		else
		{
			_allCards.Add(card);
		}
	}

	/// <summary>
	/// Add the specified coord to the list of visited map coords.
	/// </summary>
	public bool AddVisitedMapCoord(MapCoord coord)
	{
		if (_visitedMapCoords.Contains(coord))
		{
			return false;
		}
		_visitedMapCoords.Add(coord);
		NextRoomId = 0;
		return true;
	}

	/// <summary>
	/// Pop the current room off the stack of rooms that we're in.
	/// </summary>
	/// <returns>The removed room.</returns>
	/// <exception cref="T:System.InvalidOperationException">If we aren't in any rooms.</exception>
	public AbstractRoom PopCurrentRoom()
	{
		if (_currentRooms.Count == 0)
		{
			throw new InvalidOperationException("Not in any rooms.");
		}
		AbstractRoom result = _currentRooms.Last();
		_currentRooms.RemoveAt(_currentRooms.Count - 1);
		return result;
	}

	/// <summary>
	/// Push the specified room onto the stack of rooms that we're in.
	/// </summary>
	/// <exception cref="T:System.InvalidOperationException">If we're already in the specified room.</exception>
	public void PushRoom(AbstractRoom room)
	{
		if (_currentRooms.Contains(room))
		{
			throw new InvalidOperationException("Already in this room.");
		}
		_currentRooms.Add(room);
	}

	/// <summary>
	/// Add the specified event to the list of visited events.
	/// </summary>
	public void AddVisitedEvent(EventModel eventModel)
	{
		_visitedEventIds.Add(eventModel.Id);
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Runs.IRunState.AppendToMapPointHistory(MegaCrit.Sts2.Core.Map.MapPointType,MegaCrit.Sts2.Core.Rooms.RoomType,MegaCrit.Sts2.Core.Models.ModelId)" />.
	/// </summary>
	public void AppendToMapPointHistory(MapPointType mapPointType, RoomType initialRoomType, ModelId? roomModelId)
	{
		if (_mapPointHistory.Count <= CurrentActIndex)
		{
			int num = CurrentActIndex + 1 - _mapPointHistory.Count;
			for (int i = 0; i < num; i++)
			{
				_mapPointHistory.Add(new List<MapPointHistoryEntry>());
			}
		}
		MapPointHistoryEntry mapPointHistoryEntry = new MapPointHistoryEntry(mapPointType, this);
		mapPointHistoryEntry.Rooms.Add(new MapPointRoomHistoryEntry
		{
			RoomType = initialRoomType,
			ModelId = roomModelId
		});
		_mapPointHistory[CurrentActIndex].Add(mapPointHistoryEntry);
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Runs.IRunState.GetHistoryEntryFor(MegaCrit.Sts2.Core.Runs.MapLocation)" />.
	/// </summary>
	public MapPointHistoryEntry? GetHistoryEntryFor(MapLocation location)
	{
		if (location.actIndex >= _mapPointHistory.Count || !location.coord.HasValue || location.coord?.row >= _mapPointHistory[location.actIndex].Count)
		{
			return null;
		}
		return _mapPointHistory[location.actIndex][location.coord.Value.row];
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Runs.IRunState.IterateHookListeners(MegaCrit.Sts2.Core.Combat.ICombatState)" />.
	/// </summary>
	public IEnumerable<AbstractModel> IterateHookListeners(ICombatState? childCombatState)
	{
		List<AbstractModel> list = new List<AbstractModel>(Players.Count * 50);
		foreach (Player player in Players)
		{
			if (!player.IsActiveForHooks)
			{
				continue;
			}
			foreach (CardModel card in player.Deck.Cards)
			{
				list.Add(card);
				if (card.Enchantment != null)
				{
					list.Add(card.Enchantment);
				}
			}
		}
		if (childCombatState == null)
		{
			foreach (Player player2 in Players)
			{
				if (player2.IsActiveForHooks)
				{
					list.AddRange(player2.Relics.Where((RelicModel r) => !r.IsMelted));
					list.AddRange(player2.Potions);
				}
			}
			list.AddRange(Modifiers);
			list.AddRange(BadgeModels);
			list.Add(MultiplayerScalingModel);
		}
		foreach (AbstractModel item in list)
		{
			if (Contains(item))
			{
				yield return item;
			}
		}
		foreach (AbstractModel item2 in ModHelper.IterateAllRunStateSubscribers(this))
		{
			yield return item2;
		}
		if (childCombatState == null)
		{
			yield break;
		}
		foreach (AbstractModel item3 in childCombatState.IterateHookListeners())
		{
			yield return item3;
		}
	}

	/// <summary>
	/// Adds a player to the players in the run.
	/// This should only ever be used in debug scenarios - typically players are passed to the run in the constructor.
	/// </summary>
	/// <param name="player">The player to add to the run.</param>
	/// <param name="index">The index at which to add them to the list. -1 to append.</param>
	public void AddPlayerDebug(Player player, int index)
	{
		if (index >= 0)
		{
			_players.Insert(index, player);
		}
		else
		{
			_players.Add(player);
		}
		player.InitializeSeed(Rng.StringSeed);
		foreach (CardModel card in player.Deck.Cards)
		{
			card.AfterCreated();
		}
		player.RunState = this;
		foreach (CardModel card2 in player.Deck.Cards)
		{
			AddCard(card2, player);
		}
		CurrentMapPointHistoryEntry.PlayerStats.Add(new PlayerMapPointHistoryEntry
		{
			PlayerId = player.NetId
		});
		if (RunManager.Instance.IsInProgress)
		{
			player.PopulateRelicGrabBagIfNecessary(Rng.UpFront);
			RunManager.Instance.ApplyAscensionEffects(player);
		}
	}

	public void SetActDebug(ActModel act)
	{
		act.AssertMutable();
		List<ActModel> list = Acts.ToList();
		list[CurrentActIndex] = act;
		Acts = list;
	}

	/// <summary>
	/// Removes any visited map coords that don't exist in the given map.
	/// Defends against old multiplayer saves (pre-v0.99.1) that had their SavedMap stripped by a
	/// canonicalization bug, causing map regeneration with different topology.
	/// The root cause is fixed but damaged saves persist.
	/// </summary>
	public void RemoveStaleVisitedMapCoords(ActMap map)
	{
		int num = _visitedMapCoords.RemoveAll((MapCoord coord) => !map.HasPoint(coord));
		if (num > 0)
		{
			Log.Error($"Removed {num} stale visited map coord(s) that don't exist in the current map");
		}
	}

	/// <summary>
	/// Clears the list of visited map coords for the current act.
	/// This should only be used in debug scenarios, like when using the dev console to re-enter an act.
	/// </summary>
	public void ClearVisitedMapCoordsDebug()
	{
		_visitedMapCoords.Clear();
		ActFloor = 0;
	}

	/// <summary>
	/// Adds a modifier to the list of modifiers applied to this run.
	/// This should never be used in a real game run.
	/// </summary>
	public void AddModifierDebug(ModifierModel modifier)
	{
		IReadOnlyList<ModifierModel> modifiers = Modifiers;
		int num = 0;
		ModifierModel[] array = new ModifierModel[1 + modifiers.Count];
		foreach (ModifierModel item in modifiers)
		{
			array[num] = item;
			num++;
		}
		array[num] = modifier;
		Modifiers = new global::_003C_003Ez__ReadOnlyArray<ModifierModel>(array);
	}

	public int GetAndIncrementNextRoomId()
	{
		int nextRoomId = NextRoomId;
		NextRoomId++;
		return nextRoomId;
	}

	/// <summary>
	/// Returns true if the RunState still contains a given model.
	/// Used in hook iteration to determine whether a model was removed by a different model's hook execution.
	/// </summary>
	private static bool Contains(AbstractModel model)
	{
		if (!(model is RelicModel relicModel))
		{
			if (!(model is PotionModel potionModel))
			{
				if (!(model is CardModel cardModel))
				{
					if (!(model is EnchantmentModel enchantmentModel))
					{
						if (!(model is AchievementModel))
						{
							if (!(model is BadgeModel))
							{
								if (!(model is ModifierModel))
								{
									if (model is MultiplayerScalingModel)
									{
										return true;
									}
									throw new ArgumentOutOfRangeException("model", model, $"Invalid model type {model.GetType()} ({model})");
								}
								return true;
							}
							return true;
						}
						return true;
					}
					return enchantmentModel.HasCard && !enchantmentModel.Card.HasBeenRemovedFromState && enchantmentModel.Card.Owner.IsActiveForHooks;
				}
				return !cardModel.HasBeenRemovedFromState && cardModel.Owner.IsActiveForHooks;
			}
			return !potionModel.HasBeenRemovedFromState && potionModel.Owner.IsActiveForHooks;
		}
		return !relicModel.HasBeenRemovedFromState && relicModel.Owner.IsActiveForHooks;
	}
}
