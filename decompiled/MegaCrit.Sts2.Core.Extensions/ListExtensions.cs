using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Extensions;

public static class ListExtensions
{
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
