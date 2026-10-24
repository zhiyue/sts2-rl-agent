using System;
using System.Threading.Tasks;
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

/// <summary>
/// Responsible for handling Steam-related requests to join multiplayer sessions coming from outside the game.
/// </summary>
public class SteamJoinCallbackHandler : IDisposable
{
	private readonly Callback<GameLobbyJoinRequested_t> _steamJoinCallback;

	public SteamJoinCallbackHandler()
	{
		_steamJoinCallback = new Callback<GameLobbyJoinRequested_t>(OnSteamLobbyJoinRequested);
	}

	/// <summary>
	/// If the game is not yet launched and the player accepts an invite, Steam launches the game with a specific command
	/// line argument. This method handles joining the multiplayer session. It should be called relatively early after
	/// the game is launched.
	/// </summary>
	public void CheckForCommandLineJoin()
	{
		if (CommandLineHelper.TryGetValue("+connect_lobby", out string value))
		{
			ulong num = ulong.Parse(value);
			Log.Info($"Joining to host via Steam invite that caused the game to launch. Lobby id: {num}");
			TaskHelper.RunSafely(JoinToHost(num, null));
		}
	}

	public void Dispose()
	{
		_steamJoinCallback.Dispose();
	}

	private void OnSteamLobbyJoinRequested(GameLobbyJoinRequested_t lobbyJoinRequest)
	{
		Log.Info($"Joining to host via Steam invite. Lobby: {lobbyJoinRequest.m_steamIDLobby.m_SteamID} player: {lobbyJoinRequest.m_steamIDFriend.m_SteamID}");
		TaskHelper.RunSafely(JoinToHost(lobbyJoinRequest.m_steamIDLobby.m_SteamID, lobbyJoinRequest.m_steamIDFriend.m_SteamID));
	}

	private static async Task JoinToHost(ulong lobbyId, ulong? playerId, IClientConnectionInitializer? connInitializer = null)
	{
		if (NGame.Instance.RootSceneContainer.CurrentScene is NMultiplayerTest nMultiplayerTest)
		{
			if (connInitializer == null)
			{
				connInitializer = SteamClientConnectionInitializer.FromLobby(lobbyId);
			}
			await nMultiplayerTest.JoinToHost(connInitializer);
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
		if (connInitializer == null)
		{
			connInitializer = SteamClientConnectionInitializer.FromLobby(lobbyId);
		}
		await NGame.Instance.MainMenu.JoinGame(connInitializer);
	}
}
