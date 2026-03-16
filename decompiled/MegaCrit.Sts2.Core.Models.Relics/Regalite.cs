using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class Regalite : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Uncommon;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new BlockVar(2m, ValueProp.Unpowered));

	public override async Task AfterCardEnteredCombat(CardModel card)
	{
		if (card.Owner == base.Owner && card.VisualCardPool.IsColorless)
		{
			await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, null, fast: true);
		}
	}
}
