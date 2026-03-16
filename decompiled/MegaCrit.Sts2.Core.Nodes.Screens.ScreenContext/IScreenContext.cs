using Godot;

namespace MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;

public interface IScreenContext
{
	Control? DefaultFocusedControl { get; }

	Control? FocusedControlFromTopBar => DefaultFocusedControl;
}
