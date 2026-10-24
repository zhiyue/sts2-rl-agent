using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Platform.Steam;
using Steamworks;

namespace MegaCrit.Sts2.Core.Multiplayer.Transport.Steam;

public class SteamClient : NetClient
{
	private struct ConnectionResult
	{
		public HSteamNetConnection? connection;

		public SteamDisconnectionReason? disconnectionReason;

		public string? debugReason;
	}

	private CSteamID? _lobbyId;

	private CSteamID _hostNetId;

	private HSteamNetConnection? _conn;

	private bool _isConnected;

	private TaskCompletionSource<ConnectionResult>? _connectingTaskCompletionSource;

	private Callback<SteamNetConnectionStatusChangedCallback_t>? _netStatusChangedCallback;

	private readonly Logger _logger = new Logger("SteamClient", LogType.Network);

	public override bool IsConnected => _isConnected;

	public override ulong NetId => SteamUser.GetSteamID().m_SteamID;

	public override ulong HostNetId => _hostNetId.m_SteamID;

	public CSteamID? LobbyId => _lobbyId;

	public SteamClient(INetClientHandler handler)
		: base(handler)
	{
	}

	public Task<NetErrorInfo?> ConnectToLobbyOwnedByFriend(ulong steamPlayerId, CancellationToken cancelToken = default(CancellationToken))
	{
		_logger.Info($"Attempting to connect to lobby. Our player id: {NetId}. Other player's id: {steamPlayerId}.");
		if (!SteamFriends.GetFriendGamePlayed(new CSteamID(steamPlayerId), out var pFriendGameInfo))
		{
			Log.Warn("Tried to join a friend's Steam game, but they're not playing any game. Likely a stale invite");
			return Task.FromResult((NetErrorInfo?)new NetErrorInfo(NetError.InvalidJoin, selfInitiated: false));
		}
		if (pFriendGameInfo.m_gameID != new CGameID(2868840uL) || pFriendGameInfo.m_steamIDLobby.m_SteamID == 0L)
		{
			Log.Warn("Tried to join a friend's Steam game, but they are not playing STS2 or they are not in a lobby. Likely a stale invite");
			return Task.FromResult((NetErrorInfo?)new NetErrorInfo(NetError.InvalidJoin, selfInitiated: false));
		}
		return ConnectToLobby(pFriendGameInfo.m_steamIDLobby.m_SteamID, cancelToken);
	}

