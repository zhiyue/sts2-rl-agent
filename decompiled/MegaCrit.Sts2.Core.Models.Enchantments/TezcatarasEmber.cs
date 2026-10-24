using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Enchantments;

public sealed class TezcatarasEmber : EnchantmentModel
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromKeyword(CardKeyword.Eternal));

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DamageVar(3m, ValueProp.Move));

	protected override void OnEnchant()
	{
		base.Card.EnergyCost.UpgradeBy(-base.Card.EnergyCost.GetWithModifiers(CostModifiers.None));
		base.Card.AddKeyword(CardKeyword.Eternal);
	}

	public override decimal EnchantDamageAdditive(decimal originalDamage, ValueProp props)
	{
		if (!props.IsPoweredAttack())
		{
			return 0m;
		}
		return base.DynamicVars.Damage.BaseValue;
	}
}
