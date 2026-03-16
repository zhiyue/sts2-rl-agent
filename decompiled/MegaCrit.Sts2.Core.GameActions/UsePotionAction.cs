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

	public UsePotionAction(PotionModel potion, Creature? target, bool isCombatInIsProgress)
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
		WasEnqueuedInCombat = isCombatInIsProgress;
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
			throw new InvalidOperationException($"Attempted to execute {"UsePotionAction"} with potion index {PotionIndex}, but the potion at that index was null!");
		}
		Creature creature = null;
		if (CombatManager.Instance.IsInProgress)
		{
			if (!TargetId.HasValue && potion.TargetType.IsSingleTarget())
			{
				throw new InvalidOperationException("Attempted to execute UsePotionAction with single target potion during combat, but the target ID is null!");
			}
			creature = await Player.Creature.CombatState.GetCreatureAsync(TargetId, 10.0);
		}
		else
		{
			if (!TargetPlayerId.HasValue && potion.TargetType != TargetType.TargetedNoCreature)
			{
				throw new InvalidOperationException("Attempted to execute UsePotionAction outside of combat, but the target player ID is null!");
			}
			if (TargetPlayerId.HasValue)
			{
				creature = Player.RunState.GetPlayer(TargetPlayerId.Value).Creature;
			}
		}
		string text = ((creature == null) ? null : (creature.IsPlayer ? $"Player {creature.Player.NetId}" : creature.Name));
		string value = ((text != null) ? $"targeting {text} (index {Player.Creature.CombatState?.Creatures.IndexOf(creature)})" : "no target");
		Log.Info($"Player {potion.Owner.NetId} using potion {potion.Id.Entry} ({value})");
		PlayerChoiceContext = new GameActionPlayerChoiceContext(this);
		await potion.OnUseWrapper(PlayerChoiceContext, creature);
	}

	protected override void CancelAction()
	{
		PotionModel potionAtSlotIndex = Player.GetPotionAtSlotIndex((int)PotionIndex);
		if (TestMode.IsOff && NRun.Instance != null && LocalContext.IsMe(Player) && potionAtSlotIndex != null)
		{
			NRun.Instance.GlobalUi.TopBar.PotionContainer.OnPotionUseCanceled(potionAtSlotIndex);
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
