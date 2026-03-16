using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;

namespace MegaCrit.Sts2.Core.Models.Modifiers;

public class Hoarder : ModifierModel
{
	private readonly HashSet<CardModel> _cardsToSkip = new HashSet<CardModel>();

	public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? source)
	{
		if (oldPileType != PileType.None)
		{
			return;
		}
		CardPile? pile = card.Pile;
		if (pile != null && pile.Type == PileType.Deck && source == null && !_cardsToSkip.Remove(card))
		{
			for (int i = 0; i < 2; i++)
			{
				CardModel cardModel = card.Owner.RunState.CloneCard(card);
				_cardsToSkip.Add(cardModel);
				CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(cardModel, PileType.Deck, CardPilePosition.Bottom, this));
			}
		}
	}

	public override bool ShouldAllowMerchantCardRemoval(Player player)
	{
		return false;
	}
}
