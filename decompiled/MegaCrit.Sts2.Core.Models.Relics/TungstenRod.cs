using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class TungstenRod : RelicModel
{
	private const string _hpLossReductionKey = "HpLossReduction";

	public override RelicRarity Rarity => RelicRarity.Rare;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("HpLossReduction", 1m));

	public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target != base.Owner.Creature)
		{
			return amount;
		}
		return Math.Max(0m, amount - base.DynamicVars["HpLossReduction"].BaseValue);
	}

	public override Task AfterModifyingHpLostAfterOsty()
	{
		Flash();
		return Task.CompletedTask;
	}
}
