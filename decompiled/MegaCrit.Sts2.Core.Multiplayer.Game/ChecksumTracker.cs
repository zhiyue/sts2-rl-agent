using System;
using System.Collections.Generic;
using System.IO.Hashing;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Checksums;
using MegaCrit.Sts2.Core.Multiplayer.Replay;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;
using Sentry;

namespace MegaCrit.Sts2.Core.Multiplayer.Game;

/// <summary>
/// Calculates checksums, sends them to peers, and validates received checksums to ensure there are no state divergences
/// during deterministic execution (currently: during combat, after events, and after rest sites).
/// </summary>
public class ChecksumTracker : IDisposable
{
	private struct TrackedChecksum
	{
		public NetChecksumData data;

		public string context;

		public string fingerprintContext;

		public NetFullCombatState fullState;
	}

	private struct QueuedRemoteChecksum
	{
		public NetChecksumData data;

		public ulong senderId;
	}

	private const int _checksumsToSave = 20;

	private readonly List<TrackedChecksum> _checksums = new List<TrackedChecksum>();

	private readonly List<QueuedRemoteChecksum> _queuedRemoteChecksums = new List<QueuedRemoteChecksum>();

	private readonly INetGameService _netService;

	private readonly IRunState _runState;

	private readonly PacketWriter _packetWriter = new PacketWriter();

	private readonly Logger _logger = new Logger("ChecksumTracker", LogType.GameSync);

	private List<ReplayChecksumData>? _replayChecksums;

	public uint NextId { get; private set; }

	public bool IsEnabled { get; set; } = true;

	public event Action<NetFullCombatState>? StateDiverged;

	public event Action<NetChecksumData, string, NetFullCombatState>? ChecksumGenerated;

	public ChecksumTracker(INetGameService netService, IRunState runState)
	{
		_netService = netService;
		_runState = runState;
		_netService.RegisterMessageHandler<ChecksumDataMessage>(OnReceivedChecksumDataMessage);
		_netService.RegisterMessageHandler<StateDivergenceMessage>(OnReceivedStateDivergenceMessage);
	}

	public void Dispose()
	{
		_netService.UnregisterMessageHandler<ChecksumDataMessage>(OnReceivedChecksumDataMessage);
		_netService.UnregisterMessageHandler<StateDivergenceMessage>(OnReceivedStateDivergenceMessage);
	}

	/// <summary>
	/// Main entry point for this class. Generates a checksum of the full combat state for validation against peers'.
	/// On host, the checksum is saved for comparison against those received from clients.
	/// On clients, the checksum is saved and sent to the host for validation.
	/// This should be called the exact same amount of times for every peer, otherwise false positive mismatches will be
	/// generated.
	/// </summary>
	public NetChecksumData GenerateChecksum(string context, GameAction? action)
	{
		if (!IsEnabled)
		{
			return default(NetChecksumData);
		}
		_logger.Debug($"Generating checksum for context: {context} action: {action} id: {NextId}");
		NetChecksumData netChecksumData = ObtainAndTrackChecksum(context, action);
		if (_netService.Type == NetGameType.Client)
		{
			_netService.SendMessage(new ChecksumDataMessage
			{
				checksumData = netChecksumData
			});
		}
		CheckAgainstReplayChecksum(netChecksumData, context);
		return netChecksumData;
	}

	/// <summary>
	/// Called on the host when a client sends a message containing the checksum for their current game state.
	/// </summary>
	private void OnReceivedChecksumDataMessage(ChecksumDataMessage message, ulong senderId)
	{
		if (_netService.Type != NetGameType.Host)
		{
			throw new InvalidOperationException("Received ChecksumDataMessage as non-host player!");
		}
		NetChecksumData remoteChecksumData = message.checksumData;
		int num = _checksums.FindIndex((TrackedChecksum c) => c.data.id == remoteChecksumData.id);
		if (num < 0)
		{
			_queuedRemoteChecksums.Add(new QueuedRemoteChecksum
			{
				data = remoteChecksumData,
				senderId = senderId
			});
		}
		else
		{
			TrackedChecksum localChecksum = _checksums[num];
			CompareChecksums(localChecksum, remoteChecksumData, senderId);
		}
	}

