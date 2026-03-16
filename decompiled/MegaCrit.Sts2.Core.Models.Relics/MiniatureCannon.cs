using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class MiniatureCannon : RelicModel
{
	private const string _extraDamageKey = "ExtraDamage";

	public override RelicRarity Rarity => RelicRarity.Uncommon;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("ExtraDamage", 3m));

	public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (!props.IsPoweredAttack())
		{
			return 0m;
		}
		if (cardSource == null)
		{
			return 0m;
		}
		if (!cardSource.IsUpgraded)
		{
			return 0m;
		}
		if (dealer != base.Owner.Creature && cardSource.Owner != base.Owner)
		{
			return 0m;
		}
		return base.DynamicVars["ExtraDamage"].BaseValue;
	}
}
