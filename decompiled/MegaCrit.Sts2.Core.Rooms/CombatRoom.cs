using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Rooms;

public class CombatRoom : AbstractRoom, ICombatRoomVisuals
{
	private bool _isPreFinished;

	private readonly Dictionary<Player, List<Reward>> _extraRewards = new Dictionary<Player, List<Reward>>();

	public override RoomType RoomType => Encounter.RoomType;

	public override ModelId ModelId => Encounter.Id;

	/// <summary>
	/// The mutable encounter that the player is facing in this room.
	/// </summary>
	public EncounterModel Encounter => CombatState.Encounter;

	/// <summary>
	/// The state of the combat that is currently in progress in this room.
	/// </summary>
	public CombatState CombatState { get; }

	public IEnumerable<Creature> Allies => CombatState.Allies;

	public IEnumerable<Creature> Enemies => CombatState.Enemies;

	public ActModel Act => CombatState.RunState.Act;

	public override bool IsPreFinished => _isPreFinished;

	public float GoldProportion { get; private set; } = 1f;

	public IReadOnlyDictionary<Player, List<Reward>> ExtraRewards => _extraRewards;

	/// <summary>
	/// Whether to create a combat room node for this combat room.
	/// Usually true, but false for "delayed-start" combats, like events with combat-style layouts that can transition
	/// to combats if certain options are selected.
	/// </summary>
	public bool ShouldCreateCombat { get; init; } = true;

	/// <summary>
	/// If this combat room is nested within an event room, should we resume the parent event after combat ends?
	/// Usually true, but false for some combat-style events. In many of these cases, you transition from a visual-only
	/// combat room into a real combat, and then you just proceed to the next map point after combat ends.
	/// </summary>
	public bool ShouldResumeParentEventAfterCombat { get; init; } = true;

	/// <summary>
	/// If this combat room is nested within an event room that should resume after combat,
	/// this stores the parent event's ID so the event room can be recreated on load.
	/// </summary>
	public ModelId? ParentEventId { get; init; }

	/// <summary>
	/// Creates a combat room for the given encounter.
	/// </summary>
	/// <param name="encounter">Encounter that is taking place in this combat room.</param>
	/// <param name="runState">State of the run that this combat room is a part of.</param>
	public CombatRoom(EncounterModel encounter, IRunState? runState)
	{
		encounter.AssertMutable();
		CombatState = new CombatState(encounter, runState, runState?.Modifiers, runState?.BadgeModels, runState?.MultiplayerScalingModel);
	}

	public CombatRoom(CombatState combatState)
	{
		CombatState = combatState;
	}

	public new static CombatRoom FromSerializable(SerializableRoom serializableRoom, IRunState? runState)
	{
		if (serializableRoom.ExtraRewards.Count > 0 && runState == null)
		{
			throw new InvalidOperationException("Cannot load extra rewards without a run state.");
		}
		EncounterModel encounterModel = SaveUtil.EncounterOrDeprecated(serializableRoom.EncounterId).ToMutable();
		encounterModel.LoadCustomState(serializableRoom.EncounterState);
		CombatRoom combatRoom = new CombatRoom(encounterModel, runState)
		{
			GoldProportion = serializableRoom.GoldProportion,
			_isPreFinished = serializableRoom.IsPreFinished,
			ShouldResumeParentEventAfterCombat = serializableRoom.ShouldResumeParentEvent,
			ParentEventId = serializableRoom.ParentEventId
		};
		foreach (KeyValuePair<ulong, List<SerializableReward>> extraReward in serializableRoom.ExtraRewards)
		{
			extraReward.Deconstruct(out var key, out var value);
			ulong netId = key;
			List<SerializableReward> source = value;
			Player player = runState.GetPlayer(netId);
			List<Reward> value2 = source.Select((SerializableReward sr) => Reward.FromSerializable(sr, player)).ToList();
			combatRoom._extraRewards.Add(player, value2);
		}
		if (serializableRoom.IsPreFinished)
		{
			combatRoom.MarkPreFinished();
		}
		return combatRoom;
	}

	public override async Task EnterInternal(IRunState? runState, bool isRestoringRoomStackBase)
	{
		if (isRestoringRoomStackBase)
		{
			throw new InvalidOperationException("CombatRoom does not support room stack reconstruction.");
		}
		if (CombatState.Players.Count == 0)
		{
			foreach (Player item in runState?.Players ?? Array.Empty<Player>())
			{
				CombatState.AddPlayer(item);
			}
		}
		if (IsPreFinished)
		{
			await StartPreFinishedCombat();
		}
		else
		{
			await StartCombat(runState);
		}
	}

