using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Multiplayer.Connection;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Lobby;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Multiplayer.Game;

public class JoinFlow
{
	private TaskCompletionSource<InitialGameInfoMessage>? _connectCompletion;

	private TaskCompletionSource<ClientRejoinResponseMessage>? _rejoinCompletion;

	private TaskCompletionSource<ClientLoadJoinResponseMessage>? _loadJoinCompletion;

	private TaskCompletionSource<ClientLobbyJoinResponseMessage>? _joinCompletion;

	private MegaCrit.Sts2.Core.Logging.Logger _logger = new MegaCrit.Sts2.Core.Logging.Logger("JoinFlow", LogType.Network);

	public NetClientGameService? NetService { get; private set; }

	public CancellationTokenSource CancelToken { get; private set; } = new CancellationTokenSource();

	public async Task<JoinResult> Begin(IClientConnectionInitializer initializer, SceneTree sceneTree)
	{
		MegaCrit.Sts2.Core.Logging.Logger.logLevelTypeMap[LogType.Network] = LogLevel.Debug;
		MegaCrit.Sts2.Core.Logging.Logger.logLevelTypeMap[LogType.Actions] = LogLevel.VeryDebug;
		MegaCrit.Sts2.Core.Logging.Logger.logLevelTypeMap[LogType.GameSync] = LogLevel.VeryDebug;
		if (_connectCompletion != null)
		{
			throw new InvalidOperationException("RejoinFlow object can only be used once!");
		}
		_logger.Info($"Beginning join with initializer {initializer}");
		NetService = new NetClientGameService();
		CancelToken.Token.Register(Cancel);
		CancellationTokenSource updateLoopCancelSource = new CancellationTokenSource();
		TaskHelper.RunSafely(NetServiceUpdateLoop(updateLoopCancelSource, sceneTree));
		JoinResult result;
		try
		{
			_ = 4;
			try
			{
				NetService.RegisterMessageHandler<InitialGameInfoMessage>(HandleInitialGameInfoMessage);
				NetService.RegisterMessageHandler<ClientLobbyJoinResponseMessage>(HandleJoinResponseMessage);
				NetService.RegisterMessageHandler<ClientLoadJoinResponseMessage>(HandleLoadJoinResponseMessage);
				NetService.RegisterMessageHandler<ClientRejoinResponseMessage>(HandleRejoinResponseMessage);
				NetService.Disconnected += OnDisconnected;
				_connectCompletion = new TaskCompletionSource<InitialGameInfoMessage>();
				NetErrorInfo? value = await initializer.Connect(NetService, CancelToken.Token);
				if (value.HasValue)
				{
					_logger.Info($"Connection failed: {value}");
					throw new ClientConnectionFailedException("Could not connect", value.Value);
				}
				_logger.Info("Initializer connection completed, awaiting initial game info message");
				InitialGameInfoMessage initialMessage = await _connectCompletion.Task;
				if (initialMessage.connectionFailureReason.HasValue)
				{
					_logger.Info($"Received initial join message with failure: {initialMessage.connectionFailureReason}");
					throw new ClientConnectionFailedException("Got connection failure from host", new NetErrorInfo(initialMessage.connectionFailureReason.Value));
				}
				RunSessionState state = initialMessage.sessionState;
				_logger.Info($"Got initial game info message. Version: {initialMessage.version} Hash: {initialMessage.idDatabaseHash} Type: {initialMessage.gameMode} State: {state}");
				string text = ReleaseInfoManager.Instance.ReleaseInfo?.Version ?? GitHelper.ShortCommitId ?? "UNKNOWN";
				if (initialMessage.version != text)
				{
					throw new ClientConnectionFailedException("Version mismatch. Host: " + initialMessage.version + " Ours: " + text, new NetErrorInfo(ConnectionFailureReason.VersionMismatch));
				}
				List<string> list = ModManager.GetModNameList() ?? new List<string>();
				List<string> list2 = initialMessage.mods ?? new List<string>();
				List<string> list3 = list2.Except(list).ToList();
				List<string> list4 = list.Except(list2).ToList();
				ConnectionFailureExtraInfo extraInfo = new ConnectionFailureExtraInfo
				{
					missingModsOnHost = list4,
					missingModsOnLocal = list3
				};
				if (list3.Count > 0 || list4.Count > 0)
				{
					_logger.Warn($"Mod mismatch! Mods that host has that we don't: {string.Join(",", list3)}. Mods that we have that host doesn't: {string.Join(",", list4)}.");
					throw new ClientConnectionFailedException("Mod mismatch. Host mods: " + string.Join(",", list2) + " Local mods: " + string.Join(",", list), new NetErrorInfo(ConnectionFailureReason.ModMismatch, extraInfo));
				}
				if (initialMessage.idDatabaseHash != ModelIdSerializationCache.Hash)
				{
					_logger.Warn("Our version " + text + " matches the host's, but our Model ID hash does not! Disconnecting");
					throw new ClientConnectionFailedException($"ModelDb hash mismatch. Host: {initialMessage.idDatabaseHash} Ours: {ModelIdSerializationCache.Hash}", new NetErrorInfo(ConnectionFailureReason.VersionMismatch, extraInfo));
				}
				switch (state)
				{
				case RunSessionState.InLobby:
				{
					ClientLobbyJoinResponseMessage value4 = await AttemptJoin(NetService);
					result = new JoinResult
					{
						gameMode = initialMessage.gameMode,
						sessionState = state,
						joinResponse = value4
					};
					break;
				}
				case RunSessionState.InLoadedLobby:
				{
					ClientLoadJoinResponseMessage value3 = await AttemptLoadJoin(NetService);
					result = new JoinResult
					{
						gameMode = initialMessage.gameMode,
						sessionState = state,
						loadJoinResponse = value3
					};
					break;
				}
				case RunSessionState.Running:
				{
					ClientRejoinResponseMessage value2 = await AttemptRejoin(NetService);
					result = new JoinResult
					{
						gameMode = initialMessage.gameMode,
						sessionState = state,
						rejoinResponse = value2
					};
					break;
				}
				default:
					NetService.Disconnect(NetError.InternalError, now: true);
					throw new InvalidOperationException($"Received invalid state {state} from connection!");
				}
			}
			catch (Exception ex)
			{
				if (NetService.IsConnected)
				{
					NetError reason = ((ex is OperationCanceledException) ? NetError.CancelledJoin : NetError.InternalError);
					NetService.Disconnect(reason);
				}
				MegaCrit.Sts2.Core.Logging.Logger.logLevelTypeMap[LogType.Network] = LogLevel.Info;
				MegaCrit.Sts2.Core.Logging.Logger.logLevelTypeMap[LogType.Actions] = LogLevel.Info;
				MegaCrit.Sts2.Core.Logging.Logger.logLevelTypeMap[LogType.GameSync] = LogLevel.Info;
				throw;
			}
		}
		finally
		{
			await updateLoopCancelSource.CancelAsync();
			NetService.UnregisterMessageHandler<InitialGameInfoMessage>(HandleInitialGameInfoMessage);
			NetService.UnregisterMessageHandler<ClientLobbyJoinResponseMessage>(HandleJoinResponseMessage);
			NetService.UnregisterMessageHandler<ClientLoadJoinResponseMessage>(HandleLoadJoinResponseMessage);
			NetService.UnregisterMessageHandler<ClientRejoinResponseMessage>(HandleRejoinResponseMessage);
			NetService.Disconnected -= OnDisconnected;
		}
		return result;
	}

