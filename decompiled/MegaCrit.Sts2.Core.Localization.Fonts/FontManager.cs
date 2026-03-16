using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Localization.Fonts.FontPathSets;

namespace MegaCrit.Sts2.Core.Localization.Fonts;

public static class FontManager
{
	private static readonly FontPathSet _russian = new RusFontPathSet();

	private static readonly IReadOnlyDictionary<string, FontPathSet> _languageFontPathSets = new Dictionary<string, FontPathSet>
	{
		["jpn"] = new JpnFontPathSet(),
		["kor"] = new KorFontPathSet(),
		["pol"] = _russian,
		["rus"] = _russian,
		["tha"] = new ThaFontPathSet(),
		["zhs"] = new ZhsFontPathSet()
	};

	private static readonly Dictionary<string, Dictionary<FontType, Font>> _localeFonts = new Dictionary<string, Dictionary<FontType, Font>>();

	public static bool NeedsFontSubstitution(string language)
	{
		return _languageFontPathSets.ContainsKey(language);
	}

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
