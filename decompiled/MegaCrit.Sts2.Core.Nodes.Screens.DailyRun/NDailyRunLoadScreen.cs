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

namespace MegaCrit.Sts2.Core.Nodes.Screens.DailyRun;

[ScriptPath("res://src/Core/Nodes/Screens/DailyRun/NDailyRunLoadScreen.cs")]
public class NDailyRunLoadScreen : NSubmenu, ILoadRunLobbyListener
{
	public new class MethodName : NSubmenu.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName OnSubmenuOpened = "OnSubmenuOpened";

		public new static readonly StringName OnSubmenuClosed = "OnSubmenuClosed";

		public new static readonly StringName OnSubmenuShown = "OnSubmenuShown";

		public new static readonly StringName OnSubmenuHidden = "OnSubmenuHidden";

		public static readonly StringName InitializeDisplay = "InitializeDisplay";

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

		public static readonly StringName _dateLabel = "_dateLabel";

		public static readonly StringName _embarkButton = "_embarkButton";

		public new static readonly StringName _backButton = "_backButton";

		public static readonly StringName _unreadyButton = "_unreadyButton";

		public static readonly StringName _characterContainer = "_characterContainer";

		public static readonly StringName _leaderboard = "_leaderboard";

		public static readonly StringName _modifiersTitleLabel = "_modifiersTitleLabel";

		public static readonly StringName _modifiersContainer = "_modifiersContainer";

		public static readonly StringName _remotePlayerContainer = "_remotePlayerContainer";

