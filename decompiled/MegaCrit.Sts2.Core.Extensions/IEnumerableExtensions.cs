using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Extensions;

public static class IEnumerableExtensions
{
	/// <summary>
	/// Gets a random selection of elements from an IEnumerable.
	/// </summary>
	/// <typeparam name="T">Type of elements in the IEnumerable.</typeparam>
	/// <param name="collection">IEnumerable to get random elements from.</param>
	/// <param name="count">Maximum number of elements that should be in the new IEnumerable.</param>
	/// <param name="rng">Random number generator to use for shuffling.</param>
	/// <returns>New IEnumerable with random elements.</returns>
	public static IEnumerable<T> TakeRandom<T>(this IEnumerable<T> collection, int count, Rng rng)
	{
		return collection.ToList().UnstableShuffle(rng).Take(count);
	}
}
