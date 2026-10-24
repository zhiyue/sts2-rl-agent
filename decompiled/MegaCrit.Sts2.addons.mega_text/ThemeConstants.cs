using Godot;

namespace MegaCrit.Sts2.addons.mega_text;

/// <summary>
/// Central source of truth for Godot theme property names.
/// These names are defined by Godot and must match exactly.
/// Tests verify these constants are valid against the current Godot version.
/// Using StringName for efficiency since Godot's theme methods take StringName parameters.
/// </summary>
public static class ThemeConstants
{
	/// <summary>
	/// Theme property names for Label nodes.
	/// </summary>
	public static class Label
	{
		public static readonly StringName FontSize = "font_size";

		public static readonly StringName Font = "font";

		public static readonly StringName LineSpacing = "line_spacing";

		public static readonly StringName OutlineSize = "outline_size";

		public static readonly StringName FontColor = "font_color";

		public static readonly StringName FontOutlineColor = "font_outline_color";

		public static readonly StringName FontShadowColor = "font_shadow_color";
	}

	/// <summary>
	/// Theme property names for RichTextLabel nodes.
	/// Note: RichTextLabel uses different names than Label (e.g., "line_separation" vs "line_spacing").
	/// </summary>
	public static class RichTextLabel
	{
		public static readonly StringName NormalFont = "normal_font";

		public static readonly StringName BoldFont = "bold_font";

		public static readonly StringName ItalicsFont = "italics_font";

		public static readonly StringName LineSpacing = "line_separation";

		public static readonly StringName NormalFontSize = "normal_font_size";

		public static readonly StringName BoldFontSize = "bold_font_size";

		public static readonly StringName BoldItalicsFontSize = "bold_italics_font_size";

		public static readonly StringName ItalicsFontSize = "italics_font_size";

		public static readonly StringName MonoFontSize = "mono_font_size";

		/// <summary>
		/// All font size property names for RichTextLabel.
		/// Used when setting all font sizes to the same value.
		/// </summary>
		public static readonly StringName[] AllFontSizes = new StringName[5] { NormalFontSize, BoldFontSize, BoldItalicsFontSize, ItalicsFontSize, MonoFontSize };

		public static readonly StringName DefaultColor = "default_color";

		public static readonly StringName FontOutlineColor = "font_outline_color";

		public static readonly StringName FontShadowColor = "font_shadow_color";
	}

	/// <summary>
	/// Theme property names for Control nodes (shared across many control types).
	/// </summary>
	public static class Control
	{
		public static readonly StringName Focus = "focus";
	}

	/// <summary>
	/// Theme property names for MarginContainer nodes.
	/// </summary>
	public static class MarginContainer
	{
		public static readonly StringName MarginLeft = "margin_left";

		public static readonly StringName MarginRight = "margin_right";

		public static readonly StringName MarginTop = "margin_top";

		public static readonly StringName MarginBottom = "margin_bottom";
	}

	/// <summary>
	/// Theme property names for HBoxContainer and VBoxContainer nodes.
	/// </summary>
	public static class BoxContainer
	{
		public static readonly StringName Separation = "separation";
	}

	/// <summary>
	/// Theme property names for FlowContainer nodes.
	/// </summary>
	public static class FlowContainer
	{
		public static readonly StringName HSeparation = "h_separation";

		public static readonly StringName VSeparation = "v_separation";
	}

	/// <summary>
	/// Theme property names for TextEdit nodes.
	/// </summary>
	public static class TextEdit
	{
		public static readonly StringName Font = "font";
	}

	/// <summary>
	/// Theme property names for LineEdit nodes.
	/// </summary>
	public static class LineEdit
	{
		public static readonly StringName Font = "font";
	}
}