	public override Task Exit(IRunState? runState)
	{
		CombatManager.Instance.Reset(graceful: true);
		if (IsPreFinished)
		{
			foreach (Creature item in CombatState.PlayerCreatures.ToList())
			{
				CombatState.RemoveCreature(item);
			}
		}
		return Task.CompletedTask;
	}

	public override Task Resume(AbstractRoom _, IRunState? runState)
	{
		throw new NotImplementedException();
	}

	public override SerializableRoom ToSerializable()
	{
		if (ParentEventId != null && !IsPreFinished)
		{
			throw new InvalidOperationException("Cannot serialize a CombatRoom with a ParentEventId that is not pre-finished.");
		}
		SerializableRoom serializableRoom = base.ToSerializable();
		serializableRoom.EncounterId = Encounter.Id;
		serializableRoom.IsPreFinished = IsPreFinished;
		serializableRoom.GoldProportion = GoldProportion;
		serializableRoom.ParentEventId = ParentEventId;
		serializableRoom.ShouldResumeParentEvent = ShouldResumeParentEventAfterCombat;
		serializableRoom.EncounterState = Encounter.SaveCustomState();
		foreach (var (player2, source) in ExtraRewards)
		{
			serializableRoom.ExtraRewards[player2.NetId] = source.Select((Reward r) => r.ToSerializable()).ToList();
		}
		return serializableRoom;
	}

	public void MarkPreFinished()
	{
		_isPreFinished = true;
	}

	public void AddExtraReward(Player player, Reward reward)
	{
		if (!ExtraRewards.ContainsKey(player))
		{
			_extraRewards.Add(player, new List<Reward>());
		}
		ExtraRewards[player].Add(reward);
	}

	private async Task StartCombat(IRunState? runState)
	{
		if (!Encounter.HaveMonstersBeenGenerated)
		{
			Encounter.GenerateMonstersWithSlots(CombatState.RunState);
		}
		if (ShouldCreateCombat)
		{
			await PreloadManager.LoadRoomCombatAssets(Encounter, runState ?? NullRunState.Instance);
		}
		foreach (var (monsterModel, slot) in Encounter.MonstersWithSlots)
		{
			monsterModel.AssertMutable();
			if (ShouldCreateCombat)
			{
				Creature creature = CombatState.CreateCreature(monsterModel, CombatSide.Enemy, slot);
				CombatState.AddCreature(creature);
			}
			CombatState.RunState.CurrentMapPointHistoryEntry?.Rooms.Last().MonsterIds.Add(monsterModel.Id);
		}
		if (ShouldCreateCombat)
		{
			NRun.Instance?.SetCurrentRoom(NCombatRoom.Create(this, CombatRoomMode.ActiveCombat));
		}
		else
		{
			NCombatRoom.Instance?.TransitionToActiveCombat(this);
		}
		CombatManager.Instance.SetUpCombat(CombatState);
		if (runState != null)
		{
			await Hook.AfterRoomEntered(runState, this);
		}
		CombatManager.Instance.AfterCombatRoomLoaded();
	}

	public void OnCombatEnded()
	{
		GoldProportion = Encounter.CalculateGoldProportion(CombatState);
	}

	private async Task StartPreFinishedCombat()
	{
		Encounter.GenerateMonstersWithSlots(CombatState.RunState);
		await PreloadManager.LoadRoomCombatAssets(Encounter, CombatState.RunState);
		NCombatRoom nCombatRoom = NCombatRoom.Create(this, CombatRoomMode.FinishedCombat);
		NRun.Instance?.SetCurrentRoom(nCombatRoom);
		nCombatRoom?.SetUpBackground(CombatState.RunState);
		NMapScreen.Instance?.SetTravelEnabled(enabled: true);
		foreach (Player player in CombatState.RunState.Players)
		{
			player.ResetCombatState();
		}
		RunManager.Instance.ActionExecutor.Unpause();
		if (Encounter.ShouldGiveRewards)
		{
			await OfferRoomEndRewards();
		}
		else if (nCombatRoom != null)
		{
			await nCombatRoom.Ui.ProceedWithoutRewards();
		}
	}

	public async Task OfferRoomEndRewards()
	{
		List<RewardsSet> rewards = new List<RewardsSet>();
		foreach (Player player in CombatState.Players)
		{
			List<RewardsSet> list = rewards;
			list.Add(await RewardsCmd.GenerateForRoomEnd(player, this));
		}
		foreach (RewardsSet item in rewards)
		{
			TaskHelper.RunSafely(item.Offer());
		}
	}
}
