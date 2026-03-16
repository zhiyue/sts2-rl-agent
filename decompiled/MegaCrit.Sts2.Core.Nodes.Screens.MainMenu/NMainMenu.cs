using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Daily;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Connection;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Nodes.Screens.ModdingScreen;
using MegaCrit.Sts2.Core.Nodes.Screens.ProfileScreen;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Nodes.Screens.Settings;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Platform.Steam;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using MegaCrit.Sts2.Core.Timeline.Epochs;

namespace MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

[ScriptPath("res://src/Core/Nodes/Screens/MainMenu/NMainMenu.cs")]
public class NMainMenu : Control, IScreenContext
{
	public new class MethodName : Control.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName ConnectMainMenuTextButtonFocusLogic = "ConnectMainMenuTextButtonFocusLogic";

		public static readonly StringName MainMenuButtonFocused = "MainMenuButtonFocused";

		public static readonly StringName MainMenuButtonUnfocused = "MainMenuButtonUnfocused";

		public static readonly StringName EnableBackstop = "EnableBackstop";

		public static readonly StringName DisableBackstop = "DisableBackstop";

		public static readonly StringName DisableBackstopInstantly = "DisableBackstopInstantly";

		public static readonly StringName EnableBackstopInstantly = "EnableBackstopInstantly";

		public static readonly StringName UpdateShaderMix = "UpdateShaderMix";

		public static readonly StringName UpdateShaderLod = "UpdateShaderLod";

		public static readonly StringName RefreshButtons = "RefreshButtons";

		public static readonly StringName UpdateTimelineButtonBehavior = "UpdateTimelineButtonBehavior";

		public static readonly StringName OnSubmenuStackChanged = "OnSubmenuStackChanged";

		public static readonly StringName OnContinueButtonPressed = "OnContinueButtonPressed";

		public static readonly StringName DisplayLoadSaveError = "DisplayLoadSaveError";

		public static readonly StringName OnAbandonRunButtonPressed = "OnAbandonRunButtonPressed";

		public static readonly StringName AbandonRun = "AbandonRun";

		public static readonly StringName SingleplayerButtonPressed = "SingleplayerButtonPressed";

		public static readonly StringName OpenSingleplayerSubmenu = "OpenSingleplayerSubmenu";

		public static readonly StringName OpenMultiplayerSubmenu = "OpenMultiplayerSubmenu";

		public static readonly StringName OpenCompendiumSubmenu = "OpenCompendiumSubmenu";

		public static readonly StringName OpenTimelineScreen = "OpenTimelineScreen";

		public static readonly StringName OpenSettingsMenu = "OpenSettingsMenu";

		public static readonly StringName OpenProfileScreen = "OpenProfileScreen";

		public static readonly StringName OpenPatchNotes = "OpenPatchNotes";

		public static readonly StringName Quit = "Quit";

		public static readonly StringName OnWindowChange = "OnWindowChange";

		public static readonly StringName CheckCommandLineArgs = "CheckCommandLineArgs";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName PatchNotesScreen = "PatchNotesScreen";

		public static readonly StringName BlurBackstop = "BlurBackstop";

		public static readonly StringName MainMenuButtons = "MainMenuButtons";

		public static readonly StringName SubmenuStack = "SubmenuStack";

		public static readonly StringName ContinueRunInfo = "ContinueRunInfo";

		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public static readonly StringName _window = "_window";

		public static readonly StringName _continueButton = "_continueButton";

		public static readonly StringName _abandonRunButton = "_abandonRunButton";

		public static readonly StringName _singleplayerButton = "_singleplayerButton";

		public static readonly StringName _compendiumButton = "_compendiumButton";

		public static readonly StringName _timelineButton = "_timelineButton";

		public static readonly StringName _settingsButton = "_settingsButton";

		public static readonly StringName _quitButton = "_quitButton";

		public static readonly StringName _multiplayerButton = "_multiplayerButton";

		public static readonly StringName _buttonReticleLeft = "_buttonReticleLeft";

		public static readonly StringName _buttonReticleRight = "_buttonReticleRight";

		public static readonly StringName _reticleTween = "_reticleTween";

		public static readonly StringName _patchNotesButtonNode = "_patchNotesButtonNode";

		public static readonly StringName _openProfileScreenButton = "_openProfileScreenButton";

		public static readonly StringName _lastHitButton = "_lastHitButton";

		public static readonly StringName _runInfo = "_runInfo";

		public static readonly StringName _timelineNotificationDot = "_timelineNotificationDot";

		public static readonly StringName _backstopTween = "_backstopTween";

		public static readonly StringName _bg = "_bg";

		public static readonly StringName _blur = "_blur";

		public static readonly StringName _openTimeline = "_openTimeline";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private static readonly StringName _lod = new StringName("lod");

	private static readonly StringName _mixPercentage = new StringName("mix_percentage");

	private const string _scenePath = "res://scenes/screens/main_menu.tscn";

	private const string _menuMusicParam = "menu_progress";

	private Window _window;

	private NMainMenuTextButton _continueButton;

	private NMainMenuTextButton _abandonRunButton;

	private NMainMenuTextButton _singleplayerButton;

	private NMainMenuTextButton _compendiumButton;

	private NMainMenuTextButton _timelineButton;

	private NMainMenuTextButton _settingsButton;

	private NMainMenuTextButton _quitButton;

	private NMainMenuTextButton _multiplayerButton;

	private const float _reticleYOffset = 5f;

	private const float _reticlePadding = 28f;

	private Control _buttonReticleLeft;

	private Control _buttonReticleRight;

	private Tween? _reticleTween;

	private NPatchNotesButton _patchNotesButtonNode;

	private NOpenProfileScreenButton _openProfileScreenButton;

	private NMainMenuTextButton? _lastHitButton;

	private NContinueRunInfo _runInfo;

	private Control _timelineNotificationDot;

	private Tween? _backstopTween;

	private NMainMenuBg _bg;

	private ShaderMaterial _blur;

