using System;
using System.Collections.Generic;
using System.Linq;

namespace MegaCrit.Sts2.Core.Multiplayer.Serialization;

public static class MaxEnumValueCache
{
	private static readonly Dictionary<Type, int> _maxEnumValues = new Dictionary<Type, int>();

	public static int Get<T>() where T : struct, Enum
	{
		if (!_maxEnumValues.TryGetValue(typeof(T), out var value))
		{
			if (!typeof(int).IsAssignableFrom(Enum.GetUnderlyingType(typeof(T))))
			{
				throw new InvalidOperationException($"Trying to get max value for enum type {typeof(T)} that is not assignable to int!");
			}
			value = (from v in Enum.GetValues<T>()
				select Convert.ToInt32(v)).Max();
			_maxEnumValues[typeof(T)] = value;
		}
		return value;
	}
}