	private async Task NetServiceUpdateLoop(CancellationTokenSource token, SceneTree sceneTree)
	{
		while (!token.IsCancellationRequested)
		{
			try
			{
				NetService.Update();
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
			}
			await sceneTree.ToSignal(sceneTree, SceneTree.SignalName.ProcessFrame);
		}
	}

	private async Task<ClientLobbyJoinResponseMessage> AttemptJoin(NetClientGameService gameService)
	{
		_joinCompletion = new TaskCompletionSource<ClientLobbyJoinResponseMessage>();
		_logger.Info("Sending ClientLobbyJoinRequestMessage and waiting for response message");
		UnlockState unlockState = SaveManager.Instance.GenerateUnlockStateFromProgress();
		ClientLobbyJoinRequestMessage message = new ClientLobbyJoinRequestMessage
		{
			maxAscensionUnlocked = SaveManager.Instance.Progress.MaxMultiplayerAscension,
			unlockState = unlockState.ToSerializable()
		};
		gameService.SendMessage(message);
		ClientLobbyJoinResponseMessage clientLobbyJoinResponseMessage = await _joinCompletion.Task;
		_logger.Info($"Received {"ClientLobbyJoinResponseMessage"}: {clientLobbyJoinResponseMessage}");
		return clientLobbyJoinResponseMessage;
	}

	private async Task<ClientLoadJoinResponseMessage> AttemptLoadJoin(NetClientGameService gameService)
	{
		_loadJoinCompletion = new TaskCompletionSource<ClientLoadJoinResponseMessage>();
		_logger.Info("Sending ClientLoadJoinRequestMessage and waiting for rejoin response message");
		gameService.SendMessage(default(ClientLoadJoinRequestMessage));
		ClientLoadJoinResponseMessage clientLoadJoinResponseMessage = await _loadJoinCompletion.Task;
		_logger.Info($"Received ClientLoadJoinResponseMessage: {clientLoadJoinResponseMessage}");
		return clientLoadJoinResponseMessage;
	}

	private async Task<ClientRejoinResponseMessage> AttemptRejoin(NetClientGameService gameService)
	{
		_rejoinCompletion = new TaskCompletionSource<ClientRejoinResponseMessage>();
		_logger.Info("Sending ClientRequestRejoinMessage and waiting for rejoin response message");
		gameService.SendMessage(default(ClientRejoinRequestMessage));
		ClientRejoinResponseMessage clientRejoinResponseMessage = await _rejoinCompletion.Task;
		_logger.Info($"Received ClientRejoinResponseMessage: {clientRejoinResponseMessage}");
		return clientRejoinResponseMessage;
	}

