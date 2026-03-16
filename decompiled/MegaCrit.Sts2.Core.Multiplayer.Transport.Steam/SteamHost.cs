using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Platform.Steam;
using Steamworks;

namespace MegaCrit.Sts2.Core.Multiplayer.Transport.Steam;

public class SteamHost : NetHost
{
	private struct ClientConnection
	{
		public HSteamNetConnection conn;

		public SteamNetworkingIdentity netId;
	}

	private static readonly List<ClientConnection> _connectionsCache = new List<ClientConnection>();

	private readonly Logger _logger = new Logger("SteamHost", LogType.Network);

	private Callback<SteamNetConnectionStatusChangedCallback_t>? _netStatusChangedCallback;

	private CSteamID? _lobbyId;

	private HSteamListenSocket _socket;

	private readonly List<ClientConnection> _connections = new List<ClientConnection>();

	private bool _isConnected;

	public override bool IsConnected => _isConnected;

	public override ulong NetId => SteamUser.GetSteamID().m_SteamID;

	public override IEnumerable<ulong> ConnectedPeerIds => _connections.Select((ClientConnection c) => c.netId.GetSteamID64());

	public CSteamID? LobbyId => _lobbyId;

	public SteamHost(INetHostHandler handler)
		: base(handler)
	{
	}

