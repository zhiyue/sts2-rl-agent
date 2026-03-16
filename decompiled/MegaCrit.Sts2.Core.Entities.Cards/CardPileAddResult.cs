using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Entities.Cards;

public struct CardPileAddResult
{
	public bool success;

	public CardModel cardAdded;

	public CardPile? oldPile;

	public List<AbstractModel>? modifyingModels;
}
