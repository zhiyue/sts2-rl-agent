using MegaCrit.Sts2.Core.Entities.Cards;

namespace MegaCrit.Sts2.Core.Models.Enchantments.Mocks;

public sealed class MockFreeEnchantment : EnchantmentModel
{
	protected override void OnEnchant()
	{
		base.Card.EnergyCost.UpgradeBy(-base.Card.EnergyCost.GetWithModifiers(CostModifiers.None));
	}
}
