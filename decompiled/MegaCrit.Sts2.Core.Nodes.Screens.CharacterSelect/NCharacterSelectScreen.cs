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
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Lobby;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Debug;
using MegaCrit.Sts2.Core.Nodes.Ftue;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.Unlocks;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

[ScriptPath("res://src/Core/Nodes/Screens/CharacterSelect/NCharacterSelectScreen.cs")]
public class NCharacterSelectScreen : NSubmenu, IStartRunLobbyListener, ICharacterSelectButtonDelegate
{
	public new class MethodName : NSubmenu.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName InitializeSingleplayer = "InitializeSingleplayer";

		public static readonly StringName InitCharacterButtons = "InitCharacterButtons";

		public static readonly StringName UpdateRandomCharacterVisibility = "UpdateRandomCharacterVisibility";

		public new static readonly StringName _Input = "_Input";

		public static readonly StringName DebugUnlockAllCharacters = "DebugUnlockAllCharacters";

		public new static readonly StringName OnSubmenuOpened = "OnSubmenuOpened";

		public new static readonly StringName OnSubmenuClosed = "OnSubmenuClosed";

		public static readonly StringName OnEmbarkPressed = "OnEmbarkPressed";

		public new static readonly StringName _Process = "_Process";

		public static readonly StringName CleanUpLobby = "CleanUpLobby";

		public static readonly StringName RollRandomCharacter = "RollRandomCharacter";

		public static readonly StringName OnAscensionPanelLevelChanged = "OnAscensionPanelLevelChanged";

		public static readonly StringName OnUnreadyPressed = "OnUnreadyPressed";

		public static readonly StringName UpdateRichPresence = "UpdateRichPresence";

		public static readonly StringName MaxAscensionChanged = "MaxAscensionChanged";

		public static readonly StringName AscensionChanged = "AscensionChanged";

		public static readonly StringName SeedChanged = "SeedChanged";

		public static readonly StringName ModifiersChanged = "ModifiersChanged";

