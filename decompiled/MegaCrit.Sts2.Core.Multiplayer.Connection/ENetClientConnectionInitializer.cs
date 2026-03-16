using System;
using System.Threading;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Transport.ENet;
using MegaCrit.Sts2.Core.Platform;

namespace MegaCrit.Sts2.Core.Multiplayer.Connection;

public class ENetClientConnectionInitializer : IClientConnectionInitializer
{
	private readonly ulong _netId;

	private readonly string _ip;

	private readonly ushort _port;

	public ENetClientConnectionInitializer(ulong netId, string ip, ushort port)
	{
		_netId = netId;
		_ip = ip;
		_port = port;
	}

	public async Task<NetErrorInfo?> Connect(NetClientGameService gameService, CancellationToken cancelToken = default(CancellationToken))
	{
		if (gameService.IsConnected)
		{
			throw new InvalidOperationException("NetClientGameService must not be connected when passed to ENetClientConnectionInitializer!");
		}
		ENetClient eNetClient = new ENetClient(gameService);
		gameService.Initialize(eNetClient, PlatformType.None);
		return await eNetClient.ConnectToHost(_netId, _ip, _port, cancelToken);
	}
}
