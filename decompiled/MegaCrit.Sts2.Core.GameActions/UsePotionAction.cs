using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.GameActions;

public class UsePotionAction : GameAction
{
	public override ulong OwnerId => Player.NetId;

	public override GameActionType ActionType
	{
		get
		{
			if (!WasEnqueuedInCombat)
			{
				return GameActionType.NonCombat;
			}
			return GameActionType.CombatPlayPhaseOnly;
		}
	}

	public Player Player { get; }

	public uint PotionIndex { get; }

	public uint? TargetId { get; }

	public bool WasEnqueuedInCombat { get; }

	private ulong? TargetPlayerId { get; }

	public PlayerChoiceContext? PlayerChoiceContext { get; private set; }

	/// <summary>
	/// Constructor to use for constructing the UsePotionAction outside of serialization.
	/// </summary>
	/// <param name="potion">The potion which will be used.</param>
	/// <param name="target">The target which the player is using the potion on. Pass null if the potion is not targeted.</param>
	/// <param name="isCombatInProgress">Pass true if the potion was played during combat. The GameAction will be cancelled at
	/// the end of combat if it is still enqueued.</param>
	public UsePotionAction(PotionModel potion, Creature? target, bool isCombatInProgress)
	{
		if (potion.Owner == null)
		{
			throw new InvalidOperationException($"Cannot enqueue UsePotionAction for potion {potion} without an owner!");
		}
		int potionSlotIndex = potion.Owner.GetPotionSlotIndex(potion);
		if (potionSlotIndex < 0)
		{
			throw new InvalidOperationException($"Potion {potion} has owner {Player}, but the owner's potion list does not contain it!");
		}
		Player = potion.Owner;
		PotionIndex = (uint)potionSlotIndex;
		WasEnqueuedInCombat = isCombatInProgress;
		if (target == null)
		{
			return;
		}
		if (!target.CombatId.HasValue)
		{
			if (CombatManager.Instance.IsInProgress)
			{
				throw new InvalidOperationException($"Trying to target potion {potion} at target {target} that has no combat ID assigned during combat!");
			}
			if (!target.IsPlayer)
			{
				throw new InvalidOperationException($"Trying to target potion {potion} at target {target} outside of combat that is not a player!");
			}
		}
		TargetId = target.CombatId;
		TargetPlayerId = target.Player?.NetId;
	}

	/// <summary>
	/// Constructor to use when deserializing a NetUsePotionAction. Should not be used in other circumstances.
	/// </summary>
	/// <param name="player">The player who sent us the action.</param>
	/// <param name="potionIndex">The index of the potion that will be used.</param>
	/// <param name="targetId">The combat ID of the target.</param>
	/// <param name="targetPlayerId">The NetID of the player that is targeted, if the potion was used outside of combat.</param>
	/// <param name="isCombatInProgress">Whether or not combat was in progress when the potion was used.</param>
	public UsePotionAction(Player player, uint potionIndex, uint? targetId, ulong? targetPlayerId, bool isCombatInProgress)
	{
		Player = player;
		PotionIndex = potionIndex;
		TargetId = targetId;
		TargetPlayerId = targetPlayerId;
		WasEnqueuedInCombat = isCombatInProgress;
	}

	protected override async Task ExecuteAction()
	{
		PotionModel potion = Player.GetPotionAtSlotIndex((int)PotionIndex);
		if (potion == null)
		{
			Log.Warn($"{"UsePotionAction"}: potion at index {PotionIndex} is null for player {Player.NetId}, canceling");
			Cancel();
			return;
		}
		Creature creature = null;
		if (CombatManager.Instance.IsInProgress)
		{
			creature = ((TargetId.HasValue || !potion.TargetType.IsSingleTarget()) ? (await Player.Creature.CombatState.GetCreatureAsync(TargetId, 10.0)) : Player.Creature);
		}
		else if (!TargetPlayerId.HasValue && potion.TargetType != TargetType.TargetedNoCreature)
		{
			creature = Player.Creature;
		}
		else if (TargetPlayerId.HasValue)
		{
			creature = Player.RunState.GetPlayer(TargetPlayerId.Value).Creature;
		}
		if (!potion.IsValidTarget(creature))
		{
			Cancel();
			return;
		}
		string value = ((creature != null) ? $"targeting {creature.LogName} (index {Player.Creature.CombatState?.Creatures.IndexOf(creature)})" : "no target");
		Log.Info($"Player {potion.Owner.NetId} using potion {potion.Id.Entry} ({value})");
		PlayerChoiceContext = new GameActionPlayerChoiceContext(this);
		await potion.OnUseWrapper(PlayerChoiceContext, creature);
	}

	protected override void CancelAction()
	{
		PotionModel potionAtSlotIndex = Player.GetPotionAtSlotIndex((int)PotionIndex);
		if (TestMode.IsOff && NRun.Instance != null && LocalContext.IsMe(Player) && potionAtSlotIndex != null)
		{
			NRun.Instance.GlobalUi.TopBar.PotionContainer.OnPotionUseOrDiscardCanceled(potionAtSlotIndex);
		}
		potionAtSlotIndex?.AfterUsageCanceled();
	}

	public override INetAction ToNetAction()
	{
		return new NetUsePotionAction
		{
			potionIndex = PotionIndex,
			targetId = TargetId,
			targetPlayerId = TargetPlayerId,
			enqueuedInCombat = WasEnqueuedInCombat
		};
	}

	public override string ToString()
	{
		PotionModel potionAtSlotIndex = Player.GetPotionAtSlotIndex((int)PotionIndex);
		Creature value = Player.Creature.CombatState?.GetCreature(TargetId);
		return $"{"UsePotionAction"} {Player.NetId} {potionAtSlotIndex} index: {PotionIndex} target: {TargetId} ({value}) combat: {WasEnqueuedInCombat}";
	}
}
