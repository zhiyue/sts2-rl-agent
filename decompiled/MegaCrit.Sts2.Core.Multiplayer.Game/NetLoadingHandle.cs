using System;
using System.Collections.Generic;

namespace MegaCrit.Sts2.Core.Multiplayer.Game;

public class NetLoadingHandle : IDisposable
{
	private static readonly Dictionary<INetGameService, int> _loadCounts = new Dictionary<INetGameService, int>();

	private readonly INetGameService _netService;

	public NetLoadingHandle(INetGameService netService)
	{
		_netService = netService;
		if (!_loadCounts.TryGetValue(_netService, out var value) || value == 0)
		{
			_netService.SetGameLoading(isLoading: true);
		}
		_loadCounts[_netService] = value + 1;
	}

	public void Dispose()
	{
		if (_loadCounts[_netService] == 1)
		{
			_netService.SetGameLoading(isLoading: false);
		}
		_loadCounts[_netService]--;
	}

	public static void Release(INetGameService netService)
	{
		_loadCounts.Remove(netService);
	}
}
