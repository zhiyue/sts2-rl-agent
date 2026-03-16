using MegaCrit.Sts2.Core.Helpers;

namespace MegaCrit.Sts2.Core.ControllerInput.ControllerConfigs;

public class SwitchConfig : ControllerConfig
{
	protected override string FolderPath => "atlases/controller_atlas.sprites/switch";

	public override ControllerMappingType ControllerMappingType => ControllerMappingType.NintendoSwitch;

	protected override string FaceButtonSouthGlyph => ImageHelper.GetImagePath(FolderPath + "/b.tres");

	protected override string FaceButtonEastGlyph => ImageHelper.GetImagePath(FolderPath + "/a.tres");

	protected override string FaceButtonNorthGlyph => ImageHelper.GetImagePath(FolderPath + "/x.tres");

	protected override string FaceButtonWestGlyph => ImageHelper.GetImagePath(FolderPath + "/y.tres");
}
