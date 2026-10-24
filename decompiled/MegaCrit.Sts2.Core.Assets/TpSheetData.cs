using System.Collections.Generic;

namespace MegaCrit.Sts2.Core.Assets;

/// <summary>
/// Root structure for TexturePacker .tpsheet files.
/// </summary>
public class TpSheetData
{
	public List<TpSheetTexture> Textures { get; set; } = new List<TpSheetTexture>();
}
