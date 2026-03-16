using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Leaderboard;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Daily;

public static class DailyRunUtility
{
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

	public static async Task<bool> ShouldUploadScore(ILeaderboardHandle? handle, IReadOnlyList<ulong> playerIdsInRun)
	{
		if (handle == null)
		{
			return true;
		}
		return (await LeaderboardManager.QueryLeaderboardForUsers(handle, playerIdsInRun)).Count <= 0;
	}

	public static string GetLeaderboardName(DateTimeOffset dateTime, int playerCount)
	{
		return $"{dateTime.Year}_{dateTime.Month}_{dateTime.Day}_{playerCount}p";
	}

	public static void UploadScoreIfNecessary(SerializableRun serializableRun, ulong playerId, bool isVictory)
	{
		if (!serializableRun.DailyTime.HasValue)
		{
			throw new InvalidOperationException("Tried to upload daily score of a non-daily run!");
		}
		DateTimeOffset value = serializableRun.DailyTime.Value;
		int score = ScoreUtility.CalculateScore(serializableRun, isVictory);
		TaskHelper.RunSafely(UploadScore(value, score, serializableRun.Players));
	}
}
