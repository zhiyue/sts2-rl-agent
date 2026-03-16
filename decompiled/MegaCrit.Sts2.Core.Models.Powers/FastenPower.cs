using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public class FastenPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.Static(StaticHoverTip.Block));

	public override decimal ModifyBlockAdditive(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
	{
		if (base.Owner != target)
		{
			return 0m;
		}
		if (!props.IsPoweredCardOrMonsterMoveBlock())
		{
			return 0m;
		}
		if (cardSource != null && !cardSource.Tags.Contains(CardTag.Defend))
		{
			return 0m;
		}
		return base.Amount;
	}

	public override Task AfterModifyingBlockAmount(decimal modifiedBlock, CardModel? cardSource, CardPlay? cardPlay)
	{
		Flash();
		return Task.CompletedTask;
	}
}
