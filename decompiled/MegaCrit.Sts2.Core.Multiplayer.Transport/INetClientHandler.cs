using MegaCrit.Sts2.Core.Entities.Multiplayer;

namespace MegaCrit.Sts2.Core.Multiplayer.Transport;

public interface INetClientHandler : INetHandler
{
	void OnConnectedToHost();

	void OnDisconnectedFromHost(ulong hostNetId, NetErrorInfo info);
}
