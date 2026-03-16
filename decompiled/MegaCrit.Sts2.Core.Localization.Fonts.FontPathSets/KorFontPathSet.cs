using System;

namespace MegaCrit.Sts2.Core.Localization.Fonts.FontPathSets;

public class KorFontPathSet : FontPathSet
{
	private const string _bold = "res://themes/fonts/kor/gyeonggi_cheonnyeon_batang_bold_shared.tres";

	public override string GetPath(FontType type)
	{
		return type switch
		{
			FontType.Regular => "res://themes/fonts/kor/gyeonggi_cheonnyeon_batang_bold_shared.tres", 
			FontType.Bold => "res://themes/fonts/kor/gyeonggi_cheonnyeon_batang_bold_shared.tres", 
			FontType.Italic => "res://themes/fonts/kor/gyeonggi_cheonnyeon_batang_bold_shared.tres", 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}
}
