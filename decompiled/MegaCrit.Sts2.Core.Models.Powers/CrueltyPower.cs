using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class CrueltyPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<VulnerablePower>());

	public decimal ModifyVulnerableMultiplier(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target == base.Owner)
		{
			return amount;
		}
		if (!props.IsPoweredAttack())
		{
			return amount;
		}
		return amount + (decimal)base.Amount / 100m;
	}
}
