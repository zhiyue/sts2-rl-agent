using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO.Hashing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Timeline;

namespace MegaCrit.Sts2.Core.Multiplayer.Serialization;

public static class ModelIdSerializationCache
{
	/// For serializing over the network, to save space, we map the string names statically to integers...
	private static readonly Dictionary<string, int> _categoryNameToNetIdMap = new Dictionary<string, int> { [ModelId.none.Category] = 0 };

	/// ...and vice-versa.
	private static readonly List<string> _netIdToCategoryNameMap;

	private static readonly Dictionary<string, int> _entryNameToNetIdMap;

	private static readonly List<string> _netIdToEntryNameMap;

	private static readonly Dictionary<string, int> _epochNameToNetIdMap;

	private static readonly List<string> _netIdToEpochNameMap;

	public static int CategoryIdBitSize { get; private set; }

	public static int EntryIdBitSize { get; private set; }

	public static int EpochIdBitSize { get; private set; }

	public static int MaxCategoryId => _netIdToCategoryNameMap.Count;

	public static int MaxEntryId => _netIdToEntryNameMap.Count;

	public static int MaxEpochId => _netIdToEpochNameMap.Count;

	public static uint Hash { get; private set; }

	/// <summary>
	/// Initializes the serialization cache.
	/// Note that the alternative is to initialize in a static initializer, but if we do this we don't control the time
	/// at which it gets initialized, and it usually happens at a bad time during gameplay.
	/// </summary>
	public static void Init()
	{
		byte[] array = new byte[512];
		XxHash32 xxHash = new XxHash32();
		List<(Type, Mod)> list = new List<(Type, Mod)>();
		foreach (Type item5 in AbstractModelSubtypes.All)
		{
			list.Add((item5, null));
		}
		foreach (Mod mod in ModManager.Mods)
		{
			if (mod.state != ModLoadState.Loaded || !(mod.assembly != null))
			{
				continue;
			}
			foreach (Type item6 in ReflectionHelper.GetSubtypesFromAssembly(mod.assembly, typeof(AbstractModel)))
			{
				list.Add((item6, mod));
			}
		}
		list.Sort(delegate((Type, Mod?) p1, (Type, Mod?) p2)
		{
			(Type, Mod?) tuple = p1;
			Type item = tuple.Item1;
			Mod item2 = tuple.Item2;
			(Type, Mod?) tuple2 = p2;
			Type item3 = tuple2.Item1;
			Mod item4 = tuple2.Item2;
			int num = string.CompareOrdinal(item.Name, item3.Name);
			if (num != 0)
			{
				return num;
			}
			if (item2 != null && item4 == null)
			{
				return 1;
			}
			if (item2 == null && item4 != null)
			{
				return -1;
			}
			if (item2 == null && item4 == null)
			{
				return 0;
			}
			int num2 = string.CompareOrdinal(item2.manifest.id, item4.manifest.id);
			if (num2 == 0)
			{
				Log.Warn($"Two AbstractModels {item} and {item3} from mod {item2.manifest?.id} share an ID! This might break multiplayer.");
			}
			return num2;
		});
		List<Type> list2 = list.Select(((Type, Mod) p) => p.Item1).ToList();
		foreach (Type item7 in list2)
		{
			ModelId id = ModelDb.GetId(item7);
			if (!_categoryNameToNetIdMap.ContainsKey(id.Category))
			{
				_categoryNameToNetIdMap[id.Category] = _netIdToCategoryNameMap.Count;
				_netIdToCategoryNameMap.Add(id.Category);
			}
			if (!_entryNameToNetIdMap.ContainsKey(id.Entry))
			{
				_entryNameToNetIdMap[id.Entry] = _netIdToEntryNameMap.Count;
				_netIdToEntryNameMap.Add(id.Entry);
			}
			int bytes = Encoding.UTF8.GetBytes(id.Category, 0, id.Category.Length, array, 0);
			xxHash.Append(array.AsSpan(0, bytes));
			bytes = Encoding.UTF8.GetBytes(id.Entry, 0, id.Entry.Length, array, 0);
			xxHash.Append(array.AsSpan(0, bytes));
		}
		foreach (string allEpochId in EpochModel.AllEpochIds)
		{
			if (!_epochNameToNetIdMap.ContainsKey(allEpochId))
			{
				_epochNameToNetIdMap[allEpochId] = _netIdToEpochNameMap.Count;
				_netIdToEpochNameMap.Add(allEpochId);
			}
			int bytes2 = Encoding.UTF8.GetBytes(allEpochId, 0, allEpochId.Length, array, 0);
			xxHash.Append(array.AsSpan(0, bytes2));
		}
		CategoryIdBitSize = Mathf.CeilToInt(Math.Log2(_netIdToCategoryNameMap.Count));
		EntryIdBitSize = Mathf.CeilToInt(Math.Log2(_netIdToEntryNameMap.Count));
		EpochIdBitSize = Mathf.CeilToInt(Math.Log2(_netIdToEpochNameMap.Count));
		BinaryPrimitives.WriteInt32LittleEndian(array.AsSpan(), MaxCategoryId);
		xxHash.Append(array.AsSpan(0, 4));
		BinaryPrimitives.WriteInt32LittleEndian(array.AsSpan(), MaxEntryId);
		xxHash.Append(array.AsSpan(0, 4));
		BinaryPrimitives.WriteInt32LittleEndian(array.AsSpan(), MaxEpochId);
		xxHash.Append(array.AsSpan(0, 4));
		Hash = xxHash.GetCurrentHashAsUInt32();
		Log.Info($"ModelIdSerializationCache initialized. Categories: {MaxCategoryId} Entries: {MaxEntryId} Epochs: {MaxEpochId} Hash: {Hash}");
	}

