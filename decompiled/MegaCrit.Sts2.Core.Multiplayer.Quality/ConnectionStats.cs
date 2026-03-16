using Godot;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Messages;

namespace MegaCrit.Sts2.Core.Multiplayer.Quality;

public class ConnectionStats
{
	public const int ringBufferSize = 20;

	private const float _weightedAverageFactor = 0.2f;

	private HeartbeatStatus?[] _statuses = new HeartbeatStatus?[20];

	private int _nextIndex;

	private readonly MegaCrit.Sts2.Core.Logging.Logger _logger;

	public ulong PeerId { get; private set; }

	public float PingMsec { get; private set; }

	public float PacketLoss { get; private set; }

	public ulong? LastReceivedTime { get; private set; }

	public bool RemoteIsLoading { get; private set; }

	public ConnectionStats(ulong peerId)
	{
		PeerId = peerId;
		_logger = new MegaCrit.Sts2.Core.Logging.Logger($"{"ConnectionStats"} ({peerId})", LogType.Network);
	}

	public HeartbeatRequestMessage GenerateHeartbeat(ulong timeMsec)
	{
		_logger.VeryDebug($"Generating heartbeat {_nextIndex} for time {timeMsec}");
		int num = _nextIndex % 20;
		HeartbeatStatus value = new HeartbeatStatus
		{
			counter = _nextIndex,
			sentMsec = timeMsec
		};
		HeartbeatStatus? heartbeatStatus = _statuses[num];
		if (heartbeatStatus.HasValue && !heartbeatStatus.Value.receivedMsec.HasValue)
		{
			_logger.VeryDebug($"Heartbeat {heartbeatStatus.Value.counter} ({heartbeatStatus.Value.sentMsec}) was never received, marking as lost");
			OnPacketLost();
		}
		_statuses[num] = value;
		HeartbeatRequestMessage result = new HeartbeatRequestMessage
		{
			counter = _nextIndex
		};
		_nextIndex++;
		return result;
	}

	public void OnHeartbeatReceived(HeartbeatResponseMessage message, ulong timeMsec)
	{
		_logger.VeryDebug($"Received heartbeat for {message.counter}");
		int num = _nextIndex - 20;
		if (message.counter < num || message.counter >= _nextIndex)
		{
			_logger.VeryDebug($"Counter {message.counter} is less than {num} and greater than {_nextIndex}");
			return;
		}
		int num2 = message.counter % 20;
		if (num2 >= _statuses.Length)
		{
			return;
		}
		HeartbeatStatus valueOrDefault = _statuses[num2].GetValueOrDefault();
		if (valueOrDefault.counter != message.counter)
		{
			_logger.VeryDebug($"Counter in message {message.counter} does not match counter at index {num2}, which is {valueOrDefault.counter}");
			return;
		}
		if (valueOrDefault.receivedMsec.HasValue)
		{
			_logger.VeryDebug($"Already received message for index {message.counter}");
			return;
		}
		valueOrDefault.receivedMsec = timeMsec;
		_statuses[num2] = valueOrDefault;
		if (!message.isLoading)
		{
			int num3 = (int)(valueOrDefault.receivedMsec.Value - valueOrDefault.sentMsec);
			PingMsec = Mathf.Lerp(PingMsec, num3, 0.2f);
		}
		else
		{
			Log.Debug("Not updating ping because sender is loading");
		}
		PacketLoss = Mathf.Lerp(PacketLoss, 0f, 0.2f);
		LastReceivedTime = valueOrDefault.receivedMsec.Value;
		RemoteIsLoading = message.isLoading;
	}

	private void OnPacketLost()
	{
		PacketLoss = Mathf.Lerp(PacketLoss, 1f, 0.2f);
	}
}
