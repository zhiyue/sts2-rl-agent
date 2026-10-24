using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Saves.Migrations;

/// <summary>
/// A mutable JSON representation of the data being migrated. It serves as a loosely typed mutable blob we can
/// deserialize into and modify with our migration classes until finally parsing into the latest structured save schema.
/// This implementation uses System.Text.Json's JsonNode instead of a custom implementation.
/// </summary>
public class MigratingData
{
	private readonly JsonObject _data;

	/// <summary>
	/// Gets a property in the JSON object.
	/// </summary>
	/// <param name="key">The property name</param>
	/// <returns>The property value</returns>
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

	/// <summary>
	/// Creates a JSON object from a JsonDocument.
	/// </summary>
	/// <param name="document">The JsonDocument to convert</param>
	public MigratingData(JsonDocument document)
	{
		_data = document.Deserialize(JsonSerializationUtility.GetTypeInfo<JsonObject>());
	}

	/// <summary>
	/// Creates a JSON object from a JSON string.
	/// </summary>
	/// <param name="json">The JSON string</param>
	public MigratingData(string json)
	{
		_data = (JsonObject)JsonNode.Parse(json);
	}

	/// <summary>
	/// Creates a JSON object from a JsonObject.
	/// </summary>
	/// <param name="jsonObject">The JsonObject</param>
	public MigratingData(JsonObject jsonObject)
	{
		_data = jsonObject;
	}

	/// <summary>
	/// Parses this JsonObject to a strongly typed object.
	/// </summary>
	/// <typeparam name="T">The type to convert to</typeparam>
	/// <returns>An instance of the specified type</returns>
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

	/// <summary>
	/// Removes a property from the JSON object.
	/// </summary>
	/// <param name="key">The property name to remove</param>
	public void Remove(string key)
	{
		if (_data.ContainsKey(key))
		{
			_data.Remove(key);
		}
	}

	/// <summary>
	/// Renames a property in the JSON object.
	/// </summary>
	/// <param name="oldKey">The current property name</param>
	/// <param name="newKey">The new property name</param>
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

	/// <summary>
	/// Checks if the JSON object has a property.
	/// </summary>
	/// <param name="key">The property name</param>
	/// <returns>True if the property exists</returns>
	public bool Has(string key)
	{
		return _data.ContainsKey(key);
	}

	/// <summary>
	/// Helper method to convert JsonNode to .NET objects
	/// </summary>
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

	/// <summary>
	/// Gets a property as a specific type.
	/// </summary>
	/// <typeparam name="T">The type to convert to</typeparam>
	/// <param name="key">The property name</param>
	/// <returns>The property value as the specified type</returns>
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

	/// <summary>
	/// Gets a string value from the JSON object.
	/// </summary>
	/// <param name="key">The property name</param>
	/// <returns>The string value</returns>
	public string GetString(string key)
	{
		return GetAs<string>(key);
	}

	/// <summary>
	/// Gets a boolean value from the JSON object.
	/// </summary>
	/// <param name="key">The property name</param>
	/// <returns>The boolean value</returns>
	public bool GetBool(string key)
	{
		return GetAs<bool>(key);
	}

	/// <summary>
	/// Gets an integer value from the JSON object.
	/// </summary>
	/// <param name="key">The property name</param>
	/// <returns>The integer value</returns>
	public int GetInt(string key)
	{
		return GetAs<int>(key);
	}

	/// <summary>
	/// Gets a nested JSON object from the JSON object.
	/// </summary>
	/// <param name="key">The property name</param>
	/// <returns>The nested JSON object</returns>
	public MigratingData GetObject(string key)
	{
		return GetAs<MigratingData>(key);
	}

	/// <summary>
	/// Sets a value in the JSON object.
	/// </summary>
	/// <param name="key">The key to set.</param>
	/// <param name="value">The value to set.</param>
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

	/// <summary>
	/// Sets a nested object property in the JSON object.
	/// </summary>
	/// <param name="key">The property name</param>
	/// <param name="value">The MigratingData object to set</param>
	public void SetObject(string key, MigratingData value)
	{
		_data[key] = value._data.DeepClone();
	}

	/// <summary>
	/// Gets a list of objects from the JSON array.
	/// </summary>
	/// <param name="key">The property name</param>
	/// <returns>The list of objects</returns>
	public List<MigratingData> GetList(string key)
	{
		return GetAs<List<MigratingData>>(key);
	}

	/// <summary>
	/// Gets the raw JsonNode for a property, useful for direct manipulation in migrations.
	/// </summary>
	/// <param name="key">The property name</param>
	/// <returns>The JsonNode or null if not found</returns>
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

	/// <summary>
	/// Sets an array property in the JSON object.
	/// </summary>
	/// <typeparam name="T">The type of items in the array</typeparam>
	/// <param name="key">The property name</param>
	/// <param name="items">The collection of items to set</param>
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
