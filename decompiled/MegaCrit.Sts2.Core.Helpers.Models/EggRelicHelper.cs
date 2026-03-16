using System.Collections.Generic;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Helpers.Models;

public static class EggRelicHelper
{
	public static void UpgradeValidCards(List<CardCreationResult> cards, CardType cardType, RelicModel eggRelic)
	{
		foreach (CardCreationResult card3 in cards)
		{
			CardModel card = card3.Card;
			if (card.Type == cardType && card.IsUpgradable)
			{
				CardModel card2 = eggRelic.Owner.RunState.CloneCard(card);
				CardCmd.Upgrade(card2);
				card3.ModifyCard(card2, eggRelic);
			}
		}
	}
}
