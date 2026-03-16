using System;

namespace MegaCrit.Sts2.Core.Localization.Fonts.FontPathSets;

public class ZhsFontPathSet : FontPathSet
{
	private const string _regular = "res://themes/fonts/zhs/noto_sans_mono_cjksc_regular_shared.tres";

	private const string _bold = "res://themes/fonts/zhs/source_han_serif_sc_bold_shared.tres";

	private const string _italic = "res://themes/fonts/zhs/source_han_serif_sc_medium_shared.tres";

	public override string GetPath(FontType type)
	{
		return type switch
		{
			FontType.Regular => "res://themes/fonts/zhs/noto_sans_mono_cjksc_regular_shared.tres", 
			FontType.Bold => "res://themes/fonts/zhs/source_han_serif_sc_bold_shared.tres", 
			FontType.Italic => "res://themes/fonts/zhs/source_han_serif_sc_medium_shared.tres", 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}
}
