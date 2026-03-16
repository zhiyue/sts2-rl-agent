using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Entities.Enchantments;

public struct EnchantmentOption
{
	public readonly EnchantmentModel enchantment;

	public readonly int minAmount;

	public readonly int maxAmount;

	public EnchantmentOption(EnchantmentModel enchantment, int minAmount, int maxAmount)
	{
		this.enchantment = enchantment;
		this.minAmount = minAmount;
		this.maxAmount = maxAmount;
	}
}
