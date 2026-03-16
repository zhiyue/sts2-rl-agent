using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Helpers;

public class GrabBag<T>
{
	private readonly List<(T, double)> _entries = new List<(T, double)>();

	private double _totalWeight;

	public int Count => _entries.Count;

	public void Add(T element, double weight)
	{
		_entries.Add((element, weight));
		_totalWeight += weight;
	}

	public T? Grab(Rng rng, Func<T, bool>? predicate = null)
	{
		int num = GrabIndex(rng, predicate);
		if (num < 0)
		{
			return default(T);
		}
		return _entries[num].Item1;
	}

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
