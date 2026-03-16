using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class CalcifyPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (!(dealer?.Monster is Osty))
		{
			return 0m;
		}
		if (base.Owner != dealer.PetOwner?.Creature)
		{
			return 0m;
		}
		if (!props.IsPoweredAttack())
		{
			return 0m;
		}
		return base.Amount;
	}
}
