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
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Lobby;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

[ScriptPath("res://src/Core/Nodes/Screens/CharacterSelect/NMultiplayerLoadGameScreen.cs")]
public class NMultiplayerLoadGameScreen : NSubmenu, ILoadRunLobbyListener
{
	public new class MethodName : NSubmenu.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName OnSubmenuOpened = "OnSubmenuOpened";

		public new static readonly StringName OnSubmenuShown = "OnSubmenuShown";

		public new static readonly StringName OnSubmenuClosed = "OnSubmenuClosed";

		public new static readonly StringName OnSubmenuHidden = "OnSubmenuHidden";

		public static readonly StringName OnEmbarkPressed = "OnEmbarkPressed";

		public static readonly StringName OnUnreadyPressed = "OnUnreadyPressed";

		public static readonly StringName UpdateRichPresence = "UpdateRichPresence";

		public new static readonly StringName _Process = "_Process";

		public static readonly StringName CleanUpLobby = "CleanUpLobby";

		public static readonly StringName PlayerConnected = "PlayerConnected";

		public static readonly StringName PlayerReadyChanged = "PlayerReadyChanged";

		public static readonly StringName RemotePlayerDisconnected = "RemotePlayerDisconnected";

		public static readonly StringName BeginRun = "BeginRun";

