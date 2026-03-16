using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using Godot;
using MegaCrit.Sts2.Core.Localization.Formatters;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.TestSupport;
using Sentry;
using SmartFormat;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Parsing;
using SmartFormat.Extensions;

namespace MegaCrit.Sts2.Core.Localization;

public class LocManager
{
	public delegate void LocaleChangeCallback();

	private Dictionary<string, LocTable> _tables = new Dictionary<string, LocTable>();

	private static SmartFormatter _smartFormatter = null;

	private const string _weblateProjectSlug = "slaythespire2";

	private static readonly Dictionary<string, string> _weblateToGameLanguage = new Dictionary<string, string>
	{
		{ "de", "deu" },
		{ "es", "spa" },
		{ "es_LATAM", "esp" },
		{ "fr", "fra" },
		{ "it", "ita" },
		{ "ja", "jpn" },
		{ "ko", "kor" },
		{ "pl", "pol" },
		{ "pt_BR", "ptb" },
		{ "ru", "rus" },
		{ "th", "tha" },
		{ "tr", "tur" },
		{ "zh_Hans", "zhs" }
	};

	private static readonly Dictionary<string, string> _gameToWeblateLanguage = _weblateToGameLanguage.ToDictionary<KeyValuePair<string, string>, string, string>((KeyValuePair<string, string> kvp) => kvp.Value, (KeyValuePair<string, string> kvp) => kvp.Key);

	private Dictionary<string, int> _languageKeyCount = new Dictionary<string, int>();

	public const string locOverrideDir = "user://localization_override";

	private readonly List<LocaleChangeCallback> _localeChangeCallbacks = new List<LocaleChangeCallback>();

	private static readonly CultureInfo EnglishCultureInfo;

	public static LocManager Instance { get; private set; } = null;

	private static string LocalizationAssetDir => "res://localization";

	public bool OverridesActive { get; private set; }

	public IReadOnlyList<LocValidationError> ValidationErrors { get; private set; } = Array.Empty<LocValidationError>();

	public string Language { get; private set; }

	public static List<string> Languages { get; }

	public CultureInfo CultureInfo { get; private set; }

	public static void Initialize()
	{
		Instance = new LocManager();
		string path = ProjectSettings.GlobalizePath("user://localization_override");
		if (!DirAccess.DirExistsAbsolute(path))
		{
			DirAccess.MakeDirAbsolute(path);
		}
	}

	public LocManager()
	{
		string text = SaveManager.Instance.SettingsSave.Language;
		if (string.IsNullOrEmpty(text))
		{
			string rawLanguage = PlatformUtil.GetRawLanguage();
			text = PlatformUtil.GetThreeLetterLanguageCode();
			if (text == null || !Languages.Contains(text))
			{
				Log.Warn($"Could not initialize language from platform language: {rawLanguage} (resolved: {text}). Defaulting to english");
				text = "eng";
			}
			else
			{
				Log.Info("Initializing language for the first time from platform locale: " + rawLanguage + " -> " + text);
			}
			SaveManager.Instance.SettingsSave.Language = text;
		}
		else if (!Languages.Contains(text))
		{
			Log.Warn("Saved language '" + text + "' is not supported. Defaulting to english");
			text = "eng";
			SaveManager.Instance.SettingsSave.Language = text;
		}
		SetLanguage(text);
		LoadLocFormatters();
		LoadLocCompletionFile();
	}

	private CultureInfo CultureInfoFromThreeLetterCode(string language)
	{
		try
		{
			CultureInfo cultureInfo = System.Globalization.CultureInfo.GetCultures(CultureTypes.NeutralCultures).FirstOrDefault((CultureInfo c) => c.ThreeLetterISOLanguageName == language);
			if (cultureInfo != null)
			{
				return cultureInfo;
			}
		}
		catch (CultureNotFoundException value)
		{
			Log.Error($"Couldn't enumerate cultures: {value}");
		}
		if (language == "zhs")
		{
			return System.Globalization.CultureInfo.GetCultureInfo("zh-hans");
		}
		if (language == "zht")
		{
			return System.Globalization.CultureInfo.GetCultureInfo("zh-hant");
		}
		if (language == "ptb")
		{
			return System.Globalization.CultureInfo.GetCultureInfo("pt-br");
		}
		if (language == "esp")
		{
			return System.Globalization.CultureInfo.GetCultureInfo("es-419");
		}
		if (language == "spa")
		{
			return System.Globalization.CultureInfo.GetCultureInfo("es-ES");
		}
		if (language == "deu")
		{
			return System.Globalization.CultureInfo.GetCultureInfo("de");
		}
		if (language == "fra")
		{
			return System.Globalization.CultureInfo.GetCultureInfo("fr");
		}
		if (language == "ita")
		{
			return System.Globalization.CultureInfo.GetCultureInfo("it");
		}
		if (language == "jpn")
		{
			return System.Globalization.CultureInfo.GetCultureInfo("ja");
		}
		if (language == "kor")
		{
			return System.Globalization.CultureInfo.GetCultureInfo("ko");
		}
		if (language == "pol")
		{
			return System.Globalization.CultureInfo.GetCultureInfo("pl");
		}
		if (language == "rus")
		{
			return System.Globalization.CultureInfo.GetCultureInfo("ru");
		}
		if (language == "tha")
		{
			return System.Globalization.CultureInfo.GetCultureInfo("th");
		}
		if (language == "tur")
		{
			return System.Globalization.CultureInfo.GetCultureInfo("tr");
		}
		string text = "Language code " + language + " could not be mapped to CultureInfo! Add a new manual mapping";
		Log.Error(text);
		SentrySdk.CaptureMessage(text);
		return System.Globalization.CultureInfo.GetCultureInfo("en-us");
	}

