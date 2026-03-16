using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class TankPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		foreach (Creature item in base.CombatState.GetTeammatesOf(base.Owner))
		{
			if (item.IsAlive && item.IsPlayer && item != base.Owner)
			{
				await PowerCmd.Apply<GuardedPower>(item, base.Amount, base.Owner, null);
			}
		}
	}

	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target != base.Owner)
		{
			return 1m;
		}
		if (!props.IsPoweredAttack())
		{
			return 1m;
		}
		return 2m;
	}
}
