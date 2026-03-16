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
	public new class MethodName : NSubmenu.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName InitializeSingleplayer = "InitializeSingleplayer";

		public new static readonly StringName OnSubmenuOpened = "OnSubmenuOpened";

		public new static readonly StringName OnSubmenuClosed = "OnSubmenuClosed";

		public static readonly StringName InitializeLeaderboard = "InitializeLeaderboard";

		public static readonly StringName InitializeDisplay = "InitializeDisplay";

		public static readonly StringName SetIsLoading = "SetIsLoading";

		public new static readonly StringName _Process = "_Process";

		public static readonly StringName MaxAscensionChanged = "MaxAscensionChanged";

		public static readonly StringName AscensionChanged = "AscensionChanged";

		public static readonly StringName SeedChanged = "SeedChanged";

		public static readonly StringName ModifiersChanged = "ModifiersChanged";

		public static readonly StringName OnEmbarkPressed = "OnEmbarkPressed";

		public static readonly StringName OnUnreadyPressed = "OnUnreadyPressed";

		public static readonly StringName UpdateRichPresence = "UpdateRichPresence";

		public static readonly StringName CleanUpLobby = "CleanUpLobby";

		public static readonly StringName AfterLobbyInitialized = "AfterLobbyInitialized";
	}

	public new class PropertyName : NSubmenu.PropertyName
	{
		public new static readonly StringName InitialFocusedControl = "InitialFocusedControl";

		public static readonly StringName _titleLabel = "_titleLabel";

		public static readonly StringName _dateLabel = "_dateLabel";

		public static readonly StringName _timeLeftLabel = "_timeLeftLabel";

		public static readonly StringName _characterContainer = "_characterContainer";

		public static readonly StringName _embarkButton = "_embarkButton";

		public new static readonly StringName _backButton = "_backButton";

		public static readonly StringName _unreadyButton = "_unreadyButton";

		public static readonly StringName _leaderboard = "_leaderboard";

		public static readonly StringName _modifiersTitleLabel = "_modifiersTitleLabel";

		public static readonly StringName _modifiersContainer = "_modifiersContainer";

		public static readonly StringName _remotePlayerContainer = "_remotePlayerContainer";

		public static readonly StringName _readyAndWaitingContainer = "_readyAndWaitingContainer";
	}

	public new class SignalName : NSubmenu.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("screens/daily_run/daily_run_screen");

	private static readonly LocString _timeLeftLoc = new LocString("main_menu_ui", "DAILY_RUN_MENU.TIME_LEFT");

	public static readonly string dateFormat = LocManager.Instance.GetTable("main_menu_ui").GetRawText("DAILY_RUN_MENU.DATE_FORMAT");

	private static readonly string _timeLeftFormat = LocManager.Instance.GetTable("main_menu_ui").GetRawText("DAILY_RUN_MENU.TIME_FORMAT");

	private MegaLabel _titleLabel;

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

	private void InitializeLeaderboard()
	{
		_leaderboard.Initialize(_lobby.DailyTime.Value.serverTime, _lobby.Players.Select((LobbyPlayer p) => p.id), allowPagination: false);
	}

	private async Task SetupLobbyForHostOrSingleplayer()
	{
		if (_netService.Type != NetGameType.Host && _netService.Type != NetGameType.Singleplayer)
		{
			throw new InvalidOperationException("Should only be called as host or singleplayer!");
		}
		SetIsLoading(isLoading: true);
		TimeServerResult timeServerResult = await GetTimeServerTime();
		_lobby = new StartRunLobby(GameMode.Daily, _netService, this, timeServerResult, 4);
		_lobby.AddLocalHostPlayer(new UnlockState(SaveManager.Instance.Progress), SaveManager.Instance.Progress.MaxMultiplayerAscension);
		SetupLobbyParams(_lobby);
		AfterLobbyInitialized();
		SetIsLoading(isLoading: false);
		Log.Info($"Daily initialized with seed: {_lobby.Seed} time: {GetServerRelativeTime()}");
	}

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
				catch (HttpRequestException ex)
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
			catch (HttpRequestException ex2)
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

	private DateTimeOffset GetServerRelativeTime()
	{
		return _lobby.DailyTime.Value.serverTime + (DateTimeOffset.UtcNow - _lobby.DailyTime.Value.localReceivedTime);
	}

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

	public void PlayerChanged(LobbyPlayer player)
	{
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
		if (info.SelfInitiated && info.GetReason() == NetError.Quit)
		{
			return;
		}
		_stack.Pop();
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
		Log.Info($"Embarking on a DAILY {_lobby.LocalPlayer.character.Id.Entry} run with {_lobby.Players.Count} players. Ascension: {_lobby.Ascension} Seed: {seed}");
		SfxCmd.Play(_lobby.LocalPlayer.character.CharacterTransitionSfx);
		await NGame.Instance.Transition.FadeOut(0.8f, _lobby.LocalPlayer.character.CharacterSelectTransitionPath);
		await NGame.Instance.StartNewSingleplayerRun(_lobby.LocalPlayer.character, shouldSave: true, acts, modifiers, seed, _lobby.Ascension, _lobby.DailyTime.Value.serverTime);
		CleanUpLobby(disconnectSession: false);
	}

	public async Task StartNewMultiplayerRun(string seed, List<ActModel> acts, IReadOnlyList<ModifierModel> modifiers)
	{
		Log.Info($"Embarking on a DAILY multiplayer run. Players: {string.Join(",", _lobby.Players)}. Ascension: {_lobby.Ascension} Seed: {seed}");
		SfxCmd.Play(_lobby.LocalPlayer.character.CharacterTransitionSfx);
		await NGame.Instance.Transition.FadeOut(0.8f, _lobby.LocalPlayer.character.CharacterSelectTransitionPath);
		await NGame.Instance.StartNewMultiplayerRun(_lobby, shouldSave: true, acts, modifiers, seed, _lobby.Ascension, _lobby.DailyTime.Value.serverTime);
		CleanUpLobby(disconnectSession: false);
	}

	private void CleanUpLobby(bool disconnectSession)
	{
		_lobby?.CleanUp(disconnectSession);
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
			new PropertyInfo(Variant.Type.Bool, "disconnectSession", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AfterLobbyInitialized, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

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
		if (method == MethodName.CleanUpLobby && args.Count == 1)
		{
			CleanUpLobby(VariantUtils.ConvertTo<bool>(in args[0]));
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

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._titleLabel)
		{
			_titleLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
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

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.InitialFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._titleLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
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

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._titleLabel, Variant.From(in _titleLabel));
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

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._titleLabel, out var value))
		{
			_titleLabel = value.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._dateLabel, out var value2))
		{
			_dateLabel = value2.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._timeLeftLabel, out var value3))
		{
			_timeLeftLabel = value3.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._characterContainer, out var value4))
		{
			_characterContainer = value4.As<NDailyRunCharacterContainer>();
		}
		if (info.TryGetProperty(PropertyName._embarkButton, out var value5))
		{
			_embarkButton = value5.As<NConfirmButton>();
		}
		if (info.TryGetProperty(PropertyName._backButton, out var value6))
		{
			_backButton = value6.As<NBackButton>();
		}
		if (info.TryGetProperty(PropertyName._unreadyButton, out var value7))
		{
			_unreadyButton = value7.As<NBackButton>();
		}
		if (info.TryGetProperty(PropertyName._leaderboard, out var value8))
		{
			_leaderboard = value8.As<NDailyRunLeaderboard>();
		}
		if (info.TryGetProperty(PropertyName._modifiersTitleLabel, out var value9))
		{
			_modifiersTitleLabel = value9.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._modifiersContainer, out var value10))
		{
			_modifiersContainer = value10.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._remotePlayerContainer, out var value11))
		{
			_remotePlayerContainer = value11.As<NRemoteLobbyPlayerContainer>();
		}
		if (info.TryGetProperty(PropertyName._readyAndWaitingContainer, out var value12))
		{
			_readyAndWaitingContainer = value12.As<Control>();
		}
	}
}
