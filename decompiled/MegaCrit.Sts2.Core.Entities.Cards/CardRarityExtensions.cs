using System;
using MegaCrit.Sts2.Core.Localization;

namespace MegaCrit.Sts2.Core.Entities.Cards;

public static class CardRarityExtensions
{
	public static CardRarity GetNextHighestRarity(this CardRarity cardRarity)
	{
		return cardRarity switch
		{
			CardRarity.None => CardRarity.None, 
			CardRarity.Basic => CardRarity.Common, 
			CardRarity.Common => CardRarity.Uncommon, 
			CardRarity.Uncommon => CardRarity.Rare, 
			CardRarity.Rare => CardRarity.None, 
			CardRarity.Status => CardRarity.None, 
			CardRarity.Curse => CardRarity.None, 
			CardRarity.Event => CardRarity.None, 
			CardRarity.Token => CardRarity.None, 
			CardRarity.Quest => CardRarity.None, 
			CardRarity.Ancient => CardRarity.None, 
			_ => throw new ArgumentOutOfRangeException("cardRarity", cardRarity, null), 
		};
	}

	public static LocString ToLocString(this CardRarity cardRarity)
	{
		return cardRarity switch
		{
			CardRarity.Basic => new LocString("gameplay_ui", "CARD_RARITY.BASIC"), 
			CardRarity.Common => new LocString("gameplay_ui", "CARD_RARITY.COMMON"), 
			CardRarity.Uncommon => new LocString("gameplay_ui", "CARD_RARITY.UNCOMMON"), 
			CardRarity.Rare => new LocString("gameplay_ui", "CARD_RARITY.RARE"), 
			CardRarity.Status => new LocString("gameplay_ui", "CARD_RARITY.STATUS"), 
			CardRarity.Curse => new LocString("gameplay_ui", "CARD_RARITY.CURSE"), 
			CardRarity.Event => new LocString("gameplay_ui", "CARD_RARITY.EVENT"), 
			CardRarity.Token => new LocString("gameplay_ui", "CARD_RARITY.TOKEN"), 
			CardRarity.Quest => new LocString("gameplay_ui", "CARD_RARITY.QUEST"), 
			CardRarity.Ancient => new LocString("gameplay_ui", "CARD_RARITY.ANCIENT"), 
			_ => throw new ArgumentOutOfRangeException("cardRarity", cardRarity, null), 
		};
	}
}
