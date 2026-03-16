namespace MegaCrit.Sts2.Core.Platform;

public static class SupportedWindowModeExtensions
{
	public static bool ShouldForceFullscreen(this SupportedWindowMode mode)
	{
		if (mode != SupportedWindowMode.FullscreenOnly)
		{
			return mode == SupportedWindowMode.FullscreenOnlyDisplayToggle;
		}
		return true;
	}
}
