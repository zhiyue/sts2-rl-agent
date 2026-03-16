using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Platform.Steam;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Platform;

public static class StatsManager
{
	public static void RefreshGlobalStats()
	{
		TaskHelper.RunSafely(SteamStatsManager.RefreshGlobalStats());
	}

	public static void IncrementArchitectDamage(int score)
	{
		SteamStatsManager.IncrementArchitectDamage(score);
	}

	public static long GetPersonalArchitectDamage()
	{
		return SaveManager.Instance.Progress.ArchitectDamage;
	}

	public static long? GetGlobalArchitectDamage()
	{
		if (SteamStatsManager.IsGlobalStatsReady)
		{
			long globalArchitectDamage = SteamStatsManager.GetGlobalArchitectDamage();
			if (globalArchitectDamage > 0)
			{
				return globalArchitectDamage;
			}
		}
		return null;
	}
}
