using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.AutoSlay;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Leaderboard;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Debug;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Reaction;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Nodes.Screens.FeedbackScreen;
using MegaCrit.Sts2.Core.Nodes.Screens.InspectScreens;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Nodes.Screens.Settings;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Platform.Steam;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.Settings;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Nodes;

[ScriptPath("res://src/Core/Nodes/NGame.cs")]
public class NGame : Control
{
	[Signal]
	public delegate void WindowChangeEventHandler();

	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnWindowChange = "OnWindowChange";

		public static readonly StringName IsMainThread = "IsMainThread";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName IsReleaseGame = "IsReleaseGame";

		public static readonly StringName InitializeGraphicsPreferences = "InitializeGraphicsPreferences";

		public static readonly StringName ApplyDisplaySettings = "ApplyDisplaySettings";

		public static readonly StringName GetInspectRelicScreen = "GetInspectRelicScreen";

		public static readonly StringName GetInspectCardScreen = "GetInspectCardScreen";

		public static readonly StringName ApplySyncSetting = "ApplySyncSetting";

		public static readonly StringName Reset = "Reset";

		public new static readonly StringName _Notification = "_Notification";

		public static readonly StringName Quit = "Quit";

		public static readonly StringName Relocalize = "Relocalize";

		public static readonly StringName ReloadMainMenu = "ReloadMainMenu";

		public new static readonly StringName _Input = "_Input";

		public static readonly StringName ToggleFullscreen = "ToggleFullscreen";

		public static readonly StringName DebugModifyTimescale = "DebugModifyTimescale";

		public static readonly StringName ActivateWorldEnvironment = "ActivateWorldEnvironment";

		public static readonly StringName DeactivateWorldEnvironment = "DeactivateWorldEnvironment";

		public static readonly StringName SetScreenShakeTarget = "SetScreenShakeTarget";

		public static readonly StringName ClearScreenShakeTarget = "ClearScreenShakeTarget";

		public static readonly StringName ScreenShake = "ScreenShake";

		public static readonly StringName ScreenRumble = "ScreenRumble";

		public static readonly StringName ScreenShakeTrauma = "ScreenShakeTrauma";

		public static readonly StringName DoHitStop = "DoHitStop";

		public static readonly StringName ToggleTrailerMode = "ToggleTrailerMode";

		public static readonly StringName SetScreenshakeMultiplier = "SetScreenshakeMultiplier";

		public static readonly StringName InitPools = "InitPools";

		public static readonly StringName CheckShowLocalizationOverrideErrors = "CheckShowLocalizationOverrideErrors";

		public static readonly StringName LogResourceStats = "LogResourceStats";

		public static readonly StringName FormatBytes = "FormatBytes";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName RootSceneContainer = "RootSceneContainer";

		public static readonly StringName HoverTipsContainer = "HoverTipsContainer";

		public static readonly StringName MainMenu = "MainMenu";

		public static readonly StringName CurrentRunNode = "CurrentRunNode";

		public static readonly StringName LogoAnimation = "LogoAnimation";

		public static readonly StringName Transition = "Transition";

		public static readonly StringName TimeoutOverlay = "TimeoutOverlay";

		public static readonly StringName AudioManager = "AudioManager";

		public static readonly StringName RemoteCursorContainer = "RemoteCursorContainer";

		public static readonly StringName InputManager = "InputManager";

		public static readonly StringName HotkeyManager = "HotkeyManager";

		public static readonly StringName ReactionWheel = "ReactionWheel";

		public static readonly StringName ReactionContainer = "ReactionContainer";

		public static readonly StringName CursorManager = "CursorManager";

		public static readonly StringName DebugAudio = "DebugAudio";

		public static readonly StringName DebugSeedOverride = "DebugSeedOverride";

		public static readonly StringName StartOnMainMenu = "StartOnMainMenu";

		public static readonly StringName InspectRelicScreen = "InspectRelicScreen";

		public static readonly StringName InspectCardScreen = "InspectCardScreen";

		public static readonly StringName FeedbackScreen = "FeedbackScreen";

		public static readonly StringName WorldEnvironment = "WorldEnvironment";

		public static readonly StringName HitStop = "HitStop";

		public static readonly StringName _inspectionContainer = "_inspectionContainer";

