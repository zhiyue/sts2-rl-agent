using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Multiplayer;

namespace MegaCrit.Sts2.Core.Multiplayer.Transport;

public abstract class NetHost
{
	protected INetHostHandler _handler;

	public abstract IEnumerable<ulong> ConnectedPeerIds { get; }

	public abstract bool IsConnected { get; }

	public abstract ulong NetId { get; }

	protected NetHost(INetHostHandler handler)
	{
		_handler = handler;
	}

	public abstract void Update();

	public abstract void SetHostIsClosed(bool isClosed);

	public abstract void SendMessageToClient(ulong peerId, byte[] bytes, int length, NetTransferMode mode, int channel = 0);

	public abstract void SendMessageToAll(byte[] bytes, int length, NetTransferMode mode, int channel = 0);

	public abstract void DisconnectClient(ulong peerId, NetError reason, bool now = false);

	public abstract void StopHost(NetError reason, bool now = false);

	public abstract string? GetRawLobbyIdentifier();
}
