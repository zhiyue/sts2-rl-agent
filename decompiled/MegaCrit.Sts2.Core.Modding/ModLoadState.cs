namespace MegaCrit.Sts2.Core.Modding;

public enum ModLoadState
{
	None,
	/// <summary>
	/// Mod was successfully loaded.
	/// </summary>
	Loaded,
	/// <summary>
	/// We tried to load the mod but it failed to load.
	/// </summary>
	Failed,
	/// <summary>
	/// We didn't try to load the mod because the user has not enabled mod loading, or this specific mod is marked as
	/// disabled in SettingsSave.
	/// </summary>
	Disabled,
	/// <summary>
	/// We didn't try to load the mod because the user has this mod loaded both via Steam and local mods directory.
	/// </summary>
	DisabledDuplicate,
	/// <summary>
	/// We didn't try to load the mod because it was added too late.
	/// </summary>
	AddedAtRuntime
}
