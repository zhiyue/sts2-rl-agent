using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Multiplayer;

public class CombatStateSynchronizer : IDisposable
{
	private readonly INetGameService _netService;

	private readonly RunState _runState;

	private readonly RunLobby? _runLobby;

	private readonly Dictionary<ulong, SerializablePlayer> _syncData = new Dictionary<ulong, SerializablePlayer>();

	private SerializableRunRngSet? _rngSet;

	private SerializableRelicGrabBag? _sharedRelicGrabBag;

	private readonly Logger _logger = new Logger("CombatStateSynchronizer", LogType.GameSync);

	private TaskCompletionSource? _syncCompletionSource;

	public bool IsDisabled { get; set; }

	public CombatStateSynchronizer(INetGameService netService, RunLobby? runLobby, RunState runState)
	{
		_netService = netService;
		_runState = runState;
		_runLobby = runLobby;
		_netService.RegisterMessageHandler<SyncPlayerDataMessage>(OnSyncPlayerMessageReceived);
		_netService.RegisterMessageHandler<SyncRngMessage>(OnSyncRngMessageReceived);
		if (_runLobby != null)
		{
			_runLobby.RemotePlayerDisconnected += OnPeerDisconnected;
		}
	}

	public void Dispose()
	{
		_netService.UnregisterMessageHandler<SyncPlayerDataMessage>(OnSyncPlayerMessageReceived);
		_netService.UnregisterMessageHandler<SyncRngMessage>(OnSyncRngMessageReceived);
		if (_runLobby != null)
		{
			_runLobby.RemotePlayerDisconnected -= OnPeerDisconnected;
		}
	}

	private void OnSyncPlayerMessageReceived(SyncPlayerDataMessage syncMessage, ulong senderId)
	{
		_logger.Debug($"Received sync player message from {senderId}");
		if (_syncData.ContainsKey(senderId))
		{
			_logger.Error($"Received two player sync messages from {senderId}! Ignoring the second one");
		}
		else
		{
			_syncData[senderId] = syncMessage.player;
			CheckSyncCompleted();
		}
	}

	private void OnSyncRngMessageReceived(SyncRngMessage syncMessage, ulong senderId)
	{
		_logger.Debug($"Received sync RNG message from {senderId}");
		if (_rngSet != null)
		{
			_logger.Error($"Received two RNG sync messages from {senderId}! Ignoring the second one");
		}
		else
		{
			_rngSet = syncMessage.rng;
			_sharedRelicGrabBag = syncMessage.sharedRelicGrabBag;
			CheckSyncCompleted();
		}
	}

	private void OnPeerDisconnected(ulong peerId)
	{
		TaskCompletionSource? syncCompletionSource = _syncCompletionSource;
		if (syncCompletionSource != null && !syncCompletionSource.Task.IsCompleted)
		{
			_logger.Debug($"Peer {peerId} disconnected, checking if sync is complete");
			CheckSyncCompleted();
		}
	}

	public void StartSync()
	{
		_logger.Debug("Broadcasting combat sync message to all peers");
		if (_netService.Type != NetGameType.Singleplayer && !IsDisabled)
		{
			if (_syncCompletionSource != null)
			{
				throw new InvalidOperationException("StartSync called twice before WaitForSync!");
			}
			SyncPlayerDataMessage message = default(SyncPlayerDataMessage);
			Player me = LocalContext.GetMe(_runState);
			message.player = me.ToSerializable();
			_netService.SendMessage(message);
			_syncData[message.player.NetId] = message.player;
			_syncCompletionSource = new TaskCompletionSource();
			if (_netService.Type == NetGameType.Host)
			{
				_rngSet = me.RunState.Rng.ToSerializable();
				_sharedRelicGrabBag = me.RunState.SharedRelicGrabBag.ToSerializable();
				SyncRngMessage message2 = new SyncRngMessage
				{
					rng = _rngSet,
					sharedRelicGrabBag = _sharedRelicGrabBag
				};
				_netService.SendMessage(message2);
			}
			CheckSyncCompleted();
		}
	}

	public async Task WaitForSync()
	{
		_logger.Debug("Waiting to receive all sync messages from all clients");
		if (_netService.Type == NetGameType.Singleplayer || IsDisabled)
		{
			return;
		}
		if (_syncCompletionSource == null)
		{
			throw new InvalidOperationException("StartSync must be called before WaitForSync!");
		}
		await _syncCompletionSource.Task;
		foreach (KeyValuePair<ulong, SerializablePlayer> syncDatum in _syncData)
		{
			if (_runLobby != null && !_runLobby.ConnectedPlayerIds.Contains(syncDatum.Key))
			{
				_logger.Debug($"Skipping sync for disconnected player {syncDatum.Key}");
				continue;
			}
			Player player = _runState.GetPlayer(syncDatum.Key);
			if (!LocalContext.IsMe(player))
			{
				player.SyncWithSerializedPlayer(syncDatum.Value);
			}
		}
		if (_netService.Type != NetGameType.Host)
		{
			if (_rngSet != null)
			{
				_runState.Rng.LoadFromSerializable(_rngSet);
			}
			else if (_runState.Players.Count > 1)
			{
				_logger.Error("There are two or more players and we are a client, but we never received the RNG set!");
			}
			if (_sharedRelicGrabBag != null)
			{
				_runState.SharedRelicGrabBag.LoadFromSerializable(_sharedRelicGrabBag);
			}
			else if (_runState.Players.Count > 1)
			{
				_logger.Error("There are two or more players and we are a client, but we never received the shared relic grab bag!");
			}
		}
		_syncData.Clear();
		_rngSet = null;
		_sharedRelicGrabBag = null;
		_syncCompletionSource = null;
	}

	private void CheckSyncCompleted()
	{
		if (_syncCompletionSource == null)
		{
			return;
		}
		if (_runLobby == null)
		{
			throw new InvalidOperationException("Tried to combat sync without a valid run lobby!");
		}
		if (_netService.Type == NetGameType.Singleplayer || IsDisabled)
		{
			_syncCompletionSource.SetResult();
			return;
		}
		foreach (ulong connectedPlayerId in _runLobby.ConnectedPlayerIds)
		{
			if (!_syncData.ContainsKey(connectedPlayerId))
			{
				return;
			}
		}
		if (_rngSet != null)
		{
			_syncCompletionSource.SetResult();
		}
	}
}
