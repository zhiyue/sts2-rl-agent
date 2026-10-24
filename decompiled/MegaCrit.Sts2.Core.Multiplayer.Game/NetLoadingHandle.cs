using System;
using System.Collections.Generic;

namespace MegaCrit.Sts2.Core.Multiplayer.Game;

/// <summary>
/// A utility class for marking the start and end of a loading sequence. Basically just equivalent of calling
/// NetService.SetGameLoading with true, and then false at the end of the scope.
/// Construct one of these with a using statement any time we enter a long loading sequence to let the networking layer
/// know that the game's framerate might dip. If you don't do this, we might accidentally display a "waiting for
/// connection from host" overlay because messages aren't being processed.
/// </summary>
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
