using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Enchantments;

public sealed class Instinct : EnchantmentModel
{
	public override bool CanEnchantCardType(CardType cardType)
	{
		return cardType == CardType.Attack;
	}

	public override decimal EnchantDamageMultiplicative(decimal originalDamage, ValueProp props)
	{
		if (!props.IsPoweredAttack())
		{
			return 1m;
		}
		return 2m;
	}
}
