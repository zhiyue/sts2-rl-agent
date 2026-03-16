using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Overlays;

public interface IOverlayScreen : IScreenContext
{
	NetScreenType ScreenType { get; }

	bool UseSharedBackstop { get; }

	void AfterOverlayOpened();

	void AfterOverlayClosed();

	void AfterOverlayShown();

	void AfterOverlayHidden();
}
