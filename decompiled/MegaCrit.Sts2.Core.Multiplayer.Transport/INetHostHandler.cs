using MegaCrit.Sts2.Core.Entities.Multiplayer;

namespace MegaCrit.Sts2.Core.Multiplayer.Transport;

public interface INetHostHandler : INetHandler
{
	void OnPeerConnected(ulong peerId);

	void OnPeerDisconnected(ulong peerId, NetErrorInfo info);

	void OnDisconnected(NetErrorInfo info);
}
