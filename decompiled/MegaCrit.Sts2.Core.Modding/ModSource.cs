namespace MegaCrit.Sts2.Core.Modding;

/// <summary>
/// Where a mod was loaded from.
/// </summary>
public enum ModSource
{
	None,
	/// <summary>
	/// The mod is located in the mods directory next to the executable.
	/// </summary>
	ModsDirectory,
	/// <summary>
	/// The mod was loaded from Steam Workshop.
	/// </summary>
	SteamWorkshop
}
