using MegaCrit.Sts2.Core.Entities.Cards;

namespace MegaCrit.Sts2.Core.Models.Enchantments;

public sealed class Instinct : EnchantmentModel
{
	public override bool CanEnchantCardType(CardType cardType)
	{
		return cardType == CardType.Attack;
	}

	public override bool CanEnchant(CardModel card)
	{
		if (base.CanEnchant(card) && !card.Keywords.Contains(CardKeyword.Unplayable) && !card.EnergyCost.CostsX)
		{
			return card.EnergyCost.GetWithModifiers(CostModifiers.None) > 0;
		}
		return false;
	}

	protected override void OnEnchant()
	{
		base.Card.EnergyCost.UpgradeBy(-1);
	}
}
