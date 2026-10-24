using System;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Quality;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Platform;

namespace MegaCrit.Sts2.Core.Multiplayer.Game;

/// <summary>
/// Represents an abstraction of the networking API used to send and receive messages.
/// Host/join methods are specific to the game manager type on purpose, as those cannot be abstracted. However, once
/// connected, this interface can be used as a generic way to send/receive messages regardless of whether we are a
/// host or a client.
/// </summary>
public interface INetGameService
{
	/// <summary>
	/// The player ID of the local player.
	/// </summary>
	ulong NetId { get; }

	/// <summary>
	/// True when the net service is in a state where it can send messages.
	/// </summary>
	bool IsConnected { get; }

	/// <summary>
	/// True if the last call to SetGameLoading was called with true, false otherwise.
	/// </summary>
	bool IsGameLoading { get; }

	/// <summary>
	/// The type of manager this is (host, client, or singleplayer).
	/// </summary>
	NetGameType Type { get; }

	/// <summary>
	/// The networking backend we are using to connect to players.
	/// Note that if we ever consider cross-platform, this likely needs to be contained in the player ID, not here.
	/// </summary>
	PlatformType Platform { get; }

	/// <summary>
	/// Called when the network connection is disconnected and the sockets are disposed.
	/// On host, this occurs when we stop hosting. On clients, this can occur when we quit, our connection drops, or the
	/// host disconnects from us.
	/// </summary>
	event Action<NetErrorInfo>? Disconnected;

	/// <summary>
	/// Sends a message to a specific peer. Note that this should only be used as a host.
	/// </summary>
	/// <param name="message">The message to send.</param>
	/// <param name="playerId">The specific player to send to.</param>
	void SendMessage<T>(T message, ulong playerId) where T : INetMessage;

	/// <summary>
	/// Sends a message to all connected peers.
	/// On the host, this sends a message to all clients. On a client, this sends a message to only the host, but if the
	/// message has the broadcast flag set, the host will replicate it to all other clients.
	/// </summary>
	/// <param name="message">The message to send.</param>
	void SendMessage<T>(T message) where T : INetMessage;

	/// <summary>
	/// Registers a message handler delegate which will be called when a message of type T is received.
	/// </summary>
	/// <param name="messageHandlerDelegate">The delegate to call.</param>
	void RegisterMessageHandler<T>(MessageHandlerDelegate<T> messageHandlerDelegate) where T : INetMessage;

	/// <summary>
	/// Unregisters a message handler delegate that was previously registered. Does nothing if it was not registered.
	/// </summary>
	/// <param name="messageHandlerDelegate">The delegate to unregister.</param>
	void UnregisterMessageHandler<T>(MessageHandlerDelegate<T> messageHandlerDelegate) where T : INetMessage;

	/// <summary>
	/// Method which should be called at regular intervals.
	/// Messages, connections, or disconnections will not be processed unless this is called.
	/// </summary>
	void Update();

	/// <summary>
	/// Disconnects us from all connected peers.
	/// </summary>
	/// <param name="reason">The reason to pass to the remote end to indicate why we quit.</param>
	/// <param name="now">If false, then the connection is allowed to wait until messages are finished sending.
	/// Otherwise, the connection is terminated immediately.</param>
	void Disconnect(NetError reason, bool now = false);

	/// <summary>
	/// Returns the connection statistics for a given peer.
	/// </summary>
	ConnectionStats? GetStatsForPeer(ulong peerId);

	/// <summary>
	/// DO NOT use this directly in most circumstances! Instead, use NetLoadingHandle.
	/// Tells the net service that the game is in a loading state, and that Update may be called irregularly.
	/// See <see cref="M:MegaCrit.Sts2.Core.Multiplayer.Quality.NetQualityTracker.SetIsLoading(System.Boolean)" /> for why this is needed.
	/// </summary>
	void SetGameLoading(bool isLoading);

	/// <summary>
	/// Begins buffering all messages.
	/// When the game is transitioning from one state to another, it will unregister and register message handlers. If
	/// this takes time, then there's a chance that a message will be sent during this window that the game doesn't know
	/// how to handle yet. This method should be called with true after a sync point and then false after the game is
	/// ready to handle messages again.
	/// </summary>
	void SetBufferMessages(bool bufferMessages);

	/// <summary>
	/// Returns a unique identifier for the lobby that this service is connected to.
	/// Returns null if the net service is not yet connected.
	/// This returns a platform-specific identifier for use with rich presence. Feel free to refactor later if more
	/// info or more specific info is needed.
	/// </summary>
	string? GetRawLobbyIdentifier();
}
