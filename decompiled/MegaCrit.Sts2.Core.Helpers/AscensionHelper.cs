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

	/// <summary>
	/// Return a different value depending on whether the given ascension is higher or lower than the current
	/// ascension of the run.
	/// </summary>
	/// <param name="level">The value that we use for ascensions below the ascensionThreshold</param>
	/// <param name="ascensionValue">The value that we use for ascensions above the ascensionThreshold</param>
	/// <param name="fallbackValue">The value that we use for ascensions below the ascensionThreshold</param>
	/// <returns></returns>
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

	public static HoverTip GetHoverTip(CharacterModel character, int level, bool achievementsLocked)
	{
		LocString locString;
		if (level > 0)
		{
			locString = new LocString("ascension", "PORTRAIT_TITLE");
			locString.Add("character", character.Title);
			locString.Add("ascension", level);
		}
		else
		{
			locString = new LocString("ascension", "PORTRAIT_TITLE_NO_ASCENSION");
			locString.Add("character", character.Title);
		}
		LocString locString2 = new LocString("ascension", "PORTRAIT_DESCRIPTION");
		List<string> list = new List<string>();
		for (int i = 1; i <= level; i++)
		{
			list.Add(GetTitle(i).GetFormattedText());
		}
		locString2.Add("ascensions", list);
		if (achievementsLocked)
		{
			if (level == 0)
			{
				LocString locString3 = new LocString("gameplay_ui", "ACHIEVEMENTS_LOCKED");
				return new HoverTip(locString, locString2.GetFormattedText() + locString3.GetFormattedText());
			}
			LocString locString4 = new LocString("gameplay_ui", "ACHIEVEMENTS_LOCKED");
			return new HoverTip(locString, locString2.GetFormattedText() + "\n" + locString4.GetFormattedText());
		}
		return new HoverTip(locString, locString2);
	}
}