	private bool _openTimeline;

	private ReadSaveResult<SerializableRun>? _readRunSaveResult;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>("res://scenes/screens/main_menu.tscn");

	public NPatchNotesScreen PatchNotesScreen { get; private set; }

	public Control BlurBackstop { get; private set; }

	private NButton[] MainMenuButtons => new NButton[8] { _continueButton, _abandonRunButton, _singleplayerButton, _multiplayerButton, _timelineButton, _settingsButton, _compendiumButton, _quitButton };

	public NMainMenuSubmenuStack SubmenuStack { get; private set; }

	public NContinueRunInfo ContinueRunInfo => _runInfo;

	public Control DefaultFocusedControl
	{
		get
		{
			if (_lastHitButton == null || !_lastHitButton.IsVisible())
			{
				return MainMenuButtons.First((NButton b) => b.IsEnabled && b.IsVisible());
			}
			return _lastHitButton;
		}
	}

	public static NMainMenu Create(bool openTimeline)
	{
		NMainMenu nMainMenu = PreloadManager.Cache.GetScene("res://scenes/screens/main_menu.tscn").Instantiate<NMainMenu>(PackedScene.GenEditState.Disabled);
		nMainMenu._openTimeline = openTimeline;
		return nMainMenu;
	}

	public override void _Ready()
	{
		Log.Info($"[Startup] Time to main menu (Godot ticks): {Time.GetTicksMsec()}ms");
		_window = GetTree().Root;
		NGame.Instance.Connect(NGame.SignalName.WindowChange, Callable.From<bool>(OnWindowChange));
		if (SaveManager.Instance.SettingsSave.AspectRatioSetting == AspectRatioSetting.Auto)
		{
			OnWindowChange(isAspectRatioAuto: true);
		}
		_continueButton = GetNode<NMainMenuTextButton>("MainMenuTextButtons/ContinueButton");
		_continueButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnContinueButtonPressed));
		_continueButton.SetLocalization("CONTINUE");
		_abandonRunButton = GetNode<NMainMenuTextButton>("MainMenuTextButtons/AbandonRunButton");
		_abandonRunButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnAbandonRunButtonPressed));
		_abandonRunButton.SetLocalization("ABANDON_RUN");
		_singleplayerButton = GetNode<NMainMenuTextButton>("MainMenuTextButtons/SingleplayerButton");
		_singleplayerButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(SingleplayerButtonPressed));
		_singleplayerButton.SetLocalization("SINGLE_PLAYER");
		_multiplayerButton = GetNode<NMainMenuTextButton>("MainMenuTextButtons/MultiplayerButton");
		_multiplayerButton.Connect(NClickableControl.SignalName.Released, Callable.From((Action<NButton>)OpenMultiplayerSubmenu));
		_multiplayerButton.SetLocalization("MULTIPLAYER");
		_compendiumButton = GetNode<NMainMenuTextButton>("MainMenuTextButtons/CompendiumButton");
		_compendiumButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OpenCompendiumSubmenu));
		_compendiumButton.SetLocalization("COMPENDIUM");
		_timelineButton = GetNode<NMainMenuTextButton>("MainMenuTextButtons/TimelineButton");
		_timelineButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OpenTimelineScreen));
		_timelineButton.SetLocalization("TIMELINE");
		_timelineNotificationDot = GetNode<Control>("%TimelineNotificationDot");
		_timelineNotificationDot.Visible = SaveManager.Instance.GetDiscoveredEpochCount() > 0;
		_settingsButton = GetNode<NMainMenuTextButton>("MainMenuTextButtons/SettingsButton");
		_settingsButton.Connect(NClickableControl.SignalName.Released, Callable.From((Action<NButton>)OpenSettingsMenu));
		_settingsButton.SetLocalization("SETTINGS");
		_quitButton = GetNode<NMainMenuTextButton>("MainMenuTextButtons/QuitButton");
		_quitButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(Quit));
		_quitButton.SetLocalization("QUIT");
		_buttonReticleLeft = GetNode<Control>("%ButtonReticleLeft");
		_buttonReticleRight = GetNode<Control>("%ButtonReticleRight");
		ConnectMainMenuTextButtonFocusLogic();
		PatchNotesScreen = GetNode<NPatchNotesScreen>("%PatchNotesScreen");
		SubmenuStack = GetNode<NMainMenuSubmenuStack>("%Submenus");
		_runInfo = GetNode<NContinueRunInfo>("%ContinueRunInfo");
		_patchNotesButtonNode = GetNode<NPatchNotesButton>("%PatchNotesButton");
		_patchNotesButtonNode.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OpenPatchNotes));
		_openProfileScreenButton = GetNode<NOpenProfileScreenButton>("%ChangeProfileButton");
		_bg = GetNode<NMainMenuBg>("%MainMenuBg");
		BlurBackstop = GetNode<Control>("BlurBackstop");
		_blur = (ShaderMaterial)BlurBackstop.Material;
		_timelineButton.Visible = SaveManager.Instance.Progress.Epochs.Count > 0;
		NGame.Instance.SetScreenShakeTarget(this);
		NAudioManager.Instance?.PlayMusic("event:/music/menu_update");
		SubmenuStack.InitializeForMainMenu(this);
		SubmenuStack.Connect(NSubmenuStack.SignalName.StackModified, Callable.From(OnSubmenuStackChanged));
		OnSubmenuStackChanged();
		ActiveScreenContext.Instance.Update();
		RefreshButtons();
		CheckCommandLineArgs();
		if (SaveManager.Instance.SettingsSave.ModSettings == null && ModManager.AllMods.Count > 0)
		{
			NModalContainer.Instance.Add(NConfirmModLoadingPopup.Create());
		}
		TaskHelper.RunSafely(NGame.Instance.Transition.FadeIn(3f));
		PlatformUtil.SetRichPresence("MAIN_MENU", null, null);
		if (_openTimeline)
		{
			TaskHelper.RunSafely(OpenTimelineFromGameOverScreen());
		}
		if (NGame.IsReleaseGame() && !SaveManager.Instance.SettingsSave.SeenEaDisclaimer)
		{
			NModalContainer.Instance.Add(NEarlyAccessDisclaimer.Create());
		}
	}

	private void ConnectMainMenuTextButtonFocusLogic()
	{
		foreach (NMainMenuTextButton item in GetNode<Control>("%MainMenuTextButtons").GetChildren().OfType<NMainMenuTextButton>())
		{
			item.Connect(NClickableControl.SignalName.Unfocused, Callable.From<NMainMenuTextButton>(MainMenuButtonUnfocused));
			item.Connect(NClickableControl.SignalName.Focused, Callable.From(delegate(NMainMenuTextButton b)
			{
				Callable.From(delegate
				{
					MainMenuButtonFocused(b);
				}).CallDeferred();
			}));
		}
	}

	private void MainMenuButtonFocused(NMainMenuTextButton button)
	{
		_reticleTween?.Kill();
		_reticleTween = CreateTween().SetParallel();
		_buttonReticleLeft.GlobalPosition = new Vector2(0f, button.GlobalPosition.Y + 5f);
		_buttonReticleRight.GlobalPosition = new Vector2(0f, button.GlobalPosition.Y + 5f);
		float num = button.label?.GlobalPosition.X ?? 0f;
		float num2 = button.label?.Size.X ?? 0f;
		float num3 = num - 20f - 6f;
		float num4 = num + num2 - 20f + 6f;
		_reticleTween.TweenProperty(_buttonReticleLeft, "global_position:x", num3 - 28f, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back)
			.From(num3);
		_reticleTween.TweenProperty(_buttonReticleLeft, "modulate", StsColors.gold, 0.05).From(StsColors.transparentWhite);
		_reticleTween.TweenProperty(_buttonReticleRight, "global_position:x", num4 + 28f, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back)
			.From(num4);
		_reticleTween.TweenProperty(_buttonReticleRight, "modulate", StsColors.gold, 0.05).From(StsColors.transparentWhite);
	}

	private void MainMenuButtonUnfocused(NMainMenuTextButton obj)
	{
		_reticleTween?.Kill();
		_reticleTween = CreateTween().SetParallel();
		_reticleTween.TweenProperty(_buttonReticleLeft, "modulate", StsColors.transparentWhite, 0.25);
		_reticleTween.TweenProperty(_buttonReticleRight, "modulate", StsColors.transparentWhite, 0.25);
	}

	private async Task OpenTimelineFromGameOverScreen()
	{
		if (SaveManager.Instance.PrefsSave.FastMode != FastModeType.Instant)
		{
			await Task.Delay(500);
		}
		SubmenuStack.PushSubmenuType<NTimelineScreen>();
	}

	public void EnableBackstop()
	{
		_bg.HideLogo();
		BlurBackstop.MouseFilter = MouseFilterEnum.Stop;
		_backstopTween?.Kill();
		_backstopTween = CreateTween().SetParallel();
		_backstopTween.TweenMethod(Callable.From<float>(UpdateShaderLod), _blur.GetShaderParameter(_lod), 3f, 0.25);
		_backstopTween.TweenMethod(Callable.From<float>(UpdateShaderMix), _blur.GetShaderParameter(_mixPercentage), 0.7f, 0.25);
	}

	public void DisableBackstop()
	{
		_bg.ShowLogo();
		BlurBackstop.MouseFilter = MouseFilterEnum.Ignore;
		_backstopTween?.Kill();
		_backstopTween = CreateTween().SetParallel();
		_backstopTween.TweenMethod(Callable.From<float>(UpdateShaderLod), _blur.GetShaderParameter(_lod), 0f, 0.15);
		_backstopTween.TweenMethod(Callable.From<float>(UpdateShaderMix), _blur.GetShaderParameter(_mixPercentage), 0f, 0.15);
	}

	public void DisableBackstopInstantly()
	{
		_backstopTween?.Kill();
		_blur.SetShaderParameter(_lod, 0f);
		_blur.SetShaderParameter(_mixPercentage, 0f);
	}

	public void EnableBackstopInstantly()
	{
		_backstopTween?.Kill();
		_blur.SetShaderParameter(_lod, 3f);
		_blur.SetShaderParameter(_mixPercentage, 0.7f);
	}

	private void UpdateShaderMix(float obj)
	{
		_blur.SetShaderParameter(_mixPercentage, obj);
	}

	private void UpdateShaderLod(float obj)
	{
		_blur.SetShaderParameter(_lod, obj);
	}

	public void RefreshButtons()
	{
		if (SaveManager.Instance.HasRunSave)
		{
			_readRunSaveResult = SaveManager.Instance.LoadRunSave();
			_singleplayerButton.Visible = false;
			_abandonRunButton.Visible = true;
			_continueButton.Visible = true;
			_continueButton.SetEnabled(enabled: true);
			_runInfo.SetResult(_readRunSaveResult);
		}
		else
		{
			_readRunSaveResult = null;
			_singleplayerButton.Visible = true;
			_abandonRunButton.Visible = false;
			_continueButton.Visible = false;
			_continueButton.SetEnabled(enabled: false);
			_runInfo.SetResult(null);
		}
		UpdateTimelineButtonBehavior();
		_compendiumButton.Visible = SaveManager.Instance.IsCompendiumAvailable();
		ActiveScreenContext.Instance.Update();
	}

	private void UpdateTimelineButtonBehavior()
	{
		if (!DebugSettings.DevSkip && SaveManager.Instance.GetDiscoveredEpochCount() > 0 && !SaveManager.Instance.HasRunSave)
		{
			_timelineButton.Enable();
			_singleplayerButton.Disable();
			_multiplayerButton.Disable();
			_compendiumButton.Disable();
			_timelineButton.Visible = true;
			_timelineNotificationDot.Visible = true;
		}
		else if (SaveManager.Instance.Progress.Epochs.Count > 1 && SaveManager.Instance.IsEpochRevealed<NeowEpoch>())
		{
			_timelineButton.Visible = true;
			if (SaveManager.Instance.GetDiscoveredEpochCount() == 0)
			{
				_timelineButton.Enable();
			}
			else
			{
				_timelineButton.Disable();
			}
			_timelineNotificationDot.Visible = false;
			_singleplayerButton.Enable();
			_multiplayerButton.Enable();
			_compendiumButton.Enable();
		}
		else if (SaveManager.Instance.Progress.Epochs.Count > 1)
		{
			_timelineButton.Disable();
		}
		else
		{
			_timelineButton.Visible = false;
		}
	}

	private void OnSubmenuStackChanged()
	{
		GetNode<Control>("MainMenuTextButtons").Visible = !SubmenuStack.SubmenusOpen;
		_patchNotesButtonNode.Visible = !SubmenuStack.SubmenusOpen;
		if (SubmenuStack.SubmenusOpen)
		{
			_openProfileScreenButton.Visible = false;
			_patchNotesButtonNode.Visible = false;
			_openProfileScreenButton.Disable();
			_patchNotesButtonNode.Disable();
		}
		else
		{
			_openProfileScreenButton.Visible = true;
			_patchNotesButtonNode.Visible = true;
			_openProfileScreenButton.Enable();
			_patchNotesButtonNode.Enable();
			NAudioManager.Instance?.UpdateMusicParameter("menu_progress", "main");
		}
	}

	private void OnContinueButtonPressed(NButton _)
	{
		if (_readRunSaveResult == null || !_readRunSaveResult.Success || _readRunSaveResult.SaveData == null)
		{
			DisplayLoadSaveError();
		}
		else
		{
			TaskHelper.RunSafely(OnContinueButtonPressedAsync());
		}
	}

	private async Task OnContinueButtonPressedAsync()
	{
		_ = 2;
		try
		{
			_continueButton.Disable();
			NAudioManager.Instance?.StopMusic();
			SerializableRun serializableRun = _readRunSaveResult.SaveData;
			RunState runState = RunState.FromSerializable(serializableRun);
			RunManager.Instance.SetUpSavedSinglePlayer(runState, serializableRun);
			Log.Info($"Continuing run with character: {serializableRun.Players[0].CharacterId}");
			SfxCmd.Play(runState.Players[0].Character.CharacterTransitionSfx);
			await NGame.Instance.Transition.FadeOut(0.8f, runState.Players[0].Character.CharacterSelectTransitionPath);
			NGame.Instance.ReactionContainer.InitializeNetworking(new NetSingleplayerGameService());
			await NGame.Instance.LoadRun(runState, serializableRun.PreFinishedRoom);
			await NGame.Instance.Transition.FadeIn();
		}
		catch (Exception)
		{
			DisplayLoadSaveError();
			throw;
		}
	}

	private void DisplayLoadSaveError()
	{
		NErrorPopup modalToCreate = NErrorPopup.Create(new LocString("main_menu_ui", "INVALID_SAVE_POPUP.title"), new LocString("main_menu_ui", "INVALID_SAVE_POPUP.description_run"), new LocString("main_menu_ui", "INVALID_SAVE_POPUP.dismiss"), showReportBugButton: true);
		NModalContainer.Instance.Add(modalToCreate);
		NModalContainer.Instance.ShowBackstop();
		_continueButton.Disable();
	}

	private void OnAbandonRunButtonPressed(NButton _)
	{
		NModalContainer.Instance.Add(NAbandonRunConfirmPopup.Create(this));
		_lastHitButton = _abandonRunButton;
	}

	public void AbandonRun()
	{
		if (_readRunSaveResult == null)
		{
			return;
		}
		if (_readRunSaveResult.Success && _readRunSaveResult.SaveData != null)
		{
			try
			{
				Log.Info("Abandoning run from main menu");
				SerializableRun saveData = _readRunSaveResult.SaveData;
				SaveManager.Instance.UpdateProgressWithRunData(saveData, victory: false);
				RunHistoryUtilities.CreateRunHistoryEntry(saveData, victory: false, isAbandoned: true, saveData.PlatformType);
				if (saveData.DailyTime.HasValue)
				{
					int score = ScoreUtility.CalculateScore(saveData, won: false);
					TaskHelper.RunSafely(DailyRunUtility.UploadScore(saveData.DailyTime.Value, score, saveData.Players));
				}
			}
			catch (Exception value)
			{
				Log.Error($"ERROR: Failed to upload run history/metrics: {value}");
			}
		}
		else
		{
			Log.Info($"Abandoning run with invalid save (status={_readRunSaveResult.Status})");
		}
		SaveManager.Instance.DeleteCurrentRun();
		RefreshButtons();
		GC.Collect();
	}

	private void SingleplayerButtonPressed(NButton _)
	{
		if (SaveManager.Instance.Progress.NumberOfRuns > 0)
		{
			OpenSingleplayerSubmenu();
			return;
		}
		NCharacterSelectScreen submenuType = SubmenuStack.GetSubmenuType<NCharacterSelectScreen>();
		submenuType.InitializeSingleplayer();
		SubmenuStack.Push(submenuType);
		_lastHitButton = _singleplayerButton;
	}

	public NSingleplayerSubmenu OpenSingleplayerSubmenu()
	{
		_lastHitButton = _singleplayerButton;
		return SubmenuStack.PushSubmenuType<NSingleplayerSubmenu>();
	}

	public void OpenMultiplayerSubmenu(NButton _)
	{
		OpenMultiplayerSubmenu();
	}

	public NMultiplayerSubmenu OpenMultiplayerSubmenu()
	{
		_lastHitButton = _multiplayerButton;
		return SubmenuStack.PushSubmenuType<NMultiplayerSubmenu>();
	}

	private void OpenCompendiumSubmenu(NButton _)
	{
		_lastHitButton = _compendiumButton;
		SubmenuStack.PushSubmenuType<NCompendiumSubmenu>();
	}

	private void OpenTimelineScreen(NButton obj)
	{
		_lastHitButton = _timelineButton;
		NAudioManager.Instance?.UpdateMusicParameter("menu_progress", "timeline");
		SubmenuStack.PushSubmenuType<NTimelineScreen>();
	}

	private void OpenSettingsMenu(NButton _)
	{
		OpenSettingsMenu();
	}

	public void OpenProfileScreen()
	{
		SubmenuStack.PushSubmenuType<NProfileScreen>();
	}

	public void OpenSettingsMenu()
	{
		_lastHitButton = _settingsButton;
		SubmenuStack.PushSubmenuType<NSettingsScreen>();
	}

	private void OpenPatchNotes(NButton _)
	{
		PatchNotesScreen.Open();
	}

	public async Task JoinGame(IClientConnectionInitializer connInitializer)
	{
		NMultiplayerSubmenu nMultiplayerSubmenu = OpenMultiplayerSubmenu();
		NJoinFriendScreen nJoinFriendScreen = nMultiplayerSubmenu.OnJoinFriendsPressed();
		await nJoinFriendScreen.JoinGameAsync(connInitializer);
	}

	private static void Quit(NButton _)
	{
		Log.Info("Quit button pressed");
		TaskHelper.RunSafely(ConfirmAndQuit());
	}

	private static async Task ConfirmAndQuit()
	{
		NGenericPopup nGenericPopup = NGenericPopup.Create();
		NModalContainer.Instance.Add(nGenericPopup);
		if (await nGenericPopup.WaitForConfirmation(new LocString("main_menu_ui", "QUIT_CONFIRM_POPUP.body"), new LocString("main_menu_ui", "QUIT_CONFIRM_POPUP.header"), new LocString("main_menu_ui", "GENERIC_POPUP.cancel"), new LocString("main_menu_ui", "GENERIC_POPUP.confirm")))
		{
			Log.Info("Quit confirmed");
			NGame.Instance?.Quit();
		}
		else
		{
			Log.Info("Quit cancelled");
		}
	}

	private void OnWindowChange(bool isAspectRatioAuto)
	{
		if (isAspectRatioAuto)
		{
			float num = (float)_window.Size.X / (float)_window.Size.Y;
			if (num > 2.3888888f)
			{
				_window.ContentScaleAspect = Window.ContentScaleAspectEnum.KeepWidth;
				_window.ContentScaleSize = new Vector2I(2580, 1080);
			}
			else if (num < 1.3333334f)
			{
				_window.ContentScaleAspect = Window.ContentScaleAspectEnum.KeepHeight;
				_window.ContentScaleSize = new Vector2I(1680, 1260);
			}
			else
			{
				_window.ContentScaleAspect = Window.ContentScaleAspectEnum.Expand;
				_window.ContentScaleSize = new Vector2I(1680, 1080);
			}
		}
	}

	private void CheckCommandLineArgs()
	{
		if (!CommandLineHelper.TryGetValue("fastmp", out string value))
		{
			return;
		}
		NMultiplayerSubmenu nMultiplayerSubmenu = OpenMultiplayerSubmenu();
		switch (value)
		{
		case "host":
		case "host_standard":
		case "host_daily":
		case "host_custom":
		{
			GameMode gameMode = value switch
			{
				"host_standard" => GameMode.Standard, 
				"host_daily" => GameMode.Daily, 
				"host_custom" => GameMode.Custom, 
				_ => GameMode.None, 
			};
			if (gameMode != GameMode.None)
			{
				nMultiplayerSubmenu.FastHost(gameMode);
			}
			break;
		}
		case "load":
		{
			PlatformType platformType = (SteamInitializer.Initialized ? PlatformType.Steam : PlatformType.None);
			ulong localPlayerId = PlatformUtil.GetLocalPlayerId(platformType);
			ReadSaveResult<SerializableRun> readSaveResult = SaveManager.Instance.LoadAndCanonicalizeMultiplayerRunSave(localPlayerId);
			if (readSaveResult.SaveData != null)
			{
				nMultiplayerSubmenu.StartHost(readSaveResult.SaveData);
			}
			else
			{
				Log.Error("Failed to load multiplayer save");
			}
			break;
		}
		case "join":
			nMultiplayerSubmenu.OnJoinFriendsPressed();
			break;
		default:
			Log.Error("fastmp command line argument passed with invalid value: " + value + ". Expected host, load, or join");
			break;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(31);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "openTimeline", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ConnectMainMenuTextButtonFocusLogic, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.MainMenuButtonFocused, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "button", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.MainMenuButtonUnfocused, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "obj", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.EnableBackstop, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DisableBackstop, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DisableBackstopInstantly, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.EnableBackstopInstantly, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateShaderMix, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "obj", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateShaderLod, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "obj", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RefreshButtons, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateTimelineButtonBehavior, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuStackChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnContinueButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.DisplayLoadSaveError, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnAbandonRunButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AbandonRun, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SingleplayerButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OpenSingleplayerSubmenu, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OpenMultiplayerSubmenu, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OpenMultiplayerSubmenu, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OpenCompendiumSubmenu, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OpenTimelineScreen, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "obj", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OpenSettingsMenu, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OpenProfileScreen, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OpenSettingsMenu, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OpenPatchNotes, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.Quit, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnWindowChange, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "isAspectRatioAuto", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.CheckCommandLineArgs, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NMainMenu>(Create(VariantUtils.ConvertTo<bool>(in args[0])));
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ConnectMainMenuTextButtonFocusLogic && args.Count == 0)
		{
			ConnectMainMenuTextButtonFocusLogic();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.MainMenuButtonFocused && args.Count == 1)
		{
			MainMenuButtonFocused(VariantUtils.ConvertTo<NMainMenuTextButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.MainMenuButtonUnfocused && args.Count == 1)
		{
			MainMenuButtonUnfocused(VariantUtils.ConvertTo<NMainMenuTextButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EnableBackstop && args.Count == 0)
		{
			EnableBackstop();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DisableBackstop && args.Count == 0)
		{
			DisableBackstop();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DisableBackstopInstantly && args.Count == 0)
		{
			DisableBackstopInstantly();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EnableBackstopInstantly && args.Count == 0)
		{
			EnableBackstopInstantly();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateShaderMix && args.Count == 1)
		{
			UpdateShaderMix(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateShaderLod && args.Count == 1)
		{
			UpdateShaderLod(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshButtons && args.Count == 0)
		{
			RefreshButtons();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateTimelineButtonBehavior && args.Count == 0)
		{
			UpdateTimelineButtonBehavior();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSubmenuStackChanged && args.Count == 0)
		{
			OnSubmenuStackChanged();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnContinueButtonPressed && args.Count == 1)
		{
			OnContinueButtonPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DisplayLoadSaveError && args.Count == 0)
		{
			DisplayLoadSaveError();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnAbandonRunButtonPressed && args.Count == 1)
		{
			OnAbandonRunButtonPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AbandonRun && args.Count == 0)
		{
			AbandonRun();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SingleplayerButtonPressed && args.Count == 1)
		{
			SingleplayerButtonPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenSingleplayerSubmenu && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NSingleplayerSubmenu>(OpenSingleplayerSubmenu());
			return true;
		}
		if (method == MethodName.OpenMultiplayerSubmenu && args.Count == 1)
		{
			OpenMultiplayerSubmenu(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenMultiplayerSubmenu && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NMultiplayerSubmenu>(OpenMultiplayerSubmenu());
			return true;
		}
		if (method == MethodName.OpenCompendiumSubmenu && args.Count == 1)
		{
			OpenCompendiumSubmenu(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenTimelineScreen && args.Count == 1)
		{
			OpenTimelineScreen(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenSettingsMenu && args.Count == 1)
		{
			OpenSettingsMenu(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenProfileScreen && args.Count == 0)
		{
			OpenProfileScreen();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenSettingsMenu && args.Count == 0)
		{
			OpenSettingsMenu();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenPatchNotes && args.Count == 1)
		{
			OpenPatchNotes(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Quit && args.Count == 1)
		{
			Quit(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnWindowChange && args.Count == 1)
		{
			OnWindowChange(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CheckCommandLineArgs && args.Count == 0)
		{
			CheckCommandLineArgs();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NMainMenu>(Create(VariantUtils.ConvertTo<bool>(in args[0])));
			return true;
		}
		if (method == MethodName.Quit && args.Count == 1)
		{
			Quit(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
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
		if (method == MethodName.ConnectMainMenuTextButtonFocusLogic)
		{
			return true;
		}
		if (method == MethodName.MainMenuButtonFocused)
		{
			return true;
		}
		if (method == MethodName.MainMenuButtonUnfocused)
		{
			return true;
		}
		if (method == MethodName.EnableBackstop)
		{
			return true;
		}
		if (method == MethodName.DisableBackstop)
		{
			return true;
		}
		if (method == MethodName.DisableBackstopInstantly)
		{
			return true;
		}
		if (method == MethodName.EnableBackstopInstantly)
		{
			return true;
		}
		if (method == MethodName.UpdateShaderMix)
		{
			return true;
		}
		if (method == MethodName.UpdateShaderLod)
		{
			return true;
		}
		if (method == MethodName.RefreshButtons)
		{
			return true;
		}
		if (method == MethodName.UpdateTimelineButtonBehavior)
		{
			return true;
		}
		if (method == MethodName.OnSubmenuStackChanged)
		{
			return true;
		}
		if (method == MethodName.OnContinueButtonPressed)
		{
			return true;
		}
		if (method == MethodName.DisplayLoadSaveError)
		{
			return true;
		}
		if (method == MethodName.OnAbandonRunButtonPressed)
		{
			return true;
		}
		if (method == MethodName.AbandonRun)
		{
			return true;
		}
		if (method == MethodName.SingleplayerButtonPressed)
		{
			return true;
		}
		if (method == MethodName.OpenSingleplayerSubmenu)
		{
			return true;
		}
		if (method == MethodName.OpenMultiplayerSubmenu)
		{
			return true;
		}
		if (method == MethodName.OpenCompendiumSubmenu)
		{
			return true;
		}
		if (method == MethodName.OpenTimelineScreen)
		{
			return true;
		}
		if (method == MethodName.OpenSettingsMenu)
		{
			return true;
		}
		if (method == MethodName.OpenProfileScreen)
		{
			return true;
		}
		if (method == MethodName.OpenPatchNotes)
		{
			return true;
		}
		if (method == MethodName.Quit)
		{
			return true;
		}
		if (method == MethodName.OnWindowChange)
		{
			return true;
		}
		if (method == MethodName.CheckCommandLineArgs)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.PatchNotesScreen)
		{
			PatchNotesScreen = VariantUtils.ConvertTo<NPatchNotesScreen>(in value);
			return true;
		}
		if (name == PropertyName.BlurBackstop)
		{
			BlurBackstop = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName.SubmenuStack)
		{
			SubmenuStack = VariantUtils.ConvertTo<NMainMenuSubmenuStack>(in value);
			return true;
		}
		if (name == PropertyName._window)
		{
			_window = VariantUtils.ConvertTo<Window>(in value);
			return true;
		}
		if (name == PropertyName._continueButton)
		{
			_continueButton = VariantUtils.ConvertTo<NMainMenuTextButton>(in value);
			return true;
		}
		if (name == PropertyName._abandonRunButton)
		{
			_abandonRunButton = VariantUtils.ConvertTo<NMainMenuTextButton>(in value);
			return true;
		}
		if (name == PropertyName._singleplayerButton)
		{
			_singleplayerButton = VariantUtils.ConvertTo<NMainMenuTextButton>(in value);
			return true;
		}
		if (name == PropertyName._compendiumButton)
		{
			_compendiumButton = VariantUtils.ConvertTo<NMainMenuTextButton>(in value);
			return true;
		}
		if (name == PropertyName._timelineButton)
		{
			_timelineButton = VariantUtils.ConvertTo<NMainMenuTextButton>(in value);
			return true;
		}
		if (name == PropertyName._settingsButton)
		{
			_settingsButton = VariantUtils.ConvertTo<NMainMenuTextButton>(in value);
			return true;
		}
		if (name == PropertyName._quitButton)
		{
			_quitButton = VariantUtils.ConvertTo<NMainMenuTextButton>(in value);
			return true;
		}
		if (name == PropertyName._multiplayerButton)
		{
			_multiplayerButton = VariantUtils.ConvertTo<NMainMenuTextButton>(in value);
			return true;
		}
		if (name == PropertyName._buttonReticleLeft)
		{
			_buttonReticleLeft = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._buttonReticleRight)
		{
			_buttonReticleRight = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._reticleTween)
		{
			_reticleTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._patchNotesButtonNode)
		{
			_patchNotesButtonNode = VariantUtils.ConvertTo<NPatchNotesButton>(in value);
			return true;
		}
		if (name == PropertyName._openProfileScreenButton)
		{
			_openProfileScreenButton = VariantUtils.ConvertTo<NOpenProfileScreenButton>(in value);
			return true;
		}
		if (name == PropertyName._lastHitButton)
		{
			_lastHitButton = VariantUtils.ConvertTo<NMainMenuTextButton>(in value);
			return true;
		}
		if (name == PropertyName._runInfo)
		{
			_runInfo = VariantUtils.ConvertTo<NContinueRunInfo>(in value);
			return true;
		}
		if (name == PropertyName._timelineNotificationDot)
		{
			_timelineNotificationDot = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._backstopTween)
		{
			_backstopTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._bg)
		{
			_bg = VariantUtils.ConvertTo<NMainMenuBg>(in value);
			return true;
		}
		if (name == PropertyName._blur)
		{
			_blur = VariantUtils.ConvertTo<ShaderMaterial>(in value);
			return true;
		}
		if (name == PropertyName._openTimeline)
		{
			_openTimeline = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.PatchNotesScreen)
		{
			value = VariantUtils.CreateFrom<NPatchNotesScreen>(PatchNotesScreen);
			return true;
		}
		Control from;
		if (name == PropertyName.BlurBackstop)
		{
			from = BlurBackstop;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.MainMenuButtons)
		{
			GodotObject[] mainMenuButtons = MainMenuButtons;
			value = VariantUtils.CreateFromSystemArrayOfGodotObject(mainMenuButtons);
			return true;
		}
		if (name == PropertyName.SubmenuStack)
		{
			value = VariantUtils.CreateFrom<NMainMenuSubmenuStack>(SubmenuStack);
			return true;
		}
		if (name == PropertyName.ContinueRunInfo)
		{
			value = VariantUtils.CreateFrom<NContinueRunInfo>(ContinueRunInfo);
			return true;
		}
		if (name == PropertyName.DefaultFocusedControl)
		{
			from = DefaultFocusedControl;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName._window)
		{
			value = VariantUtils.CreateFrom(in _window);
			return true;
		}
		if (name == PropertyName._continueButton)
		{
			value = VariantUtils.CreateFrom(in _continueButton);
			return true;
		}
		if (name == PropertyName._abandonRunButton)
		{
			value = VariantUtils.CreateFrom(in _abandonRunButton);
			return true;
		}
		if (name == PropertyName._singleplayerButton)
		{
			value = VariantUtils.CreateFrom(in _singleplayerButton);
			return true;
		}
		if (name == PropertyName._compendiumButton)
		{
			value = VariantUtils.CreateFrom(in _compendiumButton);
			return true;
		}
		if (name == PropertyName._timelineButton)
		{
			value = VariantUtils.CreateFrom(in _timelineButton);
			return true;
		}
		if (name == PropertyName._settingsButton)
		{
			value = VariantUtils.CreateFrom(in _settingsButton);
			return true;
		}
		if (name == PropertyName._quitButton)
		{
			value = VariantUtils.CreateFrom(in _quitButton);
			return true;
		}
		if (name == PropertyName._multiplayerButton)
		{
			value = VariantUtils.CreateFrom(in _multiplayerButton);
			return true;
		}
		if (name == PropertyName._buttonReticleLeft)
		{
			value = VariantUtils.CreateFrom(in _buttonReticleLeft);
			return true;
		}
		if (name == PropertyName._buttonReticleRight)
		{
			value = VariantUtils.CreateFrom(in _buttonReticleRight);
			return true;
		}
		if (name == PropertyName._reticleTween)
		{
			value = VariantUtils.CreateFrom(in _reticleTween);
			return true;
		}
		if (name == PropertyName._patchNotesButtonNode)
		{
			value = VariantUtils.CreateFrom(in _patchNotesButtonNode);
			return true;
		}
		if (name == PropertyName._openProfileScreenButton)
		{
			value = VariantUtils.CreateFrom(in _openProfileScreenButton);
			return true;
		}
		if (name == PropertyName._lastHitButton)
		{
			value = VariantUtils.CreateFrom(in _lastHitButton);
			return true;
		}
		if (name == PropertyName._runInfo)
		{
			value = VariantUtils.CreateFrom(in _runInfo);
			return true;
		}
		if (name == PropertyName._timelineNotificationDot)
		{
			value = VariantUtils.CreateFrom(in _timelineNotificationDot);
			return true;
		}
		if (name == PropertyName._backstopTween)
		{
			value = VariantUtils.CreateFrom(in _backstopTween);
			return true;
		}
		if (name == PropertyName._bg)
		{
			value = VariantUtils.CreateFrom(in _bg);
			return true;
		}
		if (name == PropertyName._blur)
		{
			value = VariantUtils.CreateFrom(in _blur);
			return true;
		}
		if (name == PropertyName._openTimeline)
		{
			value = VariantUtils.CreateFrom(in _openTimeline);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.PatchNotesScreen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._window, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._continueButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._abandonRunButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._singleplayerButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._compendiumButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._timelineButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._settingsButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._quitButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._multiplayerButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._buttonReticleLeft, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._buttonReticleRight, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._reticleTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._patchNotesButtonNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.BlurBackstop, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._openProfileScreenButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName.MainMenuButtons, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._lastHitButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._runInfo, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._timelineNotificationDot, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._backstopTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bg, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._blur, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._openTimeline, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.SubmenuStack, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.ContinueRunInfo, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.PatchNotesScreen, Variant.From<NPatchNotesScreen>(PatchNotesScreen));
		info.AddProperty(PropertyName.BlurBackstop, Variant.From<Control>(BlurBackstop));
		info.AddProperty(PropertyName.SubmenuStack, Variant.From<NMainMenuSubmenuStack>(SubmenuStack));
		info.AddProperty(PropertyName._window, Variant.From(in _window));
		info.AddProperty(PropertyName._continueButton, Variant.From(in _continueButton));
		info.AddProperty(PropertyName._abandonRunButton, Variant.From(in _abandonRunButton));
		info.AddProperty(PropertyName._singleplayerButton, Variant.From(in _singleplayerButton));
		info.AddProperty(PropertyName._compendiumButton, Variant.From(in _compendiumButton));
		info.AddProperty(PropertyName._timelineButton, Variant.From(in _timelineButton));
		info.AddProperty(PropertyName._settingsButton, Variant.From(in _settingsButton));
		info.AddProperty(PropertyName._quitButton, Variant.From(in _quitButton));
		info.AddProperty(PropertyName._multiplayerButton, Variant.From(in _multiplayerButton));
		info.AddProperty(PropertyName._buttonReticleLeft, Variant.From(in _buttonReticleLeft));
		info.AddProperty(PropertyName._buttonReticleRight, Variant.From(in _buttonReticleRight));
		info.AddProperty(PropertyName._reticleTween, Variant.From(in _reticleTween));
		info.AddProperty(PropertyName._patchNotesButtonNode, Variant.From(in _patchNotesButtonNode));
		info.AddProperty(PropertyName._openProfileScreenButton, Variant.From(in _openProfileScreenButton));
		info.AddProperty(PropertyName._lastHitButton, Variant.From(in _lastHitButton));
		info.AddProperty(PropertyName._runInfo, Variant.From(in _runInfo));
		info.AddProperty(PropertyName._timelineNotificationDot, Variant.From(in _timelineNotificationDot));
		info.AddProperty(PropertyName._backstopTween, Variant.From(in _backstopTween));
		info.AddProperty(PropertyName._bg, Variant.From(in _bg));
		info.AddProperty(PropertyName._blur, Variant.From(in _blur));
		info.AddProperty(PropertyName._openTimeline, Variant.From(in _openTimeline));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.PatchNotesScreen, out var value))
		{
			PatchNotesScreen = value.As<NPatchNotesScreen>();
		}
		if (info.TryGetProperty(PropertyName.BlurBackstop, out var value2))
		{
			BlurBackstop = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName.SubmenuStack, out var value3))
		{
			SubmenuStack = value3.As<NMainMenuSubmenuStack>();
		}
		if (info.TryGetProperty(PropertyName._window, out var value4))
		{
			_window = value4.As<Window>();
		}
		if (info.TryGetProperty(PropertyName._continueButton, out var value5))
		{
			_continueButton = value5.As<NMainMenuTextButton>();
		}
		if (info.TryGetProperty(PropertyName._abandonRunButton, out var value6))
		{
			_abandonRunButton = value6.As<NMainMenuTextButton>();
		}
		if (info.TryGetProperty(PropertyName._singleplayerButton, out var value7))
		{
			_singleplayerButton = value7.As<NMainMenuTextButton>();
		}
		if (info.TryGetProperty(PropertyName._compendiumButton, out var value8))
		{
			_compendiumButton = value8.As<NMainMenuTextButton>();
		}
		if (info.TryGetProperty(PropertyName._timelineButton, out var value9))
		{
			_timelineButton = value9.As<NMainMenuTextButton>();
		}
		if (info.TryGetProperty(PropertyName._settingsButton, out var value10))
		{
			_settingsButton = value10.As<NMainMenuTextButton>();
		}
		if (info.TryGetProperty(PropertyName._quitButton, out var value11))
		{
			_quitButton = value11.As<NMainMenuTextButton>();
		}
		if (info.TryGetProperty(PropertyName._multiplayerButton, out var value12))
		{
			_multiplayerButton = value12.As<NMainMenuTextButton>();
		}
		if (info.TryGetProperty(PropertyName._buttonReticleLeft, out var value13))
		{
			_buttonReticleLeft = value13.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._buttonReticleRight, out var value14))
		{
			_buttonReticleRight = value14.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._reticleTween, out var value15))
		{
			_reticleTween = value15.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._patchNotesButtonNode, out var value16))
		{
			_patchNotesButtonNode = value16.As<NPatchNotesButton>();
		}
		if (info.TryGetProperty(PropertyName._openProfileScreenButton, out var value17))
		{
			_openProfileScreenButton = value17.As<NOpenProfileScreenButton>();
		}
		if (info.TryGetProperty(PropertyName._lastHitButton, out var value18))
		{
			_lastHitButton = value18.As<NMainMenuTextButton>();
		}
		if (info.TryGetProperty(PropertyName._runInfo, out var value19))
		{
			_runInfo = value19.As<NContinueRunInfo>();
		}
		if (info.TryGetProperty(PropertyName._timelineNotificationDot, out var value20))
		{
			_timelineNotificationDot = value20.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._backstopTween, out var value21))
		{
			_backstopTween = value21.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._bg, out var value22))
		{
			_bg = value22.As<NMainMenuBg>();
		}
		if (info.TryGetProperty(PropertyName._blur, out var value23))
		{
			_blur = value23.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._openTimeline, out var value24))
		{
			_openTimeline = value24.As<bool>();
		}
	}
}
