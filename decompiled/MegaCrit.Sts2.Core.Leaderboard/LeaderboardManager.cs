using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Platform.Null;
using MegaCrit.Sts2.Core.Platform.Steam;

namespace MegaCrit.Sts2.Core.Leaderboard;

public static class LeaderboardManager
{
	private static ILeaderboardStrategy _strategy;

	public static PlatformType CurrentPlatform => _strategy.Platform;

	public static void Initialize()
	{
		if (SteamInitializer.Initialized)
		{
			_strategy = new SteamLeaderboardStrategy();
		}
		else
		{
			_strategy = new NullLeaderboardStrategy();
		}
	}

	public static Task<ILeaderboardHandle> GetOrCreateLeaderboard(string name)
	{
		return _strategy.GetOrCreateLeaderboard(name);
	}

	public static Task<ILeaderboardHandle?> GetLeaderboard(string name)
	{
		return _strategy.GetLeaderboard(name);
	}

	public static Task UploadLocalScore(ILeaderboardHandle handle, int score, IReadOnlyList<ulong> userIds)
	{
		return _strategy.UploadLocalScore(handle, score, userIds);
	}

	public static Task<List<LeaderboardEntry>> QueryLeaderboard(ILeaderboardHandle handle, LeaderboardQueryType type, int startIndex, int resultCount)
	{
		return _strategy.QueryLeaderboard(handle, type, startIndex, resultCount);
	}

	public static Task<List<LeaderboardEntry>> QueryLeaderboardForUsers(ILeaderboardHandle handle, IReadOnlyList<ulong> userIds)
	{
		return _strategy.QueryLeaderboardForUsers(handle, userIds);
	}

	public static int GetLeaderboardEntryCount(ILeaderboardHandle handle)
	{
		return _strategy.GetLeaderboardEntryCount(handle);
	}

	public static void DebugAddEntry(ILeaderboardHandle handle, LeaderboardEntry entry)
	{
		if (!(_strategy is NullLeaderboardStrategy nullLeaderboardStrategy))
		{
			throw new NotImplementedException();
		}
		nullLeaderboardStrategy.DebugAddEntry(handle, entry);
	}
}
