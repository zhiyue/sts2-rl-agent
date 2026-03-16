using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Godot;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Saves.Runs;

public static class SavedPropertiesTypeCache
{
	private static readonly Dictionary<Type, List<PropertyInfo>> _cache;

	private static readonly Dictionary<string, int> _propertyNameToNetIdMap;

	private static readonly List<string> _netIdToPropertyNameMap;

	public static int NetIdBitSize { get; private set; }

	static SavedPropertiesTypeCache()
	{
		_cache = new Dictionary<Type, List<PropertyInfo>>();
		_propertyNameToNetIdMap = new Dictionary<string, int>();
		_netIdToPropertyNameMap = new List<string>();
		for (int i = 0; i < AbstractModelSubtypes.Count; i++)
		{
			CachePropertiesForType(AbstractModelSubtypes.Get(i));
		}
		NetIdBitSize = Mathf.CeilToInt(Math.Log2(_netIdToPropertyNameMap.Count));
	}

	private static void CachePropertiesForType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties)] Type type)
	{
		List<PropertyInfo> list = (from p in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			where p.GetCustomAttribute<SavedPropertyAttribute>() != null
			select p).ToList();
		list.Sort(CompareProperties);
		if (list.Count > 0)
		{
			_cache[type] = list;
		}
		foreach (PropertyInfo item in list)
		{
			if (!_propertyNameToNetIdMap.ContainsKey(item.Name))
			{
				_propertyNameToNetIdMap[item.Name] = _netIdToPropertyNameMap.Count;
				_netIdToPropertyNameMap.Add(item.Name);
			}
		}
	}

	private static int CompareProperties(PropertyInfo p1, PropertyInfo p2)
	{
		SavedPropertyAttribute customAttribute = p1.GetCustomAttribute<SavedPropertyAttribute>();
		SavedPropertyAttribute customAttribute2 = p2.GetCustomAttribute<SavedPropertyAttribute>();
		if (customAttribute.order != customAttribute2.order)
		{
			return customAttribute.order.CompareTo(customAttribute2.order);
		}
		return string.CompareOrdinal(p1.Name, p2.Name);
	}

	public static int GetNetIdForPropertyName(string propertyName)
	{
		if (!_propertyNameToNetIdMap.TryGetValue(propertyName, out var value))
		{
			throw new ArgumentException("SavedProperty name " + propertyName + " could not be mapped to any net ID!");
		}
		return value;
	}

	public static string GetPropertyNameForNetId(int netId)
	{
		if (netId < 0 || netId >= _netIdToPropertyNameMap.Count)
		{
			throw new ArgumentOutOfRangeException($"SavedProperty net ID {netId} is out of range! We have {_netIdToPropertyNameMap.Count} property names");
		}
		return _netIdToPropertyNameMap[netId];
	}

	public static List<PropertyInfo>? GetJsonPropertiesForType(Type t)
	{
		if (_cache.TryGetValue(t, out List<PropertyInfo> value))
		{
			return value;
		}
		return null;
	}

	public static void InjectTypeIntoCache([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties)] Type type)
	{
		CachePropertiesForType(type);
	}
}