	private void LoadLocFormatters()
	{
		_smartFormatter = new SmartFormatter();
		ListFormatter listFormatter = new ListFormatter();
		_smartFormatter.AddExtensions(listFormatter, new DictionarySource(), new ValueTupleSource(), new ReflectionSource(), new DefaultSource());
		_smartFormatter.AddExtensions(listFormatter, new PluralLocalizationFormatter(), new ConditionalFormatter(), new ChooseFormatter(), new SubStringFormatter(), new IsMatchFormatter(), new DefaultFormatter(), new AbsoluteValueFormatter(), new EnergyIconsFormatter(), new StarIconsFormatter(), new HighlightDifferencesFormatter(), new HighlightDifferencesInverseFormatter(), new PercentMoreFormatter(), new PercentLessFormatter(), new ShowIfUpgradedFormatter());
		Smart.Default = _smartFormatter;
	}

	private void LoadLocCompletionFile()
	{
		using Godot.FileAccess fileAccess = Godot.FileAccess.Open("localization/completion.json", Godot.FileAccess.ModeFlags.Read);
		if (fileAccess == null)
		{
			throw new LocException("Cannot find language completion file: localization/completion.json");
		}
		string asText = fileAccess.GetAsText();
		_languageKeyCount = JsonSerializer.Deserialize(asText, LocManagerSerializerContext.Default.DictionaryStringInt32);
	}

	public float GetLanguageCompletion(string language)
	{
		if (!_languageKeyCount.TryGetValue(language, out var value))
		{
			value = 0;
		}
		return (float)value / (float)_languageKeyCount["eng"];
	}

	public string SmartFormat(LocString locString, Dictionary<string, object> variables)
	{
		string rawText = locString.GetRawText();
		LocTable table = GetTable(locString.LocTable);
		CultureInfo provider = (table.IsLocalKey(locString.LocEntryKey) ? CultureInfo : EnglishCultureInfo);
		try
		{
			return _smartFormatter.Format(provider, rawText, variables);
		}
		catch (Exception ex) when (((ex is FormattingException || ex is ParsingErrors) ? 1 : 0) != 0)
		{
			string text = $"message={ex.Message}\ntable={locString.LocTable} key={locString.LocEntryKey} variables={ToString(variables)}";
			Log.Error("Localization formatting error! " + text);
			string errorPattern = Regex.Replace(ex.Message.Split('\n')[0], " at \\d+$", "");
			SentrySdk.CaptureException(new LocException(text), delegate(Scope scope)
			{
				scope.SetFingerprint("LocException", errorPattern);
			});
			return rawText;
		}
	}

	private static string ConvertToW(string input)
	{
		int num = 0;
		int num2 = 0;
		char[] array = new char[input.Length];
		for (int i = 0; i < input.Length; i++)
		{
			char c = input[i];
			switch (c)
			{
			case '{':
				num++;
				break;
			case '[':
				num2++;
				break;
			case '}':
				num--;
				break;
			case ']':
				num2--;
				break;
			}
			if (char.IsLetter(c) && num == 0 && num2 == 0)
			{
				array[i] = (char.IsUpper(c) ? 'W' : 'w');
			}
			else
			{
				array[i] = c;
			}
		}
		return new string(array);
	}

	private static string ToString(Dictionary<string, object> variables)
	{
		return "{" + string.Join(",", variables.Select<KeyValuePair<string, object>, string>((KeyValuePair<string, object> kp) => $"{kp.Key}:{kp.Value}")) + "}";
	}