	/// <summary>
	/// Called on client when the host sends a message indicating that the client's state has diverged from the host's.
	/// Also called on the host when the client receives the host's message, so that the host knows what state the client
	/// was in and can log both.
	/// </summary>
	private void OnReceivedStateDivergenceMessage(StateDivergenceMessage message, ulong senderId)
	{
		NetChecksumData remoteChecksumData = message.senderChecksum;
		int num = _checksums.FindIndex((TrackedChecksum c) => c.data.id == remoteChecksumData.id);
		if (num < 0)
		{
			Log.Error($"Received state divergence message for checksum ID {remoteChecksumData.id} that has fallen out of our list! (Our next ID: {NextId})");
		}
		else
		{
			TrackedChecksum localChecksum = _checksums[num];
			LogStateDivergence(localChecksum, message, senderId, num);
		}
		if (_netService.Type == NetGameType.Host)
		{
			(_netService as NetHostGameService)?.DisconnectClient(senderId, NetError.StateDivergence);
			NErrorPopup nErrorPopup = NErrorPopup.Create(new NetErrorInfo(NetError.StateDivergence, selfInitiated: true));
			if (nErrorPopup != null)
			{
				NModalContainer.Instance?.Add(nErrorPopup);
			}
		}
	}

	/// <summary>
	/// Call this to obtain checksum data that may be sent over the network with a message.
	/// </summary>
	private NetChecksumData ObtainAndTrackChecksum(string context, GameAction? action)
	{
		NetFullCombatState netFullCombatState = NetFullCombatState.FromRun(_runState, action);
		uint checksum = GenerateChecksum(netFullCombatState);
		NetChecksumData netChecksumData = new NetChecksumData
		{
			id = NextId,
			checksum = checksum
		};
		NextId++;
		TrackedChecksum trackedChecksum = new TrackedChecksum
		{
			data = netChecksumData,
			context = context,
			fingerprintContext = ((action != null) ? action.GetType().Name : context),
			fullState = netFullCombatState
		};
		this.ChecksumGenerated?.Invoke(netChecksumData, context, netFullCombatState);
		_checksums.Add(trackedChecksum);
		if (_checksums.Count > 20)
		{
			_checksums.RemoveAt(0);
		}
		for (int i = 0; i < _queuedRemoteChecksums.Count; i++)
		{
			if (_queuedRemoteChecksums[i].data.id == trackedChecksum.data.id)
			{
				CompareChecksums(trackedChecksum, _queuedRemoteChecksums[i].data, _queuedRemoteChecksums[i].senderId);
				_queuedRemoteChecksums.RemoveAt(i);
				i--;
			}
		}
		return netChecksumData;
	}

	private void CompareChecksums(TrackedChecksum localChecksum, NetChecksumData remoteChecksum, ulong remoteId)
	{
		if (localChecksum.data.id != remoteChecksum.id)
		{
			throw new InvalidOperationException("Trying to compare two checksums with different IDs!");
		}
		if (_netService.Type != NetGameType.Host)
		{
			throw new InvalidOperationException("CompareChecksums should only be called on the host!");
		}
		if (localChecksum.data.checksum != remoteChecksum.checksum)
		{
			if (!TestMode.IsOn)
			{
				Log.Error($"State divergence detected! Checksum with ID {localChecksum.data.id} for client {remoteId} doesn't match host's!\nContext: {localChecksum.context}. Local: {localChecksum.data.checksum}. Remote: {remoteChecksum.checksum}.");
			}
			StateDivergenceMessage message = new StateDivergenceMessage
			{
				senderChecksum = localChecksum.data,
				senderCombatState = localChecksum.fullState
			};
			_netService.SendMessage(message, remoteId);
		}
	}

