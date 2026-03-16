using MegaCrit.Sts2.Core.Entities.Cards;

namespace MegaCrit.Sts2.Core.Models.Enchantments;

public sealed class Steady : EnchantmentModel
{
	protected override void OnEnchant()
	{
		base.Card.AddKeyword(CardKeyword.Retain);
	}
}
