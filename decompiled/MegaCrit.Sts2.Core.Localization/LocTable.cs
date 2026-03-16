using System.Collections.Generic;
using System.Linq;

namespace MegaCrit.Sts2.Core.Localization;

public class LocTable
{
	private readonly string _name;

	private readonly Dictionary<string, string> _translations;

	private readonly LocTable? _fallback;

	public IEnumerable<string> Keys => _translations.Keys;

	public LocTable(string name, Dictionary<string, string> data, LocTable? fallback = null)
	{
		_name = name;
		_translations = data;
		_fallback = fallback;
	}

	public void MergeWith(Dictionary<string, string> otherTable)
	{
		foreach (KeyValuePair<string, string> item in otherTable)
		{
			_translations[item.Key] = item.Value;
		}
	}

	public LocString GetLocString(string key)
	{
		if (!_translations.ContainsKey(key))
		{
			LocTable? fallback = _fallback;
			if (fallback == null || !fallback.HasEntry(key))
			{
				throw new LocException("Key=" + key + " not found in table=" + _name);
			}
		}
		return new LocString(_name, key);
	}

	public string GetRawText(string key)
	{
		if (_translations.TryGetValue(key, out string value))
		{
			return value;
		}
		if (_fallback != null)
		{
			return _fallback.GetRawText(key);
		}
		throw new LocException("Key=" + key + " not found in table=" + _name);
	}

	public IReadOnlyList<LocString> GetLocStringsWithPrefix(string keyPrefix)
	{
		HashSet<string> hashSet = new HashSet<string>(_translations.Keys.Where((string k) => k.StartsWith(keyPrefix)));
		if (_fallback != null)
		{
			foreach (string item in _fallback.Keys.Where((string k) => k.StartsWith(keyPrefix)))
			{
				hashSet.Add(item);
			}
		}
		return hashSet.Select((string k) => new LocString(_name, k)).ToList();
	}

	public bool IsLocalKey(string key)
	{
		return _translations.ContainsKey(key);
	}

	public bool HasEntry(string key)
	{
		if (!_translations.ContainsKey(key))
		{
			return _fallback?.HasEntry(key) ?? false;
		}
		return true;
	}
}
