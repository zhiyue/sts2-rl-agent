using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class DoorRevivalPower : PowerModel
{
	private class Data
	{
		public bool isHalfDead;
	}

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	protected override bool IsVisibleInternal => false;

	public bool IsHalfDead => GetInternalData<Data>().isHalfDead;

	protected override object InitInternalData()
	{
		return new Data();
	}

	public override Task BeforeDeath(Creature creature)
	{
		if (creature != base.Owner)
		{
			return Task.CompletedTask;
		}
		GetInternalData<Data>().isHalfDead = true;
		return Task.CompletedTask;
	}

	public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
	{
		if (creature == base.Owner)
		{
			if (wasRemovalPrevented)
			{
				GetInternalData<Data>().isHalfDead = false;
				return;
			}
			Door door = (Door)base.Owner.Monster;
			door.PrepareForDeath();
			door.SetMoveImmediate(door.DeadState);
			door.Open();
			await CreatureCmd.Add(door.Doormaker);
			Doormaker doormaker = (Doormaker)door.Doormaker.Monster;
			await doormaker.AnimIn();
		}
	}

	public async Task DoRevive()
	{
		GetInternalData<Data>().isHalfDead = false;
		Door door = (Door)base.Owner.Monster;
		await CreatureCmd.SetMaxHp(base.Owner, base.Owner.Monster.MinInitialHp);
		await CreatureCmd.Heal(base.Owner, base.Owner.MaxHp);
		door.PrepareForRevival();
		door.Close();
	}

	public override bool ShouldAllowHitting(Creature creature)
	{
		if (creature != base.Owner)
		{
			return true;
		}
		return !IsHalfDead;
	}

	public override bool ShouldStopCombatFromEnding()
	{
		if (!IsHalfDead)
		{
			return false;
		}
		Door door = (Door)base.Owner.Monster;
		return door.Doormaker.IsAlive;
	}

	public override bool ShouldCreatureBeRemovedFromCombatAfterDeath(Creature creature)
	{
		if (creature == base.Owner)
		{
			return false;
		}
		Door door = (Door)base.Owner.Monster;
		if (creature == door.Doormaker && !base.Owner.CombatState.ContainsCreature(creature))
		{
			return false;
		}
		return true;
	}

	public override bool ShouldPowerBeRemovedAfterOwnerDeath()
	{
		return false;
	}
}
