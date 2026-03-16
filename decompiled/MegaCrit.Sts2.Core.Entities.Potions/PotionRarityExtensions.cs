using System;
using MegaCrit.Sts2.Core.Localization;

namespace MegaCrit.Sts2.Core.Entities.Potions;

public static class PotionRarityExtensions
{
	public static LocString ToLocString(this PotionRarity potionRarity)
	{
		switch (potionRarity)
		{
		case PotionRarity.Common:
			return new LocString("gameplay_ui", "POTION_RARITY.COMMON");
		case PotionRarity.Uncommon:
			return new LocString("gameplay_ui", "POTION_RARITY.UNCOMMON");
		case PotionRarity.Rare:
			return new LocString("gameplay_ui", "POTION_RARITY.RARE");
		case PotionRarity.Event:
			return new LocString("gameplay_ui", "POTION_RARITY.EVENT");
		case PotionRarity.Token:
			return new LocString("gameplay_ui", "POTION_RARITY.TOKEN");
		case PotionRarity.None:
			throw new ArgumentOutOfRangeException("potionRarity", potionRarity, null);
		default:
		{
			global::_003CPrivateImplementationDetails_003E.ThrowSwitchExpressionException(potionRarity);
			LocString result = default(LocString);
			return result;
		}
		}
	}
}
