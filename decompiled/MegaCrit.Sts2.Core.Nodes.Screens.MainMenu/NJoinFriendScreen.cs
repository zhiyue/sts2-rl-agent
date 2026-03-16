using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using MegaCrit.Sts2.Core.Multiplayer.Connection;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Nodes.Screens.CustomRun;
using MegaCrit.Sts2.Core.Nodes.Screens.DailyRun;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Platform.Steam;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

[ScriptPath("res://src/Core/Nodes/Screens/MainMenu/NJoinFriendScreen.cs")]
public class NJoinFriendScreen : NSubmenu
{
	public new class MethodName : NSubmenu.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName OnSubmenuOpened = "OnSubmenuOpened";

		public new static readonly StringName OnSubmenuClosed = "OnSubmenuClosed";

		public static readonly StringName RefreshButtonClicked = "RefreshButtonClicked";
	}

	public new class PropertyName : NSubmenu.PropertyName
	{
		public new static readonly StringName InitialFocusedControl = "InitialFocusedControl";

		public static readonly StringName DebugFriendsButtons = "DebugFriendsButtons";

		public static readonly StringName _buttonContainer = "_buttonContainer";

		public static readonly StringName _loadingOverlay = "_loadingOverlay";

		public static readonly StringName _loadingFriendsIndicator = "_loadingFriendsIndicator";

		public static readonly StringName _noFriendsLabel = "_noFriendsLabel";

		public static readonly StringName _refreshButton = "_refreshButton";
	}

	public new class SignalName : NSubmenu.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("screens/join_friend_submenu");

	private Control _buttonContainer;

	private Control _loadingOverlay;

	private Control _loadingFriendsIndicator;

	private MegaLabel _noFriendsLabel;

	private NJoinFriendRefreshButton _refreshButton;

	private Task? _refreshTask;

	private JoinFlow? _currentJoinFlow;

	protected override Control? InitialFocusedControl
	{
		get
		{
			if (_buttonContainer.GetChildCount() <= 0)
			{
				return _refreshButton;
			}
			return _buttonContainer.GetChild<Control>(0);
		}
	}

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlyArray<string>(new string[2]
	{
		_scenePath,
		NJoinFriendButton.scenePath
	});

	public bool DebugFriendsButtons => false;

	public static NJoinFriendScreen? Create()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		return PreloadManager.Cache.GetScene(_scenePath).Instantiate<NJoinFriendScreen>(PackedScene.GenEditState.Disabled);
	}

	public override void _Ready()
	{
		ConnectSignals();
		_buttonContainer = GetNode<Control>("%ButtonContainer");
		_loadingOverlay = GetNode<Control>("%LoadingOverlay");
		_loadingFriendsIndicator = GetNode<Control>("%LoadingIndicator");
		_noFriendsLabel = GetNode<MegaLabel>("%NoFriendsText");
		_refreshButton = GetNode<NJoinFriendRefreshButton>("%RefreshButton");
		GetNode<MegaLabel>("TitleLabel").SetTextAutoSize(new LocString("main_menu_ui", "JOIN_FRIENDS_MENU.title").GetFormattedText());
		_noFriendsLabel.SetTextAutoSize(new LocString("main_menu_ui", "JOIN_FRIENDS_MENU.noFriends").GetFormattedText());
		_refreshButton.Connect(NClickableControl.SignalName.Released, Callable.From<NClickableControl>(delegate
		{
			RefreshButtonClicked();
		}));
		_loadingFriendsIndicator.Visible = false;
		_noFriendsLabel.Visible = false;
	}

	public override void OnSubmenuOpened()
	{
		base.OnSubmenuOpened();
		_loadingOverlay.Visible = false;
		if ((!SteamInitializer.Initialized || CommandLineHelper.HasArg("fastmp")) && !DebugFriendsButtons)
		{
			TaskHelper.RunSafely(FastMpJoin());
		}
		else
		{
			_refreshTask = TaskHelper.RunSafely(ShowFriends());
		}
	}

	public override void OnSubmenuClosed()
	{
		_currentJoinFlow?.CancelToken.Cancel();
	}

	private async Task FastMpJoin()
	{
		ulong netId = 1000uL;
		if (CommandLineHelper.TryGetValue("clientId", out string value))
		{
			netId = ulong.Parse(value);
		}
		DisplayServer.WindowSetTitle("Slay The Spire 2 (Client)");
		await JoinGameAsync(new ENetClientConnectionInitializer(netId, "127.0.0.1", 33771));
	}

	private void RefreshButtonClicked()
	{
		Task refreshTask = _refreshTask;
		if (refreshTask == null || refreshTask.IsCompleted)
		{
			_refreshTask = TaskHelper.RunSafely(RefreshButtonClickedAsync());
		}
	}

	private async Task RefreshButtonClickedAsync()
	{
		_noFriendsLabel.Visible = false;
		if (SteamInitializer.Initialized)
		{
			_loadingFriendsIndicator.Visible = true;
			await Cmd.Wait(0.5f);
			_loadingFriendsIndicator.Visible = false;
		}
		await ShowFriends();
		InitialFocusedControl?.TryGrabFocus();
	}

	private async Task ShowFriends()
	{
		_loadingFriendsIndicator.Visible = true;
		foreach (Node child2 in _buttonContainer.GetChildren())
		{
			child2.QueueFreeSafely();
		}
		if (SteamInitializer.Initialized)
		{
			IEnumerable<ulong> enumerable = await PlatformUtil.GetFriendsWithOpenLobbies(PlatformType.Steam);
			NButton nButton = null;
			foreach (ulong item in enumerable)
			{
				NJoinFriendButton nJoinFriendButton = NJoinFriendButton.Create(item);
				_buttonContainer.AddChildSafely(nJoinFriendButton);
				SteamClientConnectionInitializer connInitializer = SteamClientConnectionInitializer.FromPlayer(item);
				nJoinFriendButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(delegate
				{
					JoinGame(connInitializer);
				}));
				if (nButton == null)
				{
					nButton = nJoinFriendButton;
				}
			}
		}
		if (DebugFriendsButtons)
		{
			for (int num = 0; num < Rng.Chaotic.NextInt(5, 20); num++)
			{
				ulong playerId = (ulong)(int)((num == 0) ? 1u : ((uint)(num * 1000)));
				NJoinFriendButton child = NJoinFriendButton.Create(playerId);
				_buttonContainer.AddChildSafely(child);
			}
		}
		ActiveScreenContext.Instance.Update();
		_loadingFriendsIndicator.Visible = false;
		_noFriendsLabel.Visible = _buttonContainer.GetChildCount() == 0;
	}

	private void JoinGame(IClientConnectionInitializer connInitializer)
	{
		TaskHelper.RunSafely(JoinGameAsync(connInitializer));
	}

	public async Task JoinGameAsync(IClientConnectionInitializer connInitializer)
	{
		if (_currentJoinFlow?.NetService?.IsConnected == true)
		{
			Log.Warn($"Tried to join game with connection {connInitializer} while we were already joining a game! Ignoring this attempt");
			return;
		}
		_loadingOverlay.Visible = true;
		_currentJoinFlow = new JoinFlow();
		try
		{
			Log.Info($"Attempting to join game with connection initializer {connInitializer}");
			JoinResult joinResult = await _currentJoinFlow.Begin(connInitializer, GetTree());
			if (joinResult.sessionState == RunSessionState.InLobby)
			{
				if (joinResult.gameMode == GameMode.Standard)
				{
					NCharacterSelectScreen submenuType = _stack.GetSubmenuType<NCharacterSelectScreen>();
					submenuType.InitializeMultiplayerAsClient(_currentJoinFlow.NetService, joinResult.joinResponse.Value);
					_stack.Push(submenuType);
					return;
				}
				if (joinResult.gameMode == GameMode.Daily)
				{
					NDailyRunScreen submenuType2 = _stack.GetSubmenuType<NDailyRunScreen>();
					submenuType2.InitializeMultiplayerAsClient(_currentJoinFlow.NetService, joinResult.joinResponse.Value);
					_stack.Push(submenuType2);
					return;
				}
				if (joinResult.gameMode != GameMode.Custom)
				{
					throw new ArgumentOutOfRangeException("gameMode", joinResult.gameMode, "Invalid game mode!");
				}
				NCustomRunScreen submenuType3 = _stack.GetSubmenuType<NCustomRunScreen>();
				submenuType3.InitializeMultiplayerAsClient(_currentJoinFlow.NetService, joinResult.joinResponse.Value);
				_stack.Push(submenuType3);
			}
			else if (joinResult.sessionState == RunSessionState.InLoadedLobby)
			{
				if (joinResult.gameMode == GameMode.Standard)
				{
					NMultiplayerLoadGameScreen submenuType4 = _stack.GetSubmenuType<NMultiplayerLoadGameScreen>();
					submenuType4.InitializeAsClient(_currentJoinFlow.NetService, joinResult.loadJoinResponse.Value);
					_stack.Push(submenuType4);
				}
				else if (joinResult.gameMode == GameMode.Daily)
				{
					NDailyRunLoadScreen submenuType5 = _stack.GetSubmenuType<NDailyRunLoadScreen>();
					submenuType5.InitializeAsClient(_currentJoinFlow.NetService, joinResult.loadJoinResponse.Value);
					_stack.Push(submenuType5);
				}
				else if (joinResult.gameMode == GameMode.Custom)
				{
					NCustomRunLoadScreen submenuType6 = _stack.GetSubmenuType<NCustomRunLoadScreen>();
					submenuType6.InitializeAsClient(_currentJoinFlow.NetService, joinResult.loadJoinResponse.Value);
					_stack.Push(submenuType6);
				}
			}
			else if (joinResult.sessionState == RunSessionState.Running)
			{
				throw new NotImplementedException("Rejoining a game is not yet implemented");
			}
		}
		catch (ClientConnectionFailedException ex)
		{
			Log.Error($"Received connection failed exception while joining game: {ex}");
			NErrorPopup nErrorPopup = NErrorPopup.Create(ex.info);
			if (nErrorPopup != null)
			{
				NModalContainer.Instance.Add(nErrorPopup);
			}
			_currentJoinFlow.NetService?.Disconnect(ex.info.GetReason());
		}
		catch (OperationCanceledException)
		{
			Log.Warn("Joining was canceled by user");
		}
		catch
		{
			Log.Error("Received unexpected exception while joining game! Disconnecting with InternalError");
			NErrorPopup nErrorPopup2 = NErrorPopup.Create(new NetErrorInfo(NetError.InternalError, selfInitiated: false));
			if (nErrorPopup2 != null)
			{
				NModalContainer.Instance.Add(nErrorPopup2);
			}
			_currentJoinFlow.NetService?.Disconnect(NetError.InternalError);
			throw;
		}
		finally
		{
			_loadingOverlay.Visible = false;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuOpened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuClosed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshButtonClicked, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NJoinFriendScreen>(Create());
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
		if (method == MethodName.RefreshButtonClicked && args.Count == 0)
		{
			RefreshButtonClicked();
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
			ret = VariantUtils.CreateFrom<NJoinFriendScreen>(Create());
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
		if (method == MethodName.RefreshButtonClicked)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._buttonContainer)
		{
			_buttonContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._loadingOverlay)
		{
			_loadingOverlay = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._loadingFriendsIndicator)
		{
			_loadingFriendsIndicator = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._noFriendsLabel)
		{
			_noFriendsLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._refreshButton)
		{
			_refreshButton = VariantUtils.ConvertTo<NJoinFriendRefreshButton>(in value);
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
		if (name == PropertyName.DebugFriendsButtons)
		{
			value = VariantUtils.CreateFrom<bool>(DebugFriendsButtons);
			return true;
		}
		if (name == PropertyName._buttonContainer)
		{
			value = VariantUtils.CreateFrom(in _buttonContainer);
			return true;
		}
		if (name == PropertyName._loadingOverlay)
		{
			value = VariantUtils.CreateFrom(in _loadingOverlay);
			return true;
		}
		if (name == PropertyName._loadingFriendsIndicator)
		{
			value = VariantUtils.CreateFrom(in _loadingFriendsIndicator);
			return true;
		}
		if (name == PropertyName._noFriendsLabel)
		{
			value = VariantUtils.CreateFrom(in _noFriendsLabel);
			return true;
		}
		if (name == PropertyName._refreshButton)
		{
			value = VariantUtils.CreateFrom(in _refreshButton);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.InitialFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._buttonContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._loadingOverlay, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._loadingFriendsIndicator, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._noFriendsLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._refreshButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.DebugFriendsButtons, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._buttonContainer, Variant.From(in _buttonContainer));
		info.AddProperty(PropertyName._loadingOverlay, Variant.From(in _loadingOverlay));
		info.AddProperty(PropertyName._loadingFriendsIndicator, Variant.From(in _loadingFriendsIndicator));
		info.AddProperty(PropertyName._noFriendsLabel, Variant.From(in _noFriendsLabel));
		info.AddProperty(PropertyName._refreshButton, Variant.From(in _refreshButton));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._buttonContainer, out var value))
		{
			_buttonContainer = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._loadingOverlay, out var value2))
		{
			_loadingOverlay = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._loadingFriendsIndicator, out var value3))
		{
			_loadingFriendsIndicator = value3.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._noFriendsLabel, out var value4))
		{
			_noFriendsLabel = value4.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._refreshButton, out var value5))
		{
			_refreshButton = value5.As<NJoinFriendRefreshButton>();
		}
	}
}
