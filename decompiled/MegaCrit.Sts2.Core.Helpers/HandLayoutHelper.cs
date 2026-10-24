using System.Collections.Generic;

namespace MegaCrit.Sts2.Core.Helpers;

public static class HandLayoutHelper
{
	/// <summary>
	/// Returns where <paramref name="target" /> belongs among <paramref name="presentCards" /> once both are ordered
	/// by <paramref name="pileOrder" />: the count of present cards (excluding the target itself) that precede the
	/// target in pile order.
	///
	/// Callers use this to turn a backend-pile position into a child index for a container that may be missing some
	/// of the pile's cards (selected, awaiting play, or being dragged). Each absent card that precedes the target
	/// inflates the raw pile index past the target's real slot; once enough precede it the index exceeds the live
	/// child count and Godot rejects the move with "Invalid new child index" (PRG-6847). The result cannot exceed
	/// the number of present cards, so when <paramref name="presentCards" /> is exactly the container's current
	/// children it is a valid child index by construction; correctness depends on the caller passing that set.
	/// </summary>
	/// <param name="pileOrder">The full backend pile order.</param>
	/// <param name="presentCards">The cards whose holders are currently in the container, in any order.</param>
	/// <param name="target">The card being (re)inserted.</param>
	/// <returns>The insert index, or -1 if the target is not in the pile (caller should skip the move).</returns>
	public static int GetInsertIndex<T>(IReadOnlyList<T> pileOrder, IEnumerable<T> presentCards, T target)
	{
		EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
		int num = IndexOf(pileOrder, target, equalityComparer);
		if (num < 0)
		{
			return -1;
		}
		int num2 = 0;
		foreach (T presentCard in presentCards)
		{
			if (!equalityComparer.Equals(presentCard, target))
			{
				int num3 = IndexOf(pileOrder, presentCard, equalityComparer);
				if (num3 >= 0 && num3 < num)
				{
					num2++;
				}
			}
		}
		return num2;
	}

	private static int IndexOf<T>(IReadOnlyList<T> list, T item, EqualityComparer<T> comparer)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (comparer.Equals(list[i], item))
			{
				return i;
			}
		}
		return -1;
	}
}
