using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Multiplayer.Game;

namespace MegaCrit.Sts2.Core.Platform;

internal interface IPlatformUtilStrategy
{
	bool SupportsInviteDialog { get; }

	string GetPlayerName(ulong playerId);

	ulong GetLocalPlayerId();

	Task<IEnumerable<ulong>> GetFriendsWithOpenLobbies();

	void OpenInviteDialog(INetGameService gameService);

	void OpenUrl(string url);

	void OpenVirtualKeyboard();

	void CloseVirtualKeyboard();

	string? GetPlatformBranch();

	string? GetThreeLetterLanguageCode();

	string GetRawLanguage();

	void SetRichPresence(string token, string? playerGroup, int? groupSize);

	void SetRichPresenceValue(string key, string? value);

	void ClearRichPresence();

	SupportedWindowMode GetSupportedWindowMode();
}
