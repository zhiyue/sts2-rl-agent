using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers.Mocks;

public sealed class MockRevivePower : PowerModel
{
	private class Data
	{
		public bool isReviving;
	}

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	private bool IsReviving => GetInternalData<Data>().isReviving;

	protected override object InitInternalData()
	{
		return new Data();
	}

	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target != base.Owner)
		{
			return 1m;
		}
		if (!IsReviving)
		{
			return 1m;
		}
		return 0m;
	}

	public override bool TryModifyPowerAmountReceived(PowerModel canonicalPower, Creature target, decimal amount, Creature? applier, out decimal modifiedAmount)
	{
		modifiedAmount = amount;
		if (target != base.Owner)
		{
			return false;
		}
		if (!IsReviving)
		{
			return false;
		}
		return true;
	}

	public override bool ShouldAllowHitting(Creature creature)
	{
		return !IsReviving;
	}

	public override bool ShouldDie(Creature creature)
	{
		if (creature != base.Owner)
		{
			return true;
		}
		return false;
	}

	public override bool ShouldStopCombatFromEnding()
	{
		return true;
	}

	public override async Task AfterPreventingDeath(Creature creature)
	{
		if (creature == base.Owner)
		{
			GetInternalData<Data>().isReviving = true;
			await CreatureCmd.Heal(base.Owner, 1m, playAnim: false);
		}
	}
}