		public static readonly StringName AfterInitialized = "AfterInitialized";
	}

	public new class PropertyName : NSubmenu.PropertyName
	{
		public new static readonly StringName InitialFocusedControl = "InitialFocusedControl";

		public static readonly StringName ShouldShowActDropdown = "ShouldShowActDropdown";

		public static readonly StringName _name = "_name";

		public static readonly StringName _infoPanel = "_infoPanel";

		public static readonly StringName _description = "_description";

		public static readonly StringName _hp = "_hp";

		public static readonly StringName _gold = "_gold";

		public static readonly StringName _relicTitle = "_relicTitle";

		public static readonly StringName _relicDescription = "_relicDescription";

		public static readonly StringName _relicIcon = "_relicIcon";

		public static readonly StringName _relicIconOutline = "_relicIconOutline";

		public static readonly StringName _selectedButton = "_selectedButton";

		public static readonly StringName _charButtonContainer = "_charButtonContainer";

		public static readonly StringName _bgContainer = "_bgContainer";

		public static readonly StringName _readyAndWaitingContainer = "_readyAndWaitingContainer";

		public new static readonly StringName _backButton = "_backButton";

		public static readonly StringName _unreadyButton = "_unreadyButton";

		public static readonly StringName _embarkButton = "_embarkButton";

		public static readonly StringName _ascensionPanel = "_ascensionPanel";

		public static readonly StringName _actDropdown = "_actDropdown";

		public static readonly StringName _actDropdownLabel = "_actDropdownLabel";

		public static readonly StringName _remotePlayerContainer = "_remotePlayerContainer";

		public static readonly StringName _characterUnlockAnimationBackstop = "_characterUnlockAnimationBackstop";

		public static readonly StringName _randomCharacterButton = "_randomCharacterButton";

		public static readonly StringName _infoPanelTween = "_infoPanelTween";

		public static readonly StringName _infoPanelPosFinalVal = "_infoPanelPosFinalVal";

		public static readonly StringName _charSelectButtonScene = "_charSelectButtonScene";
	}

	public new class SignalName : NSubmenu.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("screens/character_select_screen");

	private MegaLabel _name;

	private Control _infoPanel;

	private MegaRichTextLabel _description;

	private MegaLabel _hp;

	private MegaLabel _gold;

	private MegaRichTextLabel _relicTitle;

	private MegaRichTextLabel _relicDescription;

	private TextureRect _relicIcon;

	private TextureRect _relicIconOutline;

	private NCharacterSelectButton? _selectedButton;

	private Control _charButtonContainer;

	private Control _bgContainer;

	private Control _readyAndWaitingContainer;

	private NBackButton _backButton;

	private NBackButton _unreadyButton;

	private NConfirmButton _embarkButton;

	private NAscensionPanel _ascensionPanel;

	private NActDropdown _actDropdown;

	private MegaRichTextLabel _actDropdownLabel;

	private NRemoteLobbyPlayerContainer _remotePlayerContainer;

	private Control _characterUnlockAnimationBackstop;

	private NCharacterSelectButton _randomCharacterButton;

	private Tween? _infoPanelTween;

	private Vector2 _infoPanelPosFinalVal;

	private const string _sceneCharSelectButtonPath = "res://scenes/screens/char_select/char_select_button.tscn";

	[Export(PropertyHint.None, "")]
	private PackedScene _charSelectButtonScene;

	private IBootstrapSettings? _settings;

	private StartRunLobby _lobby;

	public StartRunLobby Lobby => _lobby;

	public static IEnumerable<string> AssetPaths
	{
		get
		{
			List<string> list = new List<string>();
			list.Add(_scenePath);
			list.Add("res://scenes/screens/char_select/char_select_button.tscn");
			list.AddRange(NCharacterSelectButton.AssetPaths);
			return new _003C_003Ez__ReadOnlyList<string>(list);
		}
	}

	protected override Control InitialFocusedControl => _charButtonContainer.GetChild<Control>(0);

	private bool ShouldShowActDropdown => false;

	public static NCharacterSelectScreen? Create()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		return ResourceLoader.Load<PackedScene>(_scenePath, null, ResourceLoader.CacheMode.Reuse).Instantiate<NCharacterSelectScreen>(PackedScene.GenEditState.Disabled);
	}

	public override void _Ready()
	{
		ConnectSignals();
		_infoPanel = GetNode<Control>("InfoPanel");
		_name = GetNode<MegaLabel>("InfoPanel/VBoxContainer/Name");
		_description = GetNode<MegaRichTextLabel>("InfoPanel/VBoxContainer/DescriptionLabel");
		_hp = GetNode<MegaLabel>("InfoPanel/VBoxContainer/HpGoldSpacer/HpGold/Hp/Label");
		_gold = GetNode<MegaLabel>("InfoPanel/VBoxContainer/HpGoldSpacer/HpGold/Gold/Label");
		_relicTitle = GetNode<MegaRichTextLabel>("InfoPanel/VBoxContainer/Relic/Name/RichTextLabel");
		_relicDescription = GetNode<MegaRichTextLabel>("InfoPanel/VBoxContainer/Relic/Description");
		_relicIcon = GetNode<TextureRect>("InfoPanel/VBoxContainer/Relic/Icon");
		_relicIconOutline = GetNode<TextureRect>("InfoPanel/VBoxContainer/Relic/Icon/Outline");
		_bgContainer = GetNode<Control>("AnimatedBg");
		_charButtonContainer = GetNode<Control>("CharSelectButtons/ButtonContainer");
		_ascensionPanel = GetNode<NAscensionPanel>("%AscensionPanel");
		_actDropdown = GetNode<NActDropdown>("%ActDropdown");
		_actDropdownLabel = GetNode<MegaRichTextLabel>("ActLabel");
		_remotePlayerContainer = GetNode<NRemoteLobbyPlayerContainer>("RemotePlayerContainer");
		_readyAndWaitingContainer = GetNode<Control>("ReadyAndWaitingPanel");
		GetNode<MegaRichTextLabel>("%WaitingForPlayers").Text = new LocString("main_menu_ui", "CHARACTER_SELECT.waitingForPlayers").GetFormattedText();
		_characterUnlockAnimationBackstop = GetNode<Control>("%CharacterUnlockAnimationBackstop");
		_backButton = GetNode<NBackButton>("BackButton");
		_unreadyButton = GetNode<NBackButton>("UnreadyButton");
		_embarkButton = GetNode<NConfirmButton>("ConfirmButton");
		_embarkButton.OverrideHotkeys(new string[1] { MegaInput.select });
		_embarkButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnEmbarkPressed));
		_ascensionPanel.Connect(NAscensionPanel.SignalName.AscensionLevelChanged, Callable.From(OnAscensionPanelLevelChanged));
		_unreadyButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnUnreadyPressed));
		_unreadyButton.Disable();
		base.ProcessMode = ProcessModeEnum.Disabled;
		InitCharacterButtons();
		Type type = BootstrapSettingsUtil.Get();
		if (type != null)
		{
			_settings = (IBootstrapSettings)Activator.CreateInstance(type);
			PreloadManager.Enabled = _settings.DoPreloading;
		}
	}

	public void InitializeMultiplayerAsHost(INetGameService gameService, int maxPlayers)
	{
		if (gameService.Type != NetGameType.Host)
		{
			throw new InvalidOperationException($"Initialized character select screen with GameService of type {gameService.Type} when hosting!");
		}
		_lobby = new StartRunLobby(GameMode.Standard, gameService, this, maxPlayers);
		_ascensionPanel.Initialize(MultiplayerUiMode.Host);
		_lobby.AddLocalHostPlayer(new UnlockState(SaveManager.Instance.Progress), SaveManager.Instance.Progress.MaxMultiplayerAscension);
		AfterInitialized();
	}

	public void InitializeMultiplayerAsClient(INetGameService gameService, ClientLobbyJoinResponseMessage message)
	{
		if (gameService.Type != NetGameType.Client)
		{
			throw new InvalidOperationException($"Initialized character select screen with GameService of type {gameService.Type} when joining!");
		}
		_lobby = new StartRunLobby(GameMode.Standard, gameService, this, -1);
		_ascensionPanel.Initialize(MultiplayerUiMode.Client);
		_lobby.InitializeFromMessage(message);
		AfterInitialized();
	}

	public void InitializeSingleplayer()
	{
		_lobby = new StartRunLobby(GameMode.Standard, new NetSingleplayerGameService(), this, 1);
		_ascensionPanel.Initialize(MultiplayerUiMode.Singleplayer);
		_lobby.AddLocalHostPlayer(new UnlockState(SaveManager.Instance.Progress), 0);
		AfterInitialized();
	}

	private void InitCharacterButtons()
	{
		foreach (CharacterModel allCharacter in ModelDb.AllCharacters)
		{
			NCharacterSelectButton nCharacterSelectButton = _charSelectButtonScene.Instantiate<NCharacterSelectButton>(PackedScene.GenEditState.Disabled);
			nCharacterSelectButton.Name = allCharacter.Id.Entry + "_button";
			_charButtonContainer.AddChildSafely(nCharacterSelectButton);
			nCharacterSelectButton.Init(allCharacter, this);
		}
		_randomCharacterButton = _charSelectButtonScene.Instantiate<NCharacterSelectButton>(PackedScene.GenEditState.Disabled);
		_charButtonContainer.AddChildSafely(_randomCharacterButton);
		_randomCharacterButton.Init(ModelDb.Character<RandomCharacter>(), this);
		UpdateRandomCharacterVisibility();
	}

	private void UpdateRandomCharacterVisibility()
	{
		if (_lobby == null)
		{
			return;
		}
		bool visible = false;
		foreach (LobbyPlayer player in _lobby.Players)
		{
			UnlockState unlockState = UnlockState.FromSerializable(player.unlockState);
			bool flag = true;
			foreach (CharacterModel allCharacter in ModelDb.AllCharacters)
			{
				if (!unlockState.Characters.Contains(allCharacter))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				visible = true;
				break;
			}
		}
		_randomCharacterButton.Visible = visible;
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (inputEvent.IsActionReleased(DebugHotkey.unlockCharacters))
		{
			DebugUnlockAllCharacters();
		}
	}

	private void DebugUnlockAllCharacters()
	{
		foreach (NCharacterSelectButton item in _charButtonContainer.GetChildren().OfType<NCharacterSelectButton>())
		{
			item.DebugUnlock();
		}
	}

	public override void OnSubmenuOpened()
	{
		base.OnSubmenuOpened();
		foreach (NCharacterSelectButton item in _charButtonContainer.GetChildren().OfType<NCharacterSelectButton>())
		{
			if (!item.IsLocked)
			{
				item.Enable();
			}
			else
			{
				item.UnlockIfPossible();
			}
			item.Reset();
		}
		_embarkButton.Enable();
		if (SaveManager.Instance.Progress.PendingCharacterUnlock == ModelId.none)
		{
			_charButtonContainer.GetChild<NCharacterSelectButton>(0).Select();
		}
		else
		{
			TaskHelper.RunSafely(PlayUnlockCharacterAnimation(SaveManager.Instance.Progress.PendingCharacterUnlock));
		}
		_remotePlayerContainer.Visible = _lobby.NetService.Type != NetGameType.Singleplayer;
		_remotePlayerContainer.Initialize(_lobby, displayLocalPlayer: true);
		if (_lobby.NetService.Type == NetGameType.Client)
		{
			_ascensionPanel.SetAscensionLevel(_lobby.Ascension);
		}
		_actDropdown.Visible = ShouldShowActDropdown;
		_actDropdownLabel.Visible = _actDropdown.Visible;
		_readyAndWaitingContainer.Visible = false;
		foreach (LobbyPlayer player in _lobby.Players)
		{
			RefreshButtonSelectionForPlayer(player);
		}
		base.ProcessMode = ProcessModeEnum.Inherit;
	}

	private async Task PlayUnlockCharacterAnimation(ModelId character)
	{
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		_backButton.Disable();
		_embarkButton.Disable();
		_infoPanel.Visible = false;
		_characterUnlockAnimationBackstop.Visible = true;
		foreach (NCharacterSelectButton button in _charButtonContainer.GetChildren().OfType<NCharacterSelectButton>())
		{
			if (button.Character.Id == character)
			{
				button.LockForAnimation();
				await Cmd.Wait(0.3f);
				await button.AnimateUnlock();
				button.Select();
			}
		}
		_infoPanel.Visible = true;
		_characterUnlockAnimationBackstop.Visible = false;
		_backButton.Enable();
		_embarkButton.Enable();
		SaveManager.Instance.Progress.PendingCharacterUnlock = ModelId.none;
	}

	public override void OnSubmenuClosed()
	{
		base.OnSubmenuClosed();
		_embarkButton.Disable();
		_remotePlayerContainer.Cleanup();
		_ascensionPanel.Cleanup();
		if (_lobby.NetService.Type.IsMultiplayer())
		{
			PlatformUtil.SetRichPresence("MAIN_MENU", null, null);
		}
		CleanUpLobby(disconnectSession: true);
	}

	private void OnEmbarkPressed(NButton _)
	{
		_embarkButton.Disable();
		if (!SaveManager.Instance.SeenFtue("accept_tutorials_ftue"))
		{
			NModalContainer.Instance.Add(NAcceptTutorialsFtue.Create(this, delegate
			{
				OnEmbarkPressed(null);
			}));
			return;
		}
		NetGameType type = _lobby.NetService.Type;
		if ((uint)(type - 1) <= 1u)
		{
			_lobby.Act1 = _actDropdown.CurrentOption;
		}
		_lobby.SetReady(ready: true);
		foreach (NCharacterSelectButton item in _charButtonContainer.GetChildren().OfType<NCharacterSelectButton>())
		{
			item.Disable();
		}
		_backButton.Disable();
		if (_lobby.NetService.Type.IsMultiplayer() && !_lobby.IsAboutToBeginGame())
		{
			_readyAndWaitingContainer.Visible = true;
			_unreadyButton.Enable();
		}
	}

	public override void _Process(double delta)
	{
		if (_lobby.NetService.IsConnected)
		{
			_lobby.NetService.Update();
		}
	}

	private void CleanUpLobby(bool disconnectSession)
	{
		_lobby.CleanUp(disconnectSession);
		_lobby = null;
		if (GodotObject.IsInstanceValid(this))
		{
			base.ProcessMode = ProcessModeEnum.Disabled;
		}
	}

	private async Task StartNewSingleplayerRun(string seed, List<ActModel> acts)
	{
		Log.Info($"Embarking on a singleplayer {_lobby.LocalPlayer.character.Id.Entry} run. Ascension: {_lobby.Ascension} Seed: {seed}");
		int ascensionToEmbark = _lobby.Ascension;
		if (_lobby.LocalPlayer.character is RandomCharacter)
		{
			RollRandomCharacter();
			CharacterModel character = _lobby.LocalPlayer.character;
			int maxAscension = SaveManager.Instance.Progress.GetOrCreateCharacterStats(_lobby.LocalPlayer.character.Id).MaxAscension;
			ascensionToEmbark = Math.Min(maxAscension, ascensionToEmbark);
			NGame.Instance?.ScreenShake(ShakeStrength.Weak, ShakeDuration.Short, 90f);
			SfxCmd.Play(character.CharacterSelectSfx);
			Control control = PreloadManager.Cache.GetScene(character.CharacterSelectBg).Instantiate<Control>(PackedScene.GenEditState.Disabled);
			control.Name = character.Id.Entry + "_bg";
			_bgContainer.AddChildSafely(control);
			if (ascensionToEmbark < maxAscension)
			{
				_ascensionPanel.SetAscensionLevel(ascensionToEmbark);
			}
			await Task.Delay(1000);
		}
		SfxCmd.Play(_lobby.LocalPlayer.character.CharacterTransitionSfx);
		await NGame.Instance.Transition.FadeOut(0.8f, _lobby.LocalPlayer.character.CharacterSelectTransitionPath);
		await NGame.Instance.StartNewSingleplayerRun(_lobby.LocalPlayer.character, shouldSave: true, acts, Array.Empty<ModifierModel>(), seed, ascensionToEmbark);
		CleanUpLobby(disconnectSession: false);
	}

	private async Task StartNewMultiplayerRun(string seed, List<ActModel> acts)
	{
		Log.Info($"Embarking on a multiplayer run. Players: {string.Join(",", _lobby.Players)}. Ascension: {_lobby.Ascension} Seed: {seed}");
		if (_lobby.LocalPlayer.character is RandomCharacter)
		{
			RollRandomCharacter();
		}
		SfxCmd.Play(_lobby.LocalPlayer.character.CharacterTransitionSfx);
		await NGame.Instance.Transition.FadeOut(0.8f, _lobby.LocalPlayer.character.CharacterSelectTransitionPath);
		IBootstrapSettings settings = _settings;
		if (settings != null && settings.BootstrapInMultiplayer)
		{
			using (new NetLoadingHandle(_lobby.NetService))
			{
				acts[0] = _settings.Act;
				RunState runState = RunState.CreateForNewRun(_lobby.Players.Select((LobbyPlayer p) => Player.CreateForNewRun(p.character, UnlockState.FromSerializable(p.unlockState), p.id)).ToList(), acts.Select((ActModel a) => a.ToMutable()).ToList(), _settings.Modifiers, _lobby.Ascension, seed);
				RunManager.Instance.SetUpNewMultiPlayer(runState, _lobby, _settings.SaveRunHistory);
				await PreloadManager.LoadRunAssets(runState.Players.Select((Player p) => p.Character));
				await RunManager.Instance.FinalizeStartingRelics();
				RunManager.Instance.Launch();
				NGame.Instance.RootSceneContainer.SetCurrentScene(NRun.Create(runState));
				await RunManager.Instance.SetActInternal(0);
				await SaveManager.Instance.SaveRun(null);
				CleanUpLobby(disconnectSession: false);
				await _settings.Setup(LocalContext.GetMe(runState));
				switch (_settings.RoomType)
				{
				case RoomType.Unassigned:
					await RunManager.Instance.EnterAct(0);
					break;
				case RoomType.Treasure:
				case RoomType.Shop:
				case RoomType.RestSite:
					await RunManager.Instance.EnterRoomDebug(_settings.RoomType, MapPointType.Unassigned, null, showTransition: false);
					RunManager.Instance.ActionExecutor.Unpause();
					break;
				case RoomType.Event:
					await RunManager.Instance.EnterRoomDebug(_settings.RoomType, MapPointType.Unassigned, _settings.Event, showTransition: false);
					break;
				default:
					await RunManager.Instance.EnterRoomDebug(_settings.RoomType, MapPointType.Unassigned, _settings.RoomType.IsCombatRoom() ? _settings.Encounter.ToMutable() : null, showTransition: false);
					break;
				}
			}
		}
		else
		{
			await NGame.Instance.StartNewMultiplayerRun(_lobby, shouldSave: true, acts, Array.Empty<ModifierModel>(), seed, _lobby.Ascension);
			CleanUpLobby(disconnectSession: false);
		}
	}

	private void RollRandomCharacter()
	{
		CharacterModel[] items = ModelDb.AllCharacters.ToArray();
		_lobby.SetLocalCharacter(Rng.Chaotic.NextItem(items));
	}

	public void SelectCharacter(NCharacterSelectButton charSelectButton, CharacterModel characterModel)
	{
		if (!charSelectButton.IsRandom)
		{
			SfxCmd.Play(characterModel.CharacterSelectSfx);
		}
		NGame.Instance?.ScreenShake(ShakeStrength.Weak, ShakeDuration.Short, 90f);
		if (_infoPanelTween != null)
		{
			_infoPanel.Position = _infoPanelPosFinalVal;
		}
		_infoPanelPosFinalVal = _infoPanel.Position;
		_infoPanelTween?.Kill();
		_infoPanelTween = CreateTween().SetParallel();
		_infoPanelTween.TweenProperty(_infoPanel, "position", _infoPanel.Position, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
			.From(_infoPanel.Position - new Vector2(300f, 0f));
		foreach (Node child in _bgContainer.GetChildren())
		{
			_bgContainer.RemoveChildSafely(child);
			child.QueueFreeSafely();
		}
		_selectedButton = charSelectButton;
		if (!charSelectButton.IsLocked)
		{
			_embarkButton.Enable();
			Control control = PreloadManager.Cache.GetScene(characterModel.CharacterSelectBg).Instantiate<Control>(PackedScene.GenEditState.Disabled);
			control.Name = characterModel.Id.Entry + "_bg";
			_bgContainer.AddChildSafely(control);
			string formattedText = new LocString("characters", characterModel.CharacterSelectTitle).GetFormattedText();
			_name.SetTextAutoSize(formattedText);
			_description.Text = new LocString("characters", characterModel.CharacterSelectDesc).GetFormattedText();
			if (!_selectedButton.IsRandom)
			{
				_hp.SetTextAutoSize($"{characterModel.StartingHp}/{characterModel.StartingHp}");
				_gold.SetTextAutoSize($"{characterModel.StartingGold}");
				RelicModel relicModel = characterModel.StartingRelics[0];
				_relicTitle.Text = relicModel.Title.GetFormattedText();
				_relicDescription.Text = relicModel.DynamicDescription.GetFormattedText();
				_relicIcon.Texture = relicModel.Icon;
				_relicIconOutline.Texture = relicModel.IconOutline;
				_relicIcon.SelfModulate = Colors.White;
				_relicIconOutline.SelfModulate = StsColors.halfTransparentBlack;
			}
			else
			{
				_hp.SetTextAutoSize("??/??");
				_gold.SetTextAutoSize("???");
				_relicIcon.SelfModulate = StsColors.transparentBlack;
				_relicIconOutline.SelfModulate = StsColors.transparentBlack;
				_relicTitle.Text = string.Empty;
				_relicDescription.Text = string.Empty;
			}
			_lobby.SetLocalCharacter(characterModel);
			if (!_lobby.NetService.Type.IsMultiplayer())
			{
				_ascensionPanel.AnimIn();
			}
		}
		else
		{
			_embarkButton.Disable();
			string formattedText2 = new LocString("main_menu_ui", "CHARACTER_SELECT.locked.title").GetFormattedText();
			_name.SetTextAutoSize(formattedText2);
			_description.Text = characterModel.GetUnlockText().GetFormattedText();
			_hp.SetTextAutoSize("??/??");
			_gold.SetTextAutoSize("???");
			if (!_selectedButton.IsRandom)
			{
				RelicModel relicModel2 = characterModel.StartingRelics[0];
				_relicTitle.Text = new LocString("main_menu_ui", "CHARACTER_SELECT.lockedRelic.title").GetFormattedText();
				_relicDescription.Text = new LocString("main_menu_ui", "CHARACTER_SELECT.lockedRelic.description").GetFormattedText();
				_relicIcon.Texture = relicModel2.Icon;
				_relicIconOutline.Texture = relicModel2.IconOutline;
				_relicIcon.SelfModulate = StsColors.ninetyPercentBlack;
				_relicIconOutline.SelfModulate = StsColors.halfTransparentWhite;
			}
			else
			{
				_relicIcon.SelfModulate = StsColors.transparentBlack;
				_relicIconOutline.SelfModulate = StsColors.transparentBlack;
				_relicTitle.Text = string.Empty;
				_relicDescription.Text = string.Empty;
			}
			_ascensionPanel.Visible = false;
		}
		foreach (NCharacterSelectButton item in _charButtonContainer.GetChildren().OfType<NCharacterSelectButton>())
		{
			if (item != _selectedButton)
			{
				item.Deselect();
			}
		}
	}

	private void OnAscensionPanelLevelChanged()
	{
		if (_lobby.NetService.Type != NetGameType.Client && _lobby.Ascension != _ascensionPanel.Ascension)
		{
			_lobby.SyncAscensionChange(_ascensionPanel.Ascension);
		}
	}

	private void OnUnreadyPressed(NButton _)
	{
		_lobby.SetReady(ready: false);
		foreach (NCharacterSelectButton item in _charButtonContainer.GetChildren().OfType<NCharacterSelectButton>())
		{
			item.Enable();
		}
		_readyAndWaitingContainer.Visible = false;
		_embarkButton.Enable();
		_backButton.Enable();
		_unreadyButton.Disable();
	}

	private void UpdateRichPresence()
	{
		if (_lobby.NetService.Type.IsMultiplayer())
		{
			PlatformUtil.SetRichPresence("STANDARD_MP_LOBBY", _lobby.NetService.GetRawLobbyIdentifier(), _lobby.Players.Count);
		}
	}

	public void MaxAscensionChanged()
	{
		_ascensionPanel.SetMaxAscension(_lobby.MaxAscension);
	}

	public void PlayerConnected(LobbyPlayer player)
	{
		_remotePlayerContainer.OnPlayerConnected(player);
		RefreshButtonSelectionForPlayer(player);
		UpdateRichPresence();
		UpdateRandomCharacterVisibility();
	}

	public void PlayerChanged(LobbyPlayer player)
	{
		_remotePlayerContainer.OnPlayerChanged(player);
		RefreshButtonSelectionForPlayer(player);
	}

	private void RefreshButtonSelectionForPlayer(LobbyPlayer player)
	{
		if (player.id == _lobby.LocalPlayer.id)
		{
			return;
		}
		foreach (NCharacterSelectButton item in _charButtonContainer.GetChildren().OfType<NCharacterSelectButton>())
		{
			if (item.RemoteSelectedPlayers.Contains(player.id) && player.character != item.Character)
			{
				item.OnRemotePlayerDeselected(player.id);
			}
			else if (player.character == item.Character)
			{
				item.OnRemotePlayerSelected(player.id);
			}
		}
	}

	public void AscensionChanged()
	{
		if (_lobby.NetService.Type == NetGameType.Client)
		{
			_ascensionPanel.Visible = _lobby.Ascension > 0;
		}
		_ascensionPanel.SetAscensionLevel(_lobby.Ascension);
	}

	public void SeedChanged()
	{
		throw new NotImplementedException("Seed should not be changed in standard mode!");
	}

	public void ModifiersChanged()
	{
		throw new NotImplementedException("Modifiers should not be changed in standard mode!");
	}

	public void RemotePlayerDisconnected(LobbyPlayer player)
	{
		_remotePlayerContainer.OnPlayerDisconnected(player);
		foreach (NCharacterSelectButton item in _charButtonContainer.GetChildren().OfType<NCharacterSelectButton>())
		{
			if (item.RemoteSelectedPlayers.Contains(player.id) && player.character == item.Character)
			{
				item.OnRemotePlayerDeselected(player.id);
			}
		}
		UpdateRichPresence();
		UpdateRandomCharacterVisibility();
	}

	public void BeginRun(string seed, List<ActModel> acts, IReadOnlyList<ModifierModel> modifiers)
	{
		if (modifiers.Count > 0)
		{
			Log.Error("Modifiers list is not empty while starting a standard run, ignoring!");
		}
		NAudioManager.Instance?.StopMusic();
		_ascensionPanel.Cleanup();
		if (_lobby.NetService.Type == NetGameType.Singleplayer)
		{
			TaskHelper.RunSafely(StartNewSingleplayerRun(seed, acts));
		}
		else
		{
			TaskHelper.RunSafely(StartNewMultiplayerRun(seed, acts));
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

	private void AfterInitialized()
	{
		NGame.Instance.RemoteCursorContainer.Initialize(_lobby.InputSynchronizer, _lobby.Players.Select((LobbyPlayer p) => p.id));
		NGame.Instance.ReactionContainer.InitializeNetworking(_lobby.NetService);
		NGame.Instance.TimeoutOverlay.Initialize(_lobby.NetService, isGameLevel: true);
		UpdateRichPresence();
		UpdateRandomCharacterVisibility();
		MegaCrit.Sts2.Core.Logging.Logger.logLevelTypeMap[LogType.Network] = ((_lobby.NetService.Type == NetGameType.Singleplayer) ? LogLevel.Info : LogLevel.Debug);
		MegaCrit.Sts2.Core.Logging.Logger.logLevelTypeMap[LogType.Actions] = ((_lobby.NetService.Type == NetGameType.Singleplayer) ? LogLevel.Info : LogLevel.VeryDebug);
		MegaCrit.Sts2.Core.Logging.Logger.logLevelTypeMap[LogType.GameSync] = ((_lobby.NetService.Type == NetGameType.Singleplayer) ? LogLevel.Info : LogLevel.VeryDebug);
		if (_lobby.NetService.Type != NetGameType.Singleplayer)
		{
			IBootstrapSettings? settings = _settings;
			if (settings != null && settings.BootstrapInMultiplayer)
			{
				NGame.Instance.DebugSeedOverride = _settings.Seed;
				return;
			}
		}
		NGame.Instance.DebugSeedOverride = null;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(21);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.InitializeSingleplayer, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.InitCharacterButtons, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateRandomCharacterVisibility, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.DebugUnlockAllCharacters, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuOpened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuClosed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnEmbarkPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.CleanUpLobby, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "disconnectSession", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RollRandomCharacter, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnAscensionPanelLevelChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnreadyPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateRichPresence, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.MaxAscensionChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AscensionChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SeedChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ModifiersChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AfterInitialized, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NCharacterSelectScreen>(Create());
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
		if (method == MethodName.InitCharacterButtons && args.Count == 0)
		{
			InitCharacterButtons();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateRandomCharacterVisibility && args.Count == 0)
		{
			UpdateRandomCharacterVisibility();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Input && args.Count == 1)
		{
			_Input(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DebugUnlockAllCharacters && args.Count == 0)
		{
			DebugUnlockAllCharacters();
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
		if (method == MethodName.OnEmbarkPressed && args.Count == 1)
		{
			OnEmbarkPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CleanUpLobby && args.Count == 1)
		{
			CleanUpLobby(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RollRandomCharacter && args.Count == 0)
		{
			RollRandomCharacter();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnAscensionPanelLevelChanged && args.Count == 0)
		{
			OnAscensionPanelLevelChanged();
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
		if (method == MethodName.AfterInitialized && args.Count == 0)
		{
			AfterInitialized();
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
			ret = VariantUtils.CreateFrom<NCharacterSelectScreen>(Create());
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
		if (method == MethodName.InitCharacterButtons)
		{
			return true;
		}
		if (method == MethodName.UpdateRandomCharacterVisibility)
		{
			return true;
		}
		if (method == MethodName._Input)
		{
			return true;
		}
		if (method == MethodName.DebugUnlockAllCharacters)
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
		if (method == MethodName.OnEmbarkPressed)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName.CleanUpLobby)
		{
			return true;
		}
		if (method == MethodName.RollRandomCharacter)
		{
			return true;
		}
		if (method == MethodName.OnAscensionPanelLevelChanged)
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
		if (method == MethodName.AfterInitialized)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._name)
		{
			_name = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._infoPanel)
		{
			_infoPanel = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._description)
		{
			_description = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._hp)
		{
			_hp = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._gold)
		{
			_gold = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._relicTitle)
		{
			_relicTitle = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._relicDescription)
		{
			_relicDescription = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._relicIcon)
		{
			_relicIcon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._relicIconOutline)
		{
			_relicIconOutline = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._selectedButton)
		{
			_selectedButton = VariantUtils.ConvertTo<NCharacterSelectButton>(in value);
			return true;
		}
		if (name == PropertyName._charButtonContainer)
		{
			_charButtonContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._bgContainer)
		{
			_bgContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._readyAndWaitingContainer)
		{
			_readyAndWaitingContainer = VariantUtils.ConvertTo<Control>(in value);
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
		if (name == PropertyName._embarkButton)
		{
			_embarkButton = VariantUtils.ConvertTo<NConfirmButton>(in value);
			return true;
		}
		if (name == PropertyName._ascensionPanel)
		{
			_ascensionPanel = VariantUtils.ConvertTo<NAscensionPanel>(in value);
			return true;
		}
		if (name == PropertyName._actDropdown)
		{
			_actDropdown = VariantUtils.ConvertTo<NActDropdown>(in value);
			return true;
		}
		if (name == PropertyName._actDropdownLabel)
		{
			_actDropdownLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._remotePlayerContainer)
		{
			_remotePlayerContainer = VariantUtils.ConvertTo<NRemoteLobbyPlayerContainer>(in value);
			return true;
		}
		if (name == PropertyName._characterUnlockAnimationBackstop)
		{
			_characterUnlockAnimationBackstop = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._randomCharacterButton)
		{
			_randomCharacterButton = VariantUtils.ConvertTo<NCharacterSelectButton>(in value);
			return true;
		}
		if (name == PropertyName._infoPanelTween)
		{
			_infoPanelTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._infoPanelPosFinalVal)
		{
			_infoPanelPosFinalVal = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._charSelectButtonScene)
		{
			_charSelectButtonScene = VariantUtils.ConvertTo<PackedScene>(in value);
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
		if (name == PropertyName.ShouldShowActDropdown)
		{
			value = VariantUtils.CreateFrom<bool>(ShouldShowActDropdown);
			return true;
		}
		if (name == PropertyName._name)
		{
			value = VariantUtils.CreateFrom(in _name);
			return true;
		}
		if (name == PropertyName._infoPanel)
		{
			value = VariantUtils.CreateFrom(in _infoPanel);
			return true;
		}
		if (name == PropertyName._description)
		{
			value = VariantUtils.CreateFrom(in _description);
			return true;
		}
		if (name == PropertyName._hp)
		{
			value = VariantUtils.CreateFrom(in _hp);
			return true;
		}
		if (name == PropertyName._gold)
		{
			value = VariantUtils.CreateFrom(in _gold);
			return true;
		}
		if (name == PropertyName._relicTitle)
		{
			value = VariantUtils.CreateFrom(in _relicTitle);
			return true;
		}
		if (name == PropertyName._relicDescription)
		{
			value = VariantUtils.CreateFrom(in _relicDescription);
			return true;
		}
		if (name == PropertyName._relicIcon)
		{
			value = VariantUtils.CreateFrom(in _relicIcon);
			return true;
		}
		if (name == PropertyName._relicIconOutline)
		{
			value = VariantUtils.CreateFrom(in _relicIconOutline);
			return true;
		}
		if (name == PropertyName._selectedButton)
		{
			value = VariantUtils.CreateFrom(in _selectedButton);
			return true;
		}
		if (name == PropertyName._charButtonContainer)
		{
			value = VariantUtils.CreateFrom(in _charButtonContainer);
			return true;
		}
		if (name == PropertyName._bgContainer)
		{
			value = VariantUtils.CreateFrom(in _bgContainer);
			return true;
		}
		if (name == PropertyName._readyAndWaitingContainer)
		{
			value = VariantUtils.CreateFrom(in _readyAndWaitingContainer);
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
		if (name == PropertyName._embarkButton)
		{
			value = VariantUtils.CreateFrom(in _embarkButton);
			return true;
		}
		if (name == PropertyName._ascensionPanel)
		{
			value = VariantUtils.CreateFrom(in _ascensionPanel);
			return true;
		}
		if (name == PropertyName._actDropdown)
		{
			value = VariantUtils.CreateFrom(in _actDropdown);
			return true;
		}
		if (name == PropertyName._actDropdownLabel)
		{
			value = VariantUtils.CreateFrom(in _actDropdownLabel);
			return true;
		}
		if (name == PropertyName._remotePlayerContainer)
		{
			value = VariantUtils.CreateFrom(in _remotePlayerContainer);
			return true;
		}
		if (name == PropertyName._characterUnlockAnimationBackstop)
		{
			value = VariantUtils.CreateFrom(in _characterUnlockAnimationBackstop);
			return true;
		}
		if (name == PropertyName._randomCharacterButton)
		{
			value = VariantUtils.CreateFrom(in _randomCharacterButton);
			return true;
		}
		if (name == PropertyName._infoPanelTween)
		{
			value = VariantUtils.CreateFrom(in _infoPanelTween);
			return true;
		}
		if (name == PropertyName._infoPanelPosFinalVal)
		{
			value = VariantUtils.CreateFrom(in _infoPanelPosFinalVal);
			return true;
		}
		if (name == PropertyName._charSelectButtonScene)
		{
			value = VariantUtils.CreateFrom(in _charSelectButtonScene);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._name, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._infoPanel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._description, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hp, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._gold, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._relicTitle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._relicDescription, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._relicIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._relicIconOutline, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._selectedButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._charButtonContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bgContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._readyAndWaitingContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._backButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._unreadyButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._embarkButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._ascensionPanel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._actDropdown, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._actDropdownLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._remotePlayerContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._characterUnlockAnimationBackstop, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._randomCharacterButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._infoPanelTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._infoPanelPosFinalVal, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._charSelectButtonScene, PropertyHint.ResourceType, "PackedScene", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.InitialFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.ShouldShowActDropdown, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._name, Variant.From(in _name));
		info.AddProperty(PropertyName._infoPanel, Variant.From(in _infoPanel));
		info.AddProperty(PropertyName._description, Variant.From(in _description));
		info.AddProperty(PropertyName._hp, Variant.From(in _hp));
		info.AddProperty(PropertyName._gold, Variant.From(in _gold));
		info.AddProperty(PropertyName._relicTitle, Variant.From(in _relicTitle));
		info.AddProperty(PropertyName._relicDescription, Variant.From(in _relicDescription));
		info.AddProperty(PropertyName._relicIcon, Variant.From(in _relicIcon));
		info.AddProperty(PropertyName._relicIconOutline, Variant.From(in _relicIconOutline));
		info.AddProperty(PropertyName._selectedButton, Variant.From(in _selectedButton));
		info.AddProperty(PropertyName._charButtonContainer, Variant.From(in _charButtonContainer));
		info.AddProperty(PropertyName._bgContainer, Variant.From(in _bgContainer));
		info.AddProperty(PropertyName._readyAndWaitingContainer, Variant.From(in _readyAndWaitingContainer));
		info.AddProperty(PropertyName._backButton, Variant.From(in _backButton));
		info.AddProperty(PropertyName._unreadyButton, Variant.From(in _unreadyButton));
		info.AddProperty(PropertyName._embarkButton, Variant.From(in _embarkButton));
		info.AddProperty(PropertyName._ascensionPanel, Variant.From(in _ascensionPanel));
		info.AddProperty(PropertyName._actDropdown, Variant.From(in _actDropdown));
		info.AddProperty(PropertyName._actDropdownLabel, Variant.From(in _actDropdownLabel));
		info.AddProperty(PropertyName._remotePlayerContainer, Variant.From(in _remotePlayerContainer));
		info.AddProperty(PropertyName._characterUnlockAnimationBackstop, Variant.From(in _characterUnlockAnimationBackstop));
		info.AddProperty(PropertyName._randomCharacterButton, Variant.From(in _randomCharacterButton));
		info.AddProperty(PropertyName._infoPanelTween, Variant.From(in _infoPanelTween));
		info.AddProperty(PropertyName._infoPanelPosFinalVal, Variant.From(in _infoPanelPosFinalVal));
		info.AddProperty(PropertyName._charSelectButtonScene, Variant.From(in _charSelectButtonScene));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._name, out var value))
		{
			_name = value.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._infoPanel, out var value2))
		{
			_infoPanel = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._description, out var value3))
		{
			_description = value3.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._hp, out var value4))
		{
			_hp = value4.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._gold, out var value5))
		{
			_gold = value5.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._relicTitle, out var value6))
		{
			_relicTitle = value6.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._relicDescription, out var value7))
		{
			_relicDescription = value7.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._relicIcon, out var value8))
		{
			_relicIcon = value8.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._relicIconOutline, out var value9))
		{
			_relicIconOutline = value9.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._selectedButton, out var value10))
		{
			_selectedButton = value10.As<NCharacterSelectButton>();
		}
		if (info.TryGetProperty(PropertyName._charButtonContainer, out var value11))
		{
			_charButtonContainer = value11.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._bgContainer, out var value12))
		{
			_bgContainer = value12.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._readyAndWaitingContainer, out var value13))
		{
			_readyAndWaitingContainer = value13.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._backButton, out var value14))
		{
			_backButton = value14.As<NBackButton>();
		}
		if (info.TryGetProperty(PropertyName._unreadyButton, out var value15))
		{
			_unreadyButton = value15.As<NBackButton>();
		}
		if (info.TryGetProperty(PropertyName._embarkButton, out var value16))
		{
			_embarkButton = value16.As<NConfirmButton>();
		}
		if (info.TryGetProperty(PropertyName._ascensionPanel, out var value17))
		{
			_ascensionPanel = value17.As<NAscensionPanel>();
		}
		if (info.TryGetProperty(PropertyName._actDropdown, out var value18))
		{
			_actDropdown = value18.As<NActDropdown>();
		}
		if (info.TryGetProperty(PropertyName._actDropdownLabel, out var value19))
		{
			_actDropdownLabel = value19.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._remotePlayerContainer, out var value20))
		{
			_remotePlayerContainer = value20.As<NRemoteLobbyPlayerContainer>();
		}
		if (info.TryGetProperty(PropertyName._characterUnlockAnimationBackstop, out var value21))
		{
			_characterUnlockAnimationBackstop = value21.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._randomCharacterButton, out var value22))
		{
			_randomCharacterButton = value22.As<NCharacterSelectButton>();
		}
		if (info.TryGetProperty(PropertyName._infoPanelTween, out var value23))
		{
			_infoPanelTween = value23.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._infoPanelPosFinalVal, out var value24))
		{
			_infoPanelPosFinalVal = value24.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._charSelectButtonScene, out var value25))
		{
			_charSelectButtonScene = value25.As<PackedScene>();
		}
	}
}
