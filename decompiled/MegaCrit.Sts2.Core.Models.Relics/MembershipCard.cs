using System.Collections.Generic;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class MembershipCard : RelicModel
{
	private const string _discountKey = "Discount";

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("Discount", 50m));

	public override RelicRarity Rarity => RelicRarity.Shop;

	public override decimal ModifyMerchantPrice(Player player, MerchantEntry entry, decimal originalPrice)
	{
		if (player != base.Owner)
		{
			return originalPrice;
		}
		if (!LocalContext.IsMe(base.Owner))
		{
			return originalPrice;
		}
		return originalPrice * (base.DynamicVars["Discount"].BaseValue / 100m);
	}
}
