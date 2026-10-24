namespace MegaCrit.Sts2.Core.Nodes.Screens.Timeline;

public enum EpochSlotState
{
	None,
	/// <summary>
	/// The Epoch is revealed and the player has inspected it before.
	/// </summary>
	Complete,
	/// <summary>
	/// The Epoch is obtained so the player is able to click on the slot in the Timeline to reveal/complete it!
	/// </summary>
	Obtained,
	/// <summary>
	/// This Epoch has never been obtained. Used to show empty slots, can be useful to communicate how to obtain the Epoch.
	/// </summary>
	NotObtained
}
