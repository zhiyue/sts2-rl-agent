using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
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
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Rooms;

public class CombatRoom : AbstractRoom, ICombatRoomVisuals
{
	private bool _isPreFinished;

	private readonly Dictionary<Player, List<Reward>> _extraRewards = new Dictionary<Player, List<Reward>>();

	public override RoomType RoomType => Encounter.RoomType;

	public override ModelId ModelId => Encounter.Id;

	public EncounterModel Encounter => CombatState.Encounter;

	public CombatState CombatState { get; }

	public IEnumerable<Creature> Allies => CombatState.Allies;

	public IEnumerable<Creature> Enemies => CombatState.Enemies;

	public ActModel Act => CombatState.RunState.Act;

	public override bool IsPreFinished => _isPreFinished;

	public float GoldProportion { get; private set; } = 1f;

	public IReadOnlyDictionary<Player, List<Reward>> ExtraRewards => _extraRewards;

	public bool ShouldCreateCombat { get; init; } = true;

	public bool ShouldResumeParentEventAfterCombat { get; init; } = true;

	public ModelId? ParentEventId { get; init; }

	public CombatRoom(EncounterModel encounter, IRunState? runState)
	{
		encounter.AssertMutable();
		CombatState = new CombatState(encounter, runState, runState?.Modifiers, runState?.MultiplayerScalingModel);
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

	public override async Task Enter(IRunState? runState, bool isRestoringRoomStackBase)
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
		CombatManager.Instance.Reset();
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
			CombatState.RunState.CurrentMapPointHistoryEntry.Rooms.Last().MonsterIds.Add(monsterModel.Id);
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
		GoldProportion = 1f - (float)CombatState.EscapedCreatures.Count / (float)Encounter.MonstersWithSlots.Count;
	}

	private async Task StartPreFinishedCombat()
	{
		if (TestMode.IsOn)
		{
			return;
		}
		Encounter.GenerateMonstersWithSlots(CombatState.RunState);
		await PreloadManager.LoadRoomCombatAssets(Encounter, CombatState.RunState);
		NCombatRoom nCombatRoom = NCombatRoom.Create(this, CombatRoomMode.FinishedCombat);
		NRun.Instance?.SetCurrentRoom(nCombatRoom);
		nCombatRoom?.SetUpBackground(CombatState.RunState);
		NMapScreen.Instance.SetTravelEnabled(enabled: true);
		foreach (Player player in CombatState.RunState.Players)
		{
			player.ResetCombatState();
		}
		RunManager.Instance.ActionExecutor.Unpause();
		Player me = LocalContext.GetMe(CombatState);
		TaskHelper.RunSafely(RewardsCmd.OfferForRoomEnd(me, this));
	}
}
