using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Helpers;

namespace MegaCrit.Sts2.Core.Random;

public class Rng
{
	private readonly System.Random _random;

	public static Rng Chaotic { get; } = new Rng((uint)DateTimeOffset.Now.ToUnixTimeSeconds());

	public int Counter { get; private set; }

	public uint Seed { get; }

	public Rng(uint seed = 0u, int counter = 0)
	{
		Counter = 0;
		Seed = seed;
		_random = new System.Random((int)seed);
		FastForwardCounter(counter);
	}

	public Rng(uint seed, string name)
		: this(seed + (uint)StringHelper.GetDeterministicHashCode(name))
	{
	}

	public void FastForwardCounter(int targetCount)
	{
		if (Counter > targetCount)
		{
			throw new InvalidOperationException($"Cannot fast-forward an Rng counter to a lower number (current = {Counter}, target = {targetCount})");
		}
		while (Counter < targetCount)
		{
			Counter++;
			_random.Next();
		}
	}

	public bool NextBool()
	{
		Counter++;
		return _random.Next(2) == 0;
	}

	public int NextInt(int maxExclusive = int.MaxValue)
	{
		Counter++;
		return _random.Next(maxExclusive);
	}

	public int NextInt(int minInclusive, int maxExclusive)
	{
		if (minInclusive >= maxExclusive)
		{
			throw new ArgumentOutOfRangeException("minInclusive", "Minimum must be lower than maximum.");
		}
		Counter++;
		return _random.Next(minInclusive, maxExclusive);
	}

	public uint NextUnsignedInt(uint maxExclusive = uint.MaxValue)
	{
		return NextUnsignedInt(0u, maxExclusive);
	}

	public uint NextUnsignedInt(uint minInclusive, uint maxExclusive)
	{
		if (minInclusive >= maxExclusive)
		{
			throw new ArgumentOutOfRangeException("minInclusive", "Minimum must be lower than maximum.");
		}
		Counter++;
		double num = _random.NextDouble();
		double num2 = maxExclusive - minInclusive;
		uint num3 = (uint)(num * num2);
		return minInclusive + num3;
	}

	public float NextFloat(float max = 1f)
	{
		return NextFloat(0f, max);
	}

	public float NextFloat(float min, float max)
	{
		if (min > max)
		{
			throw new ArgumentOutOfRangeException("min", "Minimum must not be higher than maximum.");
		}
		Counter++;
		return (float)(_random.NextDouble() * (double)(max - min) + (double)min);
	}

	public double NextDouble()
	{
		Counter++;
		return _random.NextDouble();
	}

	public double NextDouble(double min, double max)
	{
		if (min > max)
		{
			throw new ArgumentOutOfRangeException("min", "Minimum must not be higher than maximum.");
		}
		Counter++;
		return _random.NextDouble() * (max - min) + min;
	}

	public float NextGaussianFloat(float mean = 0f, float stdDev = 1f, float min = 0f, float max = 1f)
	{
		return (float)NextGaussianDouble(mean, stdDev, min, max);
	}

	public double NextGaussianDouble(double mean = 0.0, double stdDev = 1.0, double min = 0.0, double max = 1.0)
	{
		if (min > max)
		{
			throw new ArgumentOutOfRangeException("min", "Minimum must not be higher than maximum.");
		}
		Counter++;
		double num4;
		do
		{
			double d = _random.NextDouble();
			double num = _random.NextDouble();
			double num2 = Math.Sqrt(-2.0 * Math.Log(d));
			double d2 = Math.PI * 2.0 * num;
			double num3 = num2 * Math.Cos(d2);
			num4 = mean + num3 * stdDev;
		}
		while ((num4 < 0.0 || num4 > 1.0) ? true : false);
		return num4 * (max - min) + min;
	}

	public int NextGaussianInt(int mean, int stdDev, int min, int max)
	{
		int num3;
		do
		{
			double d = 1.0 - _random.NextDouble();
			double num = 1.0 - _random.NextDouble();
			double num2 = Math.Sqrt(-2.0 * Math.Log(d)) * Math.Sin(Math.PI * 2.0 * num);
			double a = (double)mean + (double)stdDev * num2;
			num3 = (int)Math.Round(a);
		}
		while (num3 < min || num3 > max);
		return num3;
	}

	public T? NextItem<T>(IEnumerable<T> items)
	{
		IEnumerable<T> source = (items as T[]) ?? items.ToArray();
		int num = source.Count();
		if (num == 0)
		{
			return default(T);
		}
		int index = NextInt(0, num);
		return source.ElementAt(index);
	}

	public T? WeightedNextItem<T>(IEnumerable<T> items, Func<T?, float> weightFetcher)
	{
		return WeightedNextItem<T>(NextFloat(), items, weightFetcher, default(T));
	}

	public static T WeightedNextItem<T>(float randInput, IEnumerable<T> items, Func<T, float> weightFetcher, T fallback)
	{
		float num = items.Sum(weightFetcher);
		float num2 = randInput * num;
		foreach (T item in items)
		{
			num2 -= weightFetcher(item);
			if (num2 <= 0f)
			{
				return item;
			}
		}
		return fallback;
	}

	public void Shuffle<T>(IList<T> list)
	{
		for (int num = list.Count - 1; num > 0; num--)
		{
			int num2 = NextInt(num + 1);
			int index = num;
			int index2 = num2;
			T value = list[num2];
			T value2 = list[num];
			list[index] = value;
			list[index2] = value2;
		}
	}
}
