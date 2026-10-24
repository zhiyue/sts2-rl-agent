using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Daily;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Modifiers;
using MegaCrit.Sts2.Core.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Lobby;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.Unlocks;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.DailyRun;

[ScriptPath("res://src/Core/Nodes/Screens/DailyRun/NDailyRunScreen.cs")]
public class NDailyRunScreen : NSubmenu, IStartRunLobbyListener
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : NSubmenu.MethodName
	{
		/// <summary>
		/// Cached name for the 'Create' method.
		/// </summary>
		public static readonly StringName Create = "Create";

		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'InitializeSingleplayer' method.
		/// </summary>
		public static readonly StringName InitializeSingleplayer = "InitializeSingleplayer";

		/// <summary>
		/// Cached name for the 'OnSubmenuOpened' method.
		/// </summary>
		public new static readonly StringName OnSubmenuOpened = "OnSubmenuOpened";

		/// <summary>
		/// Cached name for the 'OnSubmenuClosed' method.
		/// </summary>
		public new static readonly StringName OnSubmenuClosed = "OnSubmenuClosed";

		/// <summary>
		/// Cached name for the 'InitializeLeaderboard' method.
		/// </summary>
		public static readonly StringName InitializeLeaderboard = "InitializeLeaderboard";

		/// <summary>
		/// Cached name for the 'InitializeDisplay' method.
		/// </summary>
		public static readonly StringName InitializeDisplay = "InitializeDisplay";

		/// <summary>
		/// Cached name for the 'SetIsLoading' method.
		/// </summary>
		public static readonly StringName SetIsLoading = "SetIsLoading";

		/// <summary>
		/// Cached name for the '_Process' method.
		/// </summary>
		public new static readonly StringName _Process = "_Process";

		/// <summary>
		/// Cached name for the 'MaxAscensionChanged' method.
		/// </summary>
		public static readonly StringName MaxAscensionChanged = "MaxAscensionChanged";

		/// <summary>
		/// Cached name for the 'AscensionChanged' method.
		/// </summary>
		public static readonly StringName AscensionChanged = "AscensionChanged";

		/// <summary>
		/// Cached name for the 'SeedChanged' method.
		/// </summary>
		public static readonly StringName SeedChanged = "SeedChanged";

		/// <summary>
		/// Cached name for the 'ModifiersChanged' method.
		/// </summary>
		public static readonly StringName ModifiersChanged = "ModifiersChanged";

		/// <summary>
		/// Cached name for the 'OnEmbarkPressed' method.
		/// </summary>
		public static readonly StringName OnEmbarkPressed = "OnEmbarkPressed";

		/// <summary>
		/// Cached name for the 'OnUnreadyPressed' method.
		/// </summary>
		public static readonly StringName OnUnreadyPressed = "OnUnreadyPressed";

		/// <summary>
		/// Cached name for the 'UpdateRichPresence' method.
		/// </summary>
		public static readonly StringName UpdateRichPresence = "UpdateRichPresence";

		/// <summary>
		/// Cached name for the 'CleanUpLobby' method.
		/// </summary>
		public static readonly StringName CleanUpLobby = "CleanUpLobby";

		/// <summary>
		/// Cached name for the 'AfterLobbyInitialized' method.
		/// </summary>
		public static readonly StringName AfterLobbyInitialized = "AfterLobbyInitialized";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : NSubmenu.PropertyName
	{
		/// <summary>
		/// Cached name for the 'InitialFocusedControl' property.
		/// </summary>
		public new static readonly StringName InitialFocusedControl = "InitialFocusedControl";

		/// <summary>
		/// Cached name for the '_titleLabel' field.
		/// </summary>
		public static readonly StringName _titleLabel = "_titleLabel";

		/// <summary>
		/// Cached name for the '_disclaimer' field.
		/// </summary>
		public static readonly StringName _disclaimer = "_disclaimer";

		/// <summary>
		/// Cached name for the '_dateLabel' field.
		/// </summary>
		public static readonly StringName _dateLabel = "_dateLabel";

		/// <summary>
		/// Cached name for the '_timeLeftLabel' field.
		/// </summary>
		public static readonly StringName _timeLeftLabel = "_timeLeftLabel";

		/// <summary>
		/// Cached name for the '_characterContainer' field.
		/// </summary>
		public static readonly StringName _characterContainer = "_characterContainer";

		/// <summary>
		/// Cached name for the '_embarkButton' field.
		/// </summary>
		public static readonly StringName _embarkButton = "_embarkButton";

		/// <summary>
		/// Cached name for the '_backButton' field.
		/// </summary>
		public new static readonly StringName _backButton = "_backButton";

		/// <summary>
		/// Cached name for the '_unreadyButton' field.
		/// </summary>
		public static readonly StringName _unreadyButton = "_unreadyButton";

		/// <summary>
		/// Cached name for the '_leaderboard' field.
		/// </summary>
		public static readonly StringName _leaderboard = "_leaderboard";

		/// <summary>
		/// Cached name for the '_modifiersTitleLabel' field.
		/// </summary>
		public static readonly StringName _modifiersTitleLabel = "_modifiersTitleLabel";

		/// <summary>
		/// Cached name for the '_modifiersContainer' field.
		/// </summary>
		public static readonly StringName _modifiersContainer = "_modifiersContainer";

		/// <summary>
		/// Cached name for the '_remotePlayerContainer' field.
		/// </summary>
		public static readonly StringName _remotePlayerContainer = "_remotePlayerContainer";

		/// <summary>
		/// Cached name for the '_readyAndWaitingContainer' field.
		/// </summary>
		public static readonly StringName _readyAndWaitingContainer = "_readyAndWaitingContainer";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NSubmenu.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("screens/daily_run/daily_run_screen");

	private static readonly LocString _timeLeftLoc = new LocString("main_menu_ui", "DAILY_RUN_MENU.TIME_LEFT");

	public static readonly string dateFormat = LocManager.Instance.GetTable("main_menu_ui").GetRawText("DAILY_RUN_MENU.DATE_FORMAT");

	private static readonly string _timeLeftFormat = LocManager.Instance.GetTable("main_menu_ui").GetRawText("DAILY_RUN_MENU.TIME_FORMAT");

	private MegaLabel _titleLabel;

	private MegaLabel _disclaimer;

	private MegaRichTextLabel _dateLabel;

	private MegaRichTextLabel _timeLeftLabel;

	private NDailyRunCharacterContainer _characterContainer;

	private NConfirmButton _embarkButton;

	private NBackButton _backButton;

	private NBackButton _unreadyButton;

	private NDailyRunLeaderboard _leaderboard;

	private MegaLabel _modifiersTitleLabel;

	private Control _modifiersContainer;

	private readonly List<NDailyRunScreenModifier> _modifierContainers = new List<NDailyRunScreenModifier>();

	private NRemoteLobbyPlayerContainer _remotePlayerContainer;

	private Control _readyAndWaitingContainer;

	private DateTimeOffset _endOfDay;

	private INetGameService _netService;

	private StartRunLobby? _lobby;

	private int? _lastSetTimeLeftSecond;

	public static string[] AssetPaths => new string[1] { _scenePath };

	protected override Control? InitialFocusedControl => null;

	public static NDailyRunScreen? Create()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		return PreloadManager.Cache.GetScene(_scenePath).Instantiate<NDailyRunScreen>(PackedScene.GenEditState.Disabled);
	}

	public override void _Ready()
	{
		ConnectSignals();
		_titleLabel = GetNode<MegaLabel>("%Title");
		_disclaimer = GetNode<MegaLabel>("%Disclaimer");
		_dateLabel = GetNode<MegaRichTextLabel>("%Date");
		_embarkButton = GetNode<NConfirmButton>("%ConfirmButton");
		_backButton = GetNode<NBackButton>("%BackButton");
		_unreadyButton = GetNode<NBackButton>("%UnreadyButton");
		_timeLeftLabel = GetNode<MegaRichTextLabel>("%TimeLeft");
		_leaderboard = GetNode<NDailyRunLeaderboard>("%Leaderboards");
		_modifiersTitleLabel = GetNode<MegaLabel>("%ModifiersLabel");
		_modifiersContainer = GetNode<Control>("%ModifiersContainer");
		_characterContainer = GetNode<NDailyRunCharacterContainer>("ChallengeContainer/CenterContainer/HBoxContainer/CharacterContainer");
		_remotePlayerContainer = GetNode<NRemoteLobbyPlayerContainer>("%RemotePlayerContainer");
		_readyAndWaitingContainer = GetNode<Control>("%ReadyAndWaitingPanel");
		_titleLabel.SetTextAutoSize(new LocString("main_menu_ui", "DAILY_RUN_MENU.DAILY_TITLE").GetFormattedText());
		_disclaimer.SetTextAutoSize(new LocString("main_menu_ui", "DAILY_RUN_MENU.disclaimer").GetFormattedText());
		_modifiersTitleLabel.SetTextAutoSize(new LocString("main_menu_ui", "DAILY_RUN_MENU.MODIFIERS").GetFormattedText());
		_dateLabel.SetTextAutoSize(new LocString("main_menu_ui", "DAILY_RUN_MENU.FETCHING_TIME").GetFormattedText());
		foreach (NDailyRunScreenModifier item in _modifiersContainer.GetChildren().OfType<NDailyRunScreenModifier>())
		{
			_modifierContainers.Add(item);
		}
		_embarkButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnEmbarkPressed));
		_embarkButton.Disable();
		_unreadyButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnUnreadyPressed));
		_unreadyButton.Disable();
		_remotePlayerContainer.Visible = false;
		_readyAndWaitingContainer.Visible = false;
		_leaderboard.Cleanup();
	}

	public void InitializeMultiplayerAsHost(INetGameService gameService)
	{
		if (gameService.Type != NetGameType.Host)
		{
			throw new InvalidOperationException($"Initialized character select screen with GameService of type {gameService.Type} when hosting!");
		}
		_netService = gameService;
	}

	public void InitializeMultiplayerAsClient(INetGameService gameService, ClientLobbyJoinResponseMessage message)
	{
		if (gameService.Type != NetGameType.Client)
		{
			throw new InvalidOperationException($"Initialized character select screen with GameService of type {gameService.Type} when joining!");
		}
		_netService = gameService;
		_lobby = new StartRunLobby(GameMode.Daily, gameService, this, message.dailyTime.Value, -1);
		_lobby.InitializeFromMessage(message);
		SetupLobbyParams(_lobby);
		AfterLobbyInitialized();
	}

	public void InitializeSingleplayer()
	{
		_netService = new NetSingleplayerGameService();
	}

	public override void OnSubmenuOpened()
	{
		base.OnSubmenuOpened();
		NetGameType type = _netService.Type;
		if ((uint)(type - 1) <= 1u)
		{
			TaskHelper.RunSafely(SetupLobbyForHostOrSingleplayer());
		}
		else
		{
			SetIsLoading(isLoading: false);
		}
	}

	public override void OnSubmenuClosed()
	{
		_embarkButton.Disable();
		_remotePlayerContainer.Cleanup();
		_leaderboard.Cleanup();
		StartRunLobby? lobby = _lobby;
		if (lobby != null && lobby.NetService.Type.IsMultiplayer())
		{
			PlatformUtil.SetRichPresence("MAIN_MENU", null, null);
		}
		CleanUpLobby(disconnectSession: true);
	}

	/// <summary>
	/// This is called:
	/// - Once when the screen is shown
	/// - Anytime when the player count changes
	/// </summary>
	private void InitializeLeaderboard()
	{
		_leaderboard.Initialize(_lobby.DailyTime.Value.serverTime, _lobby.Players.Select((LobbyPlayer p) => p.id), allowPagination: false);
	}

	/// <summary>
	/// Fetches the time from the time server and initializes the lobby for multiplayer or singleplayer.
	/// After this completes, _lobby will be initialized. Its time will be set with the time fetched from the time server,
	/// or with the local time, if we couldn't fetch the time from the server.
	/// </summary>
	private async Task SetupLobbyForHostOrSingleplayer()
	{
		if (_netService.Type != NetGameType.Host && _netService.Type != NetGameType.Singleplayer)
		{
			throw new InvalidOperationException("Should only be called as host or singleplayer!");
		}
		SetIsLoading(isLoading: true);
		TimeServerResult timeServerResult = await GetTimeServerTime();
		if (GodotObject.IsInstanceValid(this))
		{
			_lobby = new StartRunLobby(GameMode.Daily, _netService, this, timeServerResult, 4);
			_lobby.AddLocalHostPlayer(new UnlockState(SaveManager.Instance.Progress), SaveManager.Instance.Progress.MaxMultiplayerAscension);
			SetupLobbyParams(_lobby);
			AfterLobbyInitialized();
			SetIsLoading(isLoading: false);
			Log.Info($"Daily initialized with seed: {_lobby.Seed} time: {GetServerRelativeTime()}");
		}
	}

	/// <summary>
	/// Attempt to get the time from the time server.
	/// If any time server request succeeded in the past, this uses the cached time server time. Otherwise, it requests
	/// a new time. Falls back to local time if the server is unreachable.
	/// </summary>
	private async Task<TimeServerResult> GetTimeServerTime()
	{
		TimeServerResult? result = null;
		if (TimeServer.RequestTimeTask?.IsCompleted ?? false)
		{
			if (!TimeServer.RequestTimeTask.IsFaulted)
			{
				result = await TimeServer.RequestTimeTask;
			}
			if (!result.HasValue)
			{
				try
				{
					result = await TimeServer.FetchDailyTime();
				}
				catch (Exception ex) when (((ex is HttpRequestException || ex is TaskCanceledException) ? 1 : 0) != 0)
				{
					Log.Error(ex.ToString());
				}
			}
		}
		else
		{
			try
			{
				result = await TimeServer.FetchDailyTime();
			}
			catch (Exception ex2) when (((ex2 is HttpRequestException || ex2 is TaskCanceledException) ? 1 : 0) != 0)
			{
				Log.Error(ex2.ToString());
			}
		}
		if (!result.HasValue)
		{
			Log.Info("Couldn't retrieve time from time server, using local time");
			result = new TimeServerResult
			{
				serverTime = DateTimeOffset.UtcNow,
				localReceivedTime = DateTimeOffset.UtcNow
			};
		}
		return result.Value;
	}

	/// <summary>
	/// Returns the time on the Mega Crit time server.
	/// </summary>
	private DateTimeOffset GetServerRelativeTime()
	{
		return _lobby.DailyTime.Value.serverTime + (DateTimeOffset.UtcNow - _lobby.DailyTime.Value.localReceivedTime);
	}

	/// <summary>
	/// This is called on both host and client after the lobby is setup to sync the state of the lobby.
	/// It is also called when any player leaves or rejoins the game to reroll modifiers so that CharacterCards can
	/// properly avoid hitting any characters that have been rolled.
	/// </summary>
	private void SetupLobbyParams(StartRunLobby lobby)
	{
		DateTimeOffset serverRelativeTime = GetServerRelativeTime();
		string str = SeedHelper.CanonicalizeSeed(serverRelativeTime.ToString("dd_MM_yyyy"));
		string text = SeedHelper.CanonicalizeSeed(serverRelativeTime.ToString($"dd_MM_yyyy_{lobby.Players.Count}p"));
		Rng rng = new Rng((uint)StringHelper.GetDeterministicHashCode(str));
		Rng rng2 = new Rng(rng.NextUnsignedInt());
		Rng rng3 = new Rng(rng.NextUnsignedInt());
		Rng rng4 = new Rng(rng.NextUnsignedInt());
		CharacterModel characterModel = null;
		foreach (LobbyPlayer player in lobby.Players)
		{
			CharacterModel characterModel2 = rng2.NextItem(ModelDb.AllCharacters);
			if (player.id == lobby.LocalPlayer.id)
			{
				characterModel = characterModel2;
			}
		}
		int num = rng3.NextInt(0, 11);
		List<ModifierModel> list = RollModifiers(rng4);
		NetGameType type = lobby.NetService.Type;
		if ((uint)(type - 1) <= 1u)
		{
			if (lobby.Seed != text)
			{
				lobby.SetSeed(text);
			}
			if (lobby.Ascension != num)
			{
				lobby.SyncAscensionChange(num);
			}
			if (list.Any((ModifierModel m) => lobby.Modifiers.FirstOrDefault(m.IsEquivalent) == null))
			{
				lobby.SetModifiers(list);
			}
		}
		if (lobby.LocalPlayer.character != characterModel)
		{
			lobby.SetLocalCharacter(characterModel);
		}
		InitializeDisplay();
	}

	private void InitializeDisplay()
	{
		if (_lobby == null)
		{
			throw new InvalidOperationException("Tried to initialize daily run display before lobby was initialized!");
		}
		DateTimeOffset serverRelativeTime = GetServerRelativeTime();
		_endOfDay = new DateTimeOffset(serverRelativeTime.Year, serverRelativeTime.Month, serverRelativeTime.Day, 0, 0, 0, TimeSpan.Zero) + TimeSpan.FromDays(1);
		_remotePlayerContainer.Visible = _lobby.NetService.Type.IsMultiplayer();
		CharacterModel character = _lobby.LocalPlayer.character;
		_characterContainer.Fill(character, _lobby.LocalPlayer.id, _lobby.Ascension, _lobby.NetService);
		_dateLabel.Modulate = StsColors.blue;
		_dateLabel.Text = serverRelativeTime.ToString(dateFormat);
		for (int i = 0; i < _lobby.Modifiers.Count; i++)
		{
			_modifierContainers[i].Fill(_lobby.Modifiers[i]);
		}
	}

	private List<ModifierModel> RollModifiers(Rng rng)
	{
		List<ModifierModel> list = new List<ModifierModel>();
		List<ModifierModel> list2 = ModelDb.GoodModifiers.ToList().StableShuffle(rng);
		for (int i = 0; i < 2; i++)
		{
			ModifierModel canonicalModifier = rng.NextItem(list2);
			if (canonicalModifier == null)
			{
				throw new InvalidOperationException("There were not enough good modifiers to fill the daily!");
			}
			ModifierModel modifierModel = canonicalModifier.ToMutable();
			if (modifierModel is CharacterCards characterCards)
			{
				IEnumerable<CharacterModel> second = _lobby.Players.Select((LobbyPlayer p) => p.character);
				characterCards.CharacterModel = rng.NextItem(ModelDb.AllCharacters.Except(second)).Id;
			}
			list.Add(modifierModel);
			list2.Remove(canonicalModifier);
			IReadOnlySet<ModifierModel> readOnlySet = ModelDb.MutuallyExclusiveModifiers.FirstOrDefault((IReadOnlySet<ModifierModel> s) => s.Contains(canonicalModifier));
			if (readOnlySet == null)
			{
				continue;
			}
			foreach (ModifierModel item in readOnlySet)
			{
				list2.Remove(item);
			}
		}
		list.Add(rng.NextItem(ModelDb.BadModifiers).ToMutable());
		return list;
	}

	private void SetIsLoading(bool isLoading)
	{
		if (isLoading)
		{
			_remotePlayerContainer.Visible = false;
			_readyAndWaitingContainer.Visible = false;
		}
		_timeLeftLabel.Visible = !isLoading;
		_characterContainer.Visible = !isLoading;
		_modifiersTitleLabel.Visible = !isLoading;
		_modifiersContainer.Visible = !isLoading;
		if (isLoading)
		{
			_embarkButton.Disable();
		}
		else
		{
			_embarkButton.Enable();
		}
	}

	public override void _Process(double delta)
	{
		if (_lobby != null)
		{
			DateTimeOffset serverRelativeTime = GetServerRelativeTime();
			if (serverRelativeTime > _endOfDay)
			{
				SetupLobbyParams(_lobby);
			}
			TimeSpan timeSpan = _endOfDay - serverRelativeTime;
			if (_lastSetTimeLeftSecond != timeSpan.Seconds)
			{
				string variable = timeSpan.ToString(_timeLeftFormat);
				_timeLeftLoc.Add("time", variable);
				_timeLeftLabel.Text = _timeLeftLoc.GetFormattedText();
				_lastSetTimeLeftSecond = timeSpan.Seconds;
			}
			if (_lobby.NetService.IsConnected)
			{
				_lobby.NetService.Update();
			}
		}
	}

	public void PlayerConnected(LobbyPlayer player)
	{
		_remotePlayerContainer.OnPlayerConnected(player);
		SetupLobbyParams(_lobby);
		InitializeLeaderboard();
		UpdateRichPresence();
	}

	public void PlayerChanged(LobbyPlayer player, bool isRandomCharacterResolution)
	{
		if (isRandomCharacterResolution)
		{
			throw new InvalidOperationException("Random character is not currently allowed in daily!");
		}
		_remotePlayerContainer.OnPlayerChanged(player);
		if (player.id == _netService.NetId && _netService.Type.IsMultiplayer())
		{
			_characterContainer.SetIsReady(player.isReady);
		}
	}

	public void MaxAscensionChanged()
	{
	}

	public void AscensionChanged()
	{
		InitializeDisplay();
	}

	public void SeedChanged()
	{
	}

	public void ModifiersChanged()
	{
		InitializeDisplay();
	}

	public void RemotePlayerDisconnected(LobbyPlayer player)
	{
		_remotePlayerContainer.OnPlayerDisconnected(player);
		SetupLobbyParams(_lobby);
		InitializeLeaderboard();
		UpdateRichPresence();
	}

	public void BeginRun(string seed, List<ActModel> acts, IReadOnlyList<ModifierModel> modifiers)
	{
		NAudioManager.Instance?.StopMusic();
		_embarkButton.Disable();
		_unreadyButton.Disable();
		if (_lobby.NetService.Type == NetGameType.Singleplayer)
		{
			TaskHelper.RunSafely(StartNewSingleplayerRun(seed, acts, modifiers));
		}
		else
		{
			TaskHelper.RunSafely(StartNewMultiplayerRun(seed, acts, modifiers));
		}
	}

	public void LocalPlayerDisconnected(NetErrorInfo info)
	{
		if ((info.SelfInitiated && info.GetReason() == NetError.Quit) || !this.IsValid() || _stack == null)
		{
			return;
		}
		if (_stack.Peek() == this)
		{
			_stack.Pop();
		}
		if (TestMode.IsOff)
		{
			NErrorPopup nErrorPopup = NErrorPopup.Create(info);
			if (nErrorPopup != null)
			{
				NModalContainer.Instance.Add(nErrorPopup);
			}
		}
	}

	private void OnEmbarkPressed(NButton _)
	{
		_embarkButton.Disable();
		_backButton.Disable();
		_lobby.SetReady(ready: true);
		if (_lobby.NetService.Type != NetGameType.Singleplayer && !_lobby.IsAboutToBeginGame())
		{
			_readyAndWaitingContainer.Visible = true;
			_unreadyButton.Enable();
		}
	}

	private void OnUnreadyPressed(NButton _)
	{
		_lobby.SetReady(ready: false);
		_readyAndWaitingContainer.Visible = false;
		_embarkButton.Enable();
		_backButton.Enable();
		_unreadyButton.Disable();
	}

	private void UpdateRichPresence()
	{
		StartRunLobby? lobby = _lobby;
		if (lobby != null && lobby.NetService.Type.IsMultiplayer())
		{
			PlatformUtil.SetRichPresence("DAILY_MP_LOBBY", _lobby.NetService.GetRawLobbyIdentifier(), _lobby.Players.Count);
		}
	}

	public async Task StartNewSingleplayerRun(string seed, List<ActModel> acts, IReadOnlyList<ModifierModel> modifiers)
	{
		try
		{
			Log.Info($"Embarking on a DAILY {_lobby.LocalPlayer.character.Id.Entry} run with {_lobby.Players.Count} players. Ascension: {_lobby.Ascension} Seed: {seed}");
			SfxCmd.Play(_lobby.LocalPlayer.character.CharacterTransitionSfx);
			await NGame.Instance.Transition.FadeOut(0.8f, _lobby.LocalPlayer.character.CharacterSelectTransitionPath);
			await NGame.Instance.StartNewSingleplayerRun(_lobby.LocalPlayer.character, shouldSave: true, acts, modifiers, seed, GameMode.Daily, _lobby.Ascension, _lobby.DailyTime.Value.serverTime);
		}
		catch (Exception ex)
		{
			Log.Error($"Exception starting daily singleplayer run : {ex}");
			CleanUpLobby(disconnectSession: true, NetError.InternalError);
			await NGame.Instance.ReturnToMainMenuWithInternalError(ex);
			return;
		}
		CleanUpLobby(disconnectSession: false);
	}

	public async Task StartNewMultiplayerRun(string seed, List<ActModel> acts, IReadOnlyList<ModifierModel> modifiers)
	{
		try
		{
			Log.Info($"Embarking on a DAILY multiplayer run. Players: {string.Join(",", _lobby.Players)}. Ascension: {_lobby.Ascension} Seed: {seed}");
			SfxCmd.Play(_lobby.LocalPlayer.character.CharacterTransitionSfx);
			await NGame.Instance.Transition.FadeOut(0.8f, _lobby.LocalPlayer.character.CharacterSelectTransitionPath);
			await NGame.Instance.StartNewMultiplayerRun(_lobby, shouldSave: true, acts, modifiers, seed, _lobby.Ascension, _lobby.DailyTime.Value.serverTime);
		}
		catch (Exception ex)
		{
			Log.Error($"Exception starting daily multiplayer run : {ex}");
			CleanUpLobby(disconnectSession: true, NetError.InternalError);
			await NGame.Instance.ReturnToMainMenuWithInternalError(ex);
			return;
		}
		CleanUpLobby(disconnectSession: false);
	}

	private void CleanUpLobby(bool disconnectSession, NetError error = NetError.Quit)
	{
		_lobby?.CleanUp(disconnectSession, error);
		_lobby = null;
	}

	private void AfterLobbyInitialized()
	{
		NGame.Instance.RemoteCursorContainer.Initialize(_lobby.InputSynchronizer, _lobby.Players.Select((LobbyPlayer p) => p.id));
		NGame.Instance.ReactionContainer.InitializeNetworking(_lobby.NetService);
		NGame.Instance.TimeoutOverlay.Initialize(_lobby.NetService, isGameLevel: true);
		_remotePlayerContainer.Initialize(_lobby, displayLocalPlayer: false);
		UpdateRichPresence();
		MegaCrit.Sts2.Core.Logging.Logger.logLevelTypeMap[LogType.Network] = ((_lobby.NetService.Type == NetGameType.Singleplayer) ? LogLevel.Info : LogLevel.Debug);
		MegaCrit.Sts2.Core.Logging.Logger.logLevelTypeMap[LogType.Actions] = ((_lobby.NetService.Type == NetGameType.Singleplayer) ? LogLevel.Info : LogLevel.VeryDebug);
		MegaCrit.Sts2.Core.Logging.Logger.logLevelTypeMap[LogType.GameSync] = ((_lobby.NetService.Type == NetGameType.Singleplayer) ? LogLevel.Info : LogLevel.VeryDebug);
		NGame.Instance.DebugSeedOverride = null;
		_embarkButton.Enable();
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(18);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.InitializeSingleplayer, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuOpened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuClosed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.InitializeLeaderboard, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.InitializeDisplay, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetIsLoading, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "isLoading", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.MaxAscensionChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AscensionChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SeedChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ModifiersChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnEmbarkPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnUnreadyPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateRichPresence, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CleanUpLobby, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "disconnectSession", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "error", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AfterLobbyInitialized, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NDailyRunScreen>(Create());
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.InitializeSingleplayer && args.Count == 0)
		{
			InitializeSingleplayer();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSubmenuOpened && args.Count == 0)
		{
			OnSubmenuOpened();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSubmenuClosed && args.Count == 0)
		{
			OnSubmenuClosed();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.InitializeLeaderboard && args.Count == 0)
		{
			InitializeLeaderboard();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.InitializeDisplay && args.Count == 0)
		{
			InitializeDisplay();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetIsLoading && args.Count == 1)
		{
			SetIsLoading(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.MaxAscensionChanged && args.Count == 0)
		{
			MaxAscensionChanged();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AscensionChanged && args.Count == 0)
		{
			AscensionChanged();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SeedChanged && args.Count == 0)
		{
			SeedChanged();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ModifiersChanged && args.Count == 0)
		{
			ModifiersChanged();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnEmbarkPressed && args.Count == 1)
		{
			OnEmbarkPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnUnreadyPressed && args.Count == 1)
		{
			OnUnreadyPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateRichPresence && args.Count == 0)
		{
			UpdateRichPresence();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CleanUpLobby && args.Count == 2)
		{
			CleanUpLobby(VariantUtils.ConvertTo<bool>(in args[0]), VariantUtils.ConvertTo<NetError>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AfterLobbyInitialized && args.Count == 0)
		{
			AfterLobbyInitialized();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NDailyRunScreen>(Create());
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.Create)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.InitializeSingleplayer)
		{
			return true;
		}
		if (method == MethodName.OnSubmenuOpened)
		{
			return true;
		}
		if (method == MethodName.OnSubmenuClosed)
		{
			return true;
		}
		if (method == MethodName.InitializeLeaderboard)
		{
			return true;
		}
		if (method == MethodName.InitializeDisplay)
		{
			return true;
		}
		if (method == MethodName.SetIsLoading)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName.MaxAscensionChanged)
		{
			return true;
		}
		if (method == MethodName.AscensionChanged)
		{
			return true;
		}
		if (method == MethodName.SeedChanged)
		{
			return true;
		}
		if (method == MethodName.ModifiersChanged)
		{
			return true;
		}
		if (method == MethodName.OnEmbarkPressed)
		{
			return true;
		}
		if (method == MethodName.OnUnreadyPressed)
		{
			return true;
		}
		if (method == MethodName.UpdateRichPresence)
		{
			return true;
		}
		if (method == MethodName.CleanUpLobby)
		{
			return true;
		}
		if (method == MethodName.AfterLobbyInitialized)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._titleLabel)
		{
			_titleLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._disclaimer)
		{
			_disclaimer = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._dateLabel)
		{
			_dateLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._timeLeftLabel)
		{
			_timeLeftLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._characterContainer)
		{
			_characterContainer = VariantUtils.ConvertTo<NDailyRunCharacterContainer>(in value);
			return true;
		}
		if (name == PropertyName._embarkButton)
		{
			_embarkButton = VariantUtils.ConvertTo<NConfirmButton>(in value);
			return true;
		}
		if (name == PropertyName._backButton)
		{
			_backButton = VariantUtils.ConvertTo<NBackButton>(in value);
			return true;
		}
		if (name == PropertyName._unreadyButton)
		{
			_unreadyButton = VariantUtils.ConvertTo<NBackButton>(in value);
			return true;
		}
		if (name == PropertyName._leaderboard)
		{
			_leaderboard = VariantUtils.ConvertTo<NDailyRunLeaderboard>(in value);
			return true;
		}
		if (name == PropertyName._modifiersTitleLabel)
		{
			_modifiersTitleLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._modifiersContainer)
		{
			_modifiersContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._remotePlayerContainer)
		{
			_remotePlayerContainer = VariantUtils.ConvertTo<NRemoteLobbyPlayerContainer>(in value);
			return true;
		}
		if (name == PropertyName._readyAndWaitingContainer)
		{
			_readyAndWaitingContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.InitialFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(InitialFocusedControl);
			return true;
		}
		if (name == PropertyName._titleLabel)
		{
			value = VariantUtils.CreateFrom(in _titleLabel);
			return true;
		}
		if (name == PropertyName._disclaimer)
		{
			value = VariantUtils.CreateFrom(in _disclaimer);
			return true;
		}
		if (name == PropertyName._dateLabel)
		{
			value = VariantUtils.CreateFrom(in _dateLabel);
			return true;
		}
		if (name == PropertyName._timeLeftLabel)
		{
			value = VariantUtils.CreateFrom(in _timeLeftLabel);
			return true;
		}
		if (name == PropertyName._characterContainer)
		{
			value = VariantUtils.CreateFrom(in _characterContainer);
			return true;
		}
		if (name == PropertyName._embarkButton)
		{
			value = VariantUtils.CreateFrom(in _embarkButton);
			return true;
		}
		if (name == PropertyName._backButton)
		{
			value = VariantUtils.CreateFrom(in _backButton);
			return true;
		}
		if (name == PropertyName._unreadyButton)
		{
			value = VariantUtils.CreateFrom(in _unreadyButton);
			return true;
		}
		if (name == PropertyName._leaderboard)
		{
			value = VariantUtils.CreateFrom(in _leaderboard);
			return true;
		}
		if (name == PropertyName._modifiersTitleLabel)
		{
			value = VariantUtils.CreateFrom(in _modifiersTitleLabel);
			return true;
		}
		if (name == PropertyName._modifiersContainer)
		{
			value = VariantUtils.CreateFrom(in _modifiersContainer);
			return true;
		}
		if (name == PropertyName._remotePlayerContainer)
		{
			value = VariantUtils.CreateFrom(in _remotePlayerContainer);
			return true;
		}
		if (name == PropertyName._readyAndWaitingContainer)
		{
			value = VariantUtils.CreateFrom(in _readyAndWaitingContainer);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	/// <summary>
	/// Get the property information for all the properties declared in this class.
	/// This method is used by Godot to register the available properties in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.InitialFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._titleLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._disclaimer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._dateLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._timeLeftLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._characterContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._embarkButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._backButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._unreadyButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._leaderboard, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._modifiersTitleLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._modifiersContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._remotePlayerContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._readyAndWaitingContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._titleLabel, Variant.From(in _titleLabel));
		info.AddProperty(PropertyName._disclaimer, Variant.From(in _disclaimer));
		info.AddProperty(PropertyName._dateLabel, Variant.From(in _dateLabel));
		info.AddProperty(PropertyName._timeLeftLabel, Variant.From(in _timeLeftLabel));
		info.AddProperty(PropertyName._characterContainer, Variant.From(in _characterContainer));
		info.AddProperty(PropertyName._embarkButton, Variant.From(in _embarkButton));
		info.AddProperty(PropertyName._backButton, Variant.From(in _backButton));
		info.AddProperty(PropertyName._unreadyButton, Variant.From(in _unreadyButton));
		info.AddProperty(PropertyName._leaderboard, Variant.From(in _leaderboard));
		info.AddProperty(PropertyName._modifiersTitleLabel, Variant.From(in _modifiersTitleLabel));
		info.AddProperty(PropertyName._modifiersContainer, Variant.From(in _modifiersContainer));
		info.AddProperty(PropertyName._remotePlayerContainer, Variant.From(in _remotePlayerContainer));
		info.AddProperty(PropertyName._readyAndWaitingContainer, Variant.From(in _readyAndWaitingContainer));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._titleLabel, out var value))
		{
			_titleLabel = value.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._disclaimer, out var value2))
		{
			_disclaimer = value2.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._dateLabel, out var value3))
		{
			_dateLabel = value3.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._timeLeftLabel, out var value4))
		{
			_timeLeftLabel = value4.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._characterContainer, out var value5))
		{
			_characterContainer = value5.As<NDailyRunCharacterContainer>();
		}
		if (info.TryGetProperty(PropertyName._embarkButton, out var value6))
		{
			_embarkButton = value6.As<NConfirmButton>();
		}
		if (info.TryGetProperty(PropertyName._backButton, out var value7))
		{
			_backButton = value7.As<NBackButton>();
		}
		if (info.TryGetProperty(PropertyName._unreadyButton, out var value8))
		{
			_unreadyButton = value8.As<NBackButton>();
		}
		if (info.TryGetProperty(PropertyName._leaderboard, out var value9))
		{
			_leaderboard = value9.As<NDailyRunLeaderboard>();
		}
		if (info.TryGetProperty(PropertyName._modifiersTitleLabel, out var value10))
		{
			_modifiersTitleLabel = value10.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._modifiersContainer, out var value11))
		{
			_modifiersContainer = value11.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._remotePlayerContainer, out var value12))
		{
			_remotePlayerContainer = value12.As<NRemoteLobbyPlayerContainer>();
		}
		if (info.TryGetProperty(PropertyName._readyAndWaitingContainer, out var value13))
		{
			_readyAndWaitingContainer = value13.As<Control>();
		}
	}
}
