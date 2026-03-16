using System;
using System.Linq;
using MegaCrit.Sts2.Core.Achievements;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Platform.Null;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Platform;

public static class AchievementsUtil
{
	private static readonly IAchievementStrategy _platform = new NullAchievementStrategy();

	public static event Action? AchievementsChanged;

	public static void Unlock(Achievement achievement, Player? localPlayer)
	{
	}

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
