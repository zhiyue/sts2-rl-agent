namespace MegaCrit.Sts2.Core.Platform;

/// <summary>
/// The type of fullscreen that this platform supports.
/// </summary>
public enum SupportedWindowMode
{
	/// <summary>
	/// Invalid value.
	/// </summary>
	None,
	/// <summary>
	/// This platform supports both fullscreen and non-fullscreen.
	/// </summary>
	Any,
	/// <summary>
	/// This platform forces being in fullscreen. If the settings.save says that the game should be windowed, it is
	/// forced into fullscreen anyway (with the settings.save unchanged). The settings menu toggle will show as disabled.
	/// Used on platforms for which fullscreen may be supported depending on the current configuration; for example,
	/// Big Picture mode on a PC.
	/// </summary>
	FullscreenOnlyDisplayToggle,
	/// <summary>
	/// This platform only allows being in fullscreen. If the settings.save says that the game should be windowed, it is
	/// forced into fullscreen anyway. The settings menu toggle will be hidden.
	/// Used on platforms like consoles and mobile where fullscreen is never supported.
	/// </summary>
	FullscreenOnly
}
