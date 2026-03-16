using System.Collections.Generic;

namespace MegaCrit.Sts2.Core.Assets;

public class TpSheetTexture
{
	public string Image { get; set; } = "";

	public TpSheetSize Size { get; set; } = new TpSheetSize();

	public List<TpSheetSprite> Sprites { get; set; } = new List<TpSheetSprite>();
}
