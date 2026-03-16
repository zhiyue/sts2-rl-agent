using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Platform.Null;
using MegaCrit.Sts2.Core.Platform.Steam;

namespace MegaCrit.Sts2.Core.Platform;

public static class PlatformUtil
{
	private static readonly NullPlatformUtilStrategy _null = new NullPlatformUtilStrategy();

	private static readonly SteamPlatformUtilStrategy _steam = new SteamPlatformUtilStrategy();

	public static PlatformType PrimaryPlatform
	{
		get
		{
			if (SteamInitializer.Initialized)
			{
				return PlatformType.Steam;
			}
			return PlatformType.None;
		}
	}

	private static IPlatformUtilStrategy GetPlatformUtil(PlatformType platformType)
	{
		return platformType switch
		{
			PlatformType.None => _null, 
			PlatformType.Steam => _steam, 
			_ => throw new ArgumentOutOfRangeException("platformType", platformType, null), 
		};
	}

	public static string GetPlayerName(PlatformType platformType, ulong playerId)
	{
		return GetPlatformUtil(platformType).GetPlayerName(playerId);
	}

	public static ulong GetLocalPlayerId(PlatformType platformType)
	{
		return GetPlatformUtil(platformType).GetLocalPlayerId();
	}

	public static Task<IEnumerable<ulong>> GetFriendsWithOpenLobbies(PlatformType platformType)
	{
		return GetPlatformUtil(platformType).GetFriendsWithOpenLobbies();
	}

	public static bool SupportsInviteDialog(PlatformType platformType)
	{
		return GetPlatformUtil(platformType).SupportsInviteDialog;
	}

	public static void OpenInviteDialog(INetGameService netService)
	{
		GetPlatformUtil(netService.Platform).OpenInviteDialog(netService);
	}

	public static void OpenUrl(string url)
	{
		GetPlatformUtil(PrimaryPlatform).OpenUrl(url);
	}

	public static void OpenVirtualKeyboard()
	{
		GetPlatformUtil(PrimaryPlatform).OpenVirtualKeyboard();
	}

	public static void CloseVirtualKeyboard()
	{
		GetPlatformUtil(PrimaryPlatform).CloseVirtualKeyboard();
	}

	public static string? GetPlatformBranch()
	{
		return GetPlatformUtil(PrimaryPlatform).GetPlatformBranch();
	}

	public static string? GetThreeLetterLanguageCode()
	{
		return GetPlatformUtil(PrimaryPlatform).GetThreeLetterLanguageCode();
	}

	public static string GetRawLanguage()
	{
		return GetPlatformUtil(PrimaryPlatform).GetRawLanguage();
	}

	public static void SetRichPresence(string token, string? playerGroup, int? groupSize)
	{
		GetPlatformUtil(PrimaryPlatform).SetRichPresence(token, playerGroup, groupSize);
	}

	public static void SetRichPresenceValue(string key, string? value)
	{
		GetPlatformUtil(PrimaryPlatform).SetRichPresenceValue(key, value);
	}

	public static void ClearRichPresence()
	{
		GetPlatformUtil(PrimaryPlatform).ClearRichPresence();
	}

	public static SupportedWindowMode GetSupportedWindowMode()
	{
		return GetPlatformUtil(PrimaryPlatform).GetSupportedWindowMode();
	}
}
