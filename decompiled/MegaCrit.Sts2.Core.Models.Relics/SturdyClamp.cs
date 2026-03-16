using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class SturdyClamp : RelicModel
{
	private const int _maxRetainedBlock = 10;

	public override RelicRarity Rarity => RelicRarity.Rare;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new BlockVar(10m, ValueProp.Unpowered));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.Static(StaticHoverTip.Block));

	public override bool ShouldClearBlock(Creature creature)
	{
		if (creature != base.Owner.Creature)
		{
			return true;
		}
		return false;
	}

	public override async Task AfterPreventingBlockClear(AbstractModel preventer, Creature creature)
	{
		if (this != preventer || creature != base.Owner.Creature)
		{
			return;
		}
		int block = creature.Block;
		if (block != 0)
		{
			if (block > 10)
			{
				await CreatureCmd.LoseBlock(creature, block - 10);
			}
			Flash();
		}
	}
}