		public static readonly StringName AfterMultiplayerStarted = "AfterMultiplayerStarted";
	}

	public new class PropertyName : NSubmenu.PropertyName
	{
		public new static readonly StringName InitialFocusedControl = "InitialFocusedControl";

		public static readonly StringName _name = "_name";

		public static readonly StringName _infoPanel = "_infoPanel";

		public static readonly StringName _hp = "_hp";

		public static readonly StringName _gold = "_gold";

		public static readonly StringName _selectedButton = "_selectedButton";

		public static readonly StringName _bgContainer = "_bgContainer";

		public static readonly StringName _confirmButton = "_confirmButton";

		public new static readonly StringName _backButton = "_backButton";

		public static readonly StringName _unreadyButton = "_unreadyButton";

		public static readonly StringName _ascensionPanel = "_ascensionPanel";

		public static readonly StringName _floorLabel = "_floorLabel";

		public static readonly StringName _actLabel = "_actLabel";

		public static readonly StringName _remotePlayerContainer = "_remotePlayerContainer";

		public static readonly StringName _infoPanelTween = "_infoPanelTween";

		public static readonly StringName _infoPanelPosFinalVal = "_infoPanelPosFinalVal";
	}

	public new class SignalName : NSubmenu.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("screens/multiplayer_load_game_screen");

	private MegaLabel _name;

	private Control _infoPanel;

	private MegaLabel _hp;

	private MegaLabel _gold;

	private NCharacterSelectButton? _selectedButton;

	private Control _bgContainer;

	private NConfirmButton _confirmButton;

	private NBackButton _backButton;

	private NBackButton _unreadyButton;

	private NAscensionPanel _ascensionPanel;

	private MegaRichTextLabel _floorLabel;

	private MegaRichTextLabel _actLabel;

	private NRemoteLoadLobbyPlayerContainer _remotePlayerContainer;

	private Tween? _infoPanelTween;

	private Vector2 _infoPanelPosFinalVal;

	private const string _sceneCharSelectButtonPath = "res://scenes/screens/char_select/char_select_button.tscn";

	private LoadRunLobby _runLobby;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { _scenePath, "res://scenes/screens/char_select/char_select_button.tscn" });

	protected override Control? InitialFocusedControl => null;

	public static NMultiplayerLoadGameScreen? Create()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		return PreloadManager.Cache.GetScene(_scenePath).Instantiate<NMultiplayerLoadGameScreen>(PackedScene.GenEditState.Disabled);
	}

	public override void _Ready()
	{
		ConnectSignals();
		_infoPanel = GetNode<Control>("InfoPanel");
		_name = GetNode<MegaLabel>("InfoPanel/VBoxContainer/Name");
		_hp = GetNode<MegaLabel>("InfoPanel/VBoxContainer/HpGoldSpacer/HpGold/Hp/Label");
		_gold = GetNode<MegaLabel>("InfoPanel/VBoxContainer/HpGoldSpacer/HpGold/Gold/Label");
		_actLabel = GetNode<MegaRichTextLabel>("InfoPanel/VBoxContainer/RunLocation/ActLabel");
		_floorLabel = GetNode<MegaRichTextLabel>("InfoPanel/VBoxContainer/RunLocation/FloorLabel");
		_bgContainer = GetNode<Control>("AnimatedBg");
		_ascensionPanel = GetNode<NAscensionPanel>("%AscensionPanel");
		_remotePlayerContainer = GetNode<NRemoteLoadLobbyPlayerContainer>("RemotePlayerLoadContainer");
		_confirmButton = GetNode<NConfirmButton>("ConfirmButton");
		_backButton = GetNode<NBackButton>("BackButton");
		_unreadyButton = GetNode<NBackButton>("UnreadyButton");
		_confirmButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnEmbarkPressed));
		_unreadyButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnUnreadyPressed));
		_unreadyButton.Disable();
		base.ProcessMode = ProcessModeEnum.Disabled;
	}

	public void InitializeAsHost(INetGameService gameService, SerializableRun run)
	{
		if (gameService.Type != NetGameType.Host)
		{
			throw new InvalidOperationException($"Initialized character select screen with GameService of type {gameService.Type} when hosting!");
		}
		_runLobby = new LoadRunLobby(gameService, this, run);
		try
		{
			_runLobby.AddLocalHostPlayer();
			AfterMultiplayerStarted();
		}
		catch
		{
			CleanUpLobby(disconnectSession: true);
			throw;
		}
	}

	public void InitializeAsClient(INetGameService gameService, ClientLoadJoinResponseMessage message)
	{
		if (gameService.Type != NetGameType.Client)
		{
			throw new InvalidOperationException($"Initialized character select screen with GameService of type {gameService.Type} when joining!");
		}
		_runLobby = new LoadRunLobby(gameService, this, message);
		AfterMultiplayerStarted();
	}

	public override void OnSubmenuOpened()
	{
		base.OnSubmenuOpened();
		_confirmButton.Enable();
		_remotePlayerContainer.Initialize(_runLobby, displayLocalPlayer: false);
		_ascensionPanel.Initialize(MultiplayerUiMode.Load);
		_ascensionPanel.SetAscensionLevel(_runLobby.Run.Ascension);
	}

	protected override void OnSubmenuShown()
	{
		base.ProcessMode = ProcessModeEnum.Inherit;
	}

	public override void OnSubmenuClosed()
	{
		base.OnSubmenuClosed();
		_confirmButton.Disable();
		_remotePlayerContainer.Cleanup();
		if (_runLobby.NetService.Type.IsMultiplayer())
		{
			PlatformUtil.SetRichPresence("MAIN_MENU", null, null);
		}
		CleanUpLobby(disconnectSession: true);
	}

	protected override void OnSubmenuHidden()
	{
		base.ProcessMode = ProcessModeEnum.Disabled;
	}

	private void OnEmbarkPressed(NButton _)
	{
		_confirmButton.Disable();
		_backButton.Disable();
		_unreadyButton.Enable();
		_runLobby.SetReady(ready: true);
	}

	private void OnUnreadyPressed(NButton _)
	{
		_confirmButton.Enable();
		_backButton.Enable();
		_unreadyButton.Disable();
		_runLobby.SetReady(ready: false);
	}

	private void UpdateRichPresence()
	{
		if (_runLobby.NetService.Type.IsMultiplayer())
		{
			PlatformUtil.SetRichPresence("LOADING_MP_LOBBY", _runLobby.NetService.GetRawLobbyIdentifier(), _runLobby.ConnectedPlayerIds.Count);
		}
	}

	public override void _Process(double delta)
	{
		if (_runLobby.NetService.IsConnected)
		{
			_runLobby.NetService.Update();
		}
	}

	private void CleanUpLobby(bool disconnectSession)
	{
		_runLobby.CleanUp(disconnectSession);
		_runLobby = null;
	}

	public async Task<bool> ShouldAllowRunToBegin()
	{
		if (_runLobby.ConnectedPlayerIds.Count >= _runLobby.Run.Players.Count)
		{
			return true;
		}
		LocString locString = new LocString("gameplay_ui", "CONFIRM_LOAD_SAVE.body");
		locString.Add("MissingCount", _runLobby.Run.Players.Count - _runLobby.ConnectedPlayerIds.Count);
		NGenericPopup nGenericPopup = NGenericPopup.Create();
		NModalContainer.Instance.Add(nGenericPopup);
		return await nGenericPopup.WaitForConfirmation(locString, new LocString("gameplay_ui", "CONFIRM_LOAD_SAVE.header"), new LocString("gameplay_ui", "CONFIRM_LOAD_SAVE.cancel"), new LocString("gameplay_ui", "CONFIRM_LOAD_SAVE.confirm"));
	}

	private async Task StartRun()
	{
		Log.Info("Loading a multiplayer run. Players: " + string.Join(",", _runLobby.ConnectedPlayerIds) + ".");
		SerializablePlayer serializablePlayer = _runLobby.Run.Players.First((SerializablePlayer p) => p.NetId == _runLobby.NetService.NetId);
		SfxCmd.Play(ModelDb.GetById<CharacterModel>(serializablePlayer.CharacterId).CharacterTransitionSfx);
		await NGame.Instance.Transition.FadeOut(0.8f, ModelDb.GetById<CharacterModel>(serializablePlayer.CharacterId).CharacterSelectTransitionPath);
		RunState runState = RunState.FromSerializable(_runLobby.Run);
		RunManager.Instance.SetUpSavedMultiPlayer(runState, _runLobby);
		await NGame.Instance.LoadRun(runState, _runLobby.Run.PreFinishedRoom);
		CleanUpLobby(disconnectSession: false);
		await NGame.Instance.Transition.FadeIn();
	}

	public void PlayerConnected(ulong playerId)
	{
		Log.Info($"Player connected: {playerId}");
		_remotePlayerContainer.OnPlayerConnected(playerId);
		UpdateRichPresence();
	}

	public void PlayerReadyChanged(ulong playerId)
	{
		Log.Info($"Player ready changed: {playerId}");
		_remotePlayerContainer.OnPlayerChanged(playerId);
		if (playerId == _runLobby.NetService.NetId && !_runLobby.IsPlayerReady(playerId))
		{
			_confirmButton.Enable();
		}
	}

	public void RemotePlayerDisconnected(ulong playerId)
	{
		Log.Info($"Player disconnected: {playerId}");
		_remotePlayerContainer.OnPlayerDisconnected(playerId);
		UpdateRichPresence();
	}

	public void BeginRun()
	{
		NAudioManager.Instance?.StopMusic();
		TaskHelper.RunSafely(StartRun());
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

	private void AfterMultiplayerStarted()
	{
		NGame.Instance.RemoteCursorContainer.Initialize(_runLobby.InputSynchronizer, _runLobby.ConnectedPlayerIds);
		NGame.Instance.ReactionContainer.InitializeNetworking(_runLobby.NetService);
		SerializablePlayer serializablePlayer = _runLobby.Run.Players.First((SerializablePlayer p) => p.NetId == _runLobby.NetService.NetId);
		CharacterModel byId = ModelDb.GetById<CharacterModel>(serializablePlayer.CharacterId);
		SfxCmd.Play(byId.CharacterSelectSfx);
		foreach (Node child in _bgContainer.GetChildren())
		{
			_bgContainer.RemoveChildSafely(child);
			child.QueueFreeSafely();
		}
		Control control = PreloadManager.Cache.GetScene(byId.CharacterSelectBg).Instantiate<Control>(PackedScene.GenEditState.Disabled);
		control.Name = byId.Id.Entry + "_bg";
		_bgContainer.AddChildSafely(control);
		_name.SetTextAutoSize(byId.Title.GetFormattedText());
		_hp.SetTextAutoSize($"{serializablePlayer.CurrentHp}/{serializablePlayer.MaxHp}");
		_gold.SetTextAutoSize($"{serializablePlayer.Gold}");
		LocString locString = new LocString("main_menu_ui", "MULTIPLAYER_LOAD_MENU.FLOOR");
		locString.Add("floor", _runLobby.Run.VisitedMapCoords.Count);
		_floorLabel.Text = locString.GetFormattedText();
		LocString locString2 = new LocString("main_menu_ui", "MULTIPLAYER_LOAD_MENU.ACT");
		locString2.Add("act", _runLobby.Run.CurrentActIndex + 1);
		_actLabel.Text = locString2.GetFormattedText();
		UpdateRichPresence();
		MegaCrit.Sts2.Core.Logging.Logger.logLevelTypeMap[LogType.Network] = LogLevel.Debug;
		MegaCrit.Sts2.Core.Logging.Logger.logLevelTypeMap[LogType.Actions] = LogLevel.VeryDebug;
		MegaCrit.Sts2.Core.Logging.Logger.logLevelTypeMap[LogType.GameSync] = LogLevel.VeryDebug;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(16);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuOpened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuShown, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuClosed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuHidden, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		list.Add(new MethodInfo(MethodName.PlayerConnected, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "playerId", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.PlayerReadyChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "playerId", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RemotePlayerDisconnected, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "playerId", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.BeginRun, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AfterMultiplayerStarted, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NMultiplayerLoadGameScreen>(Create());
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSubmenuOpened && args.Count == 0)
		{
			OnSubmenuOpened();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSubmenuShown && args.Count == 0)
		{
			OnSubmenuShown();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSubmenuClosed && args.Count == 0)
		{
			OnSubmenuClosed();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSubmenuHidden && args.Count == 0)
		{
			OnSubmenuHidden();
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
		if (method == MethodName.PlayerConnected && args.Count == 1)
		{
			PlayerConnected(VariantUtils.ConvertTo<ulong>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PlayerReadyChanged && args.Count == 1)
		{
			PlayerReadyChanged(VariantUtils.ConvertTo<ulong>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RemotePlayerDisconnected && args.Count == 1)
		{
			RemotePlayerDisconnected(VariantUtils.ConvertTo<ulong>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.BeginRun && args.Count == 0)
		{
			BeginRun();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AfterMultiplayerStarted && args.Count == 0)
		{
			AfterMultiplayerStarted();
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
			ret = VariantUtils.CreateFrom<NMultiplayerLoadGameScreen>(Create());
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
		if (method == MethodName.OnSubmenuOpened)
		{
			return true;
		}
		if (method == MethodName.OnSubmenuShown)
		{
			return true;
		}
		if (method == MethodName.OnSubmenuClosed)
		{
			return true;
		}
		if (method == MethodName.OnSubmenuHidden)
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
		if (method == MethodName.PlayerConnected)
		{
			return true;
		}
		if (method == MethodName.PlayerReadyChanged)
		{
			return true;
		}
		if (method == MethodName.RemotePlayerDisconnected)
		{
			return true;
		}
		if (method == MethodName.BeginRun)
		{
			return true;
		}
		if (method == MethodName.AfterMultiplayerStarted)
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
		if (name == PropertyName._selectedButton)
		{
			_selectedButton = VariantUtils.ConvertTo<NCharacterSelectButton>(in value);
			return true;
		}
		if (name == PropertyName._bgContainer)
		{
			_bgContainer = VariantUtils.ConvertTo<Control>(in value);
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
		if (name == PropertyName._floorLabel)
		{
			_floorLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._actLabel)
		{
			_actLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._remotePlayerContainer)
		{
			_remotePlayerContainer = VariantUtils.ConvertTo<NRemoteLoadLobbyPlayerContainer>(in value);
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
		if (name == PropertyName._selectedButton)
		{
			value = VariantUtils.CreateFrom(in _selectedButton);
			return true;
		}
		if (name == PropertyName._bgContainer)
		{
			value = VariantUtils.CreateFrom(in _bgContainer);
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
		if (name == PropertyName._floorLabel)
		{
			value = VariantUtils.CreateFrom(in _floorLabel);
			return true;
		}
		if (name == PropertyName._actLabel)
		{
			value = VariantUtils.CreateFrom(in _actLabel);
			return true;
		}
		if (name == PropertyName._remotePlayerContainer)
		{
			value = VariantUtils.CreateFrom(in _remotePlayerContainer);
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
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._name, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._infoPanel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hp, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._gold, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._selectedButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bgContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._confirmButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._backButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._unreadyButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._ascensionPanel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._floorLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._actLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._remotePlayerContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._infoPanelTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._infoPanelPosFinalVal, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.InitialFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._name, Variant.From(in _name));
		info.AddProperty(PropertyName._infoPanel, Variant.From(in _infoPanel));
		info.AddProperty(PropertyName._hp, Variant.From(in _hp));
		info.AddProperty(PropertyName._gold, Variant.From(in _gold));
		info.AddProperty(PropertyName._selectedButton, Variant.From(in _selectedButton));
		info.AddProperty(PropertyName._bgContainer, Variant.From(in _bgContainer));
		info.AddProperty(PropertyName._confirmButton, Variant.From(in _confirmButton));
		info.AddProperty(PropertyName._backButton, Variant.From(in _backButton));
		info.AddProperty(PropertyName._unreadyButton, Variant.From(in _unreadyButton));
		info.AddProperty(PropertyName._ascensionPanel, Variant.From(in _ascensionPanel));
		info.AddProperty(PropertyName._floorLabel, Variant.From(in _floorLabel));
		info.AddProperty(PropertyName._actLabel, Variant.From(in _actLabel));
		info.AddProperty(PropertyName._remotePlayerContainer, Variant.From(in _remotePlayerContainer));
		info.AddProperty(PropertyName._infoPanelTween, Variant.From(in _infoPanelTween));
		info.AddProperty(PropertyName._infoPanelPosFinalVal, Variant.From(in _infoPanelPosFinalVal));
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
		if (info.TryGetProperty(PropertyName._hp, out var value3))
		{
			_hp = value3.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._gold, out var value4))
		{
			_gold = value4.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._selectedButton, out var value5))
		{
			_selectedButton = value5.As<NCharacterSelectButton>();
		}
		if (info.TryGetProperty(PropertyName._bgContainer, out var value6))
		{
			_bgContainer = value6.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._confirmButton, out var value7))
		{
			_confirmButton = value7.As<NConfirmButton>();
		}
		if (info.TryGetProperty(PropertyName._backButton, out var value8))
		{
			_backButton = value8.As<NBackButton>();
		}
		if (info.TryGetProperty(PropertyName._unreadyButton, out var value9))
		{
			_unreadyButton = value9.As<NBackButton>();
		}
		if (info.TryGetProperty(PropertyName._ascensionPanel, out var value10))
		{
			_ascensionPanel = value10.As<NAscensionPanel>();
		}
		if (info.TryGetProperty(PropertyName._floorLabel, out var value11))
		{
			_floorLabel = value11.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._actLabel, out var value12))
		{
			_actLabel = value12.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._remotePlayerContainer, out var value13))
		{
			_remotePlayerContainer = value13.As<NRemoteLoadLobbyPlayerContainer>();
		}
		if (info.TryGetProperty(PropertyName._infoPanelTween, out var value14))
		{
			_infoPanelTween = value14.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._infoPanelPosFinalVal, out var value15))
		{
			_infoPanelPosFinalVal = value15.As<Vector2>();
		}
	}
}
