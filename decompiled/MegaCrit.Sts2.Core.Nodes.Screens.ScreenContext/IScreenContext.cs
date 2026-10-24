using Godot;

namespace MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;

/// <summary>
/// An interface that is shared by all screens in sts2. Used to bridge the gap between
/// all of our various screen systems (Overlays, Capstones, Rooms, Submenus)
/// </summary>
public interface IScreenContext
{
	/// <summary>
	/// The default control to focus on when it becomes the ActiveScreen while the player is using controller
	/// </summary>
	Control? DefaultFocusedControl { get; }

	/// <summary>
	/// The default control to focus on when navigating from the top bar (typically from your relics)
	/// Most of the time just equal to the DefaultFocusedControl
	/// </summary>
	Control? FocusedControlFromTopBar => DefaultFocusedControl;
}
