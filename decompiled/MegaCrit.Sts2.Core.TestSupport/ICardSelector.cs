using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.CardRewardAlternatives;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.TestSupport;

/// <summary>
/// Interface for automated card selection, used by both test mode and AutoSlay.
/// </summary>
public interface ICardSelector
{
	/// <summary>
	/// Selects cards from the available options.
	/// </summary>
	/// <param name="options">Cards available for selection.</param>
	/// <param name="minSelect">Minimum number of cards to select.</param>
	/// <param name="maxSelect">Maximum number of cards to select.</param>
	Task<IEnumerable<CardModel>> GetSelectedCards(IEnumerable<CardModel> options, int minSelect, int maxSelect);

	/// <summary>
	/// Selects a card reward from the available options.
	/// </summary>
	CardRewardSelection GetSelectedCardReward(IReadOnlyList<CardCreationResult> options, IReadOnlyList<CardRewardAlternative> alternatives);
}
