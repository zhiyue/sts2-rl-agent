namespace MegaCrit.Sts2.Core.Localization.Fonts;

/// <summary>
/// A set of paths that can be used for substitution for a given language.
/// </summary>
public abstract class FontPathSet
{
	/// <summary>
	/// Get the path for a given font type.
	/// </summary>
	/// <param name="type">What type of font to get the path for (Regular, Bold, etc.)</param>
	/// <returns>The path to the font, or null if not found.</returns>
	public abstract string? GetPath(FontType type);
}
