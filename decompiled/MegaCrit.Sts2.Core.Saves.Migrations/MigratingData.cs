using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Saves.Migrations;

public class MigratingData
{
	private readonly JsonObject _data;

	public object? this[string key]
	{
		get
		{
			if (!_data.TryGetPropertyValue(key, out JsonNode jsonNode))
			{
				return null;
			}
			return ConvertJsonNodeToObject(jsonNode);
		}
	}

	public MigratingData(JsonDocument document)
	{
		_data = document.Deserialize(JsonSerializationUtility.GetTypeInfo<JsonObject>());
	}

	public MigratingData(string json)
	{
		_data = (JsonObject)JsonNode.Parse(json);
	}

	public MigratingData(JsonObject jsonObject)
	{
		_data = jsonObject;
	}

	public T ToObject<T>() where T : new()
	{
		string json = _data.ToJsonString();
		T val = JsonSerializer.Deserialize(json, JsonSerializationUtility.GetTypeInfo<T>());
		if (val == null)
		{
			return new T();
		}
		return val;
	}

	public void Remove(string key)
	{
		if (_data.ContainsKey(key))
		{
			_data.Remove(key);
		}
	}

	public void Rename(string oldKey, string newKey)
	{
		if (_data.TryGetPropertyValue(oldKey, out JsonNode jsonNode))
		{
			_data[newKey] = jsonNode?.DeepClone();
			_data.Remove(oldKey);
			return;
		}
		throw new MigrationException("Cannot rename a key that doesn't exist. Key=" + oldKey);
	}

	public bool Has(string key)
	{
		return _data.ContainsKey(key);
	}

	private static object? ConvertJsonNodeToObject(JsonNode? node)
	{
		if (node != null)
		{
			if (!(node is JsonObject))
			{
				if (!(node is JsonArray))
				{
					if (node is JsonValue jsonValue)
					{
						JsonValue jsonValue2 = jsonValue;
						if (jsonValue2.TryGetValue<string>(out string value))
						{
							return value;
						}
						JsonValue jsonValue3 = jsonValue;
						if (jsonValue3.TryGetValue<int>(out var value2))
						{
							return value2;
						}
						JsonValue jsonValue4 = jsonValue;
						if (jsonValue4.TryGetValue<long>(out var value3))
						{
							return value3;
						}
						JsonValue jsonValue5 = jsonValue;
						if (jsonValue5.TryGetValue<float>(out var value4))
						{
							return value4;
						}
						JsonValue jsonValue6 = jsonValue;
						if (jsonValue6.TryGetValue<double>(out var value5))
						{
							return value5;
						}
						JsonValue jsonValue7 = jsonValue;
						if (jsonValue7.TryGetValue<bool>(out var value6))
						{
							return value6;
						}
						JsonValue jsonValue8 = jsonValue;
						return jsonValue8.ToString();
					}
					return null;
				}
				return node.Deserialize(JsonSerializationUtility.GetTypeInfo<List<JsonNode>>());
			}
			return node.Deserialize(JsonSerializationUtility.GetTypeInfo<Dictionary<string, object>>());
		}
		return null;
	}

	public T? GetAsOrNull<T>(string key) where T : struct
	{
		if (!Has(key))
		{
			return null;
		}
		return GetAs<T>(key);
	}

	public T GetAs<T>(string key)
	{
		if (!_data.TryGetPropertyValue(key, out JsonNode jsonNode) || jsonNode == null)
		{
			throw new MigrationException("Cannot get value of key=" + key);
		}
		try
		{
			Type typeFromHandle = typeof(T);
			if (typeFromHandle == typeof(MigratingData) && jsonNode is JsonObject jsonObject)
			{
				return (T)(object)new MigratingData(jsonObject);
			}
			if (typeFromHandle == typeof(List<MigratingData>) && jsonNode is JsonArray jsonArray)
			{
				List<MigratingData> list = new List<MigratingData>();
				foreach (JsonNode item in jsonArray)
				{
					if (item is JsonObject jsonObject2)
					{
						list.Add(new MigratingData(jsonObject2));
						continue;
					}
					throw new MigrationException($"Cannot convert array item to MigratingData: {item}");
				}
				return (T)(object)list;
			}
			if (typeFromHandle == typeof(ModelId))
			{
				string value = jsonNode.GetValue<string>();
				if (string.IsNullOrEmpty(value))
				{
					return (T)(object)ModelId.none;
				}
				return (T)(object)ModelId.Deserialize(value);
			}
			if (typeFromHandle == typeof(string))
			{
				return (T)(object)jsonNode.GetValue<string>();
			}
			if (typeFromHandle == typeof(int))
			{
				return (T)(object)jsonNode.GetValue<int>();
			}
			if (typeFromHandle == typeof(long))
			{
				return (T)(object)jsonNode.GetValue<long>();
			}
			if (typeFromHandle == typeof(double))
			{
				return (T)(object)jsonNode.GetValue<double>();
			}
			if (typeFromHandle == typeof(float))
			{
				return (T)(object)jsonNode.GetValue<float>();
			}
			if (typeFromHandle == typeof(bool))
			{
				return (T)(object)jsonNode.GetValue<bool>();
			}
			if (typeFromHandle == typeof(DateTime))
			{
				return (T)(object)jsonNode.GetValue<DateTime>();
			}
			T val = jsonNode.Deserialize(JsonSerializationUtility.GetTypeInfo<T>());
			if (val == null)
			{
				throw new MigrationException("Unable to convert " + key + " to " + typeof(T).Name);
			}
			return val;
		}
		catch (Exception ex) when (!(ex is MigrationException))
		{
			throw new MigrationException($"Cannot convert value of key={key} to {typeof(T)}: {ex.Message}");
		}
	}

	public string GetString(string key)
	{
		return GetAs<string>(key);
	}

	public bool GetBool(string key)
	{
		return GetAs<bool>(key);
	}

	public int GetInt(string key)
	{
		return GetAs<int>(key);
	}

	public MigratingData GetObject(string key)
	{
		return GetAs<MigratingData>(key);
	}

	public void Set<T>(string key, T value)
	{
		if (value is List<MigratingData> items)
		{
			SetList(key, items);
		}
		if (value is MigratingData)
		{
			throw new NotImplementedException();
		}
		_data[key] = ((value != null) ? JsonValue.Create(value, JsonSerializationUtility.GetTypeInfo<T>()) : null);
	}

	public void SetObject(string key, MigratingData value)
	{
		_data[key] = value._data.DeepClone();
	}

	public List<MigratingData> GetList(string key)
	{
		return GetAs<List<MigratingData>>(key);
	}

	public JsonNode? GetRawNode(string key)
	{
		if (!_data.TryGetPropertyValue(key, out JsonNode jsonNode))
		{
			return null;
		}
		return jsonNode;
	}

	public JsonObject GetRawNode()
	{
		return _data;
	}

	public void SetList<T>(string key, IEnumerable<T> items)
	{
		if (typeof(T) == typeof(MigratingData))
		{
			JsonArray jsonArray = new JsonArray();
			foreach (T item in items)
			{
				if (item is MigratingData migratingData)
				{
					jsonArray.Add(migratingData._data.DeepClone());
				}
			}
			_data[key] = jsonArray;
		}
		else
		{
			string json = JsonSerializer.Serialize(items, JsonSerializationUtility.GetTypeInfo<List<T>>());
			_data[key] = JsonNode.Parse(json);
		}
	}
}
