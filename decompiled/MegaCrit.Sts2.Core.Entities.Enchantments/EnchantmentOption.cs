using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Entities.Enchantments;

/// <summary>
/// Utility object representing a random enchantment that may be applied to a card.
/// </summary>
public struct EnchantmentOption
{
	public readonly EnchantmentModel enchantment;

	public readonly int minAmount;

	public readonly int maxAmount;

	/// <summary>
	/// Utility object representing a random enchantment that may be applied to a card.
	/// </summary>
	public EnchantmentOption(EnchantmentModel enchantment, int minAmount, int maxAmount)
	{
		this.enchantment = enchantment;
		this.minAmount = minAmount;
		this.maxAmount = maxAmount;
	}
}
