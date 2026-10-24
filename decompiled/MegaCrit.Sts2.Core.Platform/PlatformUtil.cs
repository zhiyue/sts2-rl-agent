using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Helpers;
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

	/// <summary>
	/// Obtains the player name for the given player ID, with bracket escaping for safe use in BBCode contexts.
	/// Use <see cref="M:MegaCrit.Sts2.Core.Platform.PlatformUtil.GetPlayerNameRaw(MegaCrit.Sts2.Core.Platform.PlatformType,System.UInt64)" /> when displaying in plain text controls.
	/// </summary>
	/// <remarks>
	/// Player names are untrusted external input (e.g. Steam display names) and may contain square brackets that
	/// would crash or corrupt the BBCode parser. This method escapes by default so that callers don't need to
	/// remember to escape individually. The majority of call sites render through MegaRichTextLabel or HoverTip
	/// (both BBCode), so safe-by-default prevents the class of bugs where a new call site forgets to escape.
	/// </remarks>
	/// <param name="platformType">The platform type to use. Must be passed for cases where multiple platforms are
	/// supported; for instance, on PC we can play on LAN (for debug) or on Steam.</param>
	/// <param name="playerId">The ID of the player whose name is retrieved.</param>
	public static string GetPlayerName(PlatformType platformType, ulong playerId)
	{
		return GetPlayerNameRaw(platformType, playerId).EscapeBbcodeTags();
	}

	/// <summary>
	/// Obtains the raw player name for the given player ID, without any escaping.
	/// Only use this for plain text contexts (e.g. <see cref="T:Godot.Label" />, <see cref="T:MegaCrit.Sts2.addons.mega_text.MegaLabel" />).
	/// For BBCode contexts (e.g. <see cref="T:MegaCrit.Sts2.addons.mega_text.MegaRichTextLabel" />, HoverTip), use
	/// <see cref="M:MegaCrit.Sts2.Core.Platform.PlatformUtil.GetPlayerName(MegaCrit.Sts2.Core.Platform.PlatformType,System.UInt64)" /> instead.
	/// </summary>
	/// <param name="platformType">The platform type to use. Must be passed for cases where multiple platforms are
	/// supported; for instance, on PC we can play on LAN (for debug) or on Steam.</param>
	/// <param name="playerId">The ID of the player whose name is retrieved.</param>
	public static string GetPlayerNameRaw(PlatformType platformType, ulong playerId)
	{
		return GetPlatformUtil(platformType).GetPlayerName(playerId);
	}

	/// <summary>
	/// Returns the ID of the player playing on this machine.
	/// </summary>
	/// <param name="platformType">The platform type to use. Must be passed for cases where multiple platforms are
	/// supported; for instance, on PC we can play on LAN (for debug) or on Steam.</param>
	public static ulong GetLocalPlayerId(PlatformType platformType)
	{
		return GetPlatformUtil(platformType).GetLocalPlayerId();
	}

	/// <summary>
	/// Returns IDs of friends who are currently in lobbies that can be joined.
	/// </summary>
	/// <param name="platformType">The platform type to use. Must be passed for cases where multiple platforms are
	/// supported; for instance, on PC we can play on LAN (for debug) or on Steam.</param>
	public static Task<IEnumerable<ulong>> GetFriendsWithOpenLobbies(PlatformType platformType)
	{
		return GetPlatformUtil(platformType).GetFriendsWithOpenLobbies();
	}

	/// <summary>
	/// Returns true if OpenInviteDialog may be called on the given platform; false otherwise.
	/// </summary>
	/// <param name="platformType">The platform type to use. Must be passed for cases where multiple platforms are
	/// supported; for instance, on PC we can play on LAN (for debug) or on Steam.</param>
	public static bool SupportsInviteDialog(PlatformType platformType)
	{
		return GetPlatformUtil(platformType).SupportsInviteDialog;
	}

	/// <summary>
	/// Opens the platform invite dialog for the given net service.
	/// </summary>
	public static void OpenInviteDialog(INetGameService netService)
	{
		GetPlatformUtil(netService.Platform).OpenInviteDialog(netService);
	}

	/// <summary>
	/// Opens a URL in a platform-specific browser.
	/// For example, on Steam, the URL is opened in the game overlay if available. On non-Steam PC, the URL is opened
	/// in the OS' default browser.
	/// </summary>
	public static void OpenUrl(string url)
	{
		GetPlatformUtil(PrimaryPlatform).OpenUrl(url);
	}

	/// <summary>
	/// Opens the virtual keyboard.
	/// Does nothing on PC.
	/// </summary>
	public static void OpenVirtualKeyboard()
	{
		GetPlatformUtil(PrimaryPlatform).OpenVirtualKeyboard();
	}

	/// <summary>
	/// Closes the virtual keyboard.
	/// </summary>
	public static void CloseVirtualKeyboard()
	{
		GetPlatformUtil(PrimaryPlatform).CloseVirtualKeyboard();
	}

	/// <summary>
	/// Returns which platform branch the player is playing on.
	/// </summary>
	public static PlatformBranch GetPlatformBranch()
	{
		return GetPlatformUtil(PrimaryPlatform).GetPlatformBranch();
	}

	/// <summary>
	/// Returns the three-letter language code for the language that the player currently has set for the primary platform.
	/// </summary>
	public static string? GetThreeLetterLanguageCode()
	{
		return GetPlatformUtil(PrimaryPlatform).GetThreeLetterLanguageCode();
	}

	/// <summary>
	/// Returns the raw string identifier of the language code that the player currently has set for primary platform.
	/// </summary>
	public static string GetRawLanguage()
	{
		return GetPlatformUtil(PrimaryPlatform).GetRawLanguage();
	}

	/// <summary>
	/// Sets the primary rich presence display data for the player.
	/// This API is currently rather Steam-centric, so feel free to refactor later if necessary.
	/// </summary>
	/// <param name="token">The key in the rich presence localization to display as the primary string. Additional
	/// key/value pairs used for substitutions can be provided through <see cref="M:MegaCrit.Sts2.Core.Platform.PlatformUtil.SetRichPresenceValue(System.String,System.String)" />.</param>
	/// <param name="playerGroup">The group to associate with. This is an arbitrary string that is used to associate
	/// players in the same multiplayer group together. Pass null if there is no multiplayer group.</param>
	/// <param name="groupSize">The size of the multiplayer group.</param>
	public static void SetRichPresence(string token, string? playerGroup, int? groupSize)
	{
		GetPlatformUtil(PrimaryPlatform).SetRichPresence(token, playerGroup, groupSize);
	}

	/// <summary>
	/// Sets a key-value pair used in rich presence loc string substitution.
	/// This API is currently rather Steam-centric, so feel free to refactor later if necessary.
	/// </summary>
	public static void SetRichPresenceValue(string key, string? value)
	{
		GetPlatformUtil(PrimaryPlatform).SetRichPresenceValue(key, value);
	}

	/// <summary>
	/// Clears all rich presence data set for the player, including the primary display data and associated key-value pairs.
	/// </summary>
	public static void ClearRichPresence()
	{
		GetPlatformUtil(PrimaryPlatform).ClearRichPresence();
	}

	/// <summary>
	/// Returns the kind of fullscreening this platform allows for.
	/// See <see cref="T:MegaCrit.Sts2.Core.Platform.SupportedWindowMode" /> for the values.
	/// </summary>
	public static SupportedWindowMode GetSupportedWindowMode()
	{
		return GetPlatformUtil(PrimaryPlatform).GetSupportedWindowMode();
	}

	/// <summary>
	/// Returns whether or not the platform overlay (ie steam overlay) is currently open
	/// </summary>
	public static bool IsPlatformOverlayOpen()
	{
		return GetPlatformUtil(PrimaryPlatform).IsPlatformOverlayOpen;
	}
}
