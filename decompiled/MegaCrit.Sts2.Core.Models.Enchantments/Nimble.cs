namespace MegaCrit.Sts2.Core.Models.Enchantments;

public sealed class Nimble : EnchantmentModel
{
	public override bool ShowAmount => true;

	public override bool CanEnchant(CardModel card)
	{
		if (base.CanEnchant(card))
		{
			return card.GainsBlock;
		}
		return false;
	}

	public override decimal EnchantBlockAdditive(decimal originalBlock)
	{
		return base.Amount;
	}
}
