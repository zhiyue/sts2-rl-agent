using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class ToricToughnessPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.Static(StaticHoverTip.Block));

	public override bool IsInstanced => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new BlockVar(0m, ValueProp.Unpowered));

	public void SetBlock(decimal block)
	{
		AssertMutable();
		base.DynamicVars.Block.BaseValue = block;
	}

	public override async Task AfterBlockCleared(Creature creature)
	{
		if (creature == base.Owner)
		{
			Flash();
			await CreatureCmd.GainBlock(base.Owner, base.DynamicVars.Block, null);
			await PowerCmd.Decrement(this);
		}
	}
}
