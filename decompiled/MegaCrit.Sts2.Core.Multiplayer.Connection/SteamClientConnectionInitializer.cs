using System;
using System.Threading;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Transport.Steam;
using MegaCrit.Sts2.Core.Platform;

namespace MegaCrit.Sts2.Core.Multiplayer.Connection;

public class SteamClientConnectionInitializer : IClientConnectionInitializer
{
	private ulong? _playerSteamId;

	private ulong? _lobbySteamId;

	public static SteamClientConnectionInitializer FromPlayer(ulong playerSteamId)
	{
		return new SteamClientConnectionInitializer
		{
			_playerSteamId = playerSteamId
		};
	}

	public static SteamClientConnectionInitializer FromLobby(ulong lobbySteamId)
	{
		return new SteamClientConnectionInitializer
		{
			_lobbySteamId = lobbySteamId
		};
	}

	public async Task<NetErrorInfo?> Connect(NetClientGameService gameService, CancellationToken cancelToken = default(CancellationToken))
	{
		if (gameService.IsConnected)
		{
			throw new InvalidOperationException("NetClientGameService must not be connected when passed to SteamClientConnectionInitializer!");
		}
		SteamClient steamClient = new SteamClient(gameService);
		gameService.Initialize(steamClient, PlatformType.Steam);
		if (_playerSteamId.HasValue)
		{
			return await steamClient.ConnectToLobbyOwnedByFriend(_playerSteamId.Value, cancelToken);
		}
		if (_lobbySteamId.HasValue)
		{
			return await steamClient.ConnectToLobby(_lobbySteamId.Value, cancelToken);
		}
		throw new InvalidOperationException("Neither player nor lobby is set!");
	}

	public override string ToString()
	{
		return $"{"SteamClientConnectionInitializer"} player: {_playerSteamId} lobby: {_lobbySteamId}";
	}
}