	public async Task<NetErrorInfo?> StartHost(int maxPlayers)
	{
		_logger.Info($"Initializing Steam host. Our player id: {NetId}");
		SteamAPICall_t call = SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, maxPlayers);
		using SteamCallResult<LobbyCreated_t> callResult = new SteamCallResult<LobbyCreated_t>(call);
		LobbyCreated_t lobbyCreated_t = await callResult.Task;
		if (lobbyCreated_t.m_eResult != EResult.k_EResultOK)
		{
			_logger.Error($"Error creating steam lobby! {lobbyCreated_t.m_eResult}");
			return new NetErrorInfo(lobbyCreated_t.m_eResult);
		}
		_lobbyId = new CSteamID(lobbyCreated_t.m_ulSteamIDLobby);
		_socket = SteamNetworkingSockets.CreateListenSocketP2P(0, 0, new SteamNetworkingConfigValue_t[2]
		{
			new SteamNetworkingConfigValue_t
			{
				m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_TimeoutInitial,
				m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32,
				m_val = new SteamNetworkingConfigValue_t.OptionValue
				{
					m_int32 = 20000
				}
			},
			new SteamNetworkingConfigValue_t
			{
				m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_TimeoutConnected,
				m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32,
				m_val = new SteamNetworkingConfigValue_t.OptionValue
				{
					m_int32 = 20000
				}
			}
		});
		_netStatusChangedCallback = new Callback<SteamNetConnectionStatusChangedCallback_t>(OnNetStatusChanged);
		_isConnected = true;
		return null;
	}

	public override void Update()
	{
		_connectionsCache.Clear();
		_connectionsCache.AddRange(_connections);
		foreach (ClientConnection item in _connectionsCache)
		{
			SteamUtil.ProcessMessages(item.conn, _handler, _logger);
		}
	}

	public override void SetHostIsClosed(bool isClosed)
	{
		SteamMatchmaking.SetLobbyType(_lobbyId.Value, (!isClosed) ? ELobbyType.k_ELobbyTypeFriendsOnly : ELobbyType.k_ELobbyTypePrivate);
	}

	private void OnNetStatusChanged(SteamNetConnectionStatusChangedCallback_t data)
	{
		_logger.Debug($"Connection status changed: ({data.m_info.m_identityRemote.GetSteamID64()}: {data.m_eOldState} -> {data.m_info.m_eState}");
		if (data.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting)
		{
			if (!IsInLobby(data.m_info.m_identityRemote))
			{
				_logger.Warn($"Player with steam id {data.m_info.m_identityRemote.GetSteamID64()} attempted to join the game, but they are not in the lobby (id {_lobbyId.Value})");
				SteamNetworkingSockets.CloseConnection(data.m_hConn, 0, "Player is not in the lobby!", bEnableLinger: false);
				return;
			}
			_logger.Info($"Accepting new connection with user {data.m_info.m_identityRemote.GetSteamID64()}");
			EResult eResult = SteamNetworkingSockets.AcceptConnection(data.m_hConn);
			if (eResult != EResult.k_EResultOK)
			{
				_logger.Error($"Tried to accept connection with user {data.m_info.m_identityRemote.GetSteamID64()} but it returned result {eResult}!");
				CloseConnectionAndRemove(data.m_hConn, SteamDisconnectionReason.AppInternalError, $"Connection accept failure: {eResult}", now: true, selfInitiated: false);
			}
		}
		else if (data.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected)
		{
			_connections.Add(new ClientConnection
			{
				conn = data.m_hConn,
				netId = data.m_info.m_identityRemote
			});
			_handler.OnPeerConnected(data.m_info.m_identityRemote.GetSteamID64());
		}
		else if (data.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally)
		{
			_logger.Info($"Steam connection closed because of problem. Reason: {data.m_info.m_eEndReason}, {data.m_info.m_szEndDebug}");
			CloseConnectionAndRemove(data.m_hConn, (SteamDisconnectionReason)data.m_info.m_eEndReason, data.m_info.m_szEndDebug, now: true, selfInitiated: false);
		}
		else if (data.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer)
		{
			_logger.Info($"Steam connection closed by peer. Reason: {data.m_info.m_eEndReason}, {data.m_info.m_szEndDebug}");
			CloseConnectionAndRemove(data.m_hConn, (SteamDisconnectionReason)data.m_info.m_eEndReason, data.m_info.m_szEndDebug, now: true, selfInitiated: false);
		}
	}

	public override void SendMessageToClient(ulong peerId, byte[] bytes, int length, NetTransferMode mode, int channel = 0)
	{
		_logger.VeryDebug($"Sending {length} bytes to client {peerId}");
		default(SteamNetworkingIdentity).SetSteamID64(peerId);
		ClientConnection? connectionForNetId = GetConnectionForNetId(peerId);
		if (!connectionForNetId.HasValue)
		{
			throw new InvalidOperationException($"Could not find connection for peer {peerId}!");
		}
		GCHandle gCHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
		try
		{
			long pOutMessageNumber;
			EResult eResult = SteamNetworkingSockets.SendMessageToConnection(connectionForNetId.Value.conn, gCHandle.AddrOfPinnedObject(), (uint)length, SteamUtil.FlagsFromMode(mode), out pOutMessageNumber);
			if (eResult != EResult.k_EResultOK)
			{
				_logger.Warn($"Failed to send message length {length} to peer {peerId}: {eResult}");
			}
		}
		finally
		{
			gCHandle.Free();
		}
	}

	public override void SendMessageToAll(byte[] bytes, int length, NetTransferMode mode, int channel = 0)
	{
		foreach (ClientConnection connection in _connections)
		{
			SteamNetworkingIdentity netId = connection.netId;
			SendMessageToClient(netId.GetSteamID64(), bytes, length, mode, channel);
		}
	}

	private ClientConnection? GetConnectionForNetId(ulong peerId)
	{
		foreach (ClientConnection connection in _connections)
		{
			SteamNetworkingIdentity netId = connection.netId;
			if (netId.GetSteamID64() == peerId)
			{
				return connection;
			}
		}
		return null;
	}

	public override void DisconnectClient(ulong peerId, NetError reason, bool now = false)
	{
		HSteamNetConnection? hSteamNetConnection = null;
		_logger.Debug($"Disconnecting peer {peerId}, reason: {reason}");
		foreach (ClientConnection connection in _connections)
		{
			SteamNetworkingIdentity netId = connection.netId;
			if (netId.GetSteamID64() == peerId)
			{
				hSteamNetConnection = connection.conn;
				break;
			}
		}
		if (hSteamNetConnection.HasValue)
		{
			CloseConnectionAndRemove(hSteamNetConnection.Value, reason.ToSteam(), null, now, selfInitiated: true);
		}
	}

	private void CloseConnectionAndRemove(HSteamNetConnection conn, SteamDisconnectionReason reason, string? debugReason, bool now, bool selfInitiated)
	{
		SteamNetworkingSockets.CloseConnection(conn, (int)reason, debugReason, !now);
		int num = _connections.FindIndex((ClientConnection c) => c.conn == conn);
		if (num >= 0)
		{
			ClientConnection clientConnection = _connections[num];
			_connections.RemoveAt(num);
			_handler.OnPeerDisconnected(clientConnection.netId.GetSteamID64(), new NetErrorInfo(reason, debugReason, selfInitiated));
		}
	}

	public override void StopHost(NetError reason, bool now = false)
	{
		_logger.Debug("Stopping host");
		foreach (ClientConnection connection in _connections)
		{
			SteamNetworkingSockets.CloseConnection(connection.conn, (int)reason.ToSteam(), null, !now);
		}
		_connections.Clear();
		SteamNetworkingSockets.CloseListenSocket(_socket);
		SteamMatchmaking.LeaveLobby(_lobbyId.Value);
		_lobbyId = null;
		_isConnected = false;
		_netStatusChangedCallback?.Dispose();
		_netStatusChangedCallback = null;
		_handler.OnDisconnected(new NetErrorInfo(reason, selfInitiated: true));
	}

	private bool IsInLobby(SteamNetworkingIdentity id)
	{
		CSteamID steamID = id.GetSteamID();
		if (steamID == CSteamID.Nil)
		{
			return false;
		}
		int numLobbyMembers = SteamMatchmaking.GetNumLobbyMembers(_lobbyId.Value);
		for (int i = 0; i < numLobbyMembers; i++)
		{
			CSteamID lobbyMemberByIndex = SteamMatchmaking.GetLobbyMemberByIndex(_lobbyId.Value, i);
			if (steamID == lobbyMemberByIndex)
			{
				return true;
			}
		}
		return false;
	}

	public override string? GetRawLobbyIdentifier()
	{
		return _lobbyId?.m_SteamID.ToString();
	}
}
