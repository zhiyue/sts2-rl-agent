using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Game.PeerInput;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Lobby;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;

public class LoadRunLobby
{
	private struct ConnectingPlayer : IEquatable<ConnectingPlayer>
	{
		public ulong id;

		public CancellationTokenSource timeoutCancelToken;

		public bool Equals(ConnectingPlayer other)
		{
			if (id == other.id)
			{
				return timeoutCancelToken.Equals(other.timeoutCancelToken);
			}
			return false;
		}

		public override bool Equals(object? obj)
		{
			if (obj is ConnectingPlayer other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(id, timeoutCancelToken);
		}
	}

	private readonly Logger _logger;

	private readonly List<ConnectingPlayer> _connectingPlayers = new List<ConnectingPlayer>();

	private bool _isBeginningRun;

	private readonly HashSet<ulong> _readyPlayers = new HashSet<ulong>();

	public INetGameService NetService { get; }

	public ILoadRunLobbyListener LobbyListener { get; }

	public PeerInputSynchronizer InputSynchronizer { get; }

	public SerializableRun Run { get; }

	public HashSet<ulong> ConnectedPlayerIds { get; } = new HashSet<ulong>();

	public GameMode GameMode
	{
		get
		{
			if (Run.Modifiers.Count <= 0)
			{
				return GameMode.Standard;
			}
			if (!Run.DailyTime.HasValue)
			{
				return GameMode.Custom;
			}
			return GameMode.Daily;
		}
	}

	public int HandshakeTimeout { get; set; } = 10000;

	public LoadRunLobby(INetGameService netService, ILoadRunLobbyListener lobbyListener, SerializableRun runSave)
	{
		Run = runSave;
		NetService = netService;
		LobbyListener = lobbyListener;
		InputSynchronizer = new PeerInputSynchronizer(NetService);
		_logger = new Logger("LoadRunLobby", LogType.Network);
		NetService.RegisterMessageHandler<ClientLoadJoinRequestMessage>(HandleClientLoadJoinRequestMessage);
		NetService.RegisterMessageHandler<ClientLobbyJoinRequestMessage>(HandleClientLobbyJoinRequestMessage);
		NetService.RegisterMessageHandler<ClientRejoinRequestMessage>(HandleClientRejoinRequestMessage);
		NetService.RegisterMessageHandler<PlayerReconnectedMessage>(HandlePlayerReconnectedMessage);
		NetService.RegisterMessageHandler<PlayerLeftMessage>(HandlePlayerLeftMessage);
		NetService.RegisterMessageHandler<LobbyPlayerSetReadyMessage>(HandlePlayerReadyMessage);
		NetService.RegisterMessageHandler<LobbyBeginLoadedRunMessage>(HandleLobbyBeginRunMessage);
		NetService.Disconnected += OnDisconnected;
		if (NetService.Type == NetGameType.Host)
		{
			INetHostGameService netHostGameService = (INetHostGameService)netService;
			netHostGameService.ClientConnected += OnConnectedToClientAsHost;
			netHostGameService.ClientDisconnected += OnDisconnectedFromClientAsHost;
		}
	}

	public LoadRunLobby(INetGameService netService, ILoadRunLobbyListener lobbyListener, ClientLoadJoinResponseMessage message)
		: this(netService, lobbyListener, message.serializableRun)
	{
		foreach (ulong item in message.playersAlreadyConnected)
		{
			ConnectedPlayerIds.Add(item);
		}
	}

	public void CleanUp(bool disconnectSession)
	{
		NetService.UnregisterMessageHandler<ClientLoadJoinRequestMessage>(HandleClientLoadJoinRequestMessage);
		NetService.UnregisterMessageHandler<ClientLobbyJoinRequestMessage>(HandleClientLobbyJoinRequestMessage);
		NetService.UnregisterMessageHandler<ClientRejoinRequestMessage>(HandleClientRejoinRequestMessage);
		NetService.UnregisterMessageHandler<PlayerReconnectedMessage>(HandlePlayerReconnectedMessage);
		NetService.UnregisterMessageHandler<PlayerLeftMessage>(HandlePlayerLeftMessage);
		NetService.UnregisterMessageHandler<LobbyPlayerSetReadyMessage>(HandlePlayerReadyMessage);
		NetService.UnregisterMessageHandler<LobbyBeginLoadedRunMessage>(HandleLobbyBeginRunMessage);
		if (disconnectSession)
		{
			if (NetService.IsConnected)
			{
				NetService.Disconnect(NetError.Quit);
			}
			InputSynchronizer.Dispose();
		}
		NetService.Disconnected -= OnDisconnected;
		if (NetService.Type == NetGameType.Host)
		{
			INetHostGameService netHostGameService = (INetHostGameService)NetService;
			netHostGameService.ClientConnected -= OnConnectedToClientAsHost;
			netHostGameService.ClientDisconnected -= OnDisconnectedFromClientAsHost;
		}
	}

