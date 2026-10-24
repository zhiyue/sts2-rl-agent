namespace MegaCrit.Sts2.Core.Nodes.Vfx;

/// <summary>
/// Various VFX may allow a shorter or longer version, this enum allows specifying the duration based on feel and not
/// exact numbers (as they may be scaled based on the game speed)
/// </summary>
public enum VfxDuration
{
	None,
	VeryShort,
	Short,
	Standard,
	Long,
	VeryLong,
	Custom,
	Forever
}
