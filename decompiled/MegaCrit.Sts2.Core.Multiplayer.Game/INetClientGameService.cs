using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace MegaCrit.Sts2.Core.Multiplayer.Game;

public interface INetClientGameService : INetGameService
{
	NetClient? NetClient { get; }
}
