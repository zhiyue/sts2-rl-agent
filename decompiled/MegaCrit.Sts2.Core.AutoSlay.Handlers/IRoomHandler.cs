using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.AutoSlay.Handlers;

/// <summary>
/// Handler for a specific room type.
/// </summary>
public interface IRoomHandler : IHandler
{
	/// <summary>The room types this handler can handle.</summary>
	RoomType[] HandledTypes { get; }
}
