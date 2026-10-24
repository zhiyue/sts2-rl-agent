namespace MegaCrit.Sts2.Core.Modding;

public enum ModManagerState
{
	/// <summary>
	/// ModManager is not yet initialized.
	/// </summary>
	None,
	/// <summary>
	/// ModManager is fully initialized. It may or may not have loaded mods.
	/// </summary>
	Initialized,
	/// <summary>
	/// ModManager skipped initialization for some reason:
	///  - We're in test mode
	///  - This is an AOT build
	///  - nomods command line argument was passed
	/// </summary>
	Skipped
}
