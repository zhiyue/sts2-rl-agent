using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Helpers;

public static class AscensionHelper
{
	public static double PovertyAscensionGoldMultiplier => 0.75;

	public static int GetValueIfAscension(AscensionLevel level, int ascensionValue, int fallbackValue)
	{
		if (!RunManager.Instance.HasAscension(level))
		{
			return fallbackValue;
		}
		return ascensionValue;
	}

	public static float GetValueIfAscension(AscensionLevel level, float ascensionValue, float fallbackValue)
	{
		if (!RunManager.Instance.HasAscension(level))
		{
			return fallbackValue;
		}
		return ascensionValue;
	}

	public static decimal GetValueIfAscension(AscensionLevel level, decimal ascensionValue, decimal fallbackValue)
	{
		if (!RunManager.Instance.HasAscension(level))
		{
			return fallbackValue;
		}
		return ascensionValue;
	}

	public static bool HasAscension(AscensionLevel level)
	{
		return RunManager.Instance.HasAscension(level);
	}

	public static LocString GetTitle(int level)
	{
		return new LocString("ascension", "LEVEL_" + GetKey(level) + ".title");
	}

	public static LocString GetDescription(int level)
	{
		return new LocString("ascension", "LEVEL_" + GetKey(level) + ".description");
	}

	private static string GetKey(int level)
	{
		return level.ToString("D2");
	}

	public static HoverTip GetHoverTip(CharacterModel character, int level)
	{
		LocString locString = new LocString("ascension", "PORTRAIT_TITLE");
		locString.Add("character", character.Title);
		locString.Add("ascension", level);
		LocString locString2 = new LocString("ascension", "PORTRAIT_DESCRIPTION");
		List<string> list = new List<string>();
		for (int i = 1; i <= level; i++)
		{
			list.Add(GetTitle(i).GetFormattedText());
		}
		locString2.Add("ascensions", list);
		return new HoverTip(locString, locString2);
	}
}
