using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Rooms;

public class MapRoom : AbstractRoom
{
	public override RoomType RoomType => RoomType.Map;

	public override ModelId? ModelId => null;

	public override Task Enter(IRunState? runState, bool isRestoringRoomStackBase)
	{
		if (isRestoringRoomStackBase)
		{
			throw new InvalidOperationException("MapRoom does not support room stack reconstruction.");
		}
		if (TestMode.IsOn)
		{
			return Task.CompletedTask;
		}
		NMapRoom currentRoom = NMapRoom.Create(runState?.Act ?? ModelDb.Act<Overgrowth>(), runState?.CurrentActIndex ?? 0);
		NRun.Instance.SetCurrentRoom(currentRoom);
		return Task.CompletedTask;
	}

	public override Task Exit(IRunState? runState)
	{
		return Task.CompletedTask;
	}

	public override Task Resume(AbstractRoom _, IRunState? runState)
	{
		throw new NotImplementedException();
	}
}