		public static readonly StringName _screenShake = "_screenShake";
	}

	public new class SignalName : Control.SignalName
	{
		public static readonly StringName WindowChange = "WindowChange";
	}

	public static readonly Vector2 devResolution = new Vector2(1920f, 1080f);

	private Control _inspectionContainer;

	private NScreenShake _screenShake;

	private static int? _mainThreadId;

	private static Window _window = null;

	private CancellationTokenSource? _logoCancelToken;

	private SteamJoinCallbackHandler? _joinCallbackHandler;

	private WindowChangeEventHandler backing_WindowChange;

	public static NGame? Instance { get; private set; }

	public NSceneContainer RootSceneContainer { get; private set; }

	public Node? HoverTipsContainer { get; private set; }

	public NMainMenu? MainMenu => RootSceneContainer.CurrentScene as NMainMenu;

	public NRun? CurrentRunNode => RootSceneContainer.CurrentScene as NRun;

	public NLogoAnimation? LogoAnimation => RootSceneContainer.CurrentScene as NLogoAnimation;

	public NTransition Transition { get; private set; }

	public NMultiplayerTimeoutOverlay TimeoutOverlay { get; private set; }

	public NAudioManager AudioManager { get; private set; }

	public NRemoteMouseCursorContainer RemoteCursorContainer { get; private set; }

	public NInputManager InputManager { get; private set; }

	public NHotkeyManager HotkeyManager { get; private set; }

	public NReactionWheel ReactionWheel { get; private set; }

	public NReactionContainer ReactionContainer { get; private set; }

	public NCursorManager CursorManager { get; private set; }

	public NDebugAudioManager DebugAudio { get; private set; }

	public string? DebugSeedOverride { get; set; }

	public bool StartOnMainMenu { get; set; } = true;

	public static bool IsTrailerMode { get; private set; }

	public static bool IsDebugHidingHoverTips { get; private set; }

	public static bool IsDebugHidingProceedButton { get; private set; }

	public NInspectRelicScreen? InspectRelicScreen { get; set; }

	public NInspectCardScreen? InspectCardScreen { get; set; }

	public NSendFeedbackScreen FeedbackScreen { get; set; }

	private WorldEnvironment WorldEnvironment { get; set; }

	private NHitStop HitStop { get; set; }

	public event Action? DebugToggleProceedButton;

	public event WindowChangeEventHandler WindowChange
	{
		add
		{
			backing_WindowChange = (WindowChangeEventHandler)Delegate.Combine(backing_WindowChange, value);
		}
		remove
		{
			backing_WindowChange = (WindowChangeEventHandler)Delegate.Remove(backing_WindowChange, value);
		}
	}

	public override void _EnterTree()
	{
		if (Instance != null)
		{
			Log.Error("NGame already exists.");
			this.QueueFreeSafely();
			return;
		}
		Instance = this;
		SentryService.Initialize();
		RootSceneContainer = GetNode<NSceneContainer>("%RootSceneContainer");
		HoverTipsContainer = GetNode<Node>("%HoverTipsContainer");
		DebugAudio = GetNode<NDebugAudioManager>("%DebugAudioManager");
		AudioManager = GetNode<NAudioManager>("%AudioManager");
		RemoteCursorContainer = GetNode<NRemoteMouseCursorContainer>("%RemoteCursorContainer");
		InputManager = GetNode<NInputManager>("%InputManager");
		CursorManager = GetNode<NCursorManager>("%CursorManager");
		ReactionWheel = GetNode<NReactionWheel>("%ReactionWheel");
		ReactionContainer = GetNode<NReactionContainer>("%ReactionContainer");
		TimeoutOverlay = GetNode<NMultiplayerTimeoutOverlay>("%MultiplayerTimeoutOverlay");
		WorldEnvironment = GetNode<WorldEnvironment>("%WorldEnvironment");
		HotkeyManager = GetNode<NHotkeyManager>("%HotkeyManager");
		_inspectionContainer = GetNode<Control>("%InspectionContainer");
		_screenShake = GetNode<NScreenShake>("ScreenShake");
		HitStop = GetNode<NHitStop>("HitStop");
		Transition = GetNode<NTransition>("%GameTransitionRect");
		FeedbackScreen = GetNode<NSendFeedbackScreen>("%FeedbackScreen");
		_mainThreadId = System.Environment.CurrentManagedThreadId;
		TaskHelper.RunSafely(GameStartupWrapper());
	}

	public override void _Ready()
	{
		_window = GetTree().Root;
		_window.Connect(Viewport.SignalName.SizeChanged, Callable.From(OnWindowChange));
		this.RemoveChildSafely(WorldEnvironment);
	}

	private async Task GameStartupWrapper()
	{
		if (!(await InitializePlatform()))
		{
			return;
		}
		TaskHelper.RunSafely(OsDebugInfo.LogSystemInfo());
		TaskHelper.RunSafely(GitHelper.Initialize());
		try
		{
			await GameStartup();
		}
		catch
		{
			TaskHelper.RunSafely(GameStartupError());
			throw;
		}
	}

	private async Task TryErrorInit()
	{
		try
		{
			if (SaveManager.Instance.SettingsSave == null)
			{
				SaveManager.Instance.InitSettingsData();
			}
			if (LocManager.Instance == null)
			{
				LocManager.Initialize();
			}
		}
		catch (Exception value)
		{
			Log.Error($"Failed to show error dialog! Exception: {value}");
			GetTree().Quit();
			throw;
		}
		if (!IsNodeReady())
		{
			await ToSignal(this, Node.SignalName.Ready);
		}
		Transition.Visible = false;
	}

	private async Task GameStartupError()
	{
		Log.Error("Encountered error on game startup! Attempting to show error dialog");
		await TryErrorInit();
		NGenericPopup nGenericPopup = NGenericPopup.Create();
		NModalContainer.Instance.Add(nGenericPopup);
		await nGenericPopup.WaitForConfirmation(new LocString("main_menu_ui", "STARTUP_ERROR.description"), new LocString("main_menu_ui", "STARTUP_ERROR.title"), null, new LocString("main_menu_ui", "QUIT"));
		GetTree().Quit();
	}

	private async Task GameStartup()
	{
		AccountScopeUserDataMigrator.MigrateToUserScopedDirectories();
		AccountScopeUserDataMigrator.ArchiveLegacyData();
		ProfileAccountScopeMigrator.MigrateToProfileScopedDirectories();
		ProfileAccountScopeMigrator.ArchiveLegacyData();
		bool flag = await SaveManager.Instance.TryFirstTimeCloudSync();
		Task cloudSavesTask = null;
		if (!flag)
		{
			cloudSavesTask = Task.Run((Func<Task?>)SaveManager.Instance.SyncCloudToLocal);
		}
		InitPools();
		OneTimeInitialization.ExecuteEssential();
		if (!IsNodeReady())
		{
			await ToSignal(this, Node.SignalName.Ready);
		}
		Callable.From(InitializeGraphicsPreferences).CallDeferred();
		AudioManager.SetMasterVol(SaveManager.Instance.SettingsSave.VolumeMaster);
		AudioManager.SetSfxVol(SaveManager.Instance.SettingsSave.VolumeSfx);
		AudioManager.SetAmbienceVol(SaveManager.Instance.SettingsSave.VolumeAmbience);
		AudioManager.SetBgmVol(SaveManager.Instance.SettingsSave.VolumeBgm);
		DebugAudio.SetMasterAudioVolume(SaveManager.Instance.SettingsSave.VolumeMaster);
		DebugAudio.SetSfxAudioVolume(SaveManager.Instance.SettingsSave.VolumeSfx);
		LeaderboardManager.Initialize();
		SteamStatsManager.Initialize();
		if (cloudSavesTask != null)
		{
			await cloudSavesTask;
		}
		SaveManager.Instance.InitProfileId();
		ReadSaveResult<SerializableProgress> progressReadResult = SaveManager.Instance.InitProgressData();
		ReadSaveResult<PrefsSave> prefsReadResult = SaveManager.Instance.InitPrefsData();
		SentryService.SetUserContext(SaveManager.Instance.Progress.UniqueId);
		string platformBranch = PlatformUtil.GetPlatformBranch();
		if (platformBranch != null)
		{
			SentryService.SetTag("platform.branch", platformBranch);
		}
		_screenShake.SetMultiplier(NScreenshakePaginator.GetShakeMultiplier(SaveManager.Instance.PrefsSave.ScreenShakeOptionIndex));
		if (!OS.HasFeature("editor") && SaveManager.Instance.PrefsSave.FastMode == FastModeType.Instant)
		{
			SaveManager.Instance.PrefsSave.FastMode = FastModeType.Fast;
		}
		if (!IsReleaseGame() && CommandLineHelper.HasArg("autoslay"))
		{
			await LaunchMainMenu(skipLogo: true);
			string seed = CommandLineHelper.GetValue("seed") ?? SeedHelper.GetRandomSeed();
			string value = CommandLineHelper.GetValue("log-file");
			AutoSlayer autoSlayer = new AutoSlayer();
			autoSlayer.Start(seed, value);
		}
		else if (CommandLineHelper.HasArg("bootstrap"))
		{
			NSceneBootstrapper child = SceneHelper.Instantiate<NSceneBootstrapper>("debug/scene_bootstrapper");
			this.AddChildSafely(child);
		}
		else if (StartOnMainMenu)
		{
			bool skipLogo = DebugSettings.DevSkip || SaveManager.Instance.SettingsSave.SkipIntroLogo || CommandLineHelper.HasArg("fastmp");
			await LaunchMainMenu(skipLogo);
			CheckShowSaveFileError(progressReadResult, prefsReadResult, OneTimeInitialization.SettingsReadResult);
			CheckShowLocalizationOverrideErrors();
		}
		ModManager.OnModDetected += OnNewModDetected;
	}

	private void OnWindowChange()
	{
		Log.Info($"Window changed! New size: {DisplayServer.WindowGetSize()}");
		EmitSignal(SignalName.WindowChange, SaveManager.Instance.SettingsSave.AspectRatioSetting == AspectRatioSetting.Auto);
	}

	public static bool IsMainThread()
	{
		if (!_mainThreadId.HasValue)
		{
			_mainThreadId = System.Environment.CurrentManagedThreadId;
			return true;
		}
		return _mainThreadId == System.Environment.CurrentManagedThreadId;
	}

	public override void _ExitTree()
	{
		ModManager.OnModDetected -= OnNewModDetected;
		ModManager.Dispose();
		_joinCallbackHandler?.Dispose();
		SteamInitializer.Uninitialize();
		SentryService.Shutdown();
	}

	public static bool IsReleaseGame()
	{
		return true;
	}

	private void InitializeGraphicsPreferences()
	{
		if (!DisplayServer.GetName().Equals("headless", StringComparison.OrdinalIgnoreCase))
		{
			ApplyDisplaySettings();
			ApplySyncSetting();
		}
		Engine.MaxFps = SaveManager.Instance.SettingsSave.FpsLimit;
	}

	public void ApplyDisplaySettings()
	{
		bool flag = false;
		SettingsSave settingsSave = SaveManager.Instance.SettingsSave;
		if (settingsSave.TargetDisplay == -1)
		{
			Log.Info("First time setup for display settings...");
			settingsSave.TargetDisplay = DisplayServer.GetPrimaryScreen();
		}
		bool flag2 = settingsSave.Fullscreen;
		if (PlatformUtil.GetSupportedWindowMode().ShouldForceFullscreen())
		{
			if (!flag2)
			{
				Log.Warn($"Settings has fullscreen set to false, but we're forcing fullscreen because the platform reports our supported window mode as {PlatformUtil.GetSupportedWindowMode()}");
			}
			flag2 = true;
		}
		Log.Info($"Applying display settings...\n  FULLSCREEN: {flag2}\n  ASPECT_RATIO: ({settingsSave.AspectRatioSetting})\n  TARGET_DISPLAY: ({settingsSave.TargetDisplay})\n  WINDOW_SIZE: {settingsSave.WindowSize}\n  POSITION: {settingsSave.WindowPosition}");
		Log.Info($"[Display] Min size: {DisplayServer.WindowGetMinSize()} Max size: {DisplayServer.WindowGetMaxSize()}");
		if (settingsSave.AspectRatioSetting != AspectRatioSetting.Auto)
		{
			_window.ContentScaleAspect = Window.ContentScaleAspectEnum.Keep;
		}
		switch (settingsSave.AspectRatioSetting)
		{
		case AspectRatioSetting.Auto:
			flag = true;
			break;
		case AspectRatioSetting.FourByThree:
			_window.ContentScaleSize = new Vector2I(1680, 1260);
			break;
		case AspectRatioSetting.SixteenByTen:
			_window.ContentScaleSize = new Vector2I(1920, 1200);
			break;
		case AspectRatioSetting.SixteenByNine:
			_window.ContentScaleSize = new Vector2I(1920, 1080);
			break;
		case AspectRatioSetting.TwentyOneByNine:
			_window.ContentScaleSize = new Vector2I(2580, 1080);
			break;
		default:
			throw new ArgumentOutOfRangeException($"Invalid Aspect Ratio: {settingsSave.AspectRatioSetting}");
		}
		int num = System.Environment.GetCommandLineArgs().IndexOf("-wpos");
		if (flag2 && num < 0)
		{
			if (_window.Unresizable)
			{
				_window.Unresizable = false;
			}
			Log.Info($"[Display] Setting FULLSCREEN on Display: {settingsSave.TargetDisplay + 1} of {DisplayServer.GetScreenCount()}");
			if (settingsSave.TargetDisplay >= DisplayServer.GetScreenCount())
			{
				Log.Warn($"[Display] FAILED: Display {settingsSave.TargetDisplay} is missing. Fallback to primary.");
				DisplayServer.WindowSetCurrentScreen(DisplayServer.GetPrimaryScreen());
				settingsSave.TargetDisplay = DisplayServer.GetPrimaryScreen();
			}
			else
			{
				DisplayServer.WindowSetCurrentScreen(settingsSave.TargetDisplay);
			}
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
		}
		else
		{
			Log.Info($"[Display] Attempting WINDOWED mode on Display {settingsSave.TargetDisplay + 1} of {DisplayServer.GetScreenCount()} at position {settingsSave.WindowPosition}");
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
			if (_window.Unresizable != !settingsSave.ResizeWindows)
			{
				_window.Unresizable = !settingsSave.ResizeWindows;
			}
			if (num >= 0)
			{
				Log.Info("[Display] -wpos called. Applying special logic.");
				settingsSave.Fullscreen = false;
				Vector2I vector2I = new Vector2I(int.Parse(System.Environment.GetCommandLineArgs()[num + 1]), int.Parse(System.Environment.GetCommandLineArgs()[num + 2]));
				Vector2I vector2I2 = DisplayServer.ScreenGetPosition(DisplayServer.WindowGetCurrentScreen());
				Log.Info($"Applying window position from command line arg: {vector2I} ({string.Join(",", System.Environment.GetCommandLineArgs())} {vector2I2})");
				DisplayServer.WindowSetPosition(vector2I2 + vector2I);
				DisplayServer.WindowSetSize(settingsSave.WindowSize);
			}
			else
			{
				Vector2I vector2I3 = DisplayServer.ScreenGetSize(settingsSave.TargetDisplay);
				Vector2I windowSize = settingsSave.WindowSize;
				if (settingsSave.WindowPosition == new Vector2I(-1, -1))
				{
					Log.Info($"[Display] Going from fullscreen to windowed. Attempting to center window on screen {settingsSave.TargetDisplay}");
					settingsSave.WindowPosition = vector2I3 / 2 - windowSize / 2;
				}
				Vector2I vector2I4 = settingsSave.WindowPosition;
				if (vector2I4.X < 0 || vector2I4.Y < 0 || vector2I4.X > vector2I3.X || vector2I4.Y > vector2I3.Y)
				{
					Log.Warn("[Display] WARN: Game Window was offscreen. Resetting to top left corner.");
					vector2I4 = new Vector2I(8, 48);
				}
				if (settingsSave.TargetDisplay >= DisplayServer.GetScreenCount())
				{
					Log.Info($"[Display] FAILED: Display {settingsSave.TargetDisplay + 1} is missing. Fallback to primary.");
					settingsSave.WindowPosition = new Vector2I(8, 48);
					DisplayServer.WindowSetSize(DisplayServer.ScreenGetSize(DisplayServer.GetPrimaryScreen()) - new Vector2I(8, 48));
					DisplayServer.WindowSetPosition(DisplayServer.ScreenGetPosition(DisplayServer.GetPrimaryScreen()) + settingsSave.WindowPosition);
				}
				else
				{
					Vector2I vector2I5 = DisplayServer.ScreenGetPosition(settingsSave.TargetDisplay);
					if (windowSize.X > vector2I3.X)
					{
						windowSize.X = vector2I3.X;
					}
					if (windowSize.Y > vector2I3.Y)
					{
						windowSize.Y = vector2I3.Y;
					}
					Log.Info($"[Display] SUCCESS: {windowSize} Windowed mode in Display {settingsSave.TargetDisplay}: Position {vector2I5 + vector2I4} ({vector2I4})");
					DisplayServer.WindowSetSize(windowSize);
					DisplayServer.WindowSetPosition(vector2I5 + vector2I4);
					Log.Info($"[Display] New size: {DisplayServer.WindowGetSize()} position: {DisplayServer.WindowGetPosition()}");
				}
			}
		}
		if (flag)
		{
			Log.Info("Manual window change signal because of auto scaling");
			EmitSignal(SignalName.WindowChange, settingsSave.AspectRatioSetting == AspectRatioSetting.Auto);
		}
	}

	public NInspectRelicScreen GetInspectRelicScreen()
	{
		if (InspectRelicScreen == null)
		{
			InspectRelicScreen = NInspectRelicScreen.Create();
			_inspectionContainer.AddChildSafely(InspectRelicScreen);
		}
		return InspectRelicScreen;
	}

	public NInspectCardScreen GetInspectCardScreen()
	{
		if (InspectCardScreen == null)
		{
			InspectCardScreen = NInspectCardScreen.Create();
			_inspectionContainer.AddChildSafely(InspectCardScreen);
		}
		return InspectCardScreen;
	}

	public static void ApplySyncSetting()
	{
		switch (SaveManager.Instance.SettingsSave.VSync)
		{
		case VSyncType.Off:
			Log.Info("VSync: Off");
			DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Disabled);
			break;
		case VSyncType.On:
			Log.Info("VSync: On");
			DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Enabled);
			break;
		case VSyncType.Adaptive:
			Log.Info("VSync: Adaptive");
			DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Adaptive);
			break;
		default:
			Log.Error($"Invalid VSync type: {SaveManager.Instance.SettingsSave.VSync}");
			break;
		}
	}

	public static void Reset()
	{
		Instance?.QueueFreeSafely();
		Instance = null;
	}

	public override void _Notification(int what)
	{
		if ((long)what == 1006)
		{
			Quit();
		}
	}

	public void Quit()
	{
		Log.Info("NGame.Quit called");
		if (!PlatformUtil.GetSupportedWindowMode().ShouldForceFullscreen() && !SaveManager.Instance.SettingsSave.Fullscreen)
		{
			SettingsSave settingsSave = SaveManager.Instance.SettingsSave;
			settingsSave.WindowSize = DisplayServer.WindowGetSize();
			settingsSave.TargetDisplay = DisplayServer.WindowGetCurrentScreen();
			settingsSave.WindowPosition = DisplayServer.WindowGetPosition() - DisplayServer.ScreenGetPosition(SaveManager.Instance.SettingsSave.TargetDisplay);
			Log.Info($"[Display] On exit, saving window size: {settingsSave.WindowSize} display: {settingsSave.TargetDisplay} position: {settingsSave.WindowPosition}");
		}
		SaveManager.Instance.SaveSettings();
		SaveManager.Instance.SavePrefsFile();
		SaveManager.Instance.SaveProgressFile();
		SaveManager.Instance.SaveProfile();
		GetTree().Quit();
	}

	private async Task LaunchMainMenu(bool skipLogo)
	{
		NLogoAnimation logoAnimation = null;
		if (skipLogo)
		{
			await PreloadManager.LoadMainMenuEssentials();
		}
		else
		{
			await PreloadManager.LoadLogoAnimation();
			logoAnimation = NLogoAnimation.Create();
			RootSceneContainer.SetCurrentScene(logoAnimation);
			await PreloadManager.LoadMainMenuEssentials();
		}
		if (logoAnimation != null)
		{
			_logoCancelToken = new CancellationTokenSource();
			await Transition.FadeIn(0.8f, "res://materials/transitions/fade_transition_mat.tres", _logoCancelToken.Token);
			await logoAnimation.PlayAnimation(_logoCancelToken.Token);
			await Transition.FadeOut();
		}
		await LoadMainMenu();
		Log.Info($"[Startup] Time to main menu: {Time.GetTicksMsec():N0}ms");
		LogResourceStats("main menu loaded (essential)");
		TaskHelper.RunSafely(LoadDeferredStartupAssetsAsync());
		_joinCallbackHandler?.CheckForCommandLineJoin();
	}

	private async Task LoadDeferredStartupAssetsAsync()
	{
		OneTimeInitialization.ExecuteDeferred();
		await PreloadManager.LoadCommonAndMainMenuAssets();
		LogResourceStats("main menu loaded (complete)");
	}

	public async Task GoToTimelineAfterRun()
	{
		await GoToTimeline();
	}

	public async Task ReturnToMainMenuAfterRun()
	{
		await ReturnToMainMenu();
	}

	public async Task GoToTimeline()
	{
		await Transition.FadeOut();
		await PreloadManager.LoadCommonAndMainMenuAssets();
		RunManager.Instance.CleanUp();
		await LoadMainMenu(openTimeline: true);
	}

	public async Task ReturnToMainMenu()
	{
		await Transition.FadeOut();
		await PreloadManager.LoadCommonAndMainMenuAssets();
		RunManager.Instance.CleanUp();
		await LoadMainMenu();
	}

	public void Relocalize()
	{
		ReloadMainMenu();
		FeedbackScreen.Relocalize();
		TimeoutOverlay.Relocalize();
	}

	public void ReloadMainMenu()
	{
		if (MainMenu == null)
		{
			throw new InvalidOperationException("Tried to reload main menu when not already on the main menu!");
		}
		TaskHelper.RunSafely(LoadMainMenu());
	}

	private async Task LoadMainMenu(bool openTimeline = false)
	{
		Task currentRunSaveTask = SaveManager.Instance.CurrentRunSaveTask;
		if (currentRunSaveTask != null)
		{
			Log.Info("Saving in progress, waiting for it to be finished before loading the main menu");
			try
			{
				await currentRunSaveTask;
			}
			catch (Exception value)
			{
				Log.Error($"Save task failed while waiting to load main menu: {value}");
			}
		}
		NMainMenu currentScene = NMainMenu.Create(openTimeline);
		RootSceneContainer.SetCurrentScene(currentScene);
	}

	public async Task<RunState> StartNewSingleplayerRun(CharacterModel character, bool shouldSave, IReadOnlyList<ActModel> acts, IReadOnlyList<ModifierModel> modifiers, string seed, int ascensionLevel = 0, DateTimeOffset? dailyTime = null)
	{
		UnlockState unlockState = SaveManager.Instance.GenerateUnlockStateFromProgress();
		RunState runState = RunState.CreateForNewRun(new global::_003C_003Ez__ReadOnlySingleElementList<Player>(Player.CreateForNewRun(character, unlockState, 1uL)), acts.Select((ActModel a) => a.ToMutable()).ToList(), modifiers, ascensionLevel, seed);
		RunManager.Instance.SetUpNewSinglePlayer(runState, shouldSave, dailyTime);
		await StartRun(runState);
		return runState;
	}

	public async Task<RunState> StartNewMultiplayerRun(StartRunLobby lobby, bool shouldSave, IReadOnlyList<ActModel> acts, IReadOnlyList<ModifierModel> modifiers, string seed, int ascensionLevel, DateTimeOffset? dailyTime = null)
	{
		RunState runState = RunState.CreateForNewRun(lobby.Players.Select((LobbyPlayer p) => Player.CreateForNewRun(p.character, UnlockState.FromSerializable(p.unlockState), p.id)).ToList(), acts.Select((ActModel a) => a.ToMutable()).ToList(), modifiers, ascensionLevel, seed);
		RunManager.Instance.SetUpNewMultiPlayer(runState, lobby, shouldSave, dailyTime);
		await StartRun(runState);
		return runState;
	}

	public async Task LoadRun(RunState runState, SerializableRoom? preFinishedRoom)
	{
		await PreloadManager.LoadRunAssets(runState.Players.Select((Player p) => p.Character));
		await PreloadManager.LoadActAssets(runState.Act);
		RunManager.Instance.Launch();
		RootSceneContainer.SetCurrentScene(NRun.Create(runState));
		await RunManager.Instance.GenerateMap();
		await RunManager.Instance.LoadIntoLatestMapCoord(AbstractRoom.FromSerializable(preFinishedRoom, runState));
		if (RunManager.Instance.MapDrawingsToLoad != null)
		{
			NRun.Instance.GlobalUi.MapScreen.Drawings.LoadDrawings(RunManager.Instance.MapDrawingsToLoad);
			RunManager.Instance.MapDrawingsToLoad = null;
		}
	}

	private async Task StartRun(RunState runState)
	{
		using (new NetLoadingHandle(RunManager.Instance.NetService))
		{
			await PreloadManager.LoadRunAssets(runState.Players.Select((Player p) => p.Character));
			await PreloadManager.LoadActAssets(runState.Acts[0]);
			await RunManager.Instance.FinalizeStartingRelics();
			RunManager.Instance.Launch();
			RootSceneContainer.SetCurrentScene(NRun.Create(runState));
			await RunManager.Instance.EnterAct(0, doTransition: false);
		}
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (inputEvent.IsActionReleased(DebugHotkey.speedUp))
		{
			DebugModifyTimescale(0.1);
		}
		else if (inputEvent.IsActionReleased(DebugHotkey.speedDown))
		{
			DebugModifyTimescale(-0.1);
		}
		else if (inputEvent.IsActionReleased(DebugHotkey.hideProceedButton))
		{
			IsDebugHidingProceedButton = !IsDebugHidingProceedButton;
			Instance.AddChildSafely(NFullscreenTextVfx.Create(IsDebugHidingProceedButton ? "Hide Proceed Button" : "Show Proceed Button"));
			this.DebugToggleProceedButton?.Invoke();
		}
		else if (inputEvent.IsActionReleased(DebugHotkey.hideHoverTips))
		{
			IsDebugHidingHoverTips = !IsDebugHidingHoverTips;
			Instance.AddChildSafely(NFullscreenTextVfx.Create(IsDebugHidingHoverTips ? "Hide HoverTips" : "Show HoverTips"));
		}
		if (inputEvent is InputEventMouseButton { Pressed: not false } || inputEvent.IsActionPressed(MegaInput.select) || inputEvent.IsActionPressed(MegaInput.cancel))
		{
			_logoCancelToken?.Cancel();
		}
		if (!(inputEvent is InputEventKey inputEventKey))
		{
			return;
		}
		if (OS.GetName().Contains("Windows"))
		{
			if (inputEventKey.Pressed && inputEventKey.AltPressed && inputEventKey.Keycode == Key.Enter)
			{
				ToggleFullscreen();
			}
		}
		else if (OS.GetName().Contains("macOS") && inputEventKey.Pressed && inputEventKey.CtrlPressed && inputEventKey.MetaPressed && inputEventKey.Keycode == Key.F)
		{
			ToggleFullscreen();
		}
	}

	private void ToggleFullscreen()
	{
		Log.Info("Used FULLSCREEN shortcut");
		NFullscreenTickbox.SetFullscreen(!SaveManager.Instance.SettingsSave.Fullscreen);
	}

	private void DebugModifyTimescale(double offset)
	{
		double value = Math.Round(Engine.TimeScale + offset, 1);
		value = Math.Clamp(value, 0.1, 4.0);
		Engine.TimeScale = value;
		this.AddChildSafely(NFullscreenTextVfx.Create($"TimeScale:{Engine.TimeScale}"));
	}

	public WorldEnvironment ActivateWorldEnvironment()
	{
		this.AddChildSafely(WorldEnvironment);
		return WorldEnvironment;
	}

	public void DeactivateWorldEnvironment()
	{
		this.RemoveChildSafely(WorldEnvironment);
	}

	public void SetScreenShakeTarget(Control target)
	{
		_screenShake.SetTarget(target);
	}

	public void ClearScreenShakeTarget()
	{
		_screenShake.ClearTarget();
	}

	public void ScreenShake(ShakeStrength strength, ShakeDuration duration, float degAngle = -1f)
	{
		if (degAngle < 0f)
		{
			degAngle = Rng.Chaotic.NextFloat(360f);
		}
		_screenShake.Shake(strength, duration, degAngle);
	}

	public void ScreenRumble(ShakeStrength strength, ShakeDuration duration, RumbleStyle style)
	{
		_screenShake.Rumble(strength, duration, style);
	}

	public void ScreenShakeTrauma(ShakeStrength strength)
	{
		_screenShake.AddTrauma(strength);
	}

	public void DoHitStop(ShakeStrength strength, ShakeDuration duration)
	{
		HitStop.DoHitStop(strength, duration);
	}

	public static void ToggleTrailerMode()
	{
		IsTrailerMode = !IsTrailerMode;
	}

	public void SetScreenshakeMultiplier(float multiplier)
	{
		_screenShake.SetMultiplier(multiplier);
	}

	private void InitPools()
	{
		NCard.InitPool();
		NGridCardHolder.InitPool();
	}

	private void OnNewModDetected(Mod mod)
	{
		if (!NModalContainer.Instance.GetChildren().OfType<NErrorPopup>().Any())
		{
			NErrorPopup modalToCreate = NErrorPopup.Create(new LocString("main_menu_ui", "MOD_NOT_LOADED_POPUP.title"), new LocString("main_menu_ui", "MOD_NOT_LOADED_POPUP.description"), null, showReportBugButton: false);
			NModalContainer.Instance.Add(modalToCreate);
		}
	}

	public void CheckShowSaveFileError(ReadSaveResult<SerializableProgress> progressReadResult, ReadSaveResult<PrefsSave> prefsReadResult, ReadSaveResult<SettingsSave>? settingsReadResult)
	{
		LocString locString = null;
		if (!progressReadResult.Success && progressReadResult.Status != ReadSaveStatus.FileNotFound)
		{
			locString = new LocString("main_menu_ui", "INVALID_SAVE_POPUP.description_progress");
		}
		else if (settingsReadResult != null && !settingsReadResult.Success && settingsReadResult.Status != ReadSaveStatus.FileNotFound)
		{
			locString = new LocString("main_menu_ui", "INVALID_SAVE_POPUP.description_settings");
		}
		else if (!prefsReadResult.Success && prefsReadResult.Status != ReadSaveStatus.FileNotFound)
		{
			locString = new LocString("main_menu_ui", "INVALID_SAVE_POPUP.description_settings");
		}
		if (locString != null)
		{
			NErrorPopup modalToCreate = NErrorPopup.Create(new LocString("main_menu_ui", "INVALID_SAVE_POPUP.title"), locString, new LocString("main_menu_ui", "INVALID_SAVE_POPUP.dismiss"), showReportBugButton: true);
			NModalContainer.Instance.Add(modalToCreate);
		}
	}

	private void CheckShowLocalizationOverrideErrors()
	{
		if (LocManager.Instance.ValidationErrors.Count != 0)
		{
			List<IGrouping<string, LocValidationError>> list = (from e in LocManager.Instance.ValidationErrors
				group e by e.FilePath).ToList();
			string text = string.Join("\n", from g in list.Take(5)
				select $"{Path.GetFileName(g.Key)} ({g.Count()} errors)");
			if (list.Count > 5)
			{
				text += $"\n... and {list.Count - 5} more files";
			}
			string body = "Errors found in the following localization override files:\n\n" + text + "\n\n[gold]Check the console logs for detailed error messages.[/gold]\n\nTo fix: Remove or correct invalid override files in your localization_override folder.";
			NErrorPopup modalToCreate = NErrorPopup.Create("Localization Override Errors", body, showReportBugButton: false);
			NModalContainer.Instance.Add(modalToCreate);
		}
	}

	private async Task<bool> InitializePlatform()
	{
		bool flag = CommandLineHelper.HasArg("force-steam");
		string text = CommandLineHelper.GetValue("force-steam") ?? "";
		if (!text.Equals("on", StringComparison.OrdinalIgnoreCase) && (!flag || !(text == string.Empty)) && (text.Equals("off", StringComparison.OrdinalIgnoreCase) || OS.HasFeature("editor")))
		{
			Log.Info("Steam initialization skipped (editor mode). Use --force-steam to enable.");
			return true;
		}
		bool steamInitialized = SteamInitializer.Initialize(this);
		if (!steamInitialized)
		{
			Log.Error("Failed to initialize Steam! Attempting to show error popup");
			await TryErrorInit();
			NGenericPopup nGenericPopup = NGenericPopup.Create();
			NModalContainer.Instance.Add(nGenericPopup);
			LocString locString = new LocString("main_menu_ui", "STEAM_INIT_ERROR.description");
			locString.Add("details", $"{SteamInitializer.InitResult}: {SteamInitializer.InitErrorMessage}");
			await nGenericPopup.WaitForConfirmation(locString, new LocString("main_menu_ui", "STEAM_INIT_ERROR.title"), null, new LocString("main_menu_ui", "QUIT"));
			GetTree().Quit();
		}
		else
		{
			_joinCallbackHandler = new SteamJoinCallbackHandler();
		}
		return steamInitialized;
	}

	public static void LogResourceStats(string context)
	{
		ulong staticMemoryUsage = OS.GetStaticMemoryUsage();
		ulong renderingInfo = RenderingServer.GetRenderingInfo(RenderingServer.RenderingInfo.VideoMemUsed);
		int value = (int)Performance.GetMonitor(Performance.Monitor.ObjectCount);
		int value2 = (int)Performance.GetMonitor(Performance.Monitor.ObjectResourceCount);
		int value3 = (int)Performance.GetMonitor(Performance.Monitor.ObjectNodeCount);
		int value4 = PreloadManager.Cache.GetCacheKeys().Count();
		Log.Info($"[Startup] Resource stats ({context}): StaticMem={FormatBytes(staticMemoryUsage)}, VRAM={FormatBytes(renderingInfo)}, Objects={value:N0}, Resources={value2:N0}, Nodes={value3:N0}, CachedAssets={value4:N0}");
	}

	private static string FormatBytes(ulong bytes)
	{
		string[] array = new string[4] { "B", "KB", "MB", "GB" };
		int num = 0;
		double num2 = bytes;
		while (num2 >= 1024.0 && num < array.Length - 1)
		{
			num2 /= 1024.0;
			num++;
		}
		return $"{num2:0.#}{array[num]}";
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(33);
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnWindowChange, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.IsMainThread, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.IsReleaseGame, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName.InitializeGraphicsPreferences, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ApplyDisplaySettings, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GetInspectRelicScreen, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GetInspectCardScreen, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ApplySyncSetting, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName.Reset, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName._Notification, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "what", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.Quit, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Relocalize, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ReloadMainMenu, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ToggleFullscreen, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DebugModifyTimescale, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "offset", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ActivateWorldEnvironment, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("WorldEnvironment"), exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DeactivateWorldEnvironment, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetScreenShakeTarget, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "target", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ClearScreenShakeTarget, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ScreenShake, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "strength", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "duration", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Float, "degAngle", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ScreenRumble, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "strength", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "duration", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "style", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ScreenShakeTrauma, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "strength", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.DoHitStop, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "strength", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "duration", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ToggleTrailerMode, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName.SetScreenshakeMultiplier, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "multiplier", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.InitPools, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CheckShowLocalizationOverrideErrors, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.LogResourceStats, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "context", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.FormatBytes, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "bytes", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._EnterTree && args.Count == 0)
		{
			_EnterTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnWindowChange && args.Count == 0)
		{
			OnWindowChange();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.IsMainThread && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(IsMainThread());
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.IsReleaseGame && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(IsReleaseGame());
			return true;
		}
		if (method == MethodName.InitializeGraphicsPreferences && args.Count == 0)
		{
			InitializeGraphicsPreferences();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ApplyDisplaySettings && args.Count == 0)
		{
			ApplyDisplaySettings();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetInspectRelicScreen && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NInspectRelicScreen>(GetInspectRelicScreen());
			return true;
		}
		if (method == MethodName.GetInspectCardScreen && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NInspectCardScreen>(GetInspectCardScreen());
			return true;
		}
		if (method == MethodName.ApplySyncSetting && args.Count == 0)
		{
			ApplySyncSetting();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Reset && args.Count == 0)
		{
			Reset();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Notification && args.Count == 1)
		{
			_Notification(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Quit && args.Count == 0)
		{
			Quit();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Relocalize && args.Count == 0)
		{
			Relocalize();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ReloadMainMenu && args.Count == 0)
		{
			ReloadMainMenu();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Input && args.Count == 1)
		{
			_Input(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ToggleFullscreen && args.Count == 0)
		{
			ToggleFullscreen();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DebugModifyTimescale && args.Count == 1)
		{
			DebugModifyTimescale(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ActivateWorldEnvironment && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<WorldEnvironment>(ActivateWorldEnvironment());
			return true;
		}
		if (method == MethodName.DeactivateWorldEnvironment && args.Count == 0)
		{
			DeactivateWorldEnvironment();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetScreenShakeTarget && args.Count == 1)
		{
			SetScreenShakeTarget(VariantUtils.ConvertTo<Control>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ClearScreenShakeTarget && args.Count == 0)
		{
			ClearScreenShakeTarget();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ScreenShake && args.Count == 3)
		{
			ScreenShake(VariantUtils.ConvertTo<ShakeStrength>(in args[0]), VariantUtils.ConvertTo<ShakeDuration>(in args[1]), VariantUtils.ConvertTo<float>(in args[2]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ScreenRumble && args.Count == 3)
		{
			ScreenRumble(VariantUtils.ConvertTo<ShakeStrength>(in args[0]), VariantUtils.ConvertTo<ShakeDuration>(in args[1]), VariantUtils.ConvertTo<RumbleStyle>(in args[2]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ScreenShakeTrauma && args.Count == 1)
		{
			ScreenShakeTrauma(VariantUtils.ConvertTo<ShakeStrength>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DoHitStop && args.Count == 2)
		{
			DoHitStop(VariantUtils.ConvertTo<ShakeStrength>(in args[0]), VariantUtils.ConvertTo<ShakeDuration>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ToggleTrailerMode && args.Count == 0)
		{
			ToggleTrailerMode();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetScreenshakeMultiplier && args.Count == 1)
		{
			SetScreenshakeMultiplier(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.InitPools && args.Count == 0)
		{
			InitPools();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CheckShowLocalizationOverrideErrors && args.Count == 0)
		{
			CheckShowLocalizationOverrideErrors();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.LogResourceStats && args.Count == 1)
		{
			LogResourceStats(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FormatBytes && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(FormatBytes(VariantUtils.ConvertTo<ulong>(in args[0])));
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.IsMainThread && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(IsMainThread());
			return true;
		}
		if (method == MethodName.IsReleaseGame && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(IsReleaseGame());
			return true;
		}
		if (method == MethodName.ApplySyncSetting && args.Count == 0)
		{
			ApplySyncSetting();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Reset && args.Count == 0)
		{
			Reset();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ToggleTrailerMode && args.Count == 0)
		{
			ToggleTrailerMode();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.LogResourceStats && args.Count == 1)
		{
			LogResourceStats(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FormatBytes && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(FormatBytes(VariantUtils.ConvertTo<ulong>(in args[0])));
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.OnWindowChange)
		{
			return true;
		}
		if (method == MethodName.IsMainThread)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.IsReleaseGame)
		{
			return true;
		}
		if (method == MethodName.InitializeGraphicsPreferences)
		{
			return true;
		}
		if (method == MethodName.ApplyDisplaySettings)
		{
			return true;
		}
		if (method == MethodName.GetInspectRelicScreen)
		{
			return true;
		}
		if (method == MethodName.GetInspectCardScreen)
		{
			return true;
		}
		if (method == MethodName.ApplySyncSetting)
		{
			return true;
		}
		if (method == MethodName.Reset)
		{
			return true;
		}
		if (method == MethodName._Notification)
		{
			return true;
		}
		if (method == MethodName.Quit)
		{
			return true;
		}
		if (method == MethodName.Relocalize)
		{
			return true;
		}
		if (method == MethodName.ReloadMainMenu)
		{
			return true;
		}
		if (method == MethodName._Input)
		{
			return true;
		}
		if (method == MethodName.ToggleFullscreen)
		{
			return true;
		}
		if (method == MethodName.DebugModifyTimescale)
		{
			return true;
		}
		if (method == MethodName.ActivateWorldEnvironment)
		{
			return true;
		}
		if (method == MethodName.DeactivateWorldEnvironment)
		{
			return true;
		}
		if (method == MethodName.SetScreenShakeTarget)
		{
			return true;
		}
		if (method == MethodName.ClearScreenShakeTarget)
		{
			return true;
		}
		if (method == MethodName.ScreenShake)
		{
			return true;
		}
		if (method == MethodName.ScreenRumble)
		{
			return true;
		}
		if (method == MethodName.ScreenShakeTrauma)
		{
			return true;
		}
		if (method == MethodName.DoHitStop)
		{
			return true;
		}
		if (method == MethodName.ToggleTrailerMode)
		{
			return true;
		}
		if (method == MethodName.SetScreenshakeMultiplier)
		{
			return true;
		}
		if (method == MethodName.InitPools)
		{
			return true;
		}
		if (method == MethodName.CheckShowLocalizationOverrideErrors)
		{
			return true;
		}
		if (method == MethodName.LogResourceStats)
		{
			return true;
		}
		if (method == MethodName.FormatBytes)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.RootSceneContainer)
		{
			RootSceneContainer = VariantUtils.ConvertTo<NSceneContainer>(in value);
			return true;
		}
		if (name == PropertyName.HoverTipsContainer)
		{
			HoverTipsContainer = VariantUtils.ConvertTo<Node>(in value);
			return true;
		}
		if (name == PropertyName.Transition)
		{
			Transition = VariantUtils.ConvertTo<NTransition>(in value);
			return true;
		}
		if (name == PropertyName.TimeoutOverlay)
		{
			TimeoutOverlay = VariantUtils.ConvertTo<NMultiplayerTimeoutOverlay>(in value);
			return true;
		}
		if (name == PropertyName.AudioManager)
		{
			AudioManager = VariantUtils.ConvertTo<NAudioManager>(in value);
			return true;
		}
		if (name == PropertyName.RemoteCursorContainer)
		{
			RemoteCursorContainer = VariantUtils.ConvertTo<NRemoteMouseCursorContainer>(in value);
			return true;
		}
		if (name == PropertyName.InputManager)
		{
			InputManager = VariantUtils.ConvertTo<NInputManager>(in value);
			return true;
		}
		if (name == PropertyName.HotkeyManager)
		{
			HotkeyManager = VariantUtils.ConvertTo<NHotkeyManager>(in value);
			return true;
		}
		if (name == PropertyName.ReactionWheel)
		{
			ReactionWheel = VariantUtils.ConvertTo<NReactionWheel>(in value);
			return true;
		}
		if (name == PropertyName.ReactionContainer)
		{
			ReactionContainer = VariantUtils.ConvertTo<NReactionContainer>(in value);
			return true;
		}
		if (name == PropertyName.CursorManager)
		{
			CursorManager = VariantUtils.ConvertTo<NCursorManager>(in value);
			return true;
		}
		if (name == PropertyName.DebugAudio)
		{
			DebugAudio = VariantUtils.ConvertTo<NDebugAudioManager>(in value);
			return true;
		}
		if (name == PropertyName.DebugSeedOverride)
		{
			DebugSeedOverride = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName.StartOnMainMenu)
		{
			StartOnMainMenu = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.InspectRelicScreen)
		{
			InspectRelicScreen = VariantUtils.ConvertTo<NInspectRelicScreen>(in value);
			return true;
		}
		if (name == PropertyName.InspectCardScreen)
		{
			InspectCardScreen = VariantUtils.ConvertTo<NInspectCardScreen>(in value);
			return true;
		}
		if (name == PropertyName.FeedbackScreen)
		{
			FeedbackScreen = VariantUtils.ConvertTo<NSendFeedbackScreen>(in value);
			return true;
		}
		if (name == PropertyName.WorldEnvironment)
		{
			WorldEnvironment = VariantUtils.ConvertTo<WorldEnvironment>(in value);
			return true;
		}
		if (name == PropertyName.HitStop)
		{
			HitStop = VariantUtils.ConvertTo<NHitStop>(in value);
			return true;
		}
		if (name == PropertyName._inspectionContainer)
		{
			_inspectionContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._screenShake)
		{
			_screenShake = VariantUtils.ConvertTo<NScreenShake>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.RootSceneContainer)
		{
			value = VariantUtils.CreateFrom<NSceneContainer>(RootSceneContainer);
			return true;
		}
		if (name == PropertyName.HoverTipsContainer)
		{
			value = VariantUtils.CreateFrom<Node>(HoverTipsContainer);
			return true;
		}
		if (name == PropertyName.MainMenu)
		{
			value = VariantUtils.CreateFrom<NMainMenu>(MainMenu);
			return true;
		}
		if (name == PropertyName.CurrentRunNode)
		{
			value = VariantUtils.CreateFrom<NRun>(CurrentRunNode);
			return true;
		}
		if (name == PropertyName.LogoAnimation)
		{
			value = VariantUtils.CreateFrom<NLogoAnimation>(LogoAnimation);
			return true;
		}
		if (name == PropertyName.Transition)
		{
			value = VariantUtils.CreateFrom<NTransition>(Transition);
			return true;
		}
		if (name == PropertyName.TimeoutOverlay)
		{
			value = VariantUtils.CreateFrom<NMultiplayerTimeoutOverlay>(TimeoutOverlay);
			return true;
		}
		if (name == PropertyName.AudioManager)
		{
			value = VariantUtils.CreateFrom<NAudioManager>(AudioManager);
			return true;
		}
		if (name == PropertyName.RemoteCursorContainer)
		{
			value = VariantUtils.CreateFrom<NRemoteMouseCursorContainer>(RemoteCursorContainer);
			return true;
		}
		if (name == PropertyName.InputManager)
		{
			value = VariantUtils.CreateFrom<NInputManager>(InputManager);
			return true;
		}
		if (name == PropertyName.HotkeyManager)
		{
			value = VariantUtils.CreateFrom<NHotkeyManager>(HotkeyManager);
			return true;
		}
		if (name == PropertyName.ReactionWheel)
		{
			value = VariantUtils.CreateFrom<NReactionWheel>(ReactionWheel);
			return true;
		}
		if (name == PropertyName.ReactionContainer)
		{
			value = VariantUtils.CreateFrom<NReactionContainer>(ReactionContainer);
			return true;
		}
		if (name == PropertyName.CursorManager)
		{
			value = VariantUtils.CreateFrom<NCursorManager>(CursorManager);
			return true;
		}
		if (name == PropertyName.DebugAudio)
		{
			value = VariantUtils.CreateFrom<NDebugAudioManager>(DebugAudio);
			return true;
		}
		if (name == PropertyName.DebugSeedOverride)
		{
			value = VariantUtils.CreateFrom<string>(DebugSeedOverride);
			return true;
		}
		if (name == PropertyName.StartOnMainMenu)
		{
			value = VariantUtils.CreateFrom<bool>(StartOnMainMenu);
			return true;
		}
		if (name == PropertyName.InspectRelicScreen)
		{
			value = VariantUtils.CreateFrom<NInspectRelicScreen>(InspectRelicScreen);
			return true;
		}
		if (name == PropertyName.InspectCardScreen)
		{
			value = VariantUtils.CreateFrom<NInspectCardScreen>(InspectCardScreen);
			return true;
		}
		if (name == PropertyName.FeedbackScreen)
		{
			value = VariantUtils.CreateFrom<NSendFeedbackScreen>(FeedbackScreen);
			return true;
		}
		if (name == PropertyName.WorldEnvironment)
		{
			value = VariantUtils.CreateFrom<WorldEnvironment>(WorldEnvironment);
			return true;
		}
		if (name == PropertyName.HitStop)
		{
			value = VariantUtils.CreateFrom<NHitStop>(HitStop);
			return true;
		}
		if (name == PropertyName._inspectionContainer)
		{
			value = VariantUtils.CreateFrom(in _inspectionContainer);
			return true;
		}
		if (name == PropertyName._screenShake)
		{
			value = VariantUtils.CreateFrom(in _screenShake);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.RootSceneContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.HoverTipsContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.MainMenu, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.CurrentRunNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.LogoAnimation, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Transition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.TimeoutOverlay, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.AudioManager, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.RemoteCursorContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.InputManager, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.HotkeyManager, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.ReactionWheel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.ReactionContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.CursorManager, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DebugAudio, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName.DebugSeedOverride, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.StartOnMainMenu, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.InspectRelicScreen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.InspectCardScreen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.FeedbackScreen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.WorldEnvironment, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.HitStop, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._inspectionContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._screenShake, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.RootSceneContainer, Variant.From<NSceneContainer>(RootSceneContainer));
		info.AddProperty(PropertyName.HoverTipsContainer, Variant.From<Node>(HoverTipsContainer));
		info.AddProperty(PropertyName.Transition, Variant.From<NTransition>(Transition));
		info.AddProperty(PropertyName.TimeoutOverlay, Variant.From<NMultiplayerTimeoutOverlay>(TimeoutOverlay));
		info.AddProperty(PropertyName.AudioManager, Variant.From<NAudioManager>(AudioManager));
		info.AddProperty(PropertyName.RemoteCursorContainer, Variant.From<NRemoteMouseCursorContainer>(RemoteCursorContainer));
		info.AddProperty(PropertyName.InputManager, Variant.From<NInputManager>(InputManager));
		info.AddProperty(PropertyName.HotkeyManager, Variant.From<NHotkeyManager>(HotkeyManager));
		info.AddProperty(PropertyName.ReactionWheel, Variant.From<NReactionWheel>(ReactionWheel));
		info.AddProperty(PropertyName.ReactionContainer, Variant.From<NReactionContainer>(ReactionContainer));
		info.AddProperty(PropertyName.CursorManager, Variant.From<NCursorManager>(CursorManager));
		info.AddProperty(PropertyName.DebugAudio, Variant.From<NDebugAudioManager>(DebugAudio));
		info.AddProperty(PropertyName.DebugSeedOverride, Variant.From<string>(DebugSeedOverride));
		info.AddProperty(PropertyName.StartOnMainMenu, Variant.From<bool>(StartOnMainMenu));
		info.AddProperty(PropertyName.InspectRelicScreen, Variant.From<NInspectRelicScreen>(InspectRelicScreen));
		info.AddProperty(PropertyName.InspectCardScreen, Variant.From<NInspectCardScreen>(InspectCardScreen));
		info.AddProperty(PropertyName.FeedbackScreen, Variant.From<NSendFeedbackScreen>(FeedbackScreen));
		info.AddProperty(PropertyName.WorldEnvironment, Variant.From<WorldEnvironment>(WorldEnvironment));
		info.AddProperty(PropertyName.HitStop, Variant.From<NHitStop>(HitStop));
		info.AddProperty(PropertyName._inspectionContainer, Variant.From(in _inspectionContainer));
		info.AddProperty(PropertyName._screenShake, Variant.From(in _screenShake));
		info.AddSignalEventDelegate(SignalName.WindowChange, backing_WindowChange);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.RootSceneContainer, out var value))
		{
			RootSceneContainer = value.As<NSceneContainer>();
		}
		if (info.TryGetProperty(PropertyName.HoverTipsContainer, out var value2))
		{
			HoverTipsContainer = value2.As<Node>();
		}
		if (info.TryGetProperty(PropertyName.Transition, out var value3))
		{
			Transition = value3.As<NTransition>();
		}
		if (info.TryGetProperty(PropertyName.TimeoutOverlay, out var value4))
		{
			TimeoutOverlay = value4.As<NMultiplayerTimeoutOverlay>();
		}
		if (info.TryGetProperty(PropertyName.AudioManager, out var value5))
		{
			AudioManager = value5.As<NAudioManager>();
		}
		if (info.TryGetProperty(PropertyName.RemoteCursorContainer, out var value6))
		{
			RemoteCursorContainer = value6.As<NRemoteMouseCursorContainer>();
		}
		if (info.TryGetProperty(PropertyName.InputManager, out var value7))
		{
			InputManager = value7.As<NInputManager>();
		}
		if (info.TryGetProperty(PropertyName.HotkeyManager, out var value8))
		{
			HotkeyManager = value8.As<NHotkeyManager>();
		}
		if (info.TryGetProperty(PropertyName.ReactionWheel, out var value9))
		{
			ReactionWheel = value9.As<NReactionWheel>();
		}
		if (info.TryGetProperty(PropertyName.ReactionContainer, out var value10))
		{
			ReactionContainer = value10.As<NReactionContainer>();
		}
		if (info.TryGetProperty(PropertyName.CursorManager, out var value11))
		{
			CursorManager = value11.As<NCursorManager>();
		}
		if (info.TryGetProperty(PropertyName.DebugAudio, out var value12))
		{
			DebugAudio = value12.As<NDebugAudioManager>();
		}
		if (info.TryGetProperty(PropertyName.DebugSeedOverride, out var value13))
		{
			DebugSeedOverride = value13.As<string>();
		}
		if (info.TryGetProperty(PropertyName.StartOnMainMenu, out var value14))
		{
			StartOnMainMenu = value14.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.InspectRelicScreen, out var value15))
		{
			InspectRelicScreen = value15.As<NInspectRelicScreen>();
		}
		if (info.TryGetProperty(PropertyName.InspectCardScreen, out var value16))
		{
			InspectCardScreen = value16.As<NInspectCardScreen>();
		}
		if (info.TryGetProperty(PropertyName.FeedbackScreen, out var value17))
		{
			FeedbackScreen = value17.As<NSendFeedbackScreen>();
		}
		if (info.TryGetProperty(PropertyName.WorldEnvironment, out var value18))
		{
			WorldEnvironment = value18.As<WorldEnvironment>();
		}
		if (info.TryGetProperty(PropertyName.HitStop, out var value19))
		{
			HitStop = value19.As<NHitStop>();
		}
		if (info.TryGetProperty(PropertyName._inspectionContainer, out var value20))
		{
			_inspectionContainer = value20.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._screenShake, out var value21))
		{
			_screenShake = value21.As<NScreenShake>();
		}
		if (info.TryGetSignalEventDelegate<WindowChangeEventHandler>(SignalName.WindowChange, out var value22))
		{
			backing_WindowChange = value22;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotSignalList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(SignalName.WindowChange, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	protected void EmitSignalWindowChange()
	{
		EmitSignal(SignalName.WindowChange);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.WindowChange && args.Count == 0)
		{
			backing_WindowChange?.Invoke();
		}
		else
		{
			base.RaiseGodotClassSignalCallbacks(in signal, args);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassSignal(in godot_string_name signal)
	{
		if (signal == SignalName.WindowChange)
		{
			return true;
		}
		return base.HasGodotClassSignal(in signal);
	}
}
