using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.CardRewardAlternatives;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.TestSupport;

public class TestCardSelector : ICardSelector
{
	public delegate CardRewardSelection CardRewardSelectionDelegate(IReadOnlyList<CardCreationResult> options, IReadOnlyList<CardRewardAlternative> alternatives);

	private readonly Queue<TaskCompletionSource<IEnumerable<CardModel>>> _cardsToSelectTaskQueue = new Queue<TaskCompletionSource<IEnumerable<CardModel>>>();

	private readonly Queue<TaskCompletionSource<IEnumerable<int>>> _indicesToSelectTaskQueue = new Queue<TaskCompletionSource<IEnumerable<int>>>();

	private CardRewardSelectionDelegate? _cardRewardSelectionDelegate;

	private bool _shouldBlock;

	private CancellationTokenSource? _blockCancelSource;

	public TaskCompletionSource? BlockedTcs { get; private set; }

	public void Cleanup()
	{
		_cardsToSelectTaskQueue.Clear();
		_indicesToSelectTaskQueue.Clear();
		_shouldBlock = false;
		BlockedTcs = null;
		_blockCancelSource?.Cancel();
		_blockCancelSource = null;
		_cardRewardSelectionDelegate = null;
	}

	public TaskCompletionSource<IEnumerable<CardModel>> SetupForAsyncCardSelection()
	{
		TaskCompletionSource<IEnumerable<CardModel>> taskCompletionSource = new TaskCompletionSource<IEnumerable<CardModel>>();
		_cardsToSelectTaskQueue.Enqueue(taskCompletionSource);
		return taskCompletionSource;
	}

	public TaskCompletionSource<IEnumerable<int>> SetupForAsyncIndexSelection()
	{
		TaskCompletionSource<IEnumerable<int>> taskCompletionSource = new TaskCompletionSource<IEnumerable<int>>();
		_indicesToSelectTaskQueue.Enqueue(taskCompletionSource);
		return taskCompletionSource;
	}

	public void PrepareToSelect(IEnumerable<CardModel> cards)
	{
		TaskCompletionSource<IEnumerable<CardModel>> taskCompletionSource = new TaskCompletionSource<IEnumerable<CardModel>>();
		taskCompletionSource.SetResult(cards);
		_cardsToSelectTaskQueue.Enqueue(taskCompletionSource);
	}

	public void PrepareToSelect(IEnumerable<int> indices)
	{
		TaskCompletionSource<IEnumerable<int>> taskCompletionSource = new TaskCompletionSource<IEnumerable<int>>();
		taskCompletionSource.SetResult(indices);
		_indicesToSelectTaskQueue.Enqueue(taskCompletionSource);
	}

	public void PrepareToSelectCardReward(CardRewardSelectionDelegate del)
	{
		_cardRewardSelectionDelegate = del;
	}

	public void PrepareToSelectCardRewardAtIndex(int index)
	{
		PrepareToSelectCardReward((IReadOnlyList<CardCreationResult> c, IReadOnlyList<CardRewardAlternative> _) => new CardRewardSelection
		{
			card = c[index].Card
		});
	}

	public void PrepareToSelectCardRewardAlternativeAtIndex(int index)
	{
		PrepareToSelectCardReward((IReadOnlyList<CardCreationResult> _, IReadOnlyList<CardRewardAlternative> a) => new CardRewardSelection
		{
			alternative = a[index]
		});
	}

	public CardRewardSelection GetSelectedCardReward(IReadOnlyList<CardCreationResult> options, IReadOnlyList<CardRewardAlternative> alternatives)
	{
		if (_cardRewardSelectionDelegate != null)
		{
			return _cardRewardSelectionDelegate(options, alternatives);
		}
		return new CardRewardSelection
		{
			card = options.FirstOrDefault()?.Card
		};
	}

	public Task PrepareToBlock()
	{
		_shouldBlock = true;
		BlockedTcs = new TaskCompletionSource();
		_blockCancelSource = new CancellationTokenSource();
		return BlockedTcs.Task;
	}

	public async Task<IEnumerable<CardModel>> GetSelectedCards(IEnumerable<CardModel> options, int minSelect, int maxSelect)
	{
		if (_shouldBlock)
		{
			BlockedTcs.SetResult();
			await Task.Delay(5000, _blockCancelSource.Token);
			throw new InvalidOperationException("Test told us to block, but it did not finish within 5 seconds!");
		}
		if (_cardsToSelectTaskQueue.Count > 0)
		{
			IEnumerable<CardModel> enumerable = await _cardsToSelectTaskQueue.Dequeue().Task;
			if (enumerable.Any((CardModel c) => !options.Contains(c)))
			{
				throw new InvalidOperationException("Selected card missing from options.");
			}
			return enumerable;
		}
		if (_indicesToSelectTaskQueue.Count > 0)
		{
			return (await _indicesToSelectTaskQueue.Dequeue().Task).Select(options.ElementAt);
		}
		return Array.Empty<CardModel>();
	}
}
