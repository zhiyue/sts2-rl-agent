using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Extensions;

public static class IEnumerableExtensions
{
	public static IEnumerable<T> TakeRandom<T>(this IEnumerable<T> collection, int count, Rng rng)
	{
		return collection.ToList().UnstableShuffle(rng).Take(count);
	}
}
