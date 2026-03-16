using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.CardRewardAlternatives;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.TestSupport;

public class TestCardSelector : ICardSelector
{
	public delegate CardModel? CardRewardSelectionDelegate(IReadOnlyList<CardCreationResult> options, IReadOnlyList<CardRewardAlternative> alternatives);

	private IEnumerable<CardModel>? _cardsToSelect;

	private IEnumerable<int>? _indicesToSelect;

	private CardRewardSelectionDelegate? _cardRewardSelectionDelegate;

	private bool _shouldBlock;

	public void Cleanup()
	{
		_cardsToSelect = null;
		_indicesToSelect = null;
		_shouldBlock = false;
		_cardRewardSelectionDelegate = null;
	}

	public void PrepareToSelect(IEnumerable<CardModel> cards)
	{
		_cardsToSelect = cards;
	}

	public void PrepareToSelect(IEnumerable<int> indices)
	{
		_indicesToSelect = indices;
	}

	public void PrepareToSelectCardReward(CardRewardSelectionDelegate del)
	{
		_cardRewardSelectionDelegate = del;
	}

	public CardModel? GetSelectedCardReward(IReadOnlyList<CardCreationResult> options, IReadOnlyList<CardRewardAlternative> alternatives)
	{
		if (_cardRewardSelectionDelegate != null)
		{
			return _cardRewardSelectionDelegate?.Invoke(options, alternatives);
		}
		return options.FirstOrDefault()?.Card;
	}

	public void PrepareToBlock()
	{
		_shouldBlock = true;
	}

	public async Task<IEnumerable<CardModel>> GetSelectedCards(IEnumerable<CardModel> options, int minSelect, int maxSelect)
	{
		if (_shouldBlock)
		{
			await Task.Delay(5000);
			throw new InvalidOperationException("Test told us to block, but it did not finish within 5 seconds!");
		}
		if (_cardsToSelect != null)
		{
			if (_cardsToSelect.Any((CardModel c) => !options.Contains(c)))
			{
				throw new InvalidOperationException("Selected card missing from options.");
			}
			return _cardsToSelect;
		}
		if (_indicesToSelect != null)
		{
			return _indicesToSelect.Select(options.ElementAt);
		}
		return Array.Empty<CardModel>();
	}
}
