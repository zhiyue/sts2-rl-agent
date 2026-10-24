using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class TheCourier : RelicModel
{
	private const string _discountKey = "Discount";

	public override RelicRarity Rarity => RelicRarity.Rare;

	public override bool IsAllowedInShops => false;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("Discount", 20m));

	public override decimal ModifyMerchantPrice(Player player, MerchantEntry entry, decimal originalPrice)
	{
		if (player != base.Owner)
		{
			return originalPrice;
		}
		return originalPrice * (1m - base.DynamicVars["Discount"].BaseValue / 100m);
	}

	public override bool ShouldRefillMerchantEntry(MerchantEntry entry, Player player)
	{
		return player == base.Owner;
	}
}