		public static readonly StringName _readyAndWaitingContainer = "_readyAndWaitingContainer";
	}

	public new class SignalName : NSubmenu.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("screens/daily_run/daily_run_load_screen");

	private static readonly LocString _ascensionLoc = new LocString("main_menu_ui", "DAILY_RUN_MENU.ASCENSION");

	public static readonly string dateFormat = LocManager.Instance.GetTable("main_menu_ui").GetRawText("DAILY_RUN_MENU.DATE_FORMAT");

	private MegaLabel _dateLabel;

	private NConfirmButton _embarkButton;

	private NBackButton _backButton;

	private NBackButton _unreadyButton;

	private NDailyRunCharacterContainer _characterContainer;

	private NDailyRunLeaderboard _leaderboard;

	private MegaLabel _modifiersTitleLabel;

	private Control _modifiersContainer;

	private readonly List<NDailyRunScreenModifier> _modifierContainers = new List<NDailyRunScreenModifier>();

	private NRemoteLoadLobbyPlayerContainer _remotePlayerContainer;

	private Control _readyAndWaitingContainer;

	private LoadRunLobby? _lobby;

	public static string[] AssetPaths => new string[1] { _scenePath };

	protected override Control? InitialFocusedControl => null;

	public static NDailyRunLoadScreen? Create()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		return PreloadManager.Cache.GetScene(_scenePath).Instantiate<NDailyRunLoadScreen>(PackedScene.GenEditState.Disabled);
	}

	public override void _Ready()
	{
		ConnectSignals();
		_embarkButton = GetNode<NConfirmButton>("%ConfirmButton");
		_backButton = GetNode<NBackButton>("%BackButton");
		_unreadyButton = GetNode<NBackButton>("%UnreadyButton");
		_dateLabel = GetNode<MegaLabel>("%Date");
		_leaderboard = GetNode<NDailyRunLeaderboard>("%Leaderboards");
		_modifiersTitleLabel = GetNode<MegaLabel>("%ModifiersLabel");
		_modifiersContainer = GetNode<Control>("%ModifiersContainer");
		_characterContainer = GetNode<NDailyRunCharacterContainer>("%CharacterContainer");
		_remotePlayerContainer = GetNode<NRemoteLoadLobbyPlayerContainer>("%RemotePlayerLoadContainer");
		_readyAndWaitingContainer = GetNode<Control>("%ReadyAndWaitingPanel");
		foreach (NDailyRunScreenModifier item in _modifiersContainer.GetChildren().OfType<NDailyRunScreenModifier>())
		{
			_modifierContainers.Add(item);
		}
		_readyAndWaitingContainer.Visible = false;
		_embarkButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnEmbarkPressed));
		_unreadyButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnUnreadyPressed));
		_unreadyButton.Disable();
		_leaderboard.Cleanup();
		base.ProcessMode = ProcessModeEnum.Disabled;
	}

	public void InitializeAsHost(INetGameService gameService, SerializableRun run)
	{
		if (gameService.Type != NetGameType.Host)
		{
			throw new InvalidOperationException($"Initialized daily run load screen with net service of type {gameService.Type} when hosting!");
		}
		_lobby = new LoadRunLobby(gameService, this, run);
		try
		{
			_lobby.AddLocalHostPlayer();
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
			throw new InvalidOperationException($"Initialized daily run load screen with net service of type {gameService.Type} when joining!");
		}
		_lobby = new LoadRunLobby(gameService, this, message);
		AfterMultiplayerStarted();
	}

	public override void OnSubmenuOpened()
	{
		base.OnSubmenuOpened();
		_leaderboard.Initialize(_lobby.Run.DailyTime.Value, _lobby.Run.Players.Select((SerializablePlayer p) => p.NetId), allowPagination: true);
		_embarkButton.Enable();
		_remotePlayerContainer.Initialize(_lobby, displayLocalPlayer: false);
	}

	public override void OnSubmenuClosed()
	{
		_embarkButton.Disable();
		_remotePlayerContainer.Cleanup();
		_leaderboard.Cleanup();
		LoadRunLobby? lobby = _lobby;
		if (lobby != null && lobby.NetService.Type.IsMultiplayer())
		{
			PlatformUtil.SetRichPresence("MAIN_MENU", null, null);
		}
		CleanUpLobby(disconnectSession: true);
	}

	protected override void OnSubmenuShown()
	{
		base.ProcessMode = ProcessModeEnum.Inherit;
	}

	protected override void OnSubmenuHidden()
	{
		base.ProcessMode = ProcessModeEnum.Disabled;
	}

	private void InitializeDisplay()
	{
		if (_lobby == null)
		{
			throw new InvalidOperationException("Tried to initialize daily run display before lobby was initialized!");
		}
		_ascensionLoc.Add("ascension", _lobby.Run.Ascension);
		DateTimeOffset value = _lobby.Run.DailyTime.Value;
		SerializablePlayer serializablePlayer = _lobby.Run.Players.First((SerializablePlayer p) => p.NetId == _lobby.NetService.NetId);
		CharacterModel byId = ModelDb.GetById<CharacterModel>(serializablePlayer.CharacterId);
		_characterContainer.Fill(byId, serializablePlayer.NetId, _lobby.Run.Ascension, _lobby.NetService);
		_dateLabel.SetTextAutoSize(value.ToString(dateFormat));
		_embarkButton.Enable();
		for (int num = 0; num < _lobby.Run.Modifiers.Count; num++)
		{
			ModifierModel modifier = ModifierModel.FromSerializable(_lobby.Run.Modifiers[num]);
			_modifierContainers[num].Fill(modifier);
		}
	}

	private void OnEmbarkPressed(NButton _)
	{
		_embarkButton.Disable();
		_backButton.Disable();
		_lobby.SetReady(ready: true);
		if (_lobby.NetService.Type.IsMultiplayer() && _lobby.Run.Players.Any((SerializablePlayer p) => !_lobby.IsPlayerReady(p.NetId)))
		{
			_readyAndWaitingContainer.Visible = true;
			_unreadyButton.Enable();
		}
	}

	private void OnUnreadyPressed(NButton _)
	{
		_embarkButton.Enable();
		_unreadyButton.Disable();
		_backButton.Enable();
		_lobby.SetReady(ready: true);
		_readyAndWaitingContainer.Visible = false;
	}

	private void UpdateRichPresence()
	{
		LoadRunLobby? lobby = _lobby;
		if (lobby != null && lobby.NetService.Type.IsMultiplayer())
		{
			PlatformUtil.SetRichPresence("LOADING_MP_LOBBY", _lobby.NetService.GetRawLobbyIdentifier(), _lobby.ConnectedPlayerIds.Count);
		}
	}

	public override void _Process(double delta)
	{
		LoadRunLobby? lobby = _lobby;
		if (lobby != null && lobby.NetService.IsConnected)
		{
			_lobby.NetService.Update();
		}
	}

	private void CleanUpLobby(bool disconnectSession)
	{
		_lobby.CleanUp(disconnectSession);
		_lobby = null;
	}

	public async Task<bool> ShouldAllowRunToBegin()
	{
		if (_lobby.ConnectedPlayerIds.Count >= _lobby.Run.Players.Count)
		{
			return true;
		}
		LocString locString = new LocString("gameplay_ui", "CONFIRM_LOAD_SAVE.body");
		locString.Add("MissingCount", _lobby.Run.Players.Count - _lobby.ConnectedPlayerIds.Count);
		NGenericPopup nGenericPopup = NGenericPopup.Create();
		NModalContainer.Instance.Add(nGenericPopup);
		return await nGenericPopup.WaitForConfirmation(locString, new LocString("gameplay_ui", "CONFIRM_LOAD_SAVE.header"), new LocString("gameplay_ui", "CONFIRM_LOAD_SAVE.cancel"), new LocString("gameplay_ui", "CONFIRM_LOAD_SAVE.confirm"));
	}

	private async Task StartRun()
	{
		Log.Info("Loading a multiplayer run. Players: " + string.Join(",", _lobby.ConnectedPlayerIds) + ".");
		SerializablePlayer serializablePlayer = _lobby.Run.Players.First((SerializablePlayer p) => p.NetId == _lobby.NetService.NetId);
		SfxCmd.Play(ModelDb.GetById<CharacterModel>(serializablePlayer.CharacterId).CharacterTransitionSfx);
		await NGame.Instance.Transition.FadeOut(0.8f, ModelDb.GetById<CharacterModel>(serializablePlayer.CharacterId).CharacterSelectTransitionPath);
		RunState runState = RunState.FromSerializable(_lobby.Run);
		RunManager.Instance.SetUpSavedMultiPlayer(runState, _lobby);
		await NGame.Instance.LoadRun(runState, _lobby.Run.PreFinishedRoom);
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
		if (playerId == _lobby.NetService.NetId && !_lobby.IsPlayerReady(playerId))
		{
			_embarkButton.Enable();
		}
		if (playerId == _lobby.NetService.NetId && _lobby.NetService.Type.IsMultiplayer())
		{
			_characterContainer.SetIsReady(_lobby.IsPlayerReady(playerId));
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
		NGame.Instance.RemoteCursorContainer.Initialize(_lobby.InputSynchronizer, _lobby.ConnectedPlayerIds);
		NGame.Instance.ReactionContainer.InitializeNetworking(_lobby.NetService);
		InitializeDisplay();
		UpdateRichPresence();
		MegaCrit.Sts2.Core.Logging.Logger.logLevelTypeMap[LogType.Network] = LogLevel.Debug;
		MegaCrit.Sts2.Core.Logging.Logger.logLevelTypeMap[LogType.Actions] = LogLevel.VeryDebug;
		MegaCrit.Sts2.Core.Logging.Logger.logLevelTypeMap[LogType.GameSync] = LogLevel.VeryDebug;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(17);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuOpened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuClosed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuShown, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuHidden, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.InitializeDisplay, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
			ret = VariantUtils.CreateFrom<NDailyRunLoadScreen>(Create());
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
		if (method == MethodName.OnSubmenuClosed && args.Count == 0)
		{
			OnSubmenuClosed();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSubmenuShown && args.Count == 0)
		{
			OnSubmenuShown();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSubmenuHidden && args.Count == 0)
		{
			OnSubmenuHidden();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.InitializeDisplay && args.Count == 0)
		{
			InitializeDisplay();
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
			ret = VariantUtils.CreateFrom<NDailyRunLoadScreen>(Create());
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
		if (method == MethodName.OnSubmenuClosed)
		{
			return true;
		}
		if (method == MethodName.OnSubmenuShown)
		{
			return true;
		}
		if (method == MethodName.OnSubmenuHidden)
		{
			return true;
		}
		if (method == MethodName.InitializeDisplay)
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
		if (name == PropertyName._dateLabel)
		{
			_dateLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
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
		if (name == PropertyName._characterContainer)
		{
			_characterContainer = VariantUtils.ConvertTo<NDailyRunCharacterContainer>(in value);
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
			_remotePlayerContainer = VariantUtils.ConvertTo<NRemoteLoadLobbyPlayerContainer>(in value);
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
		if (name == PropertyName._dateLabel)
		{
			value = VariantUtils.CreateFrom(in _dateLabel);
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
		if (name == PropertyName._characterContainer)
		{
			value = VariantUtils.CreateFrom(in _characterContainer);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._dateLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._embarkButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._backButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._unreadyButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._characterContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
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
		info.AddProperty(PropertyName._dateLabel, Variant.From(in _dateLabel));
		info.AddProperty(PropertyName._embarkButton, Variant.From(in _embarkButton));
		info.AddProperty(PropertyName._backButton, Variant.From(in _backButton));
		info.AddProperty(PropertyName._unreadyButton, Variant.From(in _unreadyButton));
		info.AddProperty(PropertyName._characterContainer, Variant.From(in _characterContainer));
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
		if (info.TryGetProperty(PropertyName._dateLabel, out var value))
		{
			_dateLabel = value.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._embarkButton, out var value2))
		{
			_embarkButton = value2.As<NConfirmButton>();
		}
		if (info.TryGetProperty(PropertyName._backButton, out var value3))
		{
			_backButton = value3.As<NBackButton>();
		}
		if (info.TryGetProperty(PropertyName._unreadyButton, out var value4))
		{
			_unreadyButton = value4.As<NBackButton>();
		}
		if (info.TryGetProperty(PropertyName._characterContainer, out var value5))
		{
			_characterContainer = value5.As<NDailyRunCharacterContainer>();
		}
		if (info.TryGetProperty(PropertyName._leaderboard, out var value6))
		{
			_leaderboard = value6.As<NDailyRunLeaderboard>();
		}
		if (info.TryGetProperty(PropertyName._modifiersTitleLabel, out var value7))
		{
			_modifiersTitleLabel = value7.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._modifiersContainer, out var value8))
		{
			_modifiersContainer = value8.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._remotePlayerContainer, out var value9))
		{
			_remotePlayerContainer = value9.As<NRemoteLoadLobbyPlayerContainer>();
		}
		if (info.TryGetProperty(PropertyName._readyAndWaitingContainer, out var value10))
		{
			_readyAndWaitingContainer = value10.As<Control>();
		}
	}
}
