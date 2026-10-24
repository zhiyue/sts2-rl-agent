using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Localization;

/// <summary>
/// A Localized String, uses:
/// https://github.com/axuno/SmartFormat
/// </summary>
public class LocString(string locTable, string locEntryKey) : IComparable<LocString>
{
	[JsonIgnore]
	private readonly Dictionary<string, object> _variables = new Dictionary<string, object>();

	[JsonPropertyName("table")]
	public string LocTable => locTable;

	[JsonPropertyName("key")]
	public string LocEntryKey => locEntryKey;

	[JsonIgnore]
	public bool IsEmpty
	{
		get
		{
			if (string.IsNullOrEmpty(LocEntryKey))
			{
				return string.IsNullOrEmpty(LocTable);
			}
			return false;
		}
	}

	[JsonIgnore]
	public IReadOnlyDictionary<string, object> Variables => _variables;

	private object this[string key]
	{
		get
		{
			return _variables[key];
		}
		set
		{
			_variables[key] = value;
		}
	}

	public static bool Exists(string table, string key)
	{
		return LocManager.Instance.GetTable(table).HasEntry(key);
	}

	public static LocString? GetIfExists(string table, string key)
	{
		if (!Exists(table, key))
		{
			return null;
		}
		return new LocString(table, key);
	}

	/// <summary>
	/// Returns a formatted string, ie:
	/// "Deal 6 damage."
	/// rather than:
	/// "Deal {Damage:diff()} damage."
	/// </summary>
	public string GetFormattedText()
	{
		return LocManager.Instance.SmartFormat(this, _variables);
	}

	/// <summary>
	/// Returns the raw text in the JSON file, ie:
	/// "Deal {Damage:diff()} damage."
	/// rather than:
	/// "Deal 6 damage."
	/// </summary>
	public string GetRawText()
	{
		return LocManager.Instance.GetTable(LocTable).GetRawText(LocEntryKey);
	}

	public bool Exists()
	{
		return Exists(LocTable, LocEntryKey);
	}

	public static bool IsNullOrWhitespace(LocString? locString)
	{
		if (locString != null && !locString.IsEmpty)
		{
			return string.IsNullOrWhiteSpace(locString.GetRawText());
		}
		return true;
	}

	public static void SubscribeToLocaleChange(LocManager.LocaleChangeCallback callback)
	{
		LocManager.Instance.SubscribeToLocaleChange(callback);
	}

	public static void UnsubscribeToLocaleChange(LocManager.LocaleChangeCallback callback)
	{
		LocManager.Instance.UnsubscribeToLocaleChange(callback);
	}

	public void Add(DynamicVar dynamicVar)
	{
		AddObj(dynamicVar.Name, dynamicVar);
	}

	public void Add(string name, decimal variable)
	{
		AddObj(name, variable);
	}

	public void Add(string name, bool variable)
	{
		AddObj(name, variable);
	}

	public void Add(string name, string variable)
	{
		AddObj(name, variable);
	}

	public void Add(string name, IList<string> variable)
	{
		AddObj(name, variable);
	}

	public void Add(string name, LocString variable)
	{
		AddObj(name, variable.GetFormattedText());
	}

	public void AddObj(string name, object variable)
	{
		ArgumentException.ThrowIfNullOrEmpty(name, "name");
		ArgumentNullException.ThrowIfNull(variable, "variable");
		name = name.Replace(' ', '-');
		if (!_variables.TryAdd(name, variable))
		{
			_variables[name] = variable;
		}
	}

	public static LocString KeyPathToLocString(string keyPath)
	{
		string[] array = keyPath.Split('/');
		return new LocString(array[0], array[1]);
	}

	public void AddVariablesFrom(LocString smartDescription)
	{
		foreach (string key in smartDescription._variables.Keys)
		{
			AddObj(key, smartDescription[key]);
		}
	}

	/// <summary>
	/// Helper function to grab a random LocString with a given key prefix.
	///
	/// Example:
	/// If there are multiple entries of MAP_POINT_HISTORY.abandon in run_history.json and you want to get a random
	/// LocString:
	///
	/// The JSON would be constructed like:
	/// "MAP_POINT_HISTORY.abandon.0": "{character} had simply given up.",
	/// "MAP_POINT_HISTORY.abandon.1": "{character} was tired...",
	/// "MAP_POINT_HISTORY.abandon.2": "{character} abandoned {pronounPossessive} destiny.",
	///
	/// You can grab a random entry via:
	/// LocString abandonMessage = LocString.GetRandomKey("run_history", "MAP_POINT_HISTORY.abandon");
	/// </summary>
	/// <param name="table">Name of the table to pull the keys from.</param>
	/// <param name="keyPrefix">Prefix for the keys we want.</param>
	/// <param name="rng">Optional rng. Chaotic will be used if not specified.</param>
	/// <returns>Random LocString with the specified key prefix.</returns>
	public static LocString GetRandomWithPrefix(string table, string keyPrefix, Rng? rng = null)
	{
		IReadOnlyList<LocString> locStringsWithPrefix = LocManager.Instance.GetTable(table).GetLocStringsWithPrefix(keyPrefix);
		if (rng == null)
		{
			rng = Rng.Chaotic;
		}
		return rng.NextItem(locStringsWithPrefix);
	}

	public int CompareTo(LocString? other)
	{
		if (locTable != other?.LocTable)
		{
			return string.Compare(LocTable, other?.LocTable, StringComparison.Ordinal);
		}
		return string.Compare(LocEntryKey, other.LocEntryKey, StringComparison.Ordinal);
	}

	public override string ToString()
	{
		return "LocString table " + LocTable + " entry " + locEntryKey;
	}
}
