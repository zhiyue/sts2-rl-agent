using Godot;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Localization.Fonts;

public static class FontControlUtils
{
	/// <summary>
	/// Applies locale-specific font substitution if the current language requires it.
	/// </summary>
	public static void ApplyLocaleFontSubstitution(this Control control, FontType fontType, StringName themeFontName)
	{
		if (!Engine.IsEditorHint() && !TestMode.IsOn && LocManager.Instance != null && FontManager.NeedsFontSubstitution(LocManager.Instance.Language))
		{
			Font substituteFont = FontManager.GetSubstituteFont(LocManager.Instance.Language, fontType);
			if (substituteFont != null)
			{
				control.AddThemeFontOverride(themeFontName, substituteFont);
			}
		}
	}
}
