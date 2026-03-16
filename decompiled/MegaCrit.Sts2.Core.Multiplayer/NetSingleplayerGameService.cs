using System;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Quality;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Platform.Steam;

namespace MegaCrit.Sts2.Core.Multiplayer;

public class NetSingleplayerGameService : INetGameService
{
	public const int defaultNetId = 1;

	private bool _isLoading;

	public bool IsConnected => true;

	public bool IsGameLoading => _isLoading;

	public ulong NetId => 1uL;

	public NetGameType Type => NetGameType.Singleplayer;

	public PlatformType Platform
	{
		get
		{
			if (!SteamInitializer.Initialized)
			{
				return PlatformType.None;
			}
			return PlatformType.Steam;
		}
	}

	public event Action<NetErrorInfo>? Disconnected;

	public void SendMessage<T>(T message, ulong playerId) where T : INetMessage
	{
	}

	public void SendMessage<T>(T message) where T : INetMessage
	{
	}

	public void RegisterMessageHandler<T>(MessageHandlerDelegate<T> handler) where T : INetMessage
	{
	}

	public void UnregisterMessageHandler<T>(MessageHandlerDelegate<T> handler) where T : INetMessage
	{
	}

	public void Update()
	{
	}

	public void Disconnect(NetError reason, bool now = false)
	{
	}

	public ConnectionStats GetStatsForPeer(ulong peerId)
	{
		throw new NotImplementedException();
	}

	public void SetGameLoading(bool isLoading)
	{
		_isLoading = isLoading;
	}

	public string? GetRawLobbyIdentifier()
	{
		return null;
	}
}
