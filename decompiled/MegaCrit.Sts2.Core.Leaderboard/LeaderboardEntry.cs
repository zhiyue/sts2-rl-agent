using System.Collections.Generic;

namespace MegaCrit.Sts2.Core.Leaderboard;

public class LeaderboardEntry
{
	public int rank;

	public required string name;

	public ulong id;

	public int score;

	public List<ulong> userIds = new List<ulong>();
}
