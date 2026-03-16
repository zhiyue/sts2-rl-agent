using MegaCrit.Sts2.Core.Entities.Multiplayer;

namespace MegaCrit.Sts2.Core.Multiplayer.Transport;

public abstract class NetClient
{
	protected INetClientHandler _handler;

	public abstract bool IsConnected { get; }

	public abstract ulong NetId { get; }

	public abstract ulong HostNetId { get; }

	protected NetClient(INetClientHandler handler)
	{
		_handler = handler;
	}

	public abstract void Update();

	public abstract void SendMessageToHost(byte[] bytes, int length, NetTransferMode mode, int channel = 0);

	public abstract void DisconnectFromHost(NetError reason, bool now = false);

	public abstract string? GetRawLobbyIdentifier();
}
