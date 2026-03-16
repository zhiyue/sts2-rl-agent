using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;

namespace MegaCrit.Sts2.Core.Nodes.Events;

public interface ICustomEventNode : IScreenContext
{
	IScreenContext CurrentScreenContext { get; }

	void Initialize(EventModel eventModel);
}
