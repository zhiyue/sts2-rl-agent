namespace MegaCrit.Sts2.Core.Nodes.Screens.Settings;

public interface IResettableSettingNode
{
	/// <summary>
	/// Ensures that the state of the node reflects SettingsSave/PrefsSave.
	/// For example, if we change SettingsSave.FastMode, then this should be called on NFastModeTickbox to ensure it
	/// is ticked/unticked appropriately.
	/// </summary>
	void SetFromSettings();
}
