using System;

namespace MegaCrit.Sts2.Core.Localization.Fonts.FontPathSets;

public class RusFontPathSet : FontPathSet
{
	private const string _regular = "res://themes/fonts/rus/fira_sans_extra_condensed_regular_shared.tres";

	private const string _bold = "res://themes/fonts/rus/fira_sans_extra_condensed_bold_shared.tres";

	private const string _italic = "res://themes/fonts/rus/fira_sans_extra_condensed_italic_shared.tres";

	public override string GetPath(FontType type)
	{
		return type switch
		{
			FontType.Regular => "res://themes/fonts/rus/fira_sans_extra_condensed_regular_shared.tres", 
			FontType.Bold => "res://themes/fonts/rus/fira_sans_extra_condensed_bold_shared.tres", 
			FontType.Italic => "res://themes/fonts/rus/fira_sans_extra_condensed_italic_shared.tres", 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}
}
