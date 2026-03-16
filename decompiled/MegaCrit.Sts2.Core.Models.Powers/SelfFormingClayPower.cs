using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class SelfFormingClayPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[1] { HoverTipFactory.Static(StaticHoverTip.Block) };

	public override async Task AfterBlockCleared(Creature creature)
	{
		if (creature == base.Owner)
		{
			await CreatureCmd.GainBlock(base.Owner, base.Amount, ValueProp.Unpowered, null);
			await PowerCmd.Remove(this);
		}
	}
}
