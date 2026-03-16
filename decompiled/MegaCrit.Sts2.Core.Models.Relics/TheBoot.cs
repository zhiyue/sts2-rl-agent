using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class TheBoot : RelicModel
{
	private const int _damageMinimum = 5;

	private const string _damageMinimumKey = "DamageMinimum";

	private const string _damageThresholdKey = "DamageThreshold";

	public override RelicRarity Rarity => RelicRarity.Event;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DynamicVar("DamageMinimum", 5m),
		new DynamicVar("DamageThreshold", 4m)
	});

	public override decimal ModifyHpLostBeforeOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (dealer != base.Owner.Creature)
		{
			return amount;
		}
		if (!props.IsPoweredAttack())
		{
			return amount;
		}
		if (amount < 1m)
		{
			return amount;
		}
		if (amount >= base.DynamicVars["DamageMinimum"].BaseValue)
		{
			return amount;
		}
		return base.DynamicVars["DamageMinimum"].BaseValue;
	}

	public override Task AfterModifyingHpLostBeforeOsty()
	{
		Flash();
		return Task.CompletedTask;
	}
}
