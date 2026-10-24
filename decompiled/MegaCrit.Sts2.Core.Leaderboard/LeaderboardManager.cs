using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Platform.Null;
using MegaCrit.Sts2.Core.Platform.Steam;

namespace MegaCrit.Sts2.Core.Leaderboard;

/// <summary>
/// Facade for platform-specific leaderboard operations. Currently only Steam is supported in production;
/// NullLeaderboardStrategy is used for dev/offline. The current implementation uses a sentinel score hack for
/// multiplayer dailies (see ScoreUtility.clientScore) which is Steam-specific and will need revisiting when
/// integrating other platforms' leaderboard APIs.
/// </summary>
public static class LeaderboardManager
{
	private static ILeaderboardStrategy _strategy;

	public static PlatformType CurrentPlatform => _strategy.Platform;

	/// <summary>
	/// This should be called after all platform initialization is completed.
	/// </summary>
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

	/// <summary>
	/// Obtains the leaderboard with the specified name. If it doesn't exist, it is created first.
	/// </summary>
	public static Task<ILeaderboardHandle> GetOrCreateLeaderboard(string name, CancellationToken cancelToken = default(CancellationToken))
	{
		return _strategy.GetOrCreateLeaderboard(name, cancelToken);
	}

	/// <summary>
	/// Obtains the leaderboard with the specified name. If it doesn't exist, null is returned.
	/// </summary>
	public static Task<ILeaderboardHandle?> GetLeaderboard(string name, CancellationToken cancelToken = default(CancellationToken))
	{
		return _strategy.GetLeaderboard(name, cancelToken);
	}

	/// <summary>
	/// Uploads a score associated with the local player.
	/// </summary>
	/// <param name="handle">The leaderboard to upload to, obtained from GetLeaderboard.</param>
	/// <param name="score">The score to upload.</param>
	/// <param name="userIds">User IDs to associate with the score. These will be returned with the leaderboard entry in
	/// future queries, but note that you can only query for the score based on the submitting player.</param>
	public static Task UploadLocalScore(ILeaderboardHandle handle, int score, IReadOnlyList<ulong> userIds)
	{
		return _strategy.UploadLocalScore(handle, score, userIds);
	}

	/// <summary>
	/// Queries the given leaderboard.
	/// </summary>
	/// <param name="handle">The leaderboard to query, obtained from GetLeaderboard.</param>
	/// <param name="type">The type of query.</param>
	/// <param name="startIndex">The index at which to start the query.</param>
	/// <param name="resultCount">The amount of results from the start index to return.</param>
	/// <param name="cancelToken">The token to use when requesting cancellation.</param>
	/// <returns>A task which will result in the entries queried.</returns>
	public static Task<List<LeaderboardEntry>> QueryLeaderboard(ILeaderboardHandle handle, LeaderboardQueryType type, int startIndex, int resultCount, CancellationToken cancelToken = default(CancellationToken))
	{
		return _strategy.QueryLeaderboard(handle, type, startIndex, resultCount, cancelToken);
	}

	/// <summary>
	/// Queries the given leaderboard, only returning entries for the passed users (if they exist).
	/// </summary>
	/// <param name="handle">The leaderboard to query, obtained from GetLeaderboard.</param>
	/// <param name="userIds">List of platform identifiers to query the leaderboard for.</param>
	/// <param name="cancelToken">The token to use when requesting cancellation.</param>
	/// <returns>A task which will result in the entries queried.</returns>
	public static Task<List<LeaderboardEntry>> QueryLeaderboardForUsers(ILeaderboardHandle handle, IReadOnlyList<ulong> userIds, CancellationToken cancelToken = default(CancellationToken))
	{
		return _strategy.QueryLeaderboardForUsers(handle, userIds, cancelToken);
	}

	/// <summary>
	/// Obtains the number of entries in the given leaderboard.
	/// </summary>
	public static int GetLeaderboardEntryCount(ILeaderboardHandle handle)
	{
		return _strategy.GetLeaderboardEntryCount(handle);
	}

	/// <summary>
	/// A method used in debugging. This will throw if the leaderboard is pointing to an actual backend.
	/// Rank is ignored. Score will be used to sort the entries.
	/// </summary>
	public static void DebugAddEntry(ILeaderboardHandle handle, LeaderboardEntry entry)
	{
		if (!(_strategy is NullLeaderboardStrategy nullLeaderboardStrategy))
		{
			throw new NotImplementedException();
		}
		nullLeaderboardStrategy.DebugAddEntry(handle, entry);
	}
}
