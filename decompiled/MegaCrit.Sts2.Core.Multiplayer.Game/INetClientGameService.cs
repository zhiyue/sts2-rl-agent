using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace MegaCrit.Sts2.Core.Multiplayer.Game;

/// <summary>
/// Provides additional host-related methods on top of the default game service.
/// </summary>
public interface INetClientGameService : INetGameService
{
	NetClient? NetClient { get; }
}
