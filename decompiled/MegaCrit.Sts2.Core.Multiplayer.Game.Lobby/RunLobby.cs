using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Lobby;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;

public class RunLobby
{
	private readonly Logger _logger;

	private readonly IPlayerCollection _playerCollection;

	private readonly IRunLobbyListener _lobbyListener;

	private readonly INetGameService _netService;

	private readonly HashSet<ulong> _connectedPlayerIds = new HashSet<ulong>();

	public IReadOnlyCollection<ulong> ConnectedPlayerIds => _connectedPlayerIds;

	public GameMode GameMode { get; }

	public event Action<ulong>? PlayerRejoined;

	public event Action<ulong>? RemotePlayerDisconnected;

	public event Action? LocalPlayerDisconnected;

	public RunLobby(GameMode gameMode, INetGameService netService, IRunLobbyListener lobbyListener, IPlayerCollection playerCollection, IEnumerable<ulong> connectedPlayerIds)
	{
		GameMode = gameMode;
		_netService = netService;
		_lobbyListener = lobbyListener;
		_playerCollection = playerCollection;
		_logger = new Logger("RunLobby", LogType.Network);
		_netService.RegisterMessageHandler<ClientLobbyJoinRequestMessage>(HandleClientLobbyJoinRequestMessage);
		_netService.RegisterMessageHandler<ClientLoadJoinRequestMessage>(HandleClientLoadJoinRequestMessage);
		_netService.RegisterMessageHandler<ClientRejoinRequestMessage>(HandleClientRejoinRequestMessage);
		_netService.RegisterMessageHandler<PlayerRejoinedMessage>(HandlePlayerRejoinedMessage);
		_netService.RegisterMessageHandler<PlayerLeftMessage>(HandlePlayerLeftMessage);
		_netService.RegisterMessageHandler<RunAbandonedMessage>(HandleRunAbandonedMessage);
		foreach (ulong connectedPlayerId in connectedPlayerIds)
		{
			_connectedPlayerIds.Add(connectedPlayerId);
		}
		_netService.Disconnected += OnDisconnected;
		if (_netService.Type == NetGameType.Host && netService is NetHostGameService netHostGameService)
		{
			netHostGameService.ClientConnected += OnConnectedToClientAsHost;
			netHostGameService.ClientDisconnected += OnDisconnectedFromClientAsHost;
		}
	}

	public void Dispose()
	{
		_netService.UnregisterMessageHandler<ClientLobbyJoinRequestMessage>(HandleClientLobbyJoinRequestMessage);
		_netService.UnregisterMessageHandler<ClientLoadJoinRequestMessage>(HandleClientLoadJoinRequestMessage);
		_netService.UnregisterMessageHandler<ClientRejoinRequestMessage>(HandleClientRejoinRequestMessage);
		_netService.UnregisterMessageHandler<PlayerRejoinedMessage>(HandlePlayerRejoinedMessage);
		_netService.UnregisterMessageHandler<PlayerLeftMessage>(HandlePlayerLeftMessage);
		_netService.UnregisterMessageHandler<RunAbandonedMessage>(HandleRunAbandonedMessage);
		_netService.Disconnected -= OnDisconnected;
		if (_netService.Type == NetGameType.Host && _netService is NetHostGameService netHostGameService)
		{
			netHostGameService.ClientConnected -= OnConnectedToClientAsHost;
			netHostGameService.ClientDisconnected -= OnDisconnectedFromClientAsHost;
		}
	}

	private void HandleClientRejoinRequestMessage(ClientRejoinRequestMessage message, ulong senderId)
	{
		if (_netService.Type != NetGameType.Host)
		{
			throw new InvalidOperationException("Received ClientRejoinRequestMessage as non-host!");
		}
		_logger.Info($"Received ClientRejoinRequestMessage for {senderId}");
		NetHostGameService netHostGameService = (NetHostGameService)_netService;
		if (_playerCollection.GetPlayer(senderId) == null)
		{
			netHostGameService.DisconnectClient(senderId, NetError.RunInProgress);
			return;
		}
		ClientRejoinResponseMessage rejoinMessage = _lobbyListener.GetRejoinMessage();
		netHostGameService.SendMessage(rejoinMessage, senderId);
		netHostGameService.SetPeerReadyForBroadcasting(senderId);
		PlayerRejoinedMessage message2 = new PlayerRejoinedMessage
		{
			playerId = senderId
		};
		foreach (NetClientData connectedPeer in netHostGameService.ConnectedPeers)
		{
			if (connectedPeer.readyForBroadcasting && connectedPeer.peerId != senderId)
			{
				netHostGameService.SendMessage(message2, connectedPeer.peerId);
			}
		}
		_connectedPlayerIds.Add(senderId);
		this.PlayerRejoined?.Invoke(senderId);
	}