	public void AddLocalHostPlayer()
	{
		if (NetService.Type == NetGameType.Client)
		{
			throw new InvalidOperationException("Tried to add local host player as client!");
		}
		_logger.Context = $"Lobby ({NetService.NetId})";
		ConnectedPlayerIds.Add(NetService.NetId);
		LobbyListener.PlayerConnected(NetService.NetId);
	}

	private void HandleClientLoadJoinRequestMessage(ClientLoadJoinRequestMessage message, ulong senderId)
	{
		if (NetService.Type != NetGameType.Host)
		{
			throw new InvalidOperationException("Received ClientLoadJoinRequestMessage as non-host!");
		}
		INetHostGameService netHostGameService = (INetHostGameService)NetService;
		try
		{
			if (Run.Players.FindIndex((SerializablePlayer p) => p.NetId == senderId) < 0)
			{
				_logger.Warn($"Client {senderId} sent ClientLoadJoinRequestMessage but they are not in the loaded run!");
				netHostGameService.DisconnectClient(senderId, NetError.NotInSaveGame);
				return;
			}
			_logger.Info($"Received ClientLoadJoinRequestMessage for {senderId}");
			ConnectedPlayerIds.Add(senderId);
			LobbyListener.PlayerConnected(senderId);
			ClientLoadJoinResponseMessage message2 = new ClientLoadJoinResponseMessage
			{
				serializableRun = Run,
				playersAlreadyConnected = ConnectedPlayerIds.ToList()
			};
			_logger.Debug($"Sending ClientLoadJoinResponseMessage to {senderId}");
			netHostGameService.SendMessage(message2, senderId);
			netHostGameService.SetPeerReadyForBroadcasting(senderId);
			PlayerReconnectedMessage message3 = new PlayerReconnectedMessage
			{
				playerId = senderId
			};
			foreach (ulong connectedPlayerId in ConnectedPlayerIds)
			{
				if (connectedPlayerId != senderId && connectedPlayerId != NetService.NetId)
				{
					_logger.Debug($"Sending PlayerReconnectedMessage to {connectedPlayerId}");
					netHostGameService.SendMessage(message3, connectedPlayerId);
				}
			}
			RemoveConnectingPlayer(senderId);
		}
		catch
		{
			netHostGameService.DisconnectClient(senderId, NetError.InternalError);
			throw;
		}
	}

	private void HandleClientLobbyJoinRequestMessage(ClientLobbyJoinRequestMessage _, ulong senderId)
	{
		if (NetService.Type != NetGameType.Host)
		{
			throw new InvalidOperationException("Received ClientLobbyJoinRequestMessage as non-host!");
		}
		_logger.Info($"Received invalid ClientLobbyJoinRequestMessage for {senderId}");
		INetHostGameService netHostGameService = (INetHostGameService)NetService;
		netHostGameService.DisconnectClient(senderId, NetError.InvalidJoin);
	}

	private void HandleClientRejoinRequestMessage(ClientRejoinRequestMessage _, ulong senderId)
	{
		if (NetService.Type != NetGameType.Host)
		{
			throw new InvalidOperationException("Received ClientRejoinRequestMessage as non-host!");
		}
		_logger.Info($"Received invalid ClientRejoinRequestMessage for {senderId}");
		INetHostGameService netHostGameService = (INetHostGameService)NetService;
		netHostGameService.DisconnectClient(senderId, NetError.InvalidJoin);
	}

	private void HandlePlayerReconnectedMessage(PlayerReconnectedMessage message, ulong _)
	{
		_logger.Debug($"Received PlayerReconnectedMessage with player ID {message.playerId}");
		ConnectedPlayerIds.Add(message.playerId);
		LobbyListener.PlayerConnected(message.playerId);
	}

	private void HandlePlayerLeftMessage(PlayerLeftMessage message, ulong senderId)
	{
		_logger.Debug($"Received PlayerLeftMessage for {message.playerId}");
		if (ConnectedPlayerIds.Contains(message.playerId))
		{
			ConnectedPlayerIds.Remove(message.playerId);
			InputSynchronizer.OnPlayerDisconnected(message.playerId);
			LobbyListener.RemotePlayerDisconnected(message.playerId);
		}
	}

	private void HandlePlayerReadyMessage(LobbyPlayerSetReadyMessage message, ulong senderId)
	{
		_logger.Debug($"Received {"LobbyPlayerSetReadyMessage"} for player {senderId} with value {message.ready}");
		if (message.ready)
		{
			if (_readyPlayers.Add(senderId))
			{
				LobbyListener.PlayerReadyChanged(senderId);
			}
		}
		else if (_readyPlayers.Remove(senderId))
		{
			LobbyListener.PlayerReadyChanged(senderId);
		}
		BeginRunIfAllPlayersReady();
	}

	private void HandleLobbyBeginRunMessage(LobbyBeginLoadedRunMessage message, ulong senderId)
	{
		_logger.Debug("Received LobbyBeginLoadedRunMessage");
		_isBeginningRun = true;
		LobbyListener.BeginRun();
	}

