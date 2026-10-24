using System.Collections.Generic;

namespace MegaCrit.Sts2.Core.Leaderboard;

/// <summary>
/// Abstracted leaderboard entry returned by querying the leaderboard API.
/// </summary>
public class LeaderboardEntry
{
	/// <summary>
	/// Rank on the leaderboard reported from the API.
	/// </summary>
	public int rank;

	/// <summary>
	/// Name of the user who submitted the entry.
	/// </summary>
	public required string name;

	/// <summary>
	/// ID of the user who submitted the entry.
	/// </summary>
	public ulong id;

	/// <summary>
	/// The score of the submission.
	/// </summary>
	public int score;

	/// <summary>
	/// Other player IDs present in the run when this score was submitted.
	/// </summary>
	public List<ulong> userIds = new List<ulong>();
}
