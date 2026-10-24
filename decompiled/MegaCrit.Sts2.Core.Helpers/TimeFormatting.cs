using System;
using MegaCrit.Sts2.Core.Localization;

namespace MegaCrit.Sts2.Core.Helpers;

public static class TimeFormatting
{
	public static string Format(float time)
	{
		TimeSpan timeSpan = TimeSpan.FromSeconds(time);
		int num = (int)timeSpan.TotalHours;
		LocString locString;
		if (num > 0)
		{
			locString = new LocString("main_menu_ui", "RUN_TIME_FORMAT_HOURS");
			locString.Add("Hours", num.ToString());
		}
		else
		{
			locString = new LocString("main_menu_ui", "RUN_TIME_FORMAT");
		}
		locString.Add("Seconds", timeSpan.Seconds.ToString("00"));
		locString.Add("Minutes", timeSpan.Minutes.ToString("00"));
		return locString.GetFormattedText();
	}
}