	private void HandleClientLobbyJoinRequestMessage(ClientLobbyJoinRequestMessage _, ulong senderId)
	{
		if (_netService.Type != NetGameType.Host)
		{
			throw new InvalidOperationException("Received ClientLobbyJoinRequestMessage as non-host!");
		}
		_logger.Info($"Received invalid ClientLobbyJoinRequestMessage for {senderId}");
		NetHostGameService netHostGameService = (NetHostGameService)_netService;
		netHostGameService.DisconnectClient(senderId, NetError.InvalidJoin);
	}

	private void HandleClientLoadJoinRequestMessage(ClientLoadJoinRequestMessage _, ulong senderId)
	{
		if (_netService.Type != NetGameType.Host)
		{
			throw new InvalidOperationException("Received ClientLoadJoinRequestMessage as non-host!");
		}
		_logger.Info($"Received invalid ClientLoadJoinRequestMessage for {senderId}");
		NetHostGameService netHostGameService = (NetHostGameService)_netService;
		netHostGameService.DisconnectClient(senderId, NetError.InvalidJoin);
	}

	private void HandlePlayerRejoinedMessage(PlayerRejoinedMessage message, ulong _)
	{
		_logger.Debug($"Received PlayerRejoinedMessage for {message.playerId}");
		_connectedPlayerIds.Add(message.playerId);
		this.PlayerRejoined?.Invoke(message.playerId);
	}

	private void HandlePlayerLeftMessage(PlayerLeftMessage message, ulong _)
	{
		_logger.Debug($"Received PlayerLeftMessage for {message.playerId}");
		_connectedPlayerIds.Remove(message.playerId);
		this.RemotePlayerDisconnected?.Invoke(message.playerId);
	}

	public void AbandonRun()
	{
		if (_netService.Type != NetGameType.Host)
		{
			throw new InvalidOperationException("Abandon run can only be called as host!");
		}
		_logger.Info("Abandoning run as host");
		NetHostGameService netHostGameService = (NetHostGameService)_netService;
		netHostGameService.SendMessage(default(RunAbandonedMessage));
		_lobbyListener.RunAbandoned();
		_netService.Disconnect(NetError.HostAbandoned);
	}

	private void HandleRunAbandonedMessage(RunAbandonedMessage message, ulong _)
	{
		_logger.Debug("Received RunAbandonedMessage");
		_lobbyListener.RunAbandoned();
		_netService.Disconnect(NetError.HostAbandoned);
	}

	private void OnConnectedToClientAsHost(ulong playerId)
	{
		_logger.Info($"Player {playerId} connected to host.");
		InitialGameInfoMessage message = InitialGameInfoMessage.Basic();
		message.sessionState = RunSessionState.Running;
		message.gameMode = GameMode;
		if (_playerCollection.GetPlayer(playerId) == null)
		{
			_logger.Warn($"Client {playerId} connected but they are not in the run!");
			message.connectionFailureReason = ConnectionFailureReason.RunInProgress;
			_netService.SendMessage(message, playerId);
			NetHostGameService netHostGameService = (NetHostGameService)_netService;
			netHostGameService.DisconnectClient(playerId, NetError.RunInProgress);
		}
		else
		{
			_netService.SendMessage(message, playerId);
		}
	}

	private void OnDisconnectedFromClientAsHost(ulong playerId, NetErrorInfo info)
	{
		_logger.Info($"Player {playerId} disconnected from host. Is in connected players: {_connectedPlayerIds.Contains(playerId)}. Reason: {info.GetReason()}");
		if (_connectedPlayerIds.Contains(playerId))
		{
			PlayerLeftMessage message = new PlayerLeftMessage
			{
				playerId = playerId
			};
			_netService.SendMessage(message);
			_connectedPlayerIds.Remove(playerId);
			this.RemotePlayerDisconnected?.Invoke(playerId);
		}
	}

	private void OnDisconnected(NetErrorInfo info)
	{
		_logger.Info($"Disconnected. Reason: {info.GetReason()}");
		_connectedPlayerIds.Clear();
		_lobbyListener.LocalPlayerDisconnected(info);
		this.LocalPlayerDisconnected?.Invoke();
	}
}
