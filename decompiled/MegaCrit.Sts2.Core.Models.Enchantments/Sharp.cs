using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Enchantments;

public sealed class Sharp : EnchantmentModel
{
	public override bool ShowAmount => true;

	public override bool CanEnchantCardType(CardType cardType)
	{
		return cardType == CardType.Attack;
	}

	public override decimal EnchantDamageAdditive(decimal originalDamage, ValueProp props)
	{
		if (!props.IsPoweredAttack())
		{
			return 0m;
		}
		return base.Amount;
	}
}
