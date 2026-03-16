using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Singleton;
using MegaCrit.Sts2.Core.Odds;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Runs;

public class NullRunState : IRunState, ICardScope, IPlayerCollection
{
	public static NullRunState Instance { get; } = new NullRunState();

	public IReadOnlyList<Player> Players => Array.Empty<Player>();

	public IReadOnlyList<ActModel> Acts => ActModel.GetDefaultList();

	public int CurrentActIndex
	{
		get
		{
			return 0;
		}
		set
		{
			throw new InvalidOperationException("Cannot set act index in a null run.");
		}
	}

	public ActModel Act => ModelDb.Act<Overgrowth>().ToMutable();

	public ActMap Map
	{
		get
		{
			return NullActMap.Instance;
		}
		set
		{
			throw new InvalidOperationException("Cannot set map in a null run.");
		}
	}

	public MapCoord? CurrentMapCoord => null;

	public MapPoint? CurrentMapPoint => null;

	public RunLocation CurrentLocation => new RunLocation(null, 0);

	public int ActFloor
	{
		get
		{
			return 0;
		}
		set
		{
			throw new InvalidOperationException("Cannot set act floor in a null run.");
		}
	}

	public int TotalFloor => 0;

	public IReadOnlyList<ModifierModel> Modifiers => Array.Empty<ModifierModel>();

	public MultiplayerScalingModel? MultiplayerScalingModel => null;

	public IReadOnlyList<IReadOnlyList<MapPointHistoryEntry>> MapPointHistory => Array.Empty<IReadOnlyList<MapPointHistoryEntry>>();

	public MapPointHistoryEntry? CurrentMapPointHistoryEntry => null;

	public int CurrentRoomCount => 0;

	public AbstractRoom? CurrentRoom => null;

	public AbstractRoom? BaseRoom => null;

	public bool IsGameOver => false;

	public int AscensionLevel => 0;

	public RunRngSet Rng => new RunRngSet(string.Empty);

	public RunOddsSet Odds => new RunOddsSet(new Rng());

	public RelicGrabBag SharedRelicGrabBag => new RelicGrabBag(refreshAllowed: false);

	public UnlockState UnlockState => MegaCrit.Sts2.Core.Unlocks.UnlockState.all;

	public ExtraRunFields ExtraFields => new ExtraRunFields();

	private NullRunState()
	{
	}

	public Player? GetPlayer(ulong netId)
	{
		return null;
	}

	public int GetPlayerSlotIndex(Player player)
	{
		return -1;
	}

	public T CreateCard<T>(Player owner) where T : CardModel
	{
		throw new InvalidOperationException("Cannot create cards in a null run.");
	}

	public CardModel CreateCard(CardModel canonicalCard, Player owner)
	{
		throw new InvalidOperationException("Cannot create cards in a null run.");
	}

	public CardModel CloneCard(CardModel mutableCard)
	{
		throw new InvalidOperationException("Cannot clone cards in a null run.");
	}

	public bool ContainsCard(CardModel card)
	{
		return false;
	}

	public void AddCard(CardModel mutableCard, Player owner)
	{
		throw new InvalidOperationException("Cannot add cards in a null run.");
	}

	public void RemoveCard(CardModel card)
	{
		throw new InvalidOperationException("Cannot remove cards in a null run.");
	}

	public CardModel LoadCard(SerializableCard serializableCard, Player owner)
	{
		throw new InvalidOperationException("Cannot load cards in a null run.");
	}

	public void AppendToMapPointHistory(MapPointType mapPointType, RoomType initialRoomType, ModelId? modelId)
	{
	}

	public MapPointHistoryEntry? GetHistoryEntryFor(RunLocation location)
	{
		return null;
	}

	public IEnumerable<AbstractModel> IterateHookListeners(CombatState? childCombatState)
	{
		return childCombatState?.IterateHookListeners() ?? Array.Empty<AbstractModel>();
	}
}
