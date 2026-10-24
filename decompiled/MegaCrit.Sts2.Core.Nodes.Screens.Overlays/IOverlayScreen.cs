using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Overlays;

/// <summary>
/// An interface used by the NOverlayStack to manage screens that need to visibly stack on top of each other
/// The requirements to what should be considered to be made an overlay screen are as follows:
/// - need specific interaction when opened on top of with other screens
/// - These screens are created and destroyed on demand
///
/// Ex : "Combat reward screen" and "Choose a Card Screen"
///   - both are used for a limited amount of time, so we can create/destroy them as needed
///   - when I open the "Choose a Card Screen", the "Combat reward screen" sits underneath it while being disabled
///   - when I close the "Choose a Card Screen", the "Combat reward screen" needs to become visible again
/// </summary>
public interface IOverlayScreen : IScreenContext
{
	/// <summary>
	/// The screen type which will be synced with peers.
	/// This is used to display an icon next to the multiplayer player's icon, and to determine whether the remote
	/// player's mouse should be shown (i.e. whether they're on the same screen as you).
	/// </summary>
	NetScreenType ScreenType { get; }

	bool UseSharedBackstop { get; }

	void AfterOverlayOpened();

	void AfterOverlayClosed();

	void AfterOverlayShown();

	void AfterOverlayHidden();
}
