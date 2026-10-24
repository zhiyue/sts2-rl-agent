namespace MegaCrit.Sts2.Core.Assets;

/// <summary>
/// Individual sprite within a texture page.
/// </summary>
public class TpSheetSprite
{
	public string Filename { get; set; } = "";

	public TpSheetRect Region { get; set; } = new TpSheetRect();

	public TpSheetRect Margin { get; set; } = new TpSheetRect();
}
