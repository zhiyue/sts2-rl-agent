using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Transport.Steam;
using Steamworks;

namespace MegaCrit.Sts2.Core.Platform.Steam;

public static class SteamStatsManager
{
	private const string ArchitectDamageStat = "architect_damage";

	private static Callback<UserStatsReceived_t>? _userStatsReceivedCallback;

	private static bool _userStatsReady;

	private static bool _globalStatsReady;

	private static long _globalArchitectDamage;

	public static bool IsGlobalStatsReady => _globalStatsReady;

	public static void Initialize()
	{
		if (SteamInitializer.Initialized)
		{
			_userStatsReady = false;
			_globalStatsReady = false;
			_globalArchitectDamage = 0L;
			_userStatsReceivedCallback = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
			SteamUserStats.RequestCurrentStats();
			TaskHelper.RunSafely(RefreshGlobalStats());
		}
	}

	public static async Task RefreshGlobalStats()
	{
		if (!SteamInitializer.Initialized)
		{
			return;
		}
		SteamAPICall_t call = SteamUserStats.RequestGlobalStats(0);
		using SteamCallResult<GlobalStatsReceived_t> callResult = new SteamCallResult<GlobalStatsReceived_t>(call);
		try
		{
			OnGlobalStatsReceived(await callResult.Task);
		}
		catch
		{
			Log.Warn("SteamStatsManager: Failed to receive global stats");
		}
	}

	public static void IncrementArchitectDamage(int score)
	{
		if (!_userStatsReady)
		{
			Log.Warn("SteamStatsManager: Cannot increment architect damage, user stats not ready");
			return;
		}
		int pData;
		bool stat = SteamUserStats.GetStat("architect_damage", out pData);
		bool value = SteamUserStats.SetStat("architect_damage", pData + score);
		bool value2 = SteamUserStats.StoreStats();
		Log.Info($"SteamStatsManager: IncrementArchitectDamage by {score} (was {pData}, now {pData + score}) [get={stat}, set={value}, store={value2}]");
	}

	public static long GetGlobalArchitectDamage()
	{
		return _globalArchitectDamage;
	}

	private static void OnUserStatsReceived(UserStatsReceived_t result)
	{
		if (result.m_nGameID == 2868840)
		{
			if (result.m_eResult == EResult.k_EResultOK)
			{
				_userStatsReady = true;
				Log.Info("SteamStatsManager: User stats received");
			}
			else
			{
				Log.Warn($"SteamStatsManager: User stats request failed with result {result.m_eResult}");
			}
		}
	}

	private static void OnGlobalStatsReceived(GlobalStatsReceived_t result)
	{
		if (result.m_nGameID == 2868840)
		{
			if (result.m_eResult == EResult.k_EResultOK)
			{
				long pData;
				bool globalStat = SteamUserStats.GetGlobalStat("architect_damage", out pData);
				_globalArchitectDamage = pData;
				_globalStatsReady = true;
				Log.Info($"SteamStatsManager: Global stats received (found={globalStat}), architect damage = {_globalArchitectDamage}");
			}
			else
			{
				Log.Warn($"SteamStatsManager: Global stats request failed with result {result.m_eResult}");
			}
		}
	}
}
