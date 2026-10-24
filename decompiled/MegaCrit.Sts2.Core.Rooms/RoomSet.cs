using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Rooms;

public class RoomSet
{
	public readonly List<EventModel> events = new List<EventModel>();

	public int eventsVisited;

	public readonly List<EncounterModel> normalEncounters = new List<EncounterModel>();

	public int normalEncountersVisited;

	public readonly List<EncounterModel> eliteEncounters = new List<EncounterModel>();

	public int eliteEncountersVisited;

	public int bossEncountersVisited;

	private AncientEventModel? _ancient;

	private EncounterModel? _boss;

	/// <summary>
	/// This is false in exactly one scenario: when Neow is not spawned as part of the player's first run
	/// </summary>
	public bool HasAncient => _ancient != null;

	/// <summary>
	/// Whether this act has a second boss (Double Boss ascension mode).
	/// </summary>
	public bool HasSecondBoss => SecondBoss != null;

	public AncientEventModel Ancient
	{
		get
		{
			return _ancient ?? throw new InvalidOperationException("RoomSet.Ancient not set! You must call GenerateRooms");
		}
		set
		{
			_ancient = value;
		}
	}

	public EncounterModel Boss
	{
		get
		{
			return _boss ?? throw new InvalidOperationException("RoomSet.Boss not set! You must call GenerateRooms");
		}
		set
		{
			_boss = value;
		}
	}

	public EncounterModel? SecondBoss { get; set; }

	public EventModel NextEvent => events[eventsVisited % events.Count];

	public EncounterModel NextNormalEncounter => normalEncounters[normalEncountersVisited % normalEncounters.Count];

	public EncounterModel NextEliteEncounter => eliteEncounters[eliteEncountersVisited % eliteEncounters.Count];

	public EncounterModel NextBossEncounter
	{
		get
		{
			if (bossEncountersVisited != 0 && SecondBoss != null)
			{
				return SecondBoss;
			}
			return Boss;
		}
	}

	public void MarkVisited(RoomType roomType)
	{
		switch (roomType)
		{
		case RoomType.Monster:
			normalEncountersVisited++;
			break;
		case RoomType.Elite:
			eliteEncountersVisited++;
			break;
		case RoomType.Event:
			eventsVisited++;
			break;
		case RoomType.Boss:
			bossEncountersVisited++;
			break;
		case RoomType.Treasure:
		case RoomType.Shop:
			break;
		}
	}

	public void EnsureNextEventIsValid(RunState runState)
	{
		if (events.Count == 0)
		{
			return;
		}
		for (int i = 0; i < events.Count; i++)
		{
			if (NextEvent.IsAllowed(runState) && !runState.VisitedEventIds.Contains(NextEvent.Id))
			{
				return;
			}
			eventsVisited++;
		}
		Log.Warn("All unique events exhausted, allowing repetition");
	}

	public SerializableRoomSet ToSave()
	{
		return new SerializableRoomSet
		{
			EventIds = events.Select((EventModel e) => e.Id).ToList(),
			EventsVisited = eventsVisited,
			NormalEncounterIds = normalEncounters.Select((EncounterModel e) => e.Id).ToList(),
			NormalEncountersVisited = normalEncountersVisited,
			EliteEncounterIds = eliteEncounters.Select((EncounterModel e) => e.Id).ToList(),
			EliteEncountersVisited = eliteEncountersVisited,
			BossEncountersVisited = bossEncountersVisited,
			BossId = _boss?.Id,
			SecondBossId = SecondBoss?.Id,
			AncientId = _ancient?.Id
		};
	}

	public static RoomSet FromSave(SerializableRoomSet save)
	{
		RoomSet roomSet = new RoomSet();
		roomSet.events.AddRange(from e in save.EventIds.Select(SaveUtil.EventOrDeprecated)
			where !(e is DeprecatedEvent)
			select e);
		roomSet.eventsVisited = save.EventsVisited;
		roomSet.normalEncounters.AddRange(from e in save.NormalEncounterIds.Select(SaveUtil.EncounterOrDeprecated)
			where !(e is DeprecatedEncounter) && e.RoomType == RoomType.Monster
			select e);
		roomSet.normalEncountersVisited = save.NormalEncountersVisited;
		roomSet.eliteEncounters.AddRange(from e in save.EliteEncounterIds.Select(SaveUtil.EncounterOrDeprecated)
			where !(e is DeprecatedEncounter) && e.RoomType == RoomType.Elite
			select e);
		roomSet.eliteEncountersVisited = save.EliteEncountersVisited;
		roomSet.bossEncountersVisited = save.BossEncountersVisited;
		roomSet._boss = ((save.BossId != null) ? SaveUtil.EncounterOrDeprecated(save.BossId) : null);
		roomSet.SecondBoss = ((save.SecondBossId != null) ? SaveUtil.EncounterOrDeprecated(save.SecondBossId) : null);
		roomSet._ancient = ((save.AncientId != null) ? SaveUtil.AncientEventOrDeprecated(save.AncientId) : null);
		return roomSet;
	}

	/// <summary>
	/// This forces a specific model type to be at a specific index in a list.
	/// First, it searches for a model of type TSpecificModel in the list. If one already exists in the list, it is
	/// swapped with the model at the passed index. Otherwise, the model at the passed index is replaced a new instance
	/// of TSpecificModel.
	/// Used when setting order of specific encounters or events in the tutorial.
	/// </summary>
	/// <param name="list">The list to search in.</param>
	/// <param name="desiredIndex">The index where the model type will be at, in the end.</param>
	/// <typeparam name="TBaseModel">The type of models contained in the list.</typeparam>
	/// <typeparam name="TSpecificModel">The specific kind of model we'll force to be at desiredIndex.</typeparam>
	public static void SwapToOrCreateAtIndex<TBaseModel, TSpecificModel>(List<TBaseModel> list, int desiredIndex) where TBaseModel : AbstractModel where TSpecificModel : TBaseModel
	{
		int num = list.FindIndex((TBaseModel elem) => elem is TSpecificModel);
		if (num >= 0)
		{
			int index = num;
			TBaseModel value = list[num];
			TBaseModel value2 = list[desiredIndex];
			list[desiredIndex] = value;
			list[index] = value2;
		}
		else
		{
			list[desiredIndex] = ModelDb.GetById<TBaseModel>(ModelDb.GetId<TSpecificModel>());
		}
	}
}
