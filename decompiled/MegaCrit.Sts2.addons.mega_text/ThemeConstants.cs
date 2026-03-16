using Godot;

namespace MegaCrit.Sts2.addons.mega_text;

public static class ThemeConstants
{
	public static class Label
	{
		public static readonly StringName fontSize = "font_size";

		public static readonly StringName font = "font";

		public static readonly StringName lineSpacing = "line_spacing";

		public static readonly StringName outlineSize = "outline_size";

		public static readonly StringName fontColor = "font_color";

		public static readonly StringName fontOutlineColor = "font_outline_color";

		public static readonly StringName fontShadowColor = "font_shadow_color";
	}

	public static class RichTextLabel
	{
		public static readonly StringName normalFont = "normal_font";

		public static readonly StringName boldFont = "bold_font";

		public static readonly StringName italicsFont = "italics_font";

		public static readonly StringName lineSpacing = "line_separation";

		public static readonly StringName normalFontSize = "normal_font_size";

		public static readonly StringName boldFontSize = "bold_font_size";

		public static readonly StringName boldItalicsFontSize = "bold_italics_font_size";

		public static readonly StringName italicsFontSize = "italics_font_size";

		public static readonly StringName monoFontSize = "mono_font_size";

		public static readonly StringName[] allFontSizes = new StringName[5] { normalFontSize, boldFontSize, boldItalicsFontSize, italicsFontSize, monoFontSize };

		public static readonly StringName defaultColor = "default_color";

		public static readonly StringName fontOutlineColor = "font_outline_color";

		public static readonly StringName fontShadowColor = "font_shadow_color";
	}

	public static class Control
	{
		public static readonly StringName focus = "focus";
	}

	public static class MarginContainer
	{
		public static readonly StringName marginLeft = "margin_left";

		public static readonly StringName marginRight = "margin_right";

		public static readonly StringName marginTop = "margin_top";

		public static readonly StringName marginBottom = "margin_bottom";
	}

	public static class BoxContainer
	{
		public static readonly StringName separation = "separation";
	}

	public static class FlowContainer
	{
		public static readonly StringName hSeparation = "h_separation";

		public static readonly StringName vSeparation = "v_separation";
	}

	public static class TextEdit
	{
		public static readonly StringName font = "font";
	}

	public static class LineEdit
	{
		public static readonly StringName font = "font";
	}
}
