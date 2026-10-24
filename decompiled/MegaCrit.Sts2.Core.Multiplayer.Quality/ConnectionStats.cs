using Godot;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Messages;

namespace MegaCrit.Sts2.Core.Multiplayer.Quality;

/// <summary>
/// Holds statistics about a particular connection to a network peer.
/// See <see cref="T:MegaCrit.Sts2.Core.Multiplayer.Quality.NetQualityTracker" /> for how this class is generally used.
/// </summary>
public class ConnectionStats
{
	public const int ringBufferSize = 20;

	private const float _weightedAverageFactor = 0.2f;

	/// <summary>
	/// A ring buffer of heartbeat statuses.
	/// Heartbeats use a monotonically increasing counter for identification. We use that index to overwrite old
	/// heartbeat statuses in this buffer for tracking.
	/// </summary>
	private HeartbeatStatus?[] _statuses = new HeartbeatStatus?[20];

	private int _nextIndex;

	/// <summary>
	/// Logger object for this connection
	/// </summary>
	private readonly MegaCrit.Sts2.Core.Logging.Logger _logger;

	/// <summary>
	/// The peer for which this object gathers statistics.
	/// </summary>
	public ulong PeerId { get; private set; }

	/// <summary>
	/// The estimated average round-trip time to the peer.
	/// </summary>
	public float PingMsec { get; private set; }

	/// <summary>
	/// The estimated average packet loss to the peer.
	/// Note that this is the sum of both outbound and inbound packet loss.
	/// </summary>
	public float PacketLoss { get; private set; }

	/// <summary>
	/// The last time (msec) we received a packet from the peer.
	/// If the difference between the current time and this is a large value, then we might be about to time out.
	/// </summary>
	public ulong? LastReceivedTime { get; private set; }

	/// <summary>
	/// True if the remote end is currently loading, and we should not expect consistent packets.
	/// Used in conjunction with LastReceivedTime to determine whether we should show a "connection interrupted" overlay.
	/// </summary>
	public bool RemoteIsLoading { get; private set; }

	public ConnectionStats(ulong peerId)
	{
		PeerId = peerId;
		_logger = new MegaCrit.Sts2.Core.Logging.Logger($"{"ConnectionStats"} ({peerId})", LogType.Network);
	}

	/// <summary>
	/// Generates a heartbeat request to be sent to the peer.
	/// </summary>
	/// <param name="timeMsec">The current time in milliseconds.</param>
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

	/// <summary>
	/// Call me when we receive a heartbeat message for the peer.
	/// </summary>
	/// <param name="message">The message that was received.</param>
	/// <param name="timeMsec">The current time in milliseconds.</param>
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

	/// <summary>
	/// Called when we're about to overwrite a heartbeat status in the ring buffer, and a response to it was never
	/// received.
	/// </summary>
	private void OnPacketLost()
	{
		PacketLoss = Mathf.Lerp(PacketLoss, 1f, 0.2f);
	}
}
