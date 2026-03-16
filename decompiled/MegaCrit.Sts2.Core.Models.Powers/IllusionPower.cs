using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class IllusionPower : PowerModel
{
	private class Data
	{
		public bool isReviving;
	}

	public const string stunTrigger = "StunTrigger";

	public const string wakeUpTrigger = "WakeUpTrigger";

	private string? _followUpStateId;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public override bool ShouldPlayVfx => false;

	public string? FollowUpStateId
	{
		get
		{
			return _followUpStateId;
		}
		set
		{
			AssertMutable();
			_followUpStateId = value;
		}
	}

	public bool IsReviving => GetInternalData<Data>().isReviving;

	protected override object InitInternalData()
	{
		return new Data();
	}

	public override bool ShouldPowerBeRemovedOnDeath(PowerModel power)
	{
		return power.Type == PowerType.Debuff;
	}

	public override Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		if (base.Owner.HasPower<MinionPower>())
		{
			return Task.CompletedTask;
		}
		return PowerCmd.Apply<MinionPower>(base.Owner, 1m, null, null);
	}

	public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
	{
		if (!wasRemovalPrevented && creature == base.Owner)
		{
			await CreatureCmd.TriggerAnim(base.Owner, "StunTrigger", 0f);
			GetInternalData<Data>().isReviving = true;
			MoveState state = new MoveState("REVIVE_MOVE", ReviveMove, new HealIntent())
			{
				FollowUpStateId = (FollowUpStateId ?? base.Owner.Monster.MoveStateMachine.StateLog.Last().Id),
				MustPerformOnceBeforeTransitioning = true
			};
			base.Owner.Monster.SetMoveImmediate(state);
		}
	}

	public override bool ShouldAllowHitting(Creature creature)
	{
		if (creature != base.Owner)
		{
			return true;
		}
		if (IsReviving)
		{
			return false;
		}
		return true;
	}

	public override bool ShouldCreatureBeRemovedFromCombatAfterDeath(Creature creature)
	{
		if (creature != base.Owner)
		{
			return true;
		}
		return false;
	}

	public override async Task AfterCombatEnd(CombatRoom room)
	{
		if (!base.Owner.IsAlive)
		{
			await CreatureCmd.TriggerAnim(base.Owner, "Dead", 0.1f);
		}
	}

	private async Task ReviveMove(IReadOnlyList<Creature> targets)
	{
		await CreatureCmd.TriggerAnim(base.Owner, "WakeUpTrigger", 0f);
		GetInternalData<Data>().isReviving = false;
		await CreatureCmd.Heal(base.Owner, base.Owner.MaxHp - base.Owner.CurrentHp);
		if (base.Owner.Monster is Parafright)
		{
			SfxCmd.Play("event:/sfx/enemy/enemy_attacks/obscura/obscura_hologram_heal");
		}
	}
}
