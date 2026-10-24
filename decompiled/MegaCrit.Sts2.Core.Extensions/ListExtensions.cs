using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Extensions;

public static class ListExtensions
{
	/// <summary>
	/// Adds a random Shuffle method to any List.
	/// https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle
	///
	/// This shuffle is "stable", meaning the result is independent of the initial order of the list.
	/// If two lists containing the same items in different orders are shuffled, the results will always be the
	/// same, so long as the passed rng is the same.
	/// </summary>
	/// <typeparam name="T">Type of elements in the list.</typeparam>
	/// <param name="list">List to shuffle.</param>
	/// <param name="rng">Random number generator to use for sorting</param>
	/// <returns>Self.</returns>
	public static List<T> StableShuffle<T>(this List<T> list, Rng rng) where T : IComparable<T>
	{
		List<T> list2 = list.ToList();
		list2.Sort();
		for (int i = 0; i < list.Count; i++)
		{
			list[i] = list2[i];
		}
		return list.UnstableShuffle(rng);
	}

	/// <summary>
	/// Adds a random Shuffle method to any List.
	/// https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle
	///
	/// This shuffle is "unstable", meaning the result is dependent on the initial order of the list.
	/// If two lists containing the same items in different orders are shuffled, the results may be different even
	/// if the passed rng is the same.
	/// </summary>
	/// <typeparam name="T">Type of elements in the list.</typeparam>
	/// <param name="list">List to shuffle.</param>
	/// <param name="rng">Random number generator to use for sorting</param>
	/// <returns>Self.</returns>
	public static List<T> UnstableShuffle<T>(this List<T> list, Rng rng)
	{
		int num = list.Count;
		while (num > 1)
		{
			num--;
			int num2 = rng.NextInt(num + 1);
			int index = num2;
			int index2 = num;
			T value = list[num];
			T value2 = list[num2];
			list[index] = value;
			list[index2] = value2;
		}
		return list;
	}

	/// <summary>
	/// Determines the index of a specific item in the IReadOnlyList.
	/// </summary>
	/// <param name="readOnlyList">The IReadOnlyList to locate the object in.</param>
	/// <param name="item">The object to locate in the IReadOnlyList.</param>
	/// <typeparam name="T">Type of elements in the list.</typeparam>
	/// <returns>The index of <paramref name="item" /> if found in the list; otherwise, -1.</returns>
	public static int IndexOf<T>(this IReadOnlyList<T> readOnlyList, T item)
	{
		if (readOnlyList is IList<T> list)
		{
			return list.IndexOf(item);
		}
		for (int i = 0; i < readOnlyList.Count; i++)
		{
			if (EqualityComparer<T>.Default.Equals(readOnlyList[i], item))
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	/// Finds the first index of an item that matches the specified predicate in the IReadOnlyList.
	/// </summary>
	/// <param name="readOnlyList">The IReadOnlyList to locate the object in.</param>
	/// <param name="match">The Predicate{T} delegate that defines the conditions of the element to search for.</param>
	/// <typeparam name="T">Type of elements in the list.</typeparam>
	/// <returns>
	/// The zero-based index of the first occurrence of an element that matches the conditions defined by match, if
	/// found; otherwise, -1.
	/// </returns>
	public static int FirstIndex<T>(this IReadOnlyList<T> readOnlyList, Predicate<T> match)
	{
		for (int i = 0; i < readOnlyList.Count; i++)
		{
			if (match(readOnlyList[i]))
			{
				return i;
			}
		}
		return -1;
	}
}
