using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.AutoSlay.Handlers;

public interface IRoomHandler : IHandler
{
	RoomType[] HandledTypes { get; }
}
