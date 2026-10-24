using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Capstones;

/// <summary>
/// An interface used for certain screens that need to supersede all other screens
/// These screens sit on top overlay screens.
/// Sometimes, these screens need to persist for the whole of the run.
/// Notes about capstone screen:
/// - capstone screens sit on top of everything. including overlay screens
/// - there's only allowed to be one capstone screen at a time (i.e: deck view screen, map screen, settings screen).
///
/// Ex "Deck View Screen":
/// - the "Deck View Screen" needs to supersede most gameplay screens (combat reward screen/choose a card screen)
/// - When a screen of the same level is opened (Map Screen, Settings Screen), this screen is destroyed
/// </summary>
public interface ICapstoneScreen : IScreenContext
{
	/// <summary>
	/// The screen type which will be synced with peers.
	/// This is used to display an icon next to the multiplayer player's icon, and to determine whether the remote
	/// player's mouse should be shown (i.e. whether they're on the same screen as you).
	/// </summary>
	NetScreenType ScreenType { get; }

	bool UseSharedBackstop { get; }

	void AfterCapstoneOpened();

	void AfterCapstoneClosed();
}
