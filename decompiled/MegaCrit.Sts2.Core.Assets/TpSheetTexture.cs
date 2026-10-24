using System.Collections.Generic;

namespace MegaCrit.Sts2.Core.Assets;

/// <summary>
/// A single texture page in the atlas (multi-page atlases have multiple entries).
/// </summary>
public class TpSheetTexture
{
	public string Image { get; set; } = "";

	public TpSheetSize Size { get; set; } = new TpSheetSize();

	public List<TpSheetSprite> Sprites { get; set; } = new List<TpSheetSprite>();
}