	private void HandleInitialGameInfoMessage(InitialGameInfoMessage message, ulong _)
	{
		if (_connectCompletion == null || _connectCompletion.Task.IsCompleted)
		{
			_logger.Warn($"Received {"InitialGameInfoMessage"} when we weren't expecting it! Completion status: {_connectCompletion}");
		}
		else
		{
			_connectCompletion.SetResult(message);
		}
	}

	private void HandleRejoinResponseMessage(ClientRejoinResponseMessage message, ulong senderId)
	{
		if (_rejoinCompletion == null || _rejoinCompletion.Task.IsCompleted)
		{
			_logger.Warn($"Received {"ClientRejoinResponseMessage"} when we weren't expecting it! Completion status: {_connectCompletion}");
		}
		else
		{
			_rejoinCompletion.SetResult(message);
		}
	}

	private void HandleLoadJoinResponseMessage(ClientLoadJoinResponseMessage message, ulong senderId)
	{
		if (_loadJoinCompletion == null || _loadJoinCompletion.Task.IsCompleted)
		{
			_logger.Warn($"Received {"ClientLoadJoinResponseMessage"} when we weren't expecting it! Completion status: {_connectCompletion}");
		}
		else
		{
			_loadJoinCompletion.SetResult(message);
		}
	}

	private void HandleJoinResponseMessage(ClientLobbyJoinResponseMessage message, ulong senderId)
	{
		if (_joinCompletion == null || _joinCompletion.Task.IsCompleted)
		{
			_logger.Warn($"Received {"ClientLobbyJoinResponseMessage"} when we weren't expecting it! Completion status: {_connectCompletion}");
		}
		else
		{
			_joinCompletion.SetResult(message);
		}
	}

	private void OnDisconnected(NetErrorInfo info)
	{
		_logger.Info($"Disconnected during join flow, reason: {info.GetReason()}. Failing with an exception");
		ClientConnectionFailedException exception = new ClientConnectionFailedException($"Unexpectedly disconnected from host while joining. Reason: {info.GetReason()}", info);
		TaskCompletionSource<InitialGameInfoMessage> connectCompletion = _connectCompletion;
		if (connectCompletion != null)
		{
			Task<InitialGameInfoMessage> task = connectCompletion.Task;
			if (task != null && !task.IsCompleted)
			{
				_connectCompletion.SetException(exception);
			}
		}
		TaskCompletionSource<ClientLobbyJoinResponseMessage> joinCompletion = _joinCompletion;
		if (joinCompletion != null)
		{
			Task<ClientLobbyJoinResponseMessage> task2 = joinCompletion.Task;
			if (task2 != null && !task2.IsCompleted)
			{
				_joinCompletion?.SetException(exception);
			}
		}
		TaskCompletionSource<ClientLoadJoinResponseMessage> loadJoinCompletion = _loadJoinCompletion;
		if (loadJoinCompletion != null)
		{
			Task<ClientLoadJoinResponseMessage> task3 = loadJoinCompletion.Task;
			if (task3 != null && !task3.IsCompleted)
			{
				_loadJoinCompletion?.SetException(exception);
			}
		}
		TaskCompletionSource<ClientRejoinResponseMessage> rejoinCompletion = _rejoinCompletion;
		if (rejoinCompletion != null)
		{
			Task<ClientRejoinResponseMessage> task4 = rejoinCompletion.Task;
			if (task4 != null && !task4.IsCompleted)
			{
				_rejoinCompletion?.SetException(exception);
			}
		}
	}

	private void Cancel()
	{
		TaskCompletionSource<InitialGameInfoMessage> connectCompletion = _connectCompletion;
		if (connectCompletion != null)
		{
			Task<InitialGameInfoMessage> task = connectCompletion.Task;
			if (task != null && !task.IsCompleted)
			{
				_connectCompletion.SetCanceled();
			}
		}
		TaskCompletionSource<ClientLobbyJoinResponseMessage> joinCompletion = _joinCompletion;
		if (joinCompletion != null)
		{
			Task<ClientLobbyJoinResponseMessage> task2 = joinCompletion.Task;
			if (task2 != null && !task2.IsCompleted)
			{
				_joinCompletion?.SetCanceled();
			}
		}
		TaskCompletionSource<ClientLoadJoinResponseMessage> loadJoinCompletion = _loadJoinCompletion;
		if (loadJoinCompletion != null)
		{
			Task<ClientLoadJoinResponseMessage> task3 = loadJoinCompletion.Task;
			if (task3 != null && !task3.IsCompleted)
			{
				_loadJoinCompletion?.SetCanceled();
			}
		}
		TaskCompletionSource<ClientRejoinResponseMessage> rejoinCompletion = _rejoinCompletion;
		if (rejoinCompletion != null)
		{
			Task<ClientRejoinResponseMessage> task4 = rejoinCompletion.Task;
			if (task4 != null && !task4.IsCompleted)
			{
				_rejoinCompletion?.SetCanceled();
			}
		}
	}
}
