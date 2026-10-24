using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.CardRewardAlternatives;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Models.Relics;

/// <summary>
/// Card selector used by Vakuu (via WhisperingEarring) during auto-play.
/// Selects cards in row-major order (top-left to bottom-right).
/// </summary>
public class VakuuCardSelector : ICardSelector
{
	public Task<IEnumerable<CardModel>> GetSelectedCards(IEnumerable<CardModel> options, int minSelect, int maxSelect)
	{
		return Task.FromResult((IEnumerable<CardModel>)options.Take(maxSelect).ToList());
	}

	public CardRewardSelection GetSelectedCardReward(IReadOnlyList<CardCreationResult> options, IReadOnlyList<CardRewardAlternative> alternatives)
	{
		return new CardRewardSelection
		{
			card = options.FirstOrDefault()?.Card
		};
	}
}