	public static int GetNetIdForCategory(string category)
	{
		if (!_categoryNameToNetIdMap.TryGetValue(category, out var value))
		{
			throw new ArgumentException("ModelId category " + category + " could not be mapped to any net ID!");
		}
		return value;
	}

	public static bool TryGetNetIdForCategory(string category, out int netId)
	{
		return _categoryNameToNetIdMap.TryGetValue(category, out netId);
	}

	public static string GetCategoryForNetId(int netId)
	{
		if (netId < 0 || netId >= MaxCategoryId)
		{
			throw new ArgumentOutOfRangeException($"ModelId category ID {netId} is out of range! We have {_netIdToCategoryNameMap.Count} categories");
		}
		return _netIdToCategoryNameMap[netId];
	}

	public static int GetNetIdForEntry(string entry)
	{
		if (!_entryNameToNetIdMap.TryGetValue(entry, out var value))
		{
			throw new ArgumentException("ModelId entry " + entry + " could not be mapped to any net ID!");
		}
		return value;
	}

	public static bool TryGetNetIdForEntry(string entry, out int netId)
	{
		return _entryNameToNetIdMap.TryGetValue(entry, out netId);
	}

	public static string GetEntryForNetId(int netId)
	{
		if (netId < 0 || netId >= MaxEntryId)
		{
			throw new ArgumentOutOfRangeException($"ModelId entry ID {netId} is out of range! We have {_netIdToEntryNameMap.Count} entries");
		}
		return _netIdToEntryNameMap[netId];
	}

	public static int GetNetIdForEpochId(string epochId)
	{
		if (!_epochNameToNetIdMap.TryGetValue(epochId, out var value))
		{
			throw new ArgumentException("Epoch ID " + epochId + " could not be mapped to any net ID!");
		}
		return value;
	}

	public static string GetEpochIdForNetId(int netId)
	{
		if (netId < 0 || netId >= MaxEpochId)
		{
			throw new ArgumentOutOfRangeException($"Epoch ID {netId} is out of range! We have {_netIdToEpochNameMap.Count} entries");
		}
		return _netIdToEpochNameMap[netId];
	}

	public static string Dump()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("CATEGORIES");
		StringBuilder stringBuilder2;
		StringBuilder.AppendInterpolatedStringHandler handler;
		for (int i = 0; i < _netIdToCategoryNameMap.Count; i++)
		{
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder3 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(2, 2, stringBuilder2);
			handler.AppendFormatted(i.ToString().PadRight(3));
			handler.AppendLiteral(": ");
			handler.AppendFormatted(_netIdToCategoryNameMap[i]);
			stringBuilder3.AppendLine(ref handler);
		}
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("ENTRIES");
		for (int j = 0; j < _netIdToEntryNameMap.Count; j++)
		{
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder4 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(2, 2, stringBuilder2);
			handler.AppendFormatted(j.ToString().PadRight(3));
			handler.AppendLiteral(": ");
			handler.AppendFormatted(_netIdToEntryNameMap[j]);
			stringBuilder4.AppendLine(ref handler);
		}
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("EPOCHS");
		for (int k = 0; k < _netIdToEpochNameMap.Count; k++)
		{
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder5 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(2, 2, stringBuilder2);
			handler.AppendFormatted(k.ToString().PadRight(3));
			handler.AppendLiteral(": ");
			handler.AppendFormatted(_netIdToEpochNameMap[k]);
			stringBuilder5.AppendLine(ref handler);
		}
		stringBuilder.AppendLine();
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder6 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(6, 1, stringBuilder2);
		handler.AppendLiteral("Hash: ");
		handler.AppendFormatted(Hash);
		stringBuilder6.AppendLine(ref handler);
		return stringBuilder.ToString();
	}

	static ModelIdSerializationCache()
	{
		int num = 1;
		List<string> list = new List<string>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<string> span = CollectionsMarshal.AsSpan(list);
		int index = 0;
		span[index] = ModelId.none.Category;
		_netIdToCategoryNameMap = list;
		_entryNameToNetIdMap = new Dictionary<string, int> { [ModelId.none.Entry] = 0 };
		index = 1;
		List<string> list2 = new List<string>(index);
		CollectionsMarshal.SetCount(list2, index);
		span = CollectionsMarshal.AsSpan(list2);
		num = 0;
		span[num] = ModelId.none.Entry;
		_netIdToEntryNameMap = list2;
		_epochNameToNetIdMap = new Dictionary<string, int>();
		_netIdToEpochNameMap = new List<string>();
	}
}
