using System;

namespace MegaCrit.Sts2.Core.Localization.Fonts.FontPathSets;

public class ThaFontPathSet : FontPathSet
{
	private const string _regular = "res://themes/fonts/tha/cs_chat_thai_ui_shared.tres";

	public override string GetPath(FontType type)
	{
		return type switch
		{
			FontType.Regular => "res://themes/fonts/tha/cs_chat_thai_ui_shared.tres", 
			FontType.Bold => "res://themes/fonts/tha/cs_chat_thai_ui_shared.tres", 
			FontType.Italic => "res://themes/fonts/tha/cs_chat_thai_ui_shared.tres", 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}
}
