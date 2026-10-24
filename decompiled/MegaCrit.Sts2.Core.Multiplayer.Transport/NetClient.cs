using MegaCrit.Sts2.Core.Entities.Multiplayer;

namespace MegaCrit.Sts2.Core.Multiplayer.Transport;

public abstract class NetClient
{
	/// <summary>
	/// Delegate to notify about events.
	/// </summary>
	protected INetClientHandler _handler;

	/// <summary>
	/// True this client is connected to a host, false otherwise.
	/// </summary>
	public abstract bool IsConnected { get; }

	/// <summary>
	/// The network ID of the client.
	/// </summary>
	public abstract ulong NetId { get; }

	/// <summary>
	/// The network ID of the host we are connected to.
	/// </summary>
	public abstract ulong HostNetId { get; }

	protected NetClient(INetClientHandler handler)
	{
		_handler = handler;
	}

	/// <summary>
	/// Should be called at regular intervals to send and receive messages.
	/// </summary>
	public abstract void Update();

	/// <summary>
	/// Sends a message to the host.
	/// </summary>
	/// <param name="bytes">The byte content of the message.</param>
	/// <param name="length">The amount of bytes, starting from index 0, to send from bytes.</param>
	/// <param name="mode">Method of transfer (reliable or unreliable).</param>
	/// <param name="channel">The channel to use to send the message.</param>
	public abstract void SendMessageToHost(byte[] bytes, int length, NetTransferMode mode, int channel = 0);

	/// <summary>
	/// Disconnects from the host.
	/// </summary>
	/// <param name="reason">THe reason to send to the client accompanying the disconnection.</param>
	/// <param name="now">If true, we will be forcibly and immediately disconnected. If false, we are disconnected at
	/// some point in the near future.</param>
	public abstract void DisconnectFromHost(NetError reason, bool now = false);

	/// <summary>
	/// Returns a unique identifier for the lobby that the client is connected to.
	/// Returns null if the client is not yet connected or has been disconnected.
	/// This returns a platform-specific identifier for use with rich presence. Feel free to refactor later if more
	/// info or more specific info is needed.
	/// </summary>
	public abstract string? GetRawLobbyIdentifier();
}
