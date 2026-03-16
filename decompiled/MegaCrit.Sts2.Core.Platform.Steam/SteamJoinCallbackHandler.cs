using System;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Connection;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Debug.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Runs;
using Steamworks;

namespace MegaCrit.Sts2.Core.Platform.Steam;

public class SteamJoinCallbackHandler : IDisposable
{
	private readonly Callback<GameLobbyJoinRequested_t> _steamJoinCallback;

	public SteamJoinCallbackHandler()
	{
		_steamJoinCallback = new Callback<GameLobbyJoinRequested_t>(OnSteamLobbyJoinRequested);
	}

	public void CheckForCommandLineJoin()
	{
		Log.Info("Command line: " + string.Join(" ", OS.GetCmdlineArgs()));
		if (CommandLineHelper.TryGetValue("+connect_lobby", out string value))
		{
			ulong lobbyId = ulong.Parse(value);
			TaskHelper.RunSafely(JoinToHost(lobbyId, null));
		}
	}

	public void Dispose()
	{
		_steamJoinCallback.Dispose();
	}

	private void OnSteamLobbyJoinRequested(GameLobbyJoinRequested_t lobbyJoinRequest)
	{
		TaskHelper.RunSafely(JoinToHost(lobbyJoinRequest.m_steamIDLobby.m_SteamID, lobbyJoinRequest.m_steamIDFriend.m_SteamID));
	}

	private async Task JoinToHost(ulong lobbyId, ulong? playerId)
	{
		if (NGame.Instance.RootSceneContainer.CurrentScene is NMultiplayerTest nMultiplayerTest)
		{
			SteamClientConnectionInitializer initializer = SteamClientConnectionInitializer.FromLobby(lobbyId);
			await nMultiplayerTest.JoinToHost(initializer);
			return;
		}
		if (RunManager.Instance.IsInProgress)
		{
			LocString locString = new LocString("gameplay_ui", "QUIT_AND_JOIN_CONFIRMATION.body");
			playerId.GetValueOrDefault();
			if (!playerId.HasValue)
			{
				ulong steamID = SteamMatchmaking.GetLobbyOwner(new CSteamID(lobbyId)).m_SteamID;
				playerId = steamID;
			}
			locString.Add("host", PlatformUtil.GetPlayerName(PlatformType.Steam, playerId.Value));
			NGenericPopup nGenericPopup = NGenericPopup.Create();
			NModalContainer.Instance.Add(nGenericPopup);
			if (!(await nGenericPopup.WaitForConfirmation(locString, new LocString("gameplay_ui", "QUIT_AND_JOIN_CONFIRMATION.header"), new LocString("gameplay_ui", "QUIT_AND_JOIN_CONFIRMATION.cancel"), new LocString("gameplay_ui", "QUIT_AND_JOIN_CONFIRMATION.confirm"))))
			{
				return;
			}
		}
		if (NGame.Instance.MainMenu == null)
		{
			await NGame.Instance.ReturnToMainMenu();
		}
		while (NGame.Instance.MainMenu?.SubmenuStack.Peek() != null)
		{
			NGame.Instance.MainMenu?.SubmenuStack.Pop();
		}
		await NGame.Instance.MainMenu.JoinGame(SteamClientConnectionInitializer.FromLobby(lobbyId));
	}
}
