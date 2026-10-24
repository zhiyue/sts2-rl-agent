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
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.CustomRun;

/// <summary>
/// Intermediate screen when loading a custom run in multiplayer. Allows players to join back into the session
/// before it is resumed.
/// </summary>
[ScriptPath("res://src/Core/Nodes/Screens/CustomRun/NCustomRunLoadScreen.cs")]
public class NCustomRunLoadScreen : NSubmenu, ILoadRunLobbyListener
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
		/// Cached name for the 'OnSubmenuOpened' method.
		/// </summary>
		public new static readonly StringName OnSubmenuOpened = "OnSubmenuOpened";

		/// <summary>
		/// Cached name for the 'OnSubmenuClosed' method.
		/// </summary>
		public new static readonly StringName OnSubmenuClosed = "OnSubmenuClosed";

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
		/// Cached name for the '_Process' method.
		/// </summary>
		public new static readonly StringName _Process = "_Process";

		/// <summary>
		/// Cached name for the 'CleanUpLobby' method.
		/// </summary>
		public static readonly StringName CleanUpLobby = "CleanUpLobby";

		/// <summary>
		/// Cached name for the 'PlayerConnected' method.
		/// </summary>
		public static readonly StringName PlayerConnected = "PlayerConnected";

		/// <summary>
		/// Cached name for the 'PlayerReadyChanged' method.
		/// </summary>
		public static readonly StringName PlayerReadyChanged = "PlayerReadyChanged";

		/// <summary>
		/// Cached name for the 'RemotePlayerDisconnected' method.
		/// </summary>
		public static readonly StringName RemotePlayerDisconnected = "RemotePlayerDisconnected";

		/// <summary>
		/// Cached name for the 'BeginRun' method.
		/// </summary>
		public static readonly StringName BeginRun = "BeginRun";

		/// <summary>
		/// Cached name for the 'AfterInitialized' method.
		/// </summary>
		public static readonly StringName AfterInitialized = "AfterInitialized";
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
		/// Cached name for the '_confirmButton' field.
		/// </summary>
		public static readonly StringName _confirmButton = "_confirmButton";

		/// <summary>
		/// Cached name for the '_backButton' field.
		/// </summary>
		public new static readonly StringName _backButton = "_backButton";

		/// <summary>
		/// Cached name for the '_unreadyButton' field.
		/// </summary>
		public static readonly StringName _unreadyButton = "_unreadyButton";

		/// <summary>
		/// Cached name for the '_ascensionPanel' field.
		/// </summary>
		public static readonly StringName _ascensionPanel = "_ascensionPanel";

		/// <summary>
		/// Cached name for the '_readyAndWaitingContainer' field.
		/// </summary>
		public static readonly StringName _readyAndWaitingContainer = "_readyAndWaitingContainer";

		/// <summary>
		/// Cached name for the '_seedInput' field.
		/// </summary>
		public static readonly StringName _seedInput = "_seedInput";

		/// <summary>
		/// Cached name for the '_remotePlayerContainer' field.
		/// </summary>
		public static readonly StringName _remotePlayerContainer = "_remotePlayerContainer";

		/// <summary>
		/// Cached name for the '_modifiersList' field.
		/// </summary>
		public static readonly StringName _modifiersList = "_modifiersList";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NSubmenu.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("screens/custom_run/custom_run_load_screen");

	private NConfirmButton _confirmButton;

	private NBackButton _backButton;

	private NBackButton _unreadyButton;

	private NAscensionPanel _ascensionPanel;

	private Control _readyAndWaitingContainer;

	private LineEdit _seedInput;

	private NRemoteLoadLobbyPlayerContainer _remotePlayerContainer;

	private NCustomRunModifiersList _modifiersList;

	private LoadRunLobby _lobby;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { _scenePath, "res://scenes/screens/custom_run/modifier_tickbox.tscn" });

	protected override Control? InitialFocusedControl => null;

	public static NCustomRunLoadScreen? Create()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		return PreloadManager.Cache.GetScene(_scenePath).Instantiate<NCustomRunLoadScreen>(PackedScene.GenEditState.Disabled);
	}

	public override void _Ready()
	{
		ConnectSignals();
		_ascensionPanel = GetNode<NAscensionPanel>("%AscensionPanel");
		_remotePlayerContainer = GetNode<NRemoteLoadLobbyPlayerContainer>("LeftContainer/RemotePlayerLoadContainer");
		_readyAndWaitingContainer = GetNode<Control>("%ReadyAndWaitingPanel");
		_modifiersList = GetNode<NCustomRunModifiersList>("%ModifiersList");
		_seedInput = GetNode<LineEdit>("%SeedInput");
		_confirmButton = GetNode<NConfirmButton>("ConfirmButton");
		_backButton = GetNode<NBackButton>("BackButton");
		_unreadyButton = GetNode<NBackButton>("UnreadyButton");
		_confirmButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnEmbarkPressed));
		_unreadyButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnUnreadyPressed));
		_unreadyButton.Disable();
		GetNode<MegaLabel>("%CustomModeTitle").SetTextAutoSize(new LocString("main_menu_ui", "CUSTOM_RUN_SCREEN.CUSTOM_MODE_TITLE").GetFormattedText());
		GetNode<MegaLabel>("%ModifiersTitle").SetTextAutoSize(new LocString("main_menu_ui", "CUSTOM_RUN_SCREEN.MODIFIERS_TITLE").GetFormattedText());
		GetNode<MegaLabel>("%SeedLabel").SetTextAutoSize(new LocString("main_menu_ui", "CUSTOM_RUN_SCREEN.SEED_LABEL").GetFormattedText());
		_seedInput.PlaceholderText = new LocString("main_menu_ui", "CUSTOM_RUN_SCREEN.SEED_RANDOM_PLACEHOLDER").GetFormattedText();
	}

	public void InitializeAsHost(INetGameService gameService, SerializableRun run)
	{
		if (gameService.Type != NetGameType.Host)
		{
			throw new InvalidOperationException($"Initialized custom run screen with NetService of type {gameService.Type} when hosting!");
		}
		_lobby = new LoadRunLobby(gameService, this, run);
		try
		{
			_lobby.AddLocalHostPlayer();
			AfterInitialized();
		}
		catch
		{
			CleanUpLobby(disconnectSession: true, NetError.InternalError);
			throw;
		}
	}

	public void InitializeAsClient(INetGameService gameService, ClientLoadJoinResponseMessage message)
	{
		if (gameService.Type != NetGameType.Client)
		{
			throw new InvalidOperationException($"Initialized character select screen with NetService of type {gameService.Type} when joining!");
		}
		_lobby = new LoadRunLobby(gameService, this, message);
		AfterInitialized();
	}

	public override void OnSubmenuOpened()
	{
		base.OnSubmenuOpened();
		_confirmButton.Enable();
		_remotePlayerContainer.Initialize(_lobby, displayLocalPlayer: true);
		_ascensionPanel.Initialize(MultiplayerUiMode.Load);
		_ascensionPanel.SetAscensionLevel(_lobby.Run.Ascension);
		_modifiersList.Initialize(MultiplayerUiMode.Load);
		_modifiersList.SyncModifierList(_lobby.Run.Modifiers.Select(ModifierModel.FromSerializable).ToList());
		_readyAndWaitingContainer.Visible = false;
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
	}

	private void OnEmbarkPressed(NButton _)
	{
		_confirmButton.Disable();
		_backButton.Disable();
		_lobby.SetReady(ready: true);
		if (!_lobby.IsAboutToBeginGame())
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
	}

	private void UpdateRichPresence()
	{
		if (_lobby.NetService.Type.IsMultiplayer())
		{
			PlatformUtil.SetRichPresence("LOADING_MP_LOBBY", _lobby.NetService.GetRawLobbyIdentifier(), _lobby.ConnectedPlayerIds.Count);
		}
	}

	public override void _Process(double delta)
	{
		if (_lobby != null && _lobby.NetService.IsConnected)
		{
			_lobby.NetService.Update();
		}
	}

	private void CleanUpLobby(bool disconnectSession, NetError error = NetError.Quit)
	{
		_lobby.CleanUp(disconnectSession, error);
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
		try
		{
			Log.Info("Loading a custom multiplayer run. Players: " + string.Join(",", _lobby.ConnectedPlayerIds) + ".");
			SerializablePlayer serializablePlayer = _lobby.Run.Players.First((SerializablePlayer p) => p.NetId == _lobby.NetService.NetId);
			SfxCmd.Play(ModelDb.GetById<CharacterModel>(serializablePlayer.CharacterId).CharacterTransitionSfx);
			await NGame.Instance.Transition.FadeOut(0.8f, ModelDb.GetById<CharacterModel>(serializablePlayer.CharacterId).CharacterSelectTransitionPath);
			RunState runState = RunState.FromSerializable(_lobby.Run);
			await RunManager.Instance.SetUpSavedMultiplayer(runState, _lobby);
			await NGame.Instance.LoadRun(runState, _lobby.Run.PreFinishedRoom);
		}
		catch (Exception ex)
		{
			Log.Error($"Exception loading custom multiplayer run : {ex}");
			CleanUpLobby(disconnectSession: true, NetError.InternalError);
			await NGame.Instance.ReturnToMainMenuWithInternalError(ex);
			return;
		}
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
		_confirmButton.Disable();
		_unreadyButton.Disable();
		TaskHelper.RunSafely(StartRun());
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

	private void AfterInitialized()
	{
		NGame.Instance.RemoteCursorContainer.Initialize(_lobby.InputSynchronizer, _lobby.ConnectedPlayerIds);
		NGame.Instance.ReactionContainer.InitializeNetworking(_lobby.NetService);
		UpdateRichPresence();
		MegaCrit.Sts2.Core.Logging.Logger.logLevelTypeMap[LogType.Network] = ((_lobby.NetService.Type == NetGameType.Singleplayer) ? LogLevel.Info : LogLevel.Debug);
		MegaCrit.Sts2.Core.Logging.Logger.logLevelTypeMap[LogType.Actions] = ((_lobby.NetService.Type == NetGameType.Singleplayer) ? LogLevel.Info : LogLevel.VeryDebug);
		MegaCrit.Sts2.Core.Logging.Logger.logLevelTypeMap[LogType.GameSync] = ((_lobby.NetService.Type == NetGameType.Singleplayer) ? LogLevel.Info : LogLevel.VeryDebug);
		NGame.Instance.DebugSeedOverride = null;
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(14);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
			new PropertyInfo(Variant.Type.Bool, "disconnectSession", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "error", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		list.Add(new MethodInfo(MethodName.AfterInitialized, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NCustomRunLoadScreen>(Create());
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
		if (method == MethodName.CleanUpLobby && args.Count == 2)
		{
			CleanUpLobby(VariantUtils.ConvertTo<bool>(in args[0]), VariantUtils.ConvertTo<NetError>(in args[1]));
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
			ret = VariantUtils.CreateFrom<NCustomRunLoadScreen>(Create());
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
		if (method == MethodName.AfterInitialized)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
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
			_remotePlayerContainer = VariantUtils.ConvertTo<NRemoteLoadLobbyPlayerContainer>(in value);
			return true;
		}
		if (name == PropertyName._modifiersList)
		{
			_modifiersList = VariantUtils.ConvertTo<NCustomRunModifiersList>(in value);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._confirmButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._backButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._unreadyButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._ascensionPanel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._readyAndWaitingContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._seedInput, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._remotePlayerContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._modifiersList, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.InitialFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._confirmButton, Variant.From(in _confirmButton));
		info.AddProperty(PropertyName._backButton, Variant.From(in _backButton));
		info.AddProperty(PropertyName._unreadyButton, Variant.From(in _unreadyButton));
		info.AddProperty(PropertyName._ascensionPanel, Variant.From(in _ascensionPanel));
		info.AddProperty(PropertyName._readyAndWaitingContainer, Variant.From(in _readyAndWaitingContainer));
		info.AddProperty(PropertyName._seedInput, Variant.From(in _seedInput));
		info.AddProperty(PropertyName._remotePlayerContainer, Variant.From(in _remotePlayerContainer));
		info.AddProperty(PropertyName._modifiersList, Variant.From(in _modifiersList));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._confirmButton, out var value))
		{
			_confirmButton = value.As<NConfirmButton>();
		}
		if (info.TryGetProperty(PropertyName._backButton, out var value2))
		{
			_backButton = value2.As<NBackButton>();
		}
		if (info.TryGetProperty(PropertyName._unreadyButton, out var value3))
		{
			_unreadyButton = value3.As<NBackButton>();
		}
		if (info.TryGetProperty(PropertyName._ascensionPanel, out var value4))
		{
			_ascensionPanel = value4.As<NAscensionPanel>();
		}
		if (info.TryGetProperty(PropertyName._readyAndWaitingContainer, out var value5))
		{
			_readyAndWaitingContainer = value5.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._seedInput, out var value6))
		{
			_seedInput = value6.As<LineEdit>();
		}
		if (info.TryGetProperty(PropertyName._remotePlayerContainer, out var value7))
		{
			_remotePlayerContainer = value7.As<NRemoteLoadLobbyPlayerContainer>();
		}
		if (info.TryGetProperty(PropertyName._modifiersList, out var value8))
		{
			_modifiersList = value8.As<NCustomRunModifiersList>();
		}
	}
}
