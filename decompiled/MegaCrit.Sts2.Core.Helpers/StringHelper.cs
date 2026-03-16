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
	[GeneratedRegex("([A-Za-z0-9]|\\G(?!^))([A-Z])")]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.12.31616")]
	private static Regex CamelCaseRegex()
	{
		return _003CRegexGenerator_g_003EFACC081AAF3D765EFF87A82C4FBB77F6FD3EA759AA2D03D993988F88E97CC0B5B__CamelCaseRegex_0.Instance;
	}

	[GeneratedRegex("(.*?)_([a-zA-Z0-9])")]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.12.31616")]
	private static Regex SnakeCaseRegex()
	{
		return _003CRegexGenerator_g_003EFACC081AAF3D765EFF87A82C4FBB77F6FD3EA759AA2D03D993988F88E97CC0B5B__SnakeCaseRegex_1.Instance;
	}

	[GeneratedRegex("\\s+")]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.12.31616")]
	private static Regex WhitespaceRegex()
	{
		return _003CRegexGenerator_g_003EFACC081AAF3D765EFF87A82C4FBB77F6FD3EA759AA2D03D993988F88E97CC0B5B__WhitespaceRegex_2.Instance;
	}

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

	public static string CompactText(string text)
	{
		return text.Trim();
	}

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

	public static LocString RatioFormat(string numerator, string denominator)
	{
		LocString locString = new LocString("stats_screen", "RATIO_FORMAT");
		locString.Add("Numerator", numerator);
		locString.Add("Denominator", denominator);
		return locString;
	}

	public static string Capitalize(string input)
	{
		return char.ToUpperInvariant(input[0]) + input.Substring(1, input.Length - 1);
	}

	public static string StripBbCode(this string text)
	{
		return Regex.Replace(text, "\\[(.*?)\\]", "");
	}
}
