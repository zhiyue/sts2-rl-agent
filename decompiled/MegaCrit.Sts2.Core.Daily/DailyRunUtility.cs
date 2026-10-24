using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Leaderboard;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Daily;

/// <summary>
/// A little helper for uploading the score at the end of a daily run.
/// </summary>
public static class DailyRunUtility
{
	/// <summary>
	/// Uploads a score to the daily leaderboard for the given time.
	/// </summary>
	public static async Task UploadScore(DateTimeOffset time, int score, List<SerializablePlayer> players)
	{
		List<ulong> playerIdsInRun = players.Select((SerializablePlayer p) => p.NetId).ToList();
		if (playerIdsInRun != null && playerIdsInRun.Count == 1 && playerIdsInRun[0] == 1)
		{
			playerIdsInRun[0] = PlatformUtil.GetLocalPlayerId(LeaderboardManager.CurrentPlatform);
		}
		string leaderboardName = GetLeaderboardName(time, players.Count);
		if (!(await ShouldUploadScore(await LeaderboardManager.GetLeaderboard(leaderboardName), playerIdsInRun)))
		{
			Log.Info($"Player already uploaded score for daily {time}, ignoring new score");
			return;
		}
		await LeaderboardManager.UploadLocalScore(await LeaderboardManager.GetOrCreateLeaderboard(leaderboardName), score, playerIdsInRun);
		Log.Info($"Uploaded score of {score} for daily {time} to leaderboard {leaderboardName}");
	}

	/// <summary>
	/// Figures out whether we should upload a score to the passed leaderboard.
	/// If any player in the run has already submitted a score, then this returns false.
	/// </summary>
	public static async Task<bool> ShouldUploadScore(ILeaderboardHandle? handle, IReadOnlyList<ulong> playerIdsInRun, CancellationToken cancelToken = default(CancellationToken))
	{
		if (handle == null)
		{
			return true;
		}
		return (await LeaderboardManager.QueryLeaderboardForUsers(handle, playerIdsInRun, cancelToken)).Count <= 0;
	}

	/// <param name="dateTime">Date to retrieve the leaderboard for. All fields other than year/month/day are ignored.</param>
	/// <param name="playerCount">Count of players to retrieve the leaderboard for.</param>
	/// <returns>The name of the leaderboard used for the passed date and player count.</returns>
	public static string GetLeaderboardName(DateTimeOffset dateTime, int playerCount)
	{
		PlatformBranch platformBranch = PlatformUtil.GetPlatformBranch();
		bool flag = (uint)(platformBranch - 2) <= 2u;
		if (!flag && NGame.IsReleaseGame())
		{
			return $"{dateTime.Year}_{dateTime.Month:D2}_{dateTime.Day:D2}_{playerCount}p";
		}
		return $"{dateTime.Year}_{dateTime.Month:D2}_{dateTime.Day:D2}_{playerCount}p_BETA";
	}
}
