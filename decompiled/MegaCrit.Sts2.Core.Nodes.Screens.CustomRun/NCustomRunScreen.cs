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
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Lobby;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.Unlocks;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.CustomRun;

[ScriptPath("res://src/Core/Nodes/Screens/CustomRun/NCustomRunScreen.cs")]
public class NCustomRunScreen : NSubmenu, IStartRunLobbyListener, ICharacterSelectButtonDelegate
{
	public new class MethodName : NSubmenu.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName InitializeSingleplayer = "InitializeSingleplayer";

		public static readonly StringName OnSeedInputSubmitted = "OnSeedInputSubmitted";

		public static readonly StringName InitCharacterButtons = "InitCharacterButtons";

		public new static readonly StringName _Input = "_Input";

		public static readonly StringName DebugUnlockAllCharacters = "DebugUnlockAllCharacters";

		public new static readonly StringName OnSubmenuOpened = "OnSubmenuOpened";

		public new static readonly StringName OnSubmenuClosed = "OnSubmenuClosed";

		public static readonly StringName OnEmbarkPressed = "OnEmbarkPressed";

		public static readonly StringName OnUnreadyPressed = "OnUnreadyPressed";

		public static readonly StringName UpdateRichPresence = "UpdateRichPresence";

		public new static readonly StringName _Process = "_Process";

		public static readonly StringName CleanUpLobby = "CleanUpLobby";

		public static readonly StringName GetModifiersString = "GetModifiersString";

		public static readonly StringName OnAscensionPanelLevelChanged = "OnAscensionPanelLevelChanged";

		public static readonly StringName OnModifiersListChanged = "OnModifiersListChanged";

		public static readonly StringName MaxAscensionChanged = "MaxAscensionChanged";

		public static readonly StringName AscensionChanged = "AscensionChanged";

		public static readonly StringName SeedChanged = "SeedChanged";

		public static readonly StringName ModifiersChanged = "ModifiersChanged";

		public static readonly StringName AfterInitialized = "AfterInitialized";

		public static readonly StringName UpdateControllerButton = "UpdateControllerButton";

