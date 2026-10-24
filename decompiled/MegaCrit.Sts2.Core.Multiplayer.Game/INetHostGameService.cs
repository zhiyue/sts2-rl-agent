using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace MegaCrit.Sts2.Core.Multiplayer.Game;

/// <summary>
/// Provides additional host-related methods on top of the default game service.
/// </summary>
public interface INetHostGameService : INetGameService
{
	IReadOnlyList<NetClientData> ConnectedPeers { get; }

	NetHost? NetHost { get; }

	/// <summary>
	/// Event called when a new client connects to the host.
	/// </summary>
	event Action<ulong>? ClientConnected;

	/// <summary>
	/// Event called when a client disconnects from the host, either voluntarily or because we disconnected them.
	/// </summary>
	event Action<ulong, NetErrorInfo>? ClientDisconnected;

	/// <summary>
	/// Disconnects a client from the host.
	/// </summary>
	/// <param name="peerId">The client to disconnect.</param>
	/// <param name="reason">Why the client is being disconnected.</param>
	/// <param name="now">If true, the client will be forcibly disconnected. The reason may not make it to the client.</param>
	void DisconnectClient(ulong peerId, NetError reason, bool now = false);

	/// <summary>
	/// Marks a client as ready to receive broadcasted messages.
	/// During the initial setup process, we don't want clients to receive broadcast messages as they may not be ready
	/// for them. Call this when initial setup is done.
	/// </summary>
	void SetPeerReadyForBroadcasting(ulong peerId);
}
