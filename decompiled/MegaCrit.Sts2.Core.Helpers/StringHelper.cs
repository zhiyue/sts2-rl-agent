using System;
using System.CodeDom.Compiler;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Text.RegularExpressions.Generated;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Helpers;

public static class StringHelper
{
	/// <remarks>
	/// Pattern:<br />
	/// <code>([A-Za-z0-9]|\\G(?!^))([A-Z])</code><br />
	/// Explanation:<br />
	/// <code>
	/// ○ 1st capture group.<br />
	///     ○ Match with 2 alternative expressions.<br />
	///         ○ Match a character in the set [0-9A-Za-z].<br />
	///         ○ Match a sequence of expressions.<br />
	///             ○ Match if at the start position.<br />
	///             ○ Zero-width negative lookahead.<br />
	///                 ○ Match if at the beginning of the string.<br />
	/// ○ 2nd capture group.<br />
	///     ○ Match a character in the set [A-Z].<br />
	/// </code>
	/// </remarks>
	[GeneratedRegex("([A-Za-z0-9]|\\G(?!^))([A-Z])")]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.12.31616")]
	private static Regex CamelCaseRegex()
	{
		return _003CRegexGenerator_g_003EFACC081AAF3D765EFF87A82C4FBB77F6FD3EA759AA2D03D993988F88E97CC0B5B__CamelCaseRegex_0.Instance;
	}

	/// <remarks>
	/// Pattern:<br />
	/// <code>(.*?)_([a-zA-Z0-9])</code><br />
	/// Explanation:<br />
	/// <code>
	/// ○ 1st capture group.<br />
	///     ○ Match a character other than '\n' lazily any number of times.<br />
	/// ○ Match '_'.<br />
	/// ○ 2nd capture group.<br />
	///     ○ Match a character in the set [0-9A-Za-z].<br />
	/// </code>
	/// </remarks>
	[GeneratedRegex("(.*?)_([a-zA-Z0-9])")]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.12.31616")]
	private static Regex SnakeCaseRegex()
	{
		return _003CRegexGenerator_g_003EFACC081AAF3D765EFF87A82C4FBB77F6FD3EA759AA2D03D993988F88E97CC0B5B__SnakeCaseRegex_1.Instance;
	}

	/// <remarks>
	/// Pattern:<br />
	/// <code>\\s+</code><br />
	/// Explanation:<br />
	/// <code>
	/// ○ Match a whitespace character atomically at least once.<br />
	/// </code>
	/// </remarks>
	[GeneratedRegex("\\s+")]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.12.31616")]
	private static Regex WhitespaceRegex()
	{
		return _003CRegexGenerator_g_003EFACC081AAF3D765EFF87A82C4FBB77F6FD3EA759AA2D03D993988F88E97CC0B5B__WhitespaceRegex_2.Instance;
	}

	/// <remarks>
	/// Pattern:<br />
	/// <code>[^A-Z0-9_]</code><br />
	/// Explanation:<br />
	/// <code>
	/// ○ Match a character in the set [^0-9A-Z_].<br />
	/// </code>
	/// </remarks>
	[GeneratedRegex("[^A-Z0-9_]")]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.12.31616")]
	private static Regex SpecialCharRegex()
	{
		return _003CRegexGenerator_g_003EFACC081AAF3D765EFF87A82C4FBB77F6FD3EA759AA2D03D993988F88E97CC0B5B__SpecialCharRegex_3.Instance;
	}

	public static string SnakeCase(string txt)
	{
		return CamelCaseRegex().Replace(txt.Trim(), "$1_$2").ToLowerInvariant();
	}

	public static string Slugify(string txt)
	{
		string text = CamelCaseRegex().Replace(txt.Trim(), "$1_$2");
		string input = WhitespaceRegex().Replace(text.ToUpperInvariant(), "_");
		return SpecialCharRegex().Replace(input, "");
	}

