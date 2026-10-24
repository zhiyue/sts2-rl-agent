namespace MegaCrit.Sts2.Core.Leaderboard;

public enum LeaderboardQueryType
{
	None,
	/// <summary>
	/// Query globally. Indexes control the range of the query.
	/// </summary>
	Global,
	/// <summary>
	/// Query around the current user. Indexes are relative to the user.
	/// </summary>
	AroundUser,
	/// <summary>
	/// Query the user's friends. Indexes are ignored.
	/// </summary>
	FriendsOnly
}