		public static readonly StringName TryFocusOnModifiersList = "TryFocusOnModifiersList";
	}

	public new class PropertyName : NSubmenu.PropertyName
	{
		public static readonly StringName ModifiersHotkey = "ModifiersHotkey";

		public new static readonly StringName InitialFocusedControl = "InitialFocusedControl";

		public static readonly StringName _selectedButton = "_selectedButton";

		public static readonly StringName _charButtonContainer = "_charButtonContainer";

		public static readonly StringName _confirmButton = "_confirmButton";

		public new static readonly StringName _backButton = "_backButton";

		public static readonly StringName _unreadyButton = "_unreadyButton";

		public static readonly StringName _ascensionPanel = "_ascensionPanel";

		public static readonly StringName _readyAndWaitingContainer = "_readyAndWaitingContainer";

		public static readonly StringName _seedInput = "_seedInput";

		public static readonly StringName _remotePlayerContainer = "_remotePlayerContainer";

		public static readonly StringName _modifiersList = "_modifiersList";

		public static readonly StringName _modifiersHotkeyIcon = "_modifiersHotkeyIcon";

		public static readonly StringName _uiMode = "_uiMode";
	}

	public new class SignalName : NSubmenu.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("screens/custom_run/custom_run_screen");

	private const string _sceneCharSelectButtonPath = "res://scenes/screens/char_select/char_select_button.tscn";

	private NCharacterSelectButton? _selectedButton;

	private Control _charButtonContainer;

	private NConfirmButton _confirmButton;

	private NBackButton _backButton;

	private NBackButton _unreadyButton;

	private NAscensionPanel _ascensionPanel;

	private Control _readyAndWaitingContainer;

	private LineEdit _seedInput;

	private NRemoteLobbyPlayerContainer _remotePlayerContainer;

	private NCustomRunModifiersList _modifiersList;

	private TextureRect _modifiersHotkeyIcon;

	private StartRunLobby _lobby;

	private MultiplayerUiMode _uiMode;

	private string ModifiersHotkey => MegaInput.topPanel;

	public StartRunLobby Lobby => _lobby;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlyArray<string>(new string[3] { _scenePath, "res://scenes/screens/char_select/char_select_button.tscn", "res://scenes/screens/custom_run/modifier_tickbox.tscn" });

	protected override Control InitialFocusedControl => _charButtonContainer.GetChild<Control>(0);

	public static NCustomRunScreen? Create()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		return PreloadManager.Cache.GetScene(_scenePath).Instantiate<NCustomRunScreen>(PackedScene.GenEditState.Disabled);
	}

	public override void _Ready()
	{
		ConnectSignals();
		_charButtonContainer = GetNode<Control>("LeftContainer/CharSelectButtons/ButtonContainer");
		_ascensionPanel = GetNode<NAscensionPanel>("%AscensionPanel");
		_remotePlayerContainer = GetNode<NRemoteLobbyPlayerContainer>("%RemotePlayerContainer");
		_readyAndWaitingContainer = GetNode<Control>("%ReadyAndWaitingPanel");
		_modifiersList = GetNode<NCustomRunModifiersList>("%ModifiersList");
		_seedInput = GetNode<LineEdit>("%SeedInput");
		_confirmButton = GetNode<NConfirmButton>("ConfirmButton");
		_backButton = GetNode<NBackButton>("BackButton");
		_unreadyButton = GetNode<NBackButton>("UnreadyButton");
		_modifiersHotkeyIcon = GetNode<TextureRect>("%ModifiersHotkeyIcon");
		_confirmButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnEmbarkPressed));
		_unreadyButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnUnreadyPressed));
		_ascensionPanel.Connect(NAscensionPanel.SignalName.AscensionLevelChanged, Callable.From(OnAscensionPanelLevelChanged));
		_modifiersList.Connect(NCustomRunModifiersList.SignalName.ModifiersChanged, Callable.From(OnModifiersListChanged));
		base.ProcessMode = ProcessModeEnum.Disabled;
		GetNode<MegaLabel>("%CustomModeTitle").SetTextAutoSize(new LocString("main_menu_ui", "CUSTOM_RUN_SCREEN.CUSTOM_MODE_TITLE").GetFormattedText());
		GetNode<MegaLabel>("%ModifiersTitle").SetTextAutoSize(new LocString("main_menu_ui", "CUSTOM_RUN_SCREEN.MODIFIERS_TITLE").GetFormattedText());
		GetNode<MegaLabel>("%SeedLabel").SetTextAutoSize(new LocString("main_menu_ui", "CUSTOM_RUN_SCREEN.SEED_LABEL").GetFormattedText());
		_seedInput.PlaceholderText = new LocString("main_menu_ui", "CUSTOM_RUN_SCREEN.SEED_RANDOM_PLACEHOLDER").GetFormattedText();
		_seedInput.Connect(LineEdit.SignalName.TextChanged, Callable.From<string>(OnSeedInputSubmitted));
		InitCharacterButtons();
		NControllerManager.Instance.Connect(NControllerManager.SignalName.MouseDetected, Callable.From(UpdateControllerButton));
		NControllerManager.Instance.Connect(NControllerManager.SignalName.ControllerDetected, Callable.From(UpdateControllerButton));
		NInputManager.Instance.Connect(NInputManager.SignalName.InputRebound, Callable.From(UpdateControllerButton));
	}

	public void InitializeMultiplayerAsHost(INetGameService gameService, int maxPlayers)
	{
		if (gameService.Type != NetGameType.Host)
		{
			throw new InvalidOperationException($"Initialized character select screen with GameService of type {gameService.Type} when hosting!");
		}
		_lobby = new StartRunLobby(GameMode.Custom, gameService, this, maxPlayers);
		_ascensionPanel.Initialize(MultiplayerUiMode.Host);
		_modifiersList.Initialize(MultiplayerUiMode.Host);
		_lobby.AddLocalHostPlayer(new UnlockState(SaveManager.Instance.Progress), SaveManager.Instance.Progress.MaxMultiplayerAscension);
		_uiMode = MultiplayerUiMode.Host;
		_remotePlayerContainer.Visible = true;
		UpdateControllerButton();
		AfterInitialized();
	}

	public void InitializeMultiplayerAsClient(INetGameService gameService, ClientLobbyJoinResponseMessage message)
	{
		if (gameService.Type != NetGameType.Client)
		{
			throw new InvalidOperationException($"Initialized character select screen with GameService of type {gameService.Type} when joining!");
		}
		_lobby = new StartRunLobby(GameMode.Custom, gameService, this, -1);
		_ascensionPanel.Initialize(MultiplayerUiMode.Client);
		_modifiersList.Initialize(MultiplayerUiMode.Client);
		_lobby.InitializeFromMessage(message);
		_seedInput.Editable = false;
		_uiMode = MultiplayerUiMode.Client;
		UpdateControllerButton();
		AfterInitialized();
	}

	public void InitializeSingleplayer()
	{
		_lobby = new StartRunLobby(GameMode.Custom, new NetSingleplayerGameService(), this, 1);
		_remotePlayerContainer.Visible = false;
		_ascensionPanel.Initialize(MultiplayerUiMode.Singleplayer);
		_modifiersList.Initialize(MultiplayerUiMode.Singleplayer);
		_lobby.AddLocalHostPlayer(new UnlockState(SaveManager.Instance.Progress), 0);
		_uiMode = MultiplayerUiMode.Singleplayer;
		UpdateControllerButton();
		AfterInitialized();
	}

	private void OnSeedInputSubmitted(string newText)
	{
		if (newText != string.Empty)
		{
			Lobby.SetSeed(newText);
		}
		else
		{
			Lobby.SetSeed(null);
		}
	}

	private void InitCharacterButtons()
	{
		foreach (CharacterModel allCharacter in ModelDb.AllCharacters)
		{
			NCharacterSelectButton nCharacterSelectButton = PreloadManager.Cache.GetScene("res://scenes/screens/char_select/char_select_button.tscn").Instantiate<NCharacterSelectButton>(PackedScene.GenEditState.Disabled);
			nCharacterSelectButton.Name = allCharacter.Id.Entry + "_button";
			_charButtonContainer.AddChildSafely(nCharacterSelectButton);
			nCharacterSelectButton.Init(allCharacter, this);
		}
		for (int i = 0; i < _charButtonContainer.GetChildCount(); i++)
		{
			Control child = _charButtonContainer.GetChild<Control>(i);
			child.FocusNeighborLeft = ((i > 0) ? _charButtonContainer.GetChild<Control>(i - 1).GetPath() : child.GetPath());
			child.FocusNeighborRight = ((i < _charButtonContainer.GetChildCount() - 1) ? _charButtonContainer.GetChild<Control>(i + 1).GetPath() : child.GetPath());
			child.FocusNeighborTop = _seedInput.GetPath();
			child.FocusNeighborBottom = child.GetPath();
		}
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
				item.Reset();
			}
			else
			{
				item.UnlockIfPossible();
			}
		}
		_confirmButton.Enable();
		_charButtonContainer.GetChild<NCharacterSelectButton>(0).Select();
		_remotePlayerContainer.Initialize(_lobby, displayLocalPlayer: true);
		if (_lobby.NetService.Type == NetGameType.Client)
		{
			_ascensionPanel.SetAscensionLevel(_lobby.Ascension);
			_seedInput.Text = _lobby.Seed ?? "";
		}
		_readyAndWaitingContainer.Visible = false;
		foreach (LobbyPlayer player in _lobby.Players)
		{
			RefreshButtonSelectionForPlayer(player);
		}
		base.ProcessMode = ProcessModeEnum.Inherit;
		NHotkeyManager.Instance.PushHotkeyPressedBinding(ModifiersHotkey, TryFocusOnModifiersList);
	}

	public override void OnSubmenuClosed()
	{
		base.OnSubmenuClosed();
		_confirmButton.Disable();
		_remotePlayerContainer.Cleanup();
		if (_lobby.NetService.Type.IsMultiplayer())
		{
			PlatformUtil.SetRichPresence("MAIN_MENU", null, null);
		}
		CleanUpLobby(disconnectSession: true);
		NHotkeyManager.Instance.RemoveHotkeyPressedBinding(ModifiersHotkey, TryFocusOnModifiersList);
	}

	private void OnEmbarkPressed(NButton _)
	{
		_confirmButton.Disable();
		_backButton.Disable();
		_lobby.SetReady(ready: true);
		foreach (NCharacterSelectButton item in _charButtonContainer.GetChildren().OfType<NCharacterSelectButton>())
		{
			item.Disable();
		}
		if (_lobby.NetService.Type.IsMultiplayer() && !_lobby.IsAboutToBeginGame())
		{
			_unreadyButton.Enable();
			_readyAndWaitingContainer.Visible = true;
		}
	}

	private void OnUnreadyPressed(NButton _)
	{
		_confirmButton.Enable();
		_backButton.Enable();
		_unreadyButton.Disable();
		_lobby.SetReady(ready: false);
		_readyAndWaitingContainer.Visible = false;
		foreach (NCharacterSelectButton item in _charButtonContainer.GetChildren().OfType<NCharacterSelectButton>())
		{
			item.Enable();
		}
	}

	private void UpdateRichPresence()
	{
		if (_lobby.NetService.Type.IsMultiplayer())
		{
			PlatformUtil.SetRichPresence("CUSTOM_MP_LOBBY", _lobby.NetService.GetRawLobbyIdentifier(), _lobby.Players.Count);
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

	private async Task StartNewSingleplayerRun(string seed, List<ActModel> acts, IReadOnlyList<ModifierModel> modifiers)
	{
		Log.Info($"Embarking on a CUSTOM {_lobby.LocalPlayer.character.Id.Entry} run. Ascension: {_lobby.Ascension} Seed: {_lobby.Seed} Modifiers: {GetModifiersString()}");
		SfxCmd.Play(_lobby.LocalPlayer.character.CharacterTransitionSfx);
		await NGame.Instance.Transition.FadeOut(0.8f, _lobby.LocalPlayer.character.CharacterSelectTransitionPath);
		await NGame.Instance.StartNewSingleplayerRun(_lobby.LocalPlayer.character, shouldSave: true, acts, modifiers, seed, _lobby.Ascension);
		CleanUpLobby(disconnectSession: false);
	}

	private async Task StartNewMultiplayerRun(string seed, List<ActModel> acts, IReadOnlyList<ModifierModel> modifiers)
	{
		Log.Info($"Embarking on a CUSTOM multiplayer run. Players: {string.Join(",", _lobby.Players)}. Ascension: {_lobby.Ascension} Seed: {_lobby.Seed} Modifiers: {GetModifiersString()}");
		SfxCmd.Play(_lobby.LocalPlayer.character.CharacterTransitionSfx);
		await NGame.Instance.Transition.FadeOut(0.8f, _lobby.LocalPlayer.character.CharacterSelectTransitionPath);
		await NGame.Instance.StartNewMultiplayerRun(_lobby, shouldSave: true, acts, modifiers, seed, _lobby.Ascension);
		CleanUpLobby(disconnectSession: false);
	}

	private string GetModifiersString()
	{
		return string.Join(",", _lobby.Modifiers.Select((ModifierModel m) => m.Id));
	}

	public void SelectCharacter(NCharacterSelectButton charSelectButton, CharacterModel characterModel)
	{
		if (_lobby == null)
		{
			throw new InvalidOperationException("Cannot select character while loading!");
		}
		SfxCmd.Play(characterModel.CharacterSelectSfx);
		_selectedButton = charSelectButton;
		foreach (NCharacterSelectButton item in _charButtonContainer.GetChildren().OfType<NCharacterSelectButton>())
		{
			if (item != _selectedButton)
			{
				item.Deselect();
			}
		}
		_lobby.SetLocalCharacter(characterModel);
	}

	private void OnAscensionPanelLevelChanged()
	{
		if (_lobby.NetService.Type != NetGameType.Client && _lobby.Ascension != _ascensionPanel.Ascension)
		{
			_lobby.SyncAscensionChange(_ascensionPanel.Ascension);
		}
	}

	private void OnModifiersListChanged()
	{
		if (_lobby.NetService.Type != NetGameType.Client)
		{
			Lobby.SetModifiers(_modifiersList.GetModifiersTickedOn());
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
		NetGameType type = _lobby.NetService.Type;
		if ((uint)(type - 1) > 1u)
		{
			_seedInput.Text = Lobby.Seed;
		}
	}

	public void ModifiersChanged()
	{
		NetGameType type = _lobby.NetService.Type;
		if ((uint)(type - 1) > 1u)
		{
			_modifiersList.SyncModifierList(Lobby.Modifiers);
		}
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

	private void AfterInitialized()
	{
		NGame.Instance.RemoteCursorContainer.Initialize(_lobby.InputSynchronizer, _lobby.Players.Select((LobbyPlayer p) => p.id));
		NGame.Instance.ReactionContainer.InitializeNetworking(_lobby.NetService);
		NGame.Instance.TimeoutOverlay.Initialize(_lobby.NetService, isGameLevel: true);
		UpdateRichPresence();
		if (!string.IsNullOrEmpty(_seedInput.Text))
		{
			_lobby.SetSeed(_seedInput.Text);
		}
		MegaCrit.Sts2.Core.Logging.Logger.logLevelTypeMap[LogType.Network] = ((_lobby.NetService.Type == NetGameType.Singleplayer) ? LogLevel.Info : LogLevel.Debug);
		MegaCrit.Sts2.Core.Logging.Logger.logLevelTypeMap[LogType.Actions] = ((_lobby.NetService.Type == NetGameType.Singleplayer) ? LogLevel.Info : LogLevel.VeryDebug);
		MegaCrit.Sts2.Core.Logging.Logger.logLevelTypeMap[LogType.GameSync] = ((_lobby.NetService.Type == NetGameType.Singleplayer) ? LogLevel.Info : LogLevel.VeryDebug);
		NGame.Instance.DebugSeedOverride = null;
	}

	private void UpdateControllerButton()
	{
		MultiplayerUiMode uiMode = _uiMode;
		if ((uint)(uiMode - 1) <= 1u)
		{
			_modifiersHotkeyIcon.Visible = NControllerManager.Instance.IsUsingController;
			_modifiersHotkeyIcon.Texture = NInputManager.Instance.GetHotkeyIcon(ModifiersHotkey);
		}
		else
		{
			_modifiersHotkeyIcon.Visible = false;
		}
	}

	private void TryFocusOnModifiersList()
	{
		Control control = GetViewport().GuiGetFocusOwner();
		if (control == null || !_modifiersList.IsAncestorOf(control))
		{
			MultiplayerUiMode uiMode = _uiMode;
			if ((uint)(uiMode - 1) <= 1u)
			{
				_modifiersList.DefaultFocusedControl?.TryGrabFocus();
			}
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(24);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.InitializeSingleplayer, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSeedInputSubmitted, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "newText", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.InitCharacterButtons, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		list.Add(new MethodInfo(MethodName.OnUnreadyPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateRichPresence, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.CleanUpLobby, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "disconnectSession", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetModifiersString, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnAscensionPanelLevelChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnModifiersListChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.MaxAscensionChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AscensionChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SeedChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ModifiersChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AfterInitialized, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateControllerButton, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TryFocusOnModifiersList, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NCustomRunScreen>(Create());
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
		if (method == MethodName.OnSeedInputSubmitted && args.Count == 1)
		{
			OnSeedInputSubmitted(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.InitCharacterButtons && args.Count == 0)
		{
			InitCharacterButtons();
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
		if (method == MethodName.GetModifiersString && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<string>(GetModifiersString());
			return true;
		}
		if (method == MethodName.OnAscensionPanelLevelChanged && args.Count == 0)
		{
			OnAscensionPanelLevelChanged();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnModifiersListChanged && args.Count == 0)
		{
			OnModifiersListChanged();
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
		if (method == MethodName.UpdateControllerButton && args.Count == 0)
		{
			UpdateControllerButton();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TryFocusOnModifiersList && args.Count == 0)
		{
			TryFocusOnModifiersList();
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
			ret = VariantUtils.CreateFrom<NCustomRunScreen>(Create());
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
		if (method == MethodName.OnSeedInputSubmitted)
		{
			return true;
		}
		if (method == MethodName.InitCharacterButtons)
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
		if (method == MethodName.OnUnreadyPressed)
		{
			return true;
		}
		if (method == MethodName.UpdateRichPresence)
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
		if (method == MethodName.GetModifiersString)
		{
			return true;
		}
		if (method == MethodName.OnAscensionPanelLevelChanged)
		{
			return true;
		}
		if (method == MethodName.OnModifiersListChanged)
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
		if (method == MethodName.UpdateControllerButton)
		{
			return true;
		}
		if (method == MethodName.TryFocusOnModifiersList)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
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
		if (name == PropertyName._confirmButton)
		{
			_confirmButton = VariantUtils.ConvertTo<NConfirmButton>(in value);
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
		if (name == PropertyName._ascensionPanel)
		{
			_ascensionPanel = VariantUtils.ConvertTo<NAscensionPanel>(in value);
			return true;
		}
		if (name == PropertyName._readyAndWaitingContainer)
		{
			_readyAndWaitingContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._seedInput)
		{
			_seedInput = VariantUtils.ConvertTo<LineEdit>(in value);
			return true;
		}
		if (name == PropertyName._remotePlayerContainer)
		{
			_remotePlayerContainer = VariantUtils.ConvertTo<NRemoteLobbyPlayerContainer>(in value);
			return true;
		}
		if (name == PropertyName._modifiersList)
		{
			_modifiersList = VariantUtils.ConvertTo<NCustomRunModifiersList>(in value);
			return true;
		}
		if (name == PropertyName._modifiersHotkeyIcon)
		{
			_modifiersHotkeyIcon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._uiMode)
		{
			_uiMode = VariantUtils.ConvertTo<MultiplayerUiMode>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.ModifiersHotkey)
		{
			value = VariantUtils.CreateFrom<string>(ModifiersHotkey);
			return true;
		}
		if (name == PropertyName.InitialFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(InitialFocusedControl);
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
		if (name == PropertyName._confirmButton)
		{
			value = VariantUtils.CreateFrom(in _confirmButton);
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
		if (name == PropertyName._ascensionPanel)
		{
			value = VariantUtils.CreateFrom(in _ascensionPanel);
			return true;
		}
		if (name == PropertyName._readyAndWaitingContainer)
		{
			value = VariantUtils.CreateFrom(in _readyAndWaitingContainer);
			return true;
		}
		if (name == PropertyName._seedInput)
		{
			value = VariantUtils.CreateFrom(in _seedInput);
			return true;
		}
		if (name == PropertyName._remotePlayerContainer)
		{
			value = VariantUtils.CreateFrom(in _remotePlayerContainer);
			return true;
		}
		if (name == PropertyName._modifiersList)
		{
			value = VariantUtils.CreateFrom(in _modifiersList);
			return true;
		}
		if (name == PropertyName._modifiersHotkeyIcon)
		{
			value = VariantUtils.CreateFrom(in _modifiersHotkeyIcon);
			return true;
		}
		if (name == PropertyName._uiMode)
		{
			value = VariantUtils.CreateFrom(in _uiMode);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName.ModifiersHotkey, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._selectedButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._charButtonContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._confirmButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._backButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._unreadyButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._ascensionPanel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._readyAndWaitingContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._seedInput, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._remotePlayerContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._modifiersList, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._modifiersHotkeyIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.InitialFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._uiMode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._selectedButton, Variant.From(in _selectedButton));
		info.AddProperty(PropertyName._charButtonContainer, Variant.From(in _charButtonContainer));
		info.AddProperty(PropertyName._confirmButton, Variant.From(in _confirmButton));
		info.AddProperty(PropertyName._backButton, Variant.From(in _backButton));
		info.AddProperty(PropertyName._unreadyButton, Variant.From(in _unreadyButton));
		info.AddProperty(PropertyName._ascensionPanel, Variant.From(in _ascensionPanel));
		info.AddProperty(PropertyName._readyAndWaitingContainer, Variant.From(in _readyAndWaitingContainer));
		info.AddProperty(PropertyName._seedInput, Variant.From(in _seedInput));
		info.AddProperty(PropertyName._remotePlayerContainer, Variant.From(in _remotePlayerContainer));
		info.AddProperty(PropertyName._modifiersList, Variant.From(in _modifiersList));
		info.AddProperty(PropertyName._modifiersHotkeyIcon, Variant.From(in _modifiersHotkeyIcon));
		info.AddProperty(PropertyName._uiMode, Variant.From(in _uiMode));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._selectedButton, out var value))
		{
			_selectedButton = value.As<NCharacterSelectButton>();
		}
		if (info.TryGetProperty(PropertyName._charButtonContainer, out var value2))
		{
			_charButtonContainer = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._confirmButton, out var value3))
		{
			_confirmButton = value3.As<NConfirmButton>();
		}
		if (info.TryGetProperty(PropertyName._backButton, out var value4))
		{
			_backButton = value4.As<NBackButton>();
		}
		if (info.TryGetProperty(PropertyName._unreadyButton, out var value5))
		{
			_unreadyButton = value5.As<NBackButton>();
		}
		if (info.TryGetProperty(PropertyName._ascensionPanel, out var value6))
		{
			_ascensionPanel = value6.As<NAscensionPanel>();
		}
		if (info.TryGetProperty(PropertyName._readyAndWaitingContainer, out var value7))
		{
			_readyAndWaitingContainer = value7.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._seedInput, out var value8))
		{
			_seedInput = value8.As<LineEdit>();
		}
		if (info.TryGetProperty(PropertyName._remotePlayerContainer, out var value9))
		{
			_remotePlayerContainer = value9.As<NRemoteLobbyPlayerContainer>();
		}
		if (info.TryGetProperty(PropertyName._modifiersList, out var value10))
		{
			_modifiersList = value10.As<NCustomRunModifiersList>();
		}
		if (info.TryGetProperty(PropertyName._modifiersHotkeyIcon, out var value11))
		{
			_modifiersHotkeyIcon = value11.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._uiMode, out var value12))
		{
			_uiMode = value12.As<MultiplayerUiMode>();
		}
	}
}
