using Godot;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Localization.Fonts;

public static class FontControlUtils
{
	public static void ApplyLocaleFontSubstitution(this Control control, FontType fontType, StringName themeFontName)
	{
		if (!Engine.IsEditorHint() && !TestMode.IsOn && FontManager.NeedsFontSubstitution(LocManager.Instance.Language))
		{
			Font substituteFont = FontManager.GetSubstituteFont(LocManager.Instance.Language, fontType);
			if (substituteFont != null)
			{
				control.AddThemeFontOverride(themeFontName, substituteFont);
			}
		}
	}
}
