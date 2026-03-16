using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Platform;

namespace MegaCrit.Sts2.Core.Leaderboard;

public interface ILeaderboardStrategy
{
	PlatformType Platform { get; }

	Task<ILeaderboardHandle> GetOrCreateLeaderboard(string name);

	Task<ILeaderboardHandle?> GetLeaderboard(string name);

	Task UploadLocalScore(ILeaderboardHandle handle, int score, IReadOnlyList<ulong> otherIds);

	Task<List<LeaderboardEntry>> QueryLeaderboard(ILeaderboardHandle handle, LeaderboardQueryType type, int startIndex, int count);

	Task<List<LeaderboardEntry>> QueryLeaderboardForUsers(ILeaderboardHandle handle, IReadOnlyList<ulong> userIds);

	int GetLeaderboardEntryCount(ILeaderboardHandle handle);
}
