namespace MegaCrit.Sts2.Core.Assets;

public class TpSheetSprite
{
	public string Filename { get; set; } = "";

	public TpSheetRect Region { get; set; } = new TpSheetRect();

	public TpSheetRect Margin { get; set; } = new TpSheetRect();
}
