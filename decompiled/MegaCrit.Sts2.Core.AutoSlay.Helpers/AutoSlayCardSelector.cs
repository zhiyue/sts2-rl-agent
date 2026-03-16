using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.CardRewardAlternatives;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.AutoSlay.Helpers;

public class AutoSlayCardSelector : ICardSelector
{
	private readonly Rng _random;

	public AutoSlayCardSelector(Rng random)
	{
		_random = random;
	}

	public Task<IEnumerable<CardModel>> GetSelectedCards(IEnumerable<CardModel> options, int minSelect, int maxSelect)
	{
		List<CardModel> list = options.ToList();
		if (list.Count == 0)
		{
			return Task.FromResult((IEnumerable<CardModel>)Array.Empty<CardModel>());
		}
		int num = Math.Min(maxSelect, list.Count);
		if (num < minSelect)
		{
			num = Math.Min(minSelect, list.Count);
		}
		_random.Shuffle(list);
		IEnumerable<CardModel> result = list.Take(num);
		AutoSlayLog.Info($"Auto-selected {num} card(s) for selection prompt");
		return Task.FromResult(result);
	}

	public CardModel? GetSelectedCardReward(IReadOnlyList<CardCreationResult> options, IReadOnlyList<CardRewardAlternative> alternatives)
	{
		if (options.Count == 0)
		{
			return null;
		}
		int index = _random.NextInt(options.Count);
		return options[index].Card;
	}
}
