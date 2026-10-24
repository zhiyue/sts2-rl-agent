namespace MegaCrit.Sts2.Core.Runs;

public static class GameModeExtension
{
	/// <summary>
	/// Helper method to check if epochs and achievements are allowed this run.
	/// Update once more game modes are created.
	/// </summary>
	public static bool AreAchievementsAndEpochsLocked(this GameMode gameMode)
	{
		return gameMode != GameMode.Standard;
	}
}
