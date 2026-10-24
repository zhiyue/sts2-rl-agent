using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Messages;

namespace MegaCrit.Sts2.Core.Multiplayer.Quality;

/// <summary>
/// Keeps track of the quality of the connection to each peer, for use in displaying "poor connection" notifications to
/// the user.
///
/// To estimate connection quality, the following strategy is used:
/// - At regular intervals, a "heartbeat" message is generated and sent to each peer. We record the time at which the
///   message is sent.
/// - When it is received, the peer echoes the heartbeat message back to us.
/// - When we receive the response, we check when it was sent, and adjust our estimated round-trip time (ping) to the
///   peer accordingly (in <see cref="T:MegaCrit.Sts2.Core.Multiplayer.Quality.ConnectionStats" />).
/// - If we go some time without seeing the echoed message, then we assume the packet is lost. We adjust our estimated
///   packet loss to the peer accordingly.
///
/// Further work can be done here to estimate jitter, but I don't think we need it for this game.
/// </summary>
public class NetQualityTracker : IDisposable
{
	/// <summary>
	/// The time that must elapse before sending another heartbeat packet.
	/// </summary>
	public const int sendRateMsec = 200;

	/// <summary>
	/// The time that must elapse before we log the connection statistics to the log.
	/// </summary>
	public const int logRateMsec = 20000;

	private readonly INetGameService _netService;

	private readonly MegaCrit.Sts2.Core.Logging.Logger _logger = new MegaCrit.Sts2.Core.Logging.Logger("NetQualityTracker", LogType.Network);

	/// <summary>
	/// Connection statistics objects, one per peer.
	/// </summary>
	private readonly List<ConnectionStats> _stats = new List<ConnectionStats>();

	/// <summary>
	/// The last time we sent a heartbeat packet.
	/// </summary>
	private ulong? _lastUpdateMsec;

	/// <summary>
	/// The last time we logged the connection statistics.
	/// </summary>
	private ulong? _lastLogMsec;

	/// <summary>
	/// True if the game is in a loading state. See <see cref="M:MegaCrit.Sts2.Core.Multiplayer.Quality.NetQualityTracker.SetIsLoading(System.Boolean)" />.
	/// </summary>
	private bool _isLoading;

	/// <summary>
	/// This is called when to get the current time. Used to mock time in tests. If null, Time.GetTicksMsec is used.
	/// </summary>
	public Func<ulong>? getTimeMsec;

	public bool IsGameLoading => _isLoading;

	public NetQualityTracker(INetGameService netService)
	{
		_netService = netService;
		netService.RegisterMessageHandler<HeartbeatRequestMessage>(HandleHeartbeatRequestMessage);
		netService.RegisterMessageHandler<HeartbeatResponseMessage>(HandleHeartbeatResponseMessage);
	}

	public void Dispose()
	{
		_netService.UnregisterMessageHandler<HeartbeatRequestMessage>(HandleHeartbeatRequestMessage);
		_netService.UnregisterMessageHandler<HeartbeatResponseMessage>(HandleHeartbeatResponseMessage);
	}

	/// <summary>
	/// Gets the current time.
	/// Pulls from godot or a mock time source.
	/// </summary>
	private ulong GetCurrentTime()
	{
		return getTimeMsec?.Invoke() ?? Time.GetTicksMsec();
	}

	/// <summary>
	/// Should be called whenever a peer connects. Begins tracking their connection statistics.
	/// </summary>
	public void OnPeerConnected(ulong peerId)
	{
		_stats.Add(new ConnectionStats(peerId));
	}

	/// <summary>
	/// Should be called whenever a peer connects. Stops tracking their connection statistics.
	/// </summary>
	public void OnPeerDisconnected(ulong peerId)
	{
		_stats.RemoveAll((ConnectionStats s) => s.PeerId == peerId);
	}

	/// <summary>
	/// Called when the game enters or exits a loading state.
	/// This is important to know because, in the middle of loading, NetService.Update can be called at irregular
	/// intervals, leading to a much larger-than-normal round-trip time even though the network connection is stable.
	/// This info is sent through the HeartbeatResponseMessage, and when it is true, the receiver should not update
	/// ping, but still update last received time.
	/// </summary>
	/// <param name="isLoading">True if the game is in a loading state, false otherwise.</param>
	public void SetIsLoading(bool isLoading)
	{
		Log.Debug($"Loading set to {isLoading}");
		_isLoading = isLoading;
	}

	/// <summary>
	/// Should be called on a regular basis to send heartbeat messages.
	/// </summary>
	public void Update()
	{
		ulong currentTime = GetCurrentTime();
		if (!_lastUpdateMsec.HasValue)
		{
			_lastUpdateMsec = currentTime;
		}
		else if (currentTime - _lastUpdateMsec.Value >= 200)
		{
			foreach (ConnectionStats stat in _stats)
			{
				HeartbeatRequestMessage message = stat.GenerateHeartbeat(currentTime);
				_netService.SendMessage(message, stat.PeerId);
			}
			_lastUpdateMsec = currentTime;
		}
		if (!_logger.WillLog(LogLevel.Debug))
		{
			return;
		}
		if (!_lastLogMsec.HasValue)
		{
			_lastLogMsec = currentTime;
		}
		else
		{
			if (!(currentTime - _lastLogMsec >= 20000) || _stats.Count <= 0)
			{
				return;
			}
			_lastLogMsec = currentTime;
			_logger.Debug("Connection statistics at " + Log.Timestamp + ":");
			foreach (ConnectionStats stat2 in _stats)
			{
				_logger.Debug($"\t{stat2.PeerId} - Ping: {stat2.PingMsec}. Packet Loss: {stat2.PacketLoss}.");
			}
		}
	}

	/// <summary>
	/// Called whenever a heartbeat request comes in.
	/// This just echoes the heartbeat response back to the sender.
	/// </summary>
	private void HandleHeartbeatRequestMessage(HeartbeatRequestMessage message, ulong senderId)
	{
		_netService.SendMessage(new HeartbeatResponseMessage
		{
			counter = message.counter,
			isLoading = _isLoading
		}, senderId);
	}

	/// <summary>
	/// Called whenever a response to our heartbeat request comes back.
	/// Processes the message and updates our statistics.
	/// </summary>
	private void HandleHeartbeatResponseMessage(HeartbeatResponseMessage message, ulong senderId)
	{
		GetStatsForPeer(senderId)?.OnHeartbeatReceived(message, GetCurrentTime());
	}

	/// <summary>
	/// Returns the statistics for a given peer.
	/// </summary>
	public ConnectionStats? GetStatsForPeer(ulong peerId)
	{
		foreach (ConnectionStats stat in _stats)
		{
			if (stat.PeerId == peerId)
			{
				return stat;
			}
		}
		return null;
	}
}
