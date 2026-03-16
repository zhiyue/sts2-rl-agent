using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class MysticLighter : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Shop;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DamageVar(9m, ValueProp.Unpowered));

	public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (!props.IsPoweredAttack())
		{
			return 0m;
		}
		if (cardSource?.Enchantment == null)
		{
			return 0m;
		}
		if (cardSource.Owner != base.Owner)
		{
			return 0m;
		}
		return base.DynamicVars.Damage.IntValue;
	}
}