	public static string Unslugify(string txt)
	{
		string text = SnakeCaseRegex().Replace(txt.Trim().ToLowerInvariant(), delegate(Match match)
		{
			string text3 = match.Groups[1].ToString();
			string text4 = match.Groups[2].ToString();
			return text3 + text4.ToUpperInvariant();
		});
		ReadOnlySpan<char> readOnlySpan = new ReadOnlySpan<char>(char.ToUpperInvariant(text[0]));
		string text2 = text;
		return string.Concat(readOnlySpan, text2.Substring(1, text2.Length - 1));
	}

	/// <summary>
	/// Trims whitespace, converts newlines to spaces, and squashes multiple whitespace characters in a row down to
	/// a single space.
	/// </summary>
	/// <remarks>
	/// Good for writing multi-line indented verbatim strings (https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/tokens/verbatim)
	/// and having them output in a natural way.
	/// </remarks>
	/// <param name="text">The string to compact.</param>
	/// <returns>A compacted copy of <paramref name="text" />.</returns>
	public static string CompactText(string text)
	{
		return text.Trim();
	}

	/// <summary>
	/// Get a deterministic hash representation of a string. Unlike GetHashCode(), it is guaranteed to be the same
	/// even across program executions.
	///
	/// This is NOT guaranteed to be unique or cryptographically safe or anything like that. Its only guarantee is
	/// to be deterministic.
	///
	/// Algorithm lifted from https://andrewlock.net/why-is-string-gethashcode-different-each-time-i-run-my-program-in-net-core/
	/// </summary>
	/// <param name="str">String to hash.</param>
	/// <returns>Hash value.</returns>
	public static int GetDeterministicHashCode(string str)
	{
		int num = 352654597;
		int num2 = num;
		for (int i = 0; i < str.Length; i += 2)
		{
			num = ((num << 5) + num) ^ str[i];
			if (i == str.Length - 1)
			{
				break;
			}
			num2 = ((num2 << 5) + num2) ^ str[i + 1];
		}
		return num + num2 * 1566083941;
	}

	/// <summary>
	/// Returns a number as a string with radix applied based on set language.
	/// i.e. one-thousand seven-hundred fifty-five returns 1,755. However, in some languages returns 1.755.
	/// Used when we show BIG numbers in the stats screen. Maybe leaderboards, I dunno.
	/// </summary>
	public static string Radix(int value)
	{
		switch (SaveManager.Instance.SettingsSave.Language)
		{
		case "deu":
		case "dut":
		case "gre":
		case "ind":
		case "ita":
		case "mal":
		case "nor":
		case "por":
		case "ptb":
		case "spa":
		case "tur":
		case "vie":
			return value.ToString("N0", new CultureInfo("es-ES"));
		case "pol":
		case "swe":
		case "cze":
		case "fin":
		case "fra":
		case "rus":
		case "ukr":
			return value.ToString("N0", new CultureInfo("fr-FR"));
		case "ben":
		case "hin":
			return value.ToString("N0", new CultureInfo("hi-IN"));
		default:
			return value.ToString("N0", new CultureInfo("en-US"));
		}
	}

	public static LocString RatioFormat(int numerator, int denominator)
	{
		return RatioFormat(numerator.ToString(), denominator.ToString());
	}

	/// <summary>
	/// Helps render text like "119/670"
	/// In case there are different ratio formats in other languages.
	/// </summary>
	public static LocString RatioFormat(string numerator, string denominator)
	{
		LocString locString = new LocString("stats_screen", "RATIO_FORMAT");
		locString.Add("Numerator", numerator);
		locString.Add("Denominator", denominator);
		return locString;
	}

	/// <summary>
	/// Simple helper to capitalize the first character of a given string.
	/// Does not work with multiple words. Improve and add exceptions if its use case expands.
	/// </summary>
	public static string Capitalize(string input)
	{
		return char.ToUpperInvariant(input[0]) + input.Substring(1, input.Length - 1);
	}

	/// <summary>
	/// Removes the bbcode tags from the given string
	/// </summary>
	public static string StripBbCode(this string text)
	{
		return Regex.Replace(text, "\\[(.*?)\\]", "");
	}

	/// <summary>
	/// Replaces brackets with [lb] and [rb] so they are not interpreted as bbcode tags.
	/// </summary>
	public static string EscapeBbcodeTags(this string text)
	{
		return text.Replace("[", "[lb]");
	}
}