	[MemberNotNull(new string[] { "CultureInfo", "Language" })]
	public void SetLanguage(string language)
	{
		(Dictionary<string, LocTable> tables, bool overridesActive, List<LocValidationError> validationErrors) tuple = LoadTablesFromPath(language);
		Dictionary<string, LocTable> item = tuple.tables;
		bool item2 = tuple.overridesActive;
		List<LocValidationError> item3 = tuple.validationErrors;
		_tables = item;
		OverridesActive = item2;
		ValidationErrors = item3.AsReadOnly();
		Language = language;
		if (OverridesActive)
		{
			Log.Info("Localization overrides are active for language '" + language + "'");
		}
		CultureInfo = CultureInfoFromThreeLetterCode(Language);
		if (TestMode.IsOn)
		{
			Callable.From(TriggerLocaleChange).CallDeferred();
		}
		else
		{
			TriggerLocaleChange();
		}
	}

	private static (Dictionary<string, LocTable> tables, bool overridesActive, List<LocValidationError> validationErrors) LoadTablesFromPath(string language)
	{
		Dictionary<string, LocTable> dictionary = null;
		if (language != "eng")
		{
			dictionary = LoadTablesFromPath("eng").tables;
		}
		Dictionary<string, LocTable> dictionary2 = new Dictionary<string, LocTable>();
		List<LocValidationError> list = new List<LocValidationError>();
		string text = LocalizationAssetDir + "/" + language;
		bool flag = false;
		bool item = false;
		if (!DirAccess.DirExistsAbsolute(text))
		{
			Log.Warn($"Dir path {text} for language {language} does not exist, falling back to eng");
			text = LocalizationAssetDir + "/eng";
			flag = OS.IsDebugBuild();
		}
		string text2 = ProjectSettings.GlobalizePath("user://localization_override");
		string text3 = Path.Combine(text2, language);
		bool flag2 = DirAccess.DirExistsAbsolute(text3);
		string text4 = Path.Combine(text2, "slaythespire2");
		bool flag3 = DirAccess.DirExistsAbsolute(text4);
		if (flag2)
		{
			Log.Info("Found flat localization override directory: " + text3);
		}
		if (flag3)
		{
			Log.Info("Found Weblate nested override directory: " + text4);
		}
		Log.Info("Loading locale path=" + text);
		IEnumerable<string> enumerable = ListLocalizationFiles(text);
		foreach (string item2 in enumerable)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(item2);
			string path = text + "/" + item2;
			Dictionary<string, string> dictionary3 = LoadTable(path);
			if (flag)
			{
				dictionary3 = dictionary3.ToDictionary((KeyValuePair<string, string> kvp) => kvp.Key, (KeyValuePair<string, string> kvp) => ConvertToW(kvp.Value));
			}
			LocTable fallback = dictionary?.GetValueOrDefault(fileNameWithoutExtension);
			LocTable locTable = new LocTable(fileNameWithoutExtension, dictionary3, fallback);
			if (!flag)
			{
				if (flag3 && TryLoadWeblateNestedOverrides(text2, language, item2, locTable, list))
				{
					item = true;
				}
				if (flag2)
				{
					string overrideFilePath = Path.Combine(text3, item2);
					if (TryLoadOverrideFile(overrideFilePath, locTable, list))
					{
						item = true;
					}
				}
			}
			foreach (string moddedLocTable in ModManager.GetModdedLocTables(language, item2))
			{
				Log.Info($"Found loc table from mod: {language} {item2}. Merging with base loc table");
				Dictionary<string, string> dictionary4 = LoadTable(moddedLocTable);
				if (flag)
				{
					dictionary4 = dictionary4.ToDictionary((KeyValuePair<string, string> kvp) => kvp.Key, (KeyValuePair<string, string> kvp) => ConvertToW(kvp.Value));
				}
				locTable.MergeWith(dictionary4);
			}
			dictionary2[fileNameWithoutExtension] = locTable;
		}
		return (tables: dictionary2, overridesActive: item, validationErrors: list);
	}

	public LocTable GetTable(string name)
	{
		if (_tables.TryGetValue(name, out LocTable value))
		{
			return value;
		}
		throw new LocException("The loc table='" + name + "' does not exist!");
	}

	private static Dictionary<string, string> LoadTable(string path)
	{
		using Godot.FileAccess fileAccess = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
		if (fileAccess == null)
		{
			throw new LocException("Cannot find language file: " + path);
		}
		string asText = fileAccess.GetAsText();
		try
		{
			return JsonSerializer.Deserialize(asText, LocManagerSerializerContext.Default.DictionaryStringString);
		}
		catch (Exception e)
		{
			throw new LocException("Failed to parse language file: " + path, e);
		}
	}

	private static IEnumerable<string> ListLocalizationFiles(string path)
	{
		using DirAccess dirAccess = DirAccess.Open(path);
		if (dirAccess == null)
		{
			throw new LocException("Path does not exist: " + path);
		}
		return (from s in dirAccess.GetFiles()
			where s.EndsWith(".json")
			select s).ToArray();
	}

	private static bool TryLoadOverrideFile(string overrideFilePath, LocTable locTable, List<LocValidationError> validationErrors)
	{
		if (!Godot.FileAccess.FileExists(overrideFilePath))
		{
			return false;
		}
		Log.Info("Loading localization override: " + overrideFilePath);
		try
		{
			Dictionary<string, string> dictionary = LoadTable(overrideFilePath);
			Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
			foreach (KeyValuePair<string, string> item in dictionary)
			{
				if (item.Key.StartsWith("EXTENSION.") || LocValidator.ValidateFormatString(item.Value, out string errorMessage))
				{
					dictionary2[item.Key] = item.Value;
					continue;
				}
				validationErrors.Add(new LocValidationError(overrideFilePath, item.Key, errorMessage));
				Log.Warn("[LocValidation] Invalid format in override file: " + overrideFilePath);
				Log.Warn("  Key: " + item.Key);
				Log.Warn("  Error: " + errorMessage);
			}
			locTable.MergeWith(dictionary2);
			return true;
		}
		catch (LocException ex)
		{
			validationErrors.Add(new LocValidationError(overrideFilePath, "(JSON parsing error)", ex.InnerException?.Message ?? ex.Message));
			Log.Warn("[LocValidation] Failed to parse override file: " + overrideFilePath);
			Log.Warn("  Error: " + (ex.InnerException?.Message ?? ex.Message));
			return false;
		}
	}

	private static bool TryLoadWeblateNestedOverrides(string globalizedOverrideDir, string language, string filename, LocTable locTable, List<LocValidationError> validationErrors)
	{
		if (!_gameToWeblateLanguage.TryGetValue(language, out string value))
		{
			return false;
		}
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
		global::_003C_003Ey__InlineArray5<string> buffer = default(global::_003C_003Ey__InlineArray5<string>);
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<global::_003C_003Ey__InlineArray5<string>, string>(ref buffer, 0) = globalizedOverrideDir;
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<global::_003C_003Ey__InlineArray5<string>, string>(ref buffer, 1) = "slaythespire2";
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<global::_003C_003Ey__InlineArray5<string>, string>(ref buffer, 2) = fileNameWithoutExtension;
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<global::_003C_003Ey__InlineArray5<string>, string>(ref buffer, 3) = value;
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<global::_003C_003Ey__InlineArray5<string>, string>(ref buffer, 4) = filename;
		string text = Path.Combine(global::_003CPrivateImplementationDetails_003E.InlineArrayAsReadOnlySpan<global::_003C_003Ey__InlineArray5<string>, string>(in buffer, 5));
		if (TryLoadOverrideFile(text, locTable, validationErrors))
		{
			Log.Info("Found Weblate nested override structure: " + text);
			return true;
		}
		return false;
	}

	public void SubscribeToLocaleChange(LocaleChangeCallback callback)
	{
		_localeChangeCallbacks.Add(callback);
	}

	public void UnsubscribeToLocaleChange(LocaleChangeCallback callback)
	{
		_localeChangeCallbacks.Remove(callback);
	}

	private void TriggerLocaleChange()
	{
		TranslationServer.SetLocale(CultureInfo.Name);
		foreach (LocaleChangeCallback localeChangeCallback in _localeChangeCallbacks)
		{
			localeChangeCallback();
		}
		GC.Collect();
	}

	static LocManager()
	{
		int num = 14;
		List<string> list = new List<string>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<string> span = CollectionsMarshal.AsSpan(list);
		int num2 = 0;
		span[num2] = "eng";
		num2++;
		span[num2] = "zhs";
		num2++;
		span[num2] = "deu";
		num2++;
		span[num2] = "esp";
		num2++;
		span[num2] = "fra";
		num2++;
		span[num2] = "ita";
		num2++;
		span[num2] = "jpn";
		num2++;
		span[num2] = "kor";
		num2++;
		span[num2] = "pol";
		num2++;
		span[num2] = "ptb";
		num2++;
		span[num2] = "rus";
		num2++;
		span[num2] = "spa";
		num2++;
		span[num2] = "tha";
		num2++;
		span[num2] = "tur";
		Languages = list;
		EnglishCultureInfo = System.Globalization.CultureInfo.GetCultureInfo("en");
	}
}
