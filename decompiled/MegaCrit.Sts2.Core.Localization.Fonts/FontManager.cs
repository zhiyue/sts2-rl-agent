using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Localization.Fonts.FontPathSets;

namespace MegaCrit.Sts2.Core.Localization.Fonts;

/// <summary>
/// Manages locale-specific font substitution for languages that require different fonts
/// (e.g., CJK languages that need fonts with appropriate glyph coverage).
/// </summary>
public static class FontManager
{
	private static readonly FontPathSet _russian = new RusFontPathSet();

	/// <summary>
	/// Languages that require font substitution.
	/// Key is language code, value is the set of font paths for that language.
	/// </summary>
	private static readonly IReadOnlyDictionary<string, FontPathSet> _languageFontPathSets = new Dictionary<string, FontPathSet>
	{
		["jpn"] = new JpnFontPathSet(),
		["kor"] = new KorFontPathSet(),
		["pol"] = _russian,
		["rus"] = _russian,
		["tha"] = new ThaFontPathSet(),
		["zhs"] = new ZhsFontPathSet()
	};

	/// <summary>
	/// Cached font resources, keyed by language code then font type.
	/// </summary>
	private static readonly Dictionary<string, Dictionary<FontType, Font>> _localeFonts = new Dictionary<string, Dictionary<FontType, Font>>();

	/// <summary>
	/// Returns true if the given language requires font substitution.
	/// </summary>
	public static bool NeedsFontSubstitution(string language)
	{
		return _languageFontPathSets.ContainsKey(language);
	}

	/// <summary>
	/// Gets the substitute font for the given type based on the current locale.
	/// Returns null if no substitution is needed.
	/// </summary>
	public static Font? GetSubstituteFont(string language, FontType type)
	{
		if (!NeedsFontSubstitution(language))
		{
			return null;
		}
		return GetFontForLanguage(language, type);
	}

	public static void ClearCache()
	{
		_localeFonts.Clear();
	}

	/// <summary>
	/// Gets the font for the specified language and type, loading and caching if necessary.
	/// </summary>
	private static Font? GetFontForLanguage(string language, FontType type)
	{
		if (_localeFonts.TryGetValue(language, out Dictionary<FontType, Font> value) && value.TryGetValue(type, out var value2))
		{
			return value2;
		}
		string path = _languageFontPathSets[language].GetPath(type);
		if (path == null)
		{
			return null;
		}
		Font font = ResourceLoader.Load<Font>(path, null, ResourceLoader.CacheMode.Reuse);
		if (!_localeFonts.TryGetValue(language, out Dictionary<FontType, Font> value3))
		{
			value3 = new Dictionary<FontType, Font>();
			_localeFonts[language] = value3;
		}
		value3[type] = font;
		return font;
	}
}
