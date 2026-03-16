using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Messages;

namespace MegaCrit.Sts2.Core.Multiplayer.Quality;

public class NetQualityTracker : IDisposable
{
	public const int sendRateMsec = 200;

	public const int logRateMsec = 20000;

	private readonly INetGameService _netService;

	private readonly MegaCrit.Sts2.Core.Logging.Logger _logger = new MegaCrit.Sts2.Core.Logging.Logger("NetQualityTracker", LogType.Network);

	private readonly List<ConnectionStats> _stats = new List<ConnectionStats>();

	private ulong? _lastUpdateMsec;

	private ulong? _lastLogMsec;

	private bool _isLoading;

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

	private ulong GetCurrentTime()
	{
		return getTimeMsec?.Invoke() ?? Time.GetTicksMsec();
	}

	public void OnPeerConnected(ulong peerId)
	{
		_stats.Add(new ConnectionStats(peerId));
	}

	public void OnPeerDisconnected(ulong peerId)
	{
		_stats.RemoveAll((ConnectionStats s) => s.PeerId == peerId);
	}

	public void SetIsLoading(bool isLoading)
	{
		Log.Debug($"Loading set to {isLoading}");
		_isLoading = isLoading;
	}

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

	private void HandleHeartbeatRequestMessage(HeartbeatRequestMessage message, ulong senderId)
	{
		_netService.SendMessage(new HeartbeatResponseMessage
		{
			counter = message.counter,
			isLoading = _isLoading
		}, senderId);
	}

	private void HandleHeartbeatResponseMessage(HeartbeatResponseMessage message, ulong senderId)
	{
		GetStatsForPeer(senderId)?.OnHeartbeatReceived(message, GetCurrentTime());
	}

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
