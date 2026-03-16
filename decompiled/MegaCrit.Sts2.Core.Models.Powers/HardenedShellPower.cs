using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class HardenedShellPower : PowerModel
{
	private class Data
	{
		public decimal damageReceivedThisTurn;
	}

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override bool ShouldScaleInMultiplayer => true;

	public override int DisplayAmount => (int)Math.Max(0m, (decimal)base.Amount - GetInternalData<Data>().damageReceivedThisTurn);

	protected override object InitInternalData()
	{
		return new Data();
	}

	public override decimal ModifyHpLostBeforeOstyLate(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target != base.Owner)
		{
			return amount;
		}
		if (amount == 0m)
		{
			return amount;
		}
		return Math.Min(amount, (decimal)base.Amount - GetInternalData<Data>().damageReceivedThisTurn);
	}

	public override Task AfterModifyingHpLostBeforeOsty()
	{
		Flash();
		return Task.CompletedTask;
	}

	public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target != base.Owner)
		{
			return Task.CompletedTask;
		}
		if (result.WasFullyBlocked)
		{
			return Task.CompletedTask;
		}
		GetInternalData<Data>().damageReceivedThisTurn += (decimal)result.UnblockedDamage;
		InvokeDisplayAmountChanged();
		return Task.CompletedTask;
	}

	public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
	{
		if (side != CombatSide.Player)
		{
			return Task.CompletedTask;
		}
		GetInternalData<Data>().damageReceivedThisTurn = default(decimal);
		InvokeDisplayAmountChanged();
		return Task.CompletedTask;
	}
}
