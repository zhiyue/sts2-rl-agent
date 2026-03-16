using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Entities.Cards;

public class CardCreationResult
{
	public readonly CardModel originalCard;

	private CardModel? _modifiedCard;

	private readonly List<RelicModel> _modifyingRelics = new List<RelicModel>();

	public CardModel Card => _modifiedCard ?? originalCard;

	public IEnumerable<RelicModel> ModifyingRelics => _modifyingRelics;

	public bool HasBeenModified => _modifiedCard != null;

	public CardCreationResult(CardModel originalCard)
	{
		this.originalCard = originalCard;
	}

	public void ModifyCard(CardModel card, RelicModel modifyingRelic)
	{
		_modifiedCard = card;
		_modifyingRelics.Add(modifyingRelic);
	}

	public void ModifyCard(CardModel card)
	{
		_modifiedCard = card;
	}
}
