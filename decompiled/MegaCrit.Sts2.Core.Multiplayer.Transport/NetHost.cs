using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Multiplayer;

namespace MegaCrit.Sts2.Core.Multiplayer.Transport;

public abstract class NetHost
{
	/// <summary>
	/// Delegate to notify about events.
	/// </summary>
	protected INetHostHandler _handler;

	/// <summary>
	/// The IDs of currently connected clients.
	/// </summary>
	public abstract IEnumerable<ulong> ConnectedPeerIds { get; }

	/// <summary>
	/// True if clients are able to connect and receive messages, false otherwise.
	/// </summary>
	public abstract bool IsConnected { get; }

	/// <summary>
	/// The network ID of the host.
	/// </summary>
	public abstract ulong NetId { get; }

	protected NetHost(INetHostHandler handler)
	{
		_handler = handler;
	}

	/// <summary>
	/// Should be called at regular intervals to send and receive messages.
	/// </summary>
	public abstract void Update();

	/// <summary>
	/// If called with true, the host will be considered closed.
	/// What this means is platform-dependent. On Steam, this prevents other people from seeing the lobby, but existing
	/// clients are unaffected.
	/// </summary>
	public abstract void SetHostIsClosed(bool isClosed);

	/// <summary>
	/// Sends a message to a specific client.
	/// </summary>
	/// <param name="peerId">The ID of the client to send to.</param>
	/// <param name="bytes">The byte content of the message.</param>
	/// <param name="length">The amount of bytes, starting from index 0, to send from bytes.</param>
	/// <param name="mode">Method of transfer (reliable or unreliable).</param>
	/// <param name="channel">The channel to use to send the message.</param>
	public abstract void SendMessageToClient(ulong peerId, byte[] bytes, int length, NetTransferMode mode, int channel = 0);

	/// <summary>
	/// Broadcasts a message to all clients.
	/// </summary>
	/// <param name="bytes">The byte content of the message.</param>
	/// <param name="length">The amount of bytes, starting from index 0, to send from bytes.</param>
	/// <param name="mode">Method of transfer (reliable or unreliable).</param>
	/// <param name="channel">The channel to use to send the message.</param>
	public abstract void SendMessageToAll(byte[] bytes, int length, NetTransferMode mode, int channel = 0);

	/// <summary>
	/// Disconnects a client.
	/// </summary>
	/// <param name="peerId">The ID of the client to disconnect.</param>
	/// <param name="reason">THe reason to send to the client accompanying the disconnection.</param>
	/// <param name="now">If true, the client will be forcibly and immediately disconnected. If false, the client is
	/// disconnected at some point in the near future.</param>
	public abstract void DisconnectClient(ulong peerId, NetError reason, bool now = false);

	/// <summary>
	/// Disconnecst all clients and stops listening for new connections.
	/// </summary>
	/// <param name="reason">The reason that the host is shutting down. Sent to connected clients.</param>
	/// <param name="now">If true, all clients will be forcibly and immediately disconnected. If false, clients are
	/// disconnected at some point in the near future.</param>
	public abstract void StopHost(NetError reason, bool now = false);

	/// <summary>
	/// Returns a unique identifier for the lobby that this host is advertising.
	/// Returns null if the host has stopped or not yet started hosting.
	/// This returns a platform-specific identifier for use with rich presence. Feel free to refactor later if more
	/// info or more specific info is needed.
	/// </summary>
	public abstract string? GetRawLobbyIdentifier();
}