	private void LogStateDivergence(TrackedChecksum localChecksum, StateDivergenceMessage message, ulong remoteId, int checksumIndex)
	{
		if (localChecksum.data.id != message.senderChecksum.id)
		{
			throw new InvalidOperationException("Trying to compare two checksums with different IDs!");
		}
		if (!TestMode.IsOn)
		{
			Log.Error($"State divergence message received for player {remoteId} checksum ID {localChecksum.data.id}! (We are {_netService.Type} {_netService.NetId})\nContext: {localChecksum.context}. Local: {localChecksum.data.checksum}. Remote: {message.senderChecksum.checksum}.\nLOCAL STATE DUMP\n{localChecksum.fullState}\nREMOTE STATE DUMP\n{message.senderCombatState}\n");
			ReportDivergenceToSentry(localChecksum, message, remoteId, checksumIndex);
		}
		if (_netService.Type == NetGameType.Client)
		{
			StateDivergenceMessage message2 = new StateDivergenceMessage
			{
				senderChecksum = localChecksum.data,
				senderCombatState = localChecksum.fullState
			};
			_netService.SendMessage(message2);
			this.StateDiverged?.Invoke(message.senderCombatState);
		}
	}

	private void ReportDivergenceToSentry(TrackedChecksum localChecksum, StateDivergenceMessage message, ulong remoteId, int checksumIndex)
	{
		string role = _netService.Type.ToString();
		string message2 = "Multiplayer state divergence: " + localChecksum.fingerprintContext;
		string localState = localChecksum.fullState.ToString();
		string remoteState = message.senderCombatState.ToString();
		SentryService.CaptureException(new StateDivergenceException(message2), delegate(Scope scope)
		{
			scope.SetFingerprint("StateDivergence", localChecksum.fingerprintContext);
			scope.SetTag("net.role", role);
			scope.SetTag("divergence.type", localChecksum.fingerprintContext);
			scope.SetExtra("divergence.context", localChecksum.context);
			scope.SetExtra("divergence.checksum_id", localChecksum.data.id);
			scope.SetExtra("divergence.local_checksum", localChecksum.data.checksum);
			scope.SetExtra("divergence.remote_checksum", message.senderChecksum.checksum);
			scope.SetExtra("divergence.local_net_id", _netService.NetId);
			scope.SetExtra("divergence.remote_net_id", remoteId);
			scope.SetExtra("divergence.lobby_id", _netService.GetRawLobbyIdentifier() ?? "unknown");
			scope.AddCompressedAttachment(localState, "local_state.txt.gz");
			scope.AddCompressedAttachment(remoteState, "remote_state.txt.gz");
			if (checksumIndex > 0)
			{
				string text = _checksums[checksumIndex - 1].fullState.ToString();
				scope.AddCompressedAttachment(text, "previous_state.txt.gz");
			}
		});
	}

	public void LoadReplayChecksums(List<ReplayChecksumData> replayChecksums, uint nextId)
	{
		NextId = nextId;
		_replayChecksums = replayChecksums;
	}

	private void CheckAgainstReplayChecksum(NetChecksumData localData, string context)
	{
		if (_replayChecksums == null)
		{
			return;
		}
		int num = _replayChecksums.FindIndex((ReplayChecksumData c) => c.checksumData.id == localData.id);
		if (num == -1)
		{
			Log.Error($"Replay state diverged! Generated checksum for {context} with id {localData.id} that doesn't exist in the replay data");
			return;
		}
		ReplayChecksumData replayChecksumData = _replayChecksums[num];
		uint num2 = GenerateChecksum(replayChecksumData.fullState);
		if (num2 != localData.checksum)
		{
			Log.Error($"Replay state divergence! Checksum ID {localData.id}!\nLocal context: {context}. Replay context: {replayChecksumData.context}. Local: {localData.checksum}. Replay: {num2}.\nLOCAL STATE DUMP\n{_checksums.Find((TrackedChecksum c) => c.data.id == localData.id).fullState}\nREPLAY STATE DUMP\n{replayChecksumData.fullState}\n");
		}
	}

	public uint GenerateChecksum(NetFullCombatState state)
	{
		_packetWriter.Reset();
		_packetWriter.Write(state);
		_packetWriter.ZeroByteRemainder();
		ReadOnlySpan<byte> source = _packetWriter.Buffer.AsSpan().Slice(0, _packetWriter.BytePosition);
		return XxHash32.HashToUInt32(source);
	}
}
