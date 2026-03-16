using System;

namespace MegaCrit.Sts2.Core.Localization.Fonts.FontPathSets;

public class JpnFontPathSet : FontPathSet
{
	private const string _regular = "res://themes/fonts/jpn/noto_sans_cjkjp_regular_shared.tres";

	private const string _bold = "res://themes/fonts/jpn/noto_sans_cjkjp_bold_shared.tres";

	private const string _italic = "res://themes/fonts/jpn/noto_sans_cjkjp_medium_shared.tres";

	public override string GetPath(FontType type)
	{
		return type switch
		{
			FontType.Regular => "res://themes/fonts/jpn/noto_sans_cjkjp_regular_shared.tres", 
			FontType.Bold => "res://themes/fonts/jpn/noto_sans_cjkjp_bold_shared.tres", 
			FontType.Italic => "res://themes/fonts/jpn/noto_sans_cjkjp_medium_shared.tres", 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}
}
