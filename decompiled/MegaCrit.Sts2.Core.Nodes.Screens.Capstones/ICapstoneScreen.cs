using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Capstones;

public interface ICapstoneScreen : IScreenContext
{
	NetScreenType ScreenType { get; }

	bool UseSharedBackstop { get; }

	void AfterCapstoneOpened();

	void AfterCapstoneClosed();
}
