using System;
using System.Linq;
using MegaCrit.Sts2.Core.Achievements;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Platform.Null;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Platform;

/// <summary>
/// Controls achievement unlocking.
/// </summary>
public static class AchievementsUtil
{
	private static readonly IAchievementStrategy _platform = new NullAchievementStrategy();

	public static event Action? AchievementsChanged;

	/// <summary>
	/// Unlocks an achievement on the current platform and profile.
	/// Remember that the player can have multiple profiles per platform account. This unlocks the achievement on the
	/// profile's progress.save, as well as on the platform account if the account does not yet have it.
	/// </summary>
	/// <param name="achievement">Achievement to unlock.</param>
	/// <param name="localPlayer">
	/// Optional local player from when the achievement was unlocked.
	/// Will be null when unlocked outside a run (like on the main menu or after game over).
	/// </param>
	public static void Unlock(Achievement achievement, Player? localPlayer)
	{
	}

	/// <summary>
	/// Revokes an achievement from the current player profile.
	/// Should only be used for debugging.
	/// </summary>
	public static void Revoke(Achievement achievement)
	{
		if (IsUnlocked(achievement))
		{
			Log.Debug($"Revoking achievement: {achievement}");
			_platform.Revoke(achievement);
			if (SaveManager.Instance.Progress.RemoveUnlockedAchievement(achievement))
			{
				SaveManager.Instance.SaveProgressFile();
			}
			AchievementsUtil.AchievementsChanged?.Invoke();
		}
	}

	/// <summary>
	/// Returns whether an achievement is unlocked on the current player profile.
	/// Note that this does _not_ tell you whether the achievement is unlocked on the player's platform account. There
	/// is likely no scenario in which we need that, but if we do, add a new API.
	/// </summary>
	public static bool IsUnlocked(Achievement achievement)
	{
		return SaveManager.Instance.Progress.IsAchievementUnlocked(achievement);
	}

	public static int TotalAchievementCount()
	{
		return Enum.GetValues<Achievement>().Length;
	}

	public static int UnlockedAchievementCount()
	{
		return Enum.GetValues<Achievement>().Count(IsUnlocked);
	}
}
