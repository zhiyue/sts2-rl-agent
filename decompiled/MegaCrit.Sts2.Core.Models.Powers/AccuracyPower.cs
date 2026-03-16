using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class AccuracyPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? card)
	{
		if (base.Owner != dealer)
		{
			return 0m;
		}
		if (!props.IsPoweredAttack())
		{
			return 0m;
		}
		if (card == null)
		{
			return 0m;
		}
		if (!card.Tags.Contains(CardTag.Shiv))
		{
			return 0m;
		}
		return base.Amount;
	}
}
