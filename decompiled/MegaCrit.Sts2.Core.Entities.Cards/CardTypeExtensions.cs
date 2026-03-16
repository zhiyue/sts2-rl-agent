using System;
using MegaCrit.Sts2.Core.Localization;

namespace MegaCrit.Sts2.Core.Entities.Cards;

public static class CardTypeExtensions
{
	public static LocString ToLocString(this CardType cardType)
	{
		return cardType switch
		{
			CardType.Attack => new LocString("gameplay_ui", "CARD_TYPE.ATTACK"), 
			CardType.Skill => new LocString("gameplay_ui", "CARD_TYPE.SKILL"), 
			CardType.Power => new LocString("gameplay_ui", "CARD_TYPE.POWER"), 
			CardType.Status => new LocString("gameplay_ui", "CARD_TYPE.STATUS"), 
			CardType.Curse => new LocString("gameplay_ui", "CARD_TYPE.CURSE"), 
			CardType.Quest => new LocString("gameplay_ui", "CARD_TYPE.QUEST"), 
			_ => throw new ArgumentOutOfRangeException("cardType", cardType, null), 
		};
	}
}