	public async Task<NetErrorInfo?> ConnectToLobby(ulong lobbyId, CancellationToken cancelToken = default(CancellationToken))
	{
		NetErrorInfo? result;
		await using (cancelToken.Register(CancelConnection))
		{
			SteamAPICall_t call = SteamMatchmaking.JoinLobby(new CSteamID(lobbyId));
			using SteamCallResult<LobbyEnter_t> callResult = new SteamCallResult<LobbyEnter_t>(call, cancelToken);
			_logger.Debug($"Attempting to enter lobby {lobbyId}");
			LobbyEnter_t lobbyEnter_t = await callResult.Task;
			if (lobbyEnter_t.m_ulSteamIDLobby != lobbyId)
			{
				_logger.Error("Joined incorrect lobby?");
				result = new NetErrorInfo(NetError.InternalError, selfInitiated: false);
			}
			else
			{
				EChatRoomEnterResponse eChatRoomEnterResponse = (EChatRoomEnterResponse)lobbyEnter_t.m_EChatRoomEnterResponse;
				if (eChatRoomEnterResponse != EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
				{
					_logger.Error($"Failed to enter lobby, response: {eChatRoomEnterResponse}");
					result = new NetErrorInfo(eChatRoomEnterResponse);
				}
				else
				{
					_lobbyId = new CSteamID(lobbyEnter_t.m_ulSteamIDLobby);
					CSteamID lobbyOwnerId = SteamMatchmaking.GetLobbyOwner(_lobbyId.Value);
					SteamNetworkingIdentity identityRemote = lobbyOwnerId.ToNetId();
					_netStatusChangedCallback = new Callback<SteamNetConnectionStatusChangedCallback_t>(OnNetStatusChanged);
					_connectingTaskCompletionSource = new TaskCompletionSource<ConnectionResult>();
					_conn = SteamNetworkingSockets.ConnectP2P(ref identityRemote, 0, 0, null);
					_logger.Debug($"Connecting to user {lobbyOwnerId.m_SteamID}");
					ConnectionResult connectionResult = await _connectingTaskCompletionSource.Task;
					if (connectionResult.disconnectionReason.HasValue)
					{
						CleanupConnection((int)connectionResult.disconnectionReason.Value, connectionResult.debugReason);
						result = new NetErrorInfo(connectionResult.disconnectionReason.Value, connectionResult.debugReason, selfInitiated: false);
					}
					else if (_conn.Value.m_HSteamNetConnection != connectionResult.connection.Value.m_HSteamNetConnection)
					{
						_logger.Error("Got different connection back from OnNetStatusChanged than we expected!");
						SteamNetworkingSockets.CloseConnection(connectionResult.connection.Value, 0, null, bEnableLinger: false);
						CleanupConnection(1017, "Invalid OnNetStatusChanged hConn");
						result = new NetErrorInfo(NetError.InternalError, selfInitiated: false);
					}
					else
					{
						_connectingTaskCompletionSource = null;
						_isConnected = true;
						_hostNetId = lobbyOwnerId;
						_handler.OnConnectedToHost();
						_logger.Debug($"Successfully connected to host {lobbyOwnerId.m_SteamID}");
						result = null;
					}
				}
			}
		}
		return result;
	}

	private void OnNetStatusChanged(SteamNetConnectionStatusChangedCallback_t data)
	{
		_logger.Debug($"Connection status changed: {data.m_eOldState} -> {data.m_info.m_eState}");
		if (data.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected)
		{
			_logger.Debug("Steam connection accepted.");
			if (_connectingTaskCompletionSource == null)
			{
				_logger.Error("Connection was accepted while we were not waiting for it!");
				DisconnectFromHostInternal(SteamDisconnectionReason.InternalError, "Not Connecting", now: true, selfInitiated: false);
			}
			else
			{
				_connectingTaskCompletionSource.SetResult(new ConnectionResult
				{
					connection = data.m_hConn
				});
			}
		}
		else if (data.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally)
		{
			_logger.Info($"Steam connection closed because of problem. Reason: {data.m_info.m_eEndReason}, {data.m_info.m_szEndDebug}");
			HandleDisconnection((SteamDisconnectionReason)data.m_info.m_eEndReason, data.m_info.m_szEndDebug);
		}
		else if (data.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer)
		{
			_logger.Info($"Steam connection closed by host. Reason: {data.m_info.m_eEndReason}, {data.m_info.m_szEndDebug}");
			HandleDisconnection((SteamDisconnectionReason)data.m_info.m_eEndReason, data.m_info.m_szEndDebug);
		}
	}

	private void HandleDisconnection(SteamDisconnectionReason reason, string debugReason)
	{
		if (_connectingTaskCompletionSource != null)
		{
			_connectingTaskCompletionSource.SetResult(new ConnectionResult
			{
				disconnectionReason = reason,
				debugReason = debugReason
			});
		}
		else
		{
			DisconnectFromHostInternal(reason, debugReason, now: true, selfInitiated: false);
		}
	}

	public override void Update()
	{
		SteamUtil.ProcessMessages(_conn.Value, _handler, _logger);
	}

	public override void SendMessageToHost(byte[] bytes, int length, NetTransferMode mode, int channel = 0)
	{
		_logger.VeryDebug($"Sending {length} bytes to host");
		GCHandle gCHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
		try
		{
			long pOutMessageNumber;
			EResult eResult = SteamNetworkingSockets.SendMessageToConnection(_conn.Value, gCHandle.AddrOfPinnedObject(), (uint)length, SteamUtil.FlagsFromMode(mode), out pOutMessageNumber);
			if (eResult != EResult.k_EResultOK)
			{
				_logger.Warn($"Failed to send message length {length}: {eResult}");
			}
		}
		finally
		{
			gCHandle.Free();
		}
	}

	public override void DisconnectFromHost(NetError reason, bool now = false)
	{
		DisconnectFromHostInternal(reason.ToSteam(), string.Empty, now, selfInitiated: true);
	}

	private void CleanupLobby()
	{
		SteamMatchmaking.LeaveLobby(_lobbyId.Value);
		_lobbyId = null;
		_netStatusChangedCallback?.Dispose();
		_netStatusChangedCallback = null;
	}

	private void CleanupConnection(int closeReason, string? debugReason)
	{
		SteamNetworkingSockets.CloseConnection(_conn.Value, closeReason, debugReason, bEnableLinger: false);
		_conn = null;
		_connectingTaskCompletionSource = null;
		CleanupLobby();
	}

	private void DisconnectFromHostInternal(SteamDisconnectionReason reason, string? debugReason, bool now, bool selfInitiated)
	{
		_logger.Debug($"Disconnecting from host (now: {now} reason: {reason} debug: {debugReason})");
		SteamNetworkingSockets.CloseConnection(_conn.Value, (int)reason, debugReason, !now);
		SteamMatchmaking.LeaveLobby(_lobbyId.Value);
		ulong steamID = _hostNetId.m_SteamID;
		_conn = null;
		_lobbyId = null;
		_connectingTaskCompletionSource = null;
		_netStatusChangedCallback?.Dispose();
		_netStatusChangedCallback = null;
		_isConnected = false;
		_hostNetId = CSteamID.Nil;
		_handler.OnDisconnectedFromHost(steamID, new NetErrorInfo(reason, debugReason, selfInitiated));
	}

	private void CancelConnection()
	{
		_connectingTaskCompletionSource?.SetCanceled();
		if (_conn.HasValue)
		{
			DisconnectFromHost(NetError.Quit);
		}
		else if (_lobbyId.HasValue)
		{
			CleanupLobby();
		}
	}

	public override string? GetRawLobbyIdentifier()
	{
		return _lobbyId?.m_SteamID.ToString();
	}
}
