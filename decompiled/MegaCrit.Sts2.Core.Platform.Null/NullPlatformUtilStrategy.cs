using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Platform.Null;

public class NullPlatformUtilStrategy : IPlatformUtilStrategy
{
	private const string _multiplayerNamesFile = "mp_names.json";

	private List<NullMultiplayerName>? _mpNames;

	public ulong LocalPlayerId { get; } = 1uL;

	public bool SupportsInviteDialog => false;

	public NullPlatformUtilStrategy()
	{
		if (CommandLineHelper.TryGetValue("clientId", out string value) && ulong.TryParse(value, out var result))
		{
			LocalPlayerId = result;
		}
		GodotFileIo godotFileIo = new GodotFileIo(".");
		if (godotFileIo.FileExists("mp_names.json"))
		{
			string json = godotFileIo.ReadFile("mp_names.json");
			_mpNames = JsonSerializer.Deserialize(json, JsonSerializationUtility.GetTypeInfo<List<NullMultiplayerName>>());
		}
	}

	public string GetPlayerName(ulong playerId)
	{
		if (_mpNames != null)
		{
			foreach (NullMultiplayerName mpName in _mpNames)
			{
				if (mpName.netId == playerId)
				{
					return mpName.name;
				}
			}
		}
		return playerId switch
		{
			1uL => "Test Host", 
			1000uL => "Test Client 1", 
			2000uL => "Test Client 2", 
			3000uL => "Test Client 3", 
			_ => playerId.ToString(), 
		};
	}

	public ulong GetLocalPlayerId()
	{
		return LocalPlayerId;
	}

	public Task<IEnumerable<ulong>> GetFriendsWithOpenLobbies()
	{
		return Task.FromResult((IEnumerable<ulong>)Array.Empty<ulong>());
	}

	public void OpenInviteDialog(INetGameService netService)
	{
		throw new NotImplementedException();
	}

	public void OpenUrl(string url)
	{
		OS.ShellOpen(url);
	}

	public void OpenVirtualKeyboard()
	{
	}

	public void CloseVirtualKeyboard()
	{
	}

	public void SetRichPresence(string token, string? playerGroup, int? groupSize)
	{
	}

	public void SetRichPresenceValue(string key, string? value)
	{
	}

	public void ClearRichPresence()
	{
	}

	public string? GetPlatformBranch()
	{
		return null;
	}

	public string? GetThreeLetterLanguageCode()
	{
		CultureInfo cultureInfo = new CultureInfo(GetRawLanguage());
		if (LocManager.Languages.Contains(cultureInfo.ThreeLetterISOLanguageName))
		{
			return cultureInfo.ThreeLetterISOLanguageName;
		}
		string text = cultureInfo.Name.ToLowerInvariant();
		text = text.Replace('-', '_');
		if (text.StartsWith("zh"))
		{
			if (text.StartsWith("zh_hans") || text.StartsWith("zh_cn"))
			{
				return "zhs";
			}
			if (text.StartsWith("zh_hant") || text.StartsWith("zh_tw"))
			{
				return "zht";
			}
			return "zhs";
		}
		if (text.StartsWith("pt"))
		{
			if (text.StartsWith("pt_br"))
			{
				return "ptb";
			}
			return "por";
		}
		Log.Error($"CultureInfo {cultureInfo} could not be mapped to a three-letter language code!");
		return null;
	}

	public string GetRawLanguage()
	{
		return OS.GetLocale().Replace('_', '-');
	}

	public SupportedWindowMode GetSupportedWindowMode()
	{
		return SupportedWindowMode.Any;
	}
}
