using System.Collections.Generic;
using System.Reflection;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Localization;

namespace MegaCrit.Sts2.Core.Modding;

/// <summary>
/// Information about a loaded mod.
/// </summary>
public class Mod
{
	/// <summary>
	/// Where the mod originated from.
	/// </summary>
	public ModSource modSource;

	/// <summary>
	/// Path where the mod files are located.
	/// </summary>
	public required string path;

	/// <summary>
	/// Whether the mod was loaded, and if it was not loaded, why.
	/// Since there's no way to unload mods while the game is running, this value cannot change after the initial mod
	/// initialization occurs.
	/// Even if the mod is set to disabled in SettingsSave, this value is the true source of whether or not the mod was
	/// loaded into the game.
	/// </summary>
	public ModLoadState state;

	/// <summary>
	/// The mod manifest.
	/// </summary>
	public ModManifest? manifest;

	/// <summary>
	/// The version parsed from the mod manifest.
	/// Null if the version is not present or is not a valid semantic version.
	/// </summary>
	public SemanticVersion? version;

	/// <summary>
	/// The C# assembly loaded with the mod, if any.
	/// </summary>
	public Assembly? assembly;

	/// <summary>
	/// If null, then no errors occurred while loading the mod.
	/// If this is set, then there was an error loading the mod that should be displayed.
	/// </summary>
	public List<LocString>? errors;
}
