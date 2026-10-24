using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Helpers;

/// <summary>
/// A generic collection that allows the caller to grab random elements with weighted probabilities.
/// </summary>
/// <example>
/// Imagine the following code:
///
/// <code>
/// GrabBag{string} bag = new GrabBag{string}();
///
/// bag.Add("dog", 1);
/// bag.Add("cat", 1);
/// bag.Add("fish", 2);
/// bag.add("lion", 0.5);
/// bag.add("tiger", 0.5);
///
/// string result = bag.Grab();
/// </code>
///
/// In the above code, 5 elements are added to a grab bag with differing weights, then a random one is chosen.
/// If the above code were run 100 times, the results should be:
/// * "dog" approximately 20 times
/// * "cat" approximately 20 times
/// * "fish" approximately 40 times (twice as many as dog or cat, since it has a weight of 2 instead of 1)
/// * "lion" approximately 10 times (half as many as dog or cat, since it has a weight of 0.5 instead of 1)
/// * "tiger" approximately 10 times (half as many as dog or cat, since it has a weight of 0.5 instead of 1)
/// </example>
/// <typeparam name="T">Type of elements contained by GrabBag.</typeparam>
public class GrabBag<T>
{
	/// <summary>
	///
	/// </summary>
	private readonly List<(T, double)> _entries = new List<(T, double)>();

	private double _totalWeight;

	public int Count => _entries.Count;

	/// <summary>
	/// Add an element with a specified weight.
	/// </summary>
	/// <param name="element">Element to add.</param>
	/// <param name="weight">Amount to weight the element.</param>
	public void Add(T element, double weight)
	{
		_entries.Add((element, weight));
		_totalWeight += weight;
	}

	/// <summary>
	/// Grab a random element from the bag, respecting weights.
	/// </summary>
	/// <param name="rng">Generator to grab the random roll from.</param>
	/// <param name="predicate">Predicate that must be matched by the returned element.</param>
	/// <returns>Randomly-chosen element. Null if the bag is empty.</returns>
	public T? Grab(Rng rng, Func<T, bool>? predicate = null)
	{
		int num = GrabIndex(rng, predicate);
		if (num < 0)
		{
			return default(T);
		}
		return _entries[num].Item1;
	}

	/// <summary>
	/// Grab a random element from the bag, respecting weights. Remove it after grabbing.
	/// </summary>
	/// <param name="rng">Generator to grab the random roll from.</param>
	/// <param name="predicate">Predicate that must be matched by the returned element.</param>
	/// <returns>Randomly-chosen element. Null if the bag is empty.</returns>
	public T? GrabAndRemove(Rng rng, Func<T, bool>? predicate = null)
	{
		int num = GrabIndex(rng, predicate);
		if (num < 0)
		{
			return default(T);
		}
		T item = _entries[num].Item1;
		Remove(num);
		return item;
	}

	public bool Any()
	{
		return _entries.Any();
	}

	private int GrabIndex(Rng rng, Func<T, bool>? predicate)
	{
		if (predicate != null && !_entries.Any(((T, double) e) => predicate(e.Item1)))
		{
			return -1;
		}
		int num;
		do
		{
			num = GrabIndex(rng);
		}
		while (predicate != null && num >= 0 && !predicate(_entries[num].Item1));
		return num;
	}

	private int GrabIndex(Rng rng)
	{
		double num = rng.NextDouble() * _totalWeight;
		double num2 = 0.0;
		for (int i = 0; i < _entries.Count; i++)
		{
			num2 += _entries[i].Item2;
			if (num < num2)
			{
				return i;
			}
		}
		return -1;
	}

	private void Remove(int index)
	{
		_totalWeight -= _entries[index].Item2;
		_entries.RemoveAt(index);
	}
}