	private async Task TryBeginRun()
	{
		if (NetService.Type == NetGameType.Client)
		{
			throw new InvalidOperationException("Can only begin run as host!");
		}
		if (!(await LobbyListener.ShouldAllowRunToBegin()))
		{
			SetReady(ready: false);
			return;
		}
		NetService.SendMessage(default(LobbyBeginLoadedRunMessage));
		_isBeginningRun = true;
		LobbyListener.BeginRun();
		if (NetService.Type == NetGameType.Host)
		{
			INetHostGameService netHostGameService = (INetHostGameService)NetService;
			netHostGameService.NetHost?.SetHostIsClosed(isClosed: true);
		}
	}

	public void SetReady(bool ready)
	{
		if (ready)
		{
			_readyPlayers.Add(NetService.NetId);
		}
		else
		{
			_readyPlayers.Remove(NetService.NetId);
		}
		LobbyPlayerSetReadyMessage message = new LobbyPlayerSetReadyMessage
		{
			ready = ready
		};
		NetService.SendMessage(message);
		LobbyListener.PlayerReadyChanged(NetService.NetId);
		BeginRunIfAllPlayersReady();
	}

	public bool IsPlayerReady(ulong playerId)
	{
		return _readyPlayers.Contains(playerId);
	}

	private void BeginRunIfAllPlayersReady()
	{
		if (_connectingPlayers.Count <= 0 && (NetService.Type == NetGameType.Host || NetService.Type == NetGameType.Singleplayer) && !ConnectedPlayerIds.Except(_readyPlayers).Any())
		{
			TaskHelper.RunSafely(TryBeginRun());
		}
	}

	private void OnConnectedToClientAsHost(ulong playerId)
	{
		_logger.Info($"Client {playerId} connected. Sending initial game info message");
		InitialGameInfoMessage message = InitialGameInfoMessage.Basic();
		message.sessionState = RunSessionState.InLoadedLobby;
		message.gameMode = GameMode;
		if (_isBeginningRun)
		{
			message.connectionFailureReason = ConnectionFailureReason.RunInProgress;
			NetService.SendMessage(message, playerId);
			_logger.Warn($"Client {playerId} connected but we are already beginning the run!");
			((INetHostGameService)NetService).DisconnectClient(playerId, NetError.RunInProgress);
		}
		else if (Run.Players.FindIndex((SerializablePlayer p) => p.NetId == playerId) < 0)
		{
			message.connectionFailureReason = ConnectionFailureReason.NotInSaveGame;
			NetService.SendMessage(message, playerId);
			_logger.Warn($"Client {playerId} connected but they were not in the loaded game!");
			((INetHostGameService)NetService).DisconnectClient(playerId, NetError.NotInSaveGame);
		}
		else
		{
			ConnectingPlayer connectingPlayer = new ConnectingPlayer
			{
				id = playerId,
				timeoutCancelToken = new CancellationTokenSource()
			};
			_connectingPlayers.Add(connectingPlayer);
			NetService.SendMessage(message, playerId);
			TaskHelper.RunSafely(BeginHandshakeTimeout(connectingPlayer));
		}
	}

	private async Task BeginHandshakeTimeout(ConnectingPlayer connectingPlayer)
	{
		await Task.Delay(HandshakeTimeout, connectingPlayer.timeoutCancelToken.Token);
		if (!connectingPlayer.timeoutCancelToken.IsCancellationRequested)
		{
			int num = _connectingPlayers.IndexOf(connectingPlayer);
			if (num >= 0)
			{
				Log.Info($"Disconnecting player {connectingPlayer.id} because they did not respond to the initial game join handshake within {HandshakeTimeout}ms");
				INetHostGameService netHostGameService = (INetHostGameService)NetService;
				netHostGameService.DisconnectClient(connectingPlayer.id, NetError.HandshakeTimeout);
			}
		}
	}

	private void OnDisconnectedFromClientAsHost(ulong playerId, NetErrorInfo info)
	{
		if (ConnectedPlayerIds.Contains(playerId))
		{
			_logger.Info($"Client {playerId} disconnected, reason: {info.GetReason()}");
			PlayerLeftMessage message = new PlayerLeftMessage
			{
				playerId = playerId
			};
			NetService.SendMessage(message);
			ConnectedPlayerIds.Remove(playerId);
			_readyPlayers.Remove(playerId);
			RemoveConnectingPlayer(playerId);
			InputSynchronizer.OnPlayerDisconnected(message.playerId);
			LobbyListener.RemotePlayerDisconnected(playerId);
			BeginRunIfAllPlayersReady();
		}
	}

	private void RemoveConnectingPlayer(ulong playerId)
	{
		for (int i = 0; i < _connectingPlayers.Count; i++)
		{
			if (_connectingPlayers[i].id == playerId)
			{
				_connectingPlayers[i].timeoutCancelToken.Cancel();
				_connectingPlayers.RemoveAt(i);
				i--;
			}
		}
	}

	private void OnDisconnected(NetErrorInfo info)
	{
		_logger.Info($"Disconnected from host, reason: {info.GetReason()}");
		ConnectedPlayerIds.Clear();
		LobbyListener.LocalPlayerDisconnected(info);
	}
}
