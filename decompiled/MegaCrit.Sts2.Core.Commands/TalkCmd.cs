using System;
using System.Text.RegularExpressions;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;

namespace MegaCrit.Sts2.Core.Commands;

/// <summary>
/// Creates a speech bubble. Used when a creature talks/screams/whispers to you. Generally during combat
/// </summary>
public static class TalkCmd
{
	public static NSpeechBubbleVfx? Play(LocString line, Creature speaker, VfxColor vfxColor, VfxDuration duration = VfxDuration.Custom)
	{
		if (speaker.IsDead)
		{
			return null;
		}
		string formattedText = line.GetFormattedText();
		double num;
		if (duration == VfxDuration.Custom)
		{
			num = (double)GetRawCharCount(formattedText) * ((SaveManager.Instance.PrefsSave.FastMode == FastModeType.Fast) ? 0.1 : 0.12);
		}
		else
		{
			num = GetDuration(duration);
			if (SaveManager.Instance.PrefsSave.FastMode == FastModeType.Fast)
			{
				num -= 0.5;
			}
		}
		num = Math.Max(0.5, num);
		NSpeechBubbleVfx nSpeechBubbleVfx = NSpeechBubbleVfx.Create(formattedText, speaker, num, vfxColor);
		if (nSpeechBubbleVfx != null)
		{
			speaker.GetVfxContainer()?.AddChildSafely(nSpeechBubbleVfx);
		}
		return nSpeechBubbleVfx;
	}

	private static double GetDuration(VfxDuration duration)
	{
		return duration switch
		{
			VfxDuration.None => 0.0, 
			VfxDuration.VeryShort => 1.0, 
			VfxDuration.Short => 1.5, 
			VfxDuration.Standard => 1.75, 
			VfxDuration.Long => 2.25, 
			VfxDuration.VeryLong => 3.0, 
			VfxDuration.Forever => 999999999.0, 
			_ => 0.0, 
		};
	}

	/// <summary>
	/// Calculates how many characters in a string without the bbcode. Also strips out new line, brackets, optionals.
	/// Example:
	/// [jitter][gold]Money![/gold][/jitter]
	/// [sine][orange]Money![/orange][/sine]
	/// returns Money!Money! -&gt; 12 characters -&gt; 1.2 second speech bubble
	/// </summary>
	private static int GetRawCharCount(string bbcodeText)
	{
		string text = Regex.Replace(bbcodeText, "\\[/?[^\\]]+\\]", "");
		return text.Replace("\n", "").Replace("\r", "").Replace(" ", "")
			.Length;
	}
}
