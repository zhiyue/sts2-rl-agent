using System;
using System.Collections.Generic;
using System.IO.Hashing;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Checksums;
using MegaCrit.Sts2.Core.Multiplayer.Replay;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Multiplayer.Game;

public class ChecksumTracker : IDisposable
{
	private struct TrackedChecksum
	{
		public NetChecksumData data;

		public string context;

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

	public NetChecksumData GenerateChecksum(string context, GameAction? action)
	{
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
			LogStateDivergence(localChecksum, message, senderId);
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

	private void LogStateDivergence(TrackedChecksum localChecksum, StateDivergenceMessage message, ulong remoteId)
	{
		if (localChecksum.data.id != message.senderChecksum.id)
		{
			throw new InvalidOperationException("Trying to compare two checksums with different IDs!");
		}
		if (!TestMode.IsOn)
		{
			Log.Error($"State divergence message received for player {remoteId} checksum ID {localChecksum.data.id}! (We are {_netService.Type} {_netService.NetId})\nContext: {localChecksum.context}. Local: {localChecksum.data.checksum}. Remote: {message.senderChecksum.checksum}.\nLOCAL STATE DUMP\n{localChecksum.fullState}\nREMOTE STATE DUMP\n{message.senderCombatState}\n");
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
