using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Daily;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Nodes.Screens.CustomRun;
using MegaCrit.Sts2.Core.Nodes.Screens.DailyRun;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Platform.Steam;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

[ScriptPath("res://src/Core/Nodes/Screens/MainMenu/NMultiplayerSubmenu.cs")]
public class NMultiplayerSubmenu : NSubmenu
{
	public new class MethodName : NSubmenu.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName UpdateButtons = "UpdateButtons";

		public static readonly StringName AbandonRun = "AbandonRun";

		public static readonly StringName StartLoad = "StartLoad";

		public static readonly StringName OnHostPressed = "OnHostPressed";

		public static readonly StringName FastHost = "FastHost";

		public static readonly StringName OpenJoinFriendsScreen = "OpenJoinFriendsScreen";

		public static readonly StringName OnJoinFriendsPressed = "OnJoinFriendsPressed";

		public new static readonly StringName OnSubmenuShown = "OnSubmenuShown";
	}

	public new class PropertyName : NSubmenu.PropertyName
	{
		public new static readonly StringName InitialFocusedControl = "InitialFocusedControl";

		public static readonly StringName _hostButton = "_hostButton";

		public static readonly StringName _loadButton = "_loadButton";

		public static readonly StringName _abandonButton = "_abandonButton";

		public static readonly StringName _joinButton = "_joinButton";

		public static readonly StringName _loadingOverlay = "_loadingOverlay";
	}

	public new class SignalName : NSubmenu.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("screens/multiplayer_submenu");

	private NSubmenuButton _hostButton;

	private NSubmenuButton _loadButton;

	private NSubmenuButton _abandonButton;

	private NSubmenuButton _joinButton;

	private const string _keyHost = "HOST";

	private const string _keyLoad = "MP_LOAD";

	private const string _keyJoin = "JOIN";

	private const string _keyAbandon = "MP_ABANDON";

	private Control _loadingOverlay;

	protected override Control InitialFocusedControl
	{
		get
		{
			if (SaveManager.Instance.HasMultiplayerRunSave)
			{
				return _loadButton;
			}
			return _hostButton;
		}
	}

	public static NMultiplayerSubmenu? Create()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		return PreloadManager.Cache.GetScene(_scenePath).Instantiate<NMultiplayerSubmenu>(PackedScene.GenEditState.Disabled);
	}

	public override void _Ready()
	{
		ConnectSignals();
		_loadingOverlay = GetNode<Control>("%LoadingOverlay");
		_hostButton = GetNode<NSubmenuButton>("ButtonContainer/HostButton");
		_hostButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnHostPressed));
		_hostButton.SetIconAndLocalization("HOST");
		_loadButton = GetNode<NSubmenuButton>("ButtonContainer/LoadButton");
		_loadButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(StartLoad));
		_loadButton.SetIconAndLocalization("MP_LOAD");
		_joinButton = GetNode<NSubmenuButton>("ButtonContainer/JoinButton");
		_joinButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OpenJoinFriendsScreen));
		_joinButton.SetIconAndLocalization("JOIN");
		_abandonButton = GetNode<NSubmenuButton>("ButtonContainer/AbandonButton");
		_abandonButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(AbandonRun));
		_abandonButton.SetIconAndLocalization("MP_ABANDON");
		UpdateButtons();
	}

	private void UpdateButtons()
	{
		_hostButton.Visible = !SaveManager.Instance.HasMultiplayerRunSave;
		_loadButton.Visible = SaveManager.Instance.HasMultiplayerRunSave;
		_abandonButton.Visible = SaveManager.Instance.HasMultiplayerRunSave;
	}

	private void AbandonRun(NButton _)
	{
		TaskHelper.RunSafely(TryAbandonMultiplayerRun());
	}

	private async Task TryAbandonMultiplayerRun()
	{
		LocString header = new LocString("main_menu_ui", "ABANDON_RUN_CONFIRMATION.header");
		LocString body = new LocString("main_menu_ui", "ABANDON_RUN_CONFIRMATION.body");
		LocString yesButton = new LocString("main_menu_ui", "GENERIC_POPUP.confirm");
		LocString noButton = new LocString("main_menu_ui", "GENERIC_POPUP.cancel");
		NGenericPopup nGenericPopup = NGenericPopup.Create();
		NModalContainer.Instance.Add(nGenericPopup);
		if (!(await nGenericPopup.WaitForConfirmation(body, header, noButton, yesButton)))
		{
			return;
		}
		ReadSaveResult<SerializableRun> readSaveResult = SaveManager.Instance.LoadAndCanonicalizeMultiplayerRunSave(PlatformUtil.GetLocalPlayerId(PlatformUtil.PrimaryPlatform));
		if (readSaveResult.Success && readSaveResult.SaveData != null)
		{
			try
			{
				SerializableRun saveData = readSaveResult.SaveData;
				SaveManager.Instance.UpdateProgressWithRunData(saveData, victory: false);
				RunHistoryUtilities.CreateRunHistoryEntry(saveData, victory: false, isAbandoned: true, saveData.PlatformType);
				if (saveData.DailyTime.HasValue)
				{
					PlatformUtil.GetLocalPlayerId(saveData.PlatformType);
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
			Log.Error($"ERROR: Failed to load multiplayer run save: status={readSaveResult.Status}. Deleting current run...");
		}
		SaveManager.Instance.DeleteCurrentMultiplayerRun();
		UpdateButtons();
	}

	private void StartLoad(NButton _)
	{
		PlatformType platformType = ((SteamInitializer.Initialized && !CommandLineHelper.HasArg("fastmp")) ? PlatformType.Steam : PlatformType.None);
		ReadSaveResult<SerializableRun> readSaveResult = SaveManager.Instance.LoadAndCanonicalizeMultiplayerRunSave(PlatformUtil.GetLocalPlayerId(platformType));
		if (!readSaveResult.Success || readSaveResult.SaveData == null)
		{
			Log.Warn("Broken multiplayer run save detected, disabling button");
			_loadButton.Disable();
			NErrorPopup modalToCreate = NErrorPopup.Create(new LocString("main_menu_ui", "INVALID_SAVE_POPUP.title"), new LocString("main_menu_ui", "INVALID_SAVE_POPUP.description_run"), new LocString("main_menu_ui", "INVALID_SAVE_POPUP.dismiss"), showReportBugButton: true);
			NModalContainer.Instance.Add(modalToCreate);
			NModalContainer.Instance.ShowBackstop();
		}
		else
		{
			StartHost(readSaveResult.SaveData);
		}
	}

	private void OnHostPressed(NButton _)
	{
		if (SaveManager.Instance.Progress.NumberOfRuns > 0)
		{
			_stack.PushSubmenuType<NMultiplayerHostSubmenu>();
		}
		else
		{
			TaskHelper.RunSafely(NMultiplayerHostSubmenu.StartHostAsync(GameMode.Standard, _loadingOverlay, _stack));
		}
	}

	public void FastHost(GameMode gameMode)
	{
		NMultiplayerHostSubmenu nMultiplayerHostSubmenu = _stack.PushSubmenuType<NMultiplayerHostSubmenu>();
		nMultiplayerHostSubmenu.StartHost(gameMode);
	}

	public void StartHost(SerializableRun run)
	{
		TaskHelper.RunSafely(StartHostAsync(run));
	}

	private async Task StartHostAsync(SerializableRun run)
	{
		PlatformType platformType = ((SteamInitializer.Initialized && !CommandLineHelper.HasArg("fastmp")) ? PlatformType.Steam : PlatformType.None);
		_loadingOverlay.Visible = true;
		try
		{
			NetHostGameService netService = new NetHostGameService();
			NetErrorInfo? netErrorInfo = null;
			if (platformType == PlatformType.Steam)
			{
				netErrorInfo = await netService.StartSteamHost(4);
			}
			else
			{
				netService.StartENetHost(33771, 4);
			}
			if (!netErrorInfo.HasValue)
			{
				if (run.Modifiers.Count > 0)
				{
					if (run.DailyTime.HasValue)
					{
						NDailyRunLoadScreen submenuType = _stack.GetSubmenuType<NDailyRunLoadScreen>();
						submenuType.InitializeAsHost(netService, run);
						_stack.Push(submenuType);
					}
					else
					{
						NCustomRunLoadScreen submenuType2 = _stack.GetSubmenuType<NCustomRunLoadScreen>();
						submenuType2.InitializeAsHost(netService, run);
						_stack.Push(submenuType2);
					}
				}
				else
				{
					NMultiplayerLoadGameScreen submenuType3 = _stack.GetSubmenuType<NMultiplayerLoadGameScreen>();
					submenuType3.InitializeAsHost(netService, run);
					_stack.Push(submenuType3);
				}
			}
			else
			{
				NErrorPopup nErrorPopup = NErrorPopup.Create(netErrorInfo.Value);
				if (nErrorPopup != null)
				{
					NModalContainer.Instance.Add(nErrorPopup);
				}
			}
		}
		finally
		{
			_loadingOverlay.Visible = false;
		}
	}

	private void OpenJoinFriendsScreen(NButton _)
	{
		OnJoinFriendsPressed();
	}

	public NJoinFriendScreen OnJoinFriendsPressed()
	{
		return _stack.PushSubmenuType<NJoinFriendScreen>();
	}

	protected override void OnSubmenuShown()
	{
		base.OnSubmenuShown();
		if (!SaveManager.Instance.SeenFtue("multiplayer_warning") && SaveManager.Instance.Progress.NumberOfRuns == 0 && !CommandLineHelper.HasArg("fastmp"))
		{
			NMultiplayerWarningPopup modalToCreate = NMultiplayerWarningPopup.Create();
			NModalContainer.Instance.Add(modalToCreate);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(10);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateButtons, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AbandonRun, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.StartLoad, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnHostPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.FastHost, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "gameMode", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OpenJoinFriendsScreen, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnJoinFriendsPressed, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuShown, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NMultiplayerSubmenu>(Create());
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateButtons && args.Count == 0)
		{
			UpdateButtons();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AbandonRun && args.Count == 1)
		{
			AbandonRun(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartLoad && args.Count == 1)
		{
			StartLoad(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnHostPressed && args.Count == 1)
		{
			OnHostPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FastHost && args.Count == 1)
		{
			FastHost(VariantUtils.ConvertTo<GameMode>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenJoinFriendsScreen && args.Count == 1)
		{
			OpenJoinFriendsScreen(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnJoinFriendsPressed && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NJoinFriendScreen>(OnJoinFriendsPressed());
			return true;
		}
		if (method == MethodName.OnSubmenuShown && args.Count == 0)
		{
			OnSubmenuShown();
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
			ret = VariantUtils.CreateFrom<NMultiplayerSubmenu>(Create());
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
		if (method == MethodName.UpdateButtons)
		{
			return true;
		}
		if (method == MethodName.AbandonRun)
		{
			return true;
		}
		if (method == MethodName.StartLoad)
		{
			return true;
		}
		if (method == MethodName.OnHostPressed)
		{
			return true;
		}
		if (method == MethodName.FastHost)
		{
			return true;
		}
		if (method == MethodName.OpenJoinFriendsScreen)
		{
			return true;
		}
		if (method == MethodName.OnJoinFriendsPressed)
		{
			return true;
		}
		if (method == MethodName.OnSubmenuShown)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._hostButton)
		{
			_hostButton = VariantUtils.ConvertTo<NSubmenuButton>(in value);
			return true;
		}
		if (name == PropertyName._loadButton)
		{
			_loadButton = VariantUtils.ConvertTo<NSubmenuButton>(in value);
			return true;
		}
		if (name == PropertyName._abandonButton)
		{
			_abandonButton = VariantUtils.ConvertTo<NSubmenuButton>(in value);
			return true;
		}
		if (name == PropertyName._joinButton)
		{
			_joinButton = VariantUtils.ConvertTo<NSubmenuButton>(in value);
			return true;
		}
		if (name == PropertyName._loadingOverlay)
		{
			_loadingOverlay = VariantUtils.ConvertTo<Control>(in value);
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
		if (name == PropertyName._hostButton)
		{
			value = VariantUtils.CreateFrom(in _hostButton);
			return true;
		}
		if (name == PropertyName._loadButton)
		{
			value = VariantUtils.CreateFrom(in _loadButton);
			return true;
		}
		if (name == PropertyName._abandonButton)
		{
			value = VariantUtils.CreateFrom(in _abandonButton);
			return true;
		}
		if (name == PropertyName._joinButton)
		{
			value = VariantUtils.CreateFrom(in _joinButton);
			return true;
		}
		if (name == PropertyName._loadingOverlay)
		{
			value = VariantUtils.CreateFrom(in _loadingOverlay);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hostButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._loadButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._abandonButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._joinButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._loadingOverlay, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.InitialFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._hostButton, Variant.From(in _hostButton));
		info.AddProperty(PropertyName._loadButton, Variant.From(in _loadButton));
		info.AddProperty(PropertyName._abandonButton, Variant.From(in _abandonButton));
		info.AddProperty(PropertyName._joinButton, Variant.From(in _joinButton));
		info.AddProperty(PropertyName._loadingOverlay, Variant.From(in _loadingOverlay));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._hostButton, out var value))
		{
			_hostButton = value.As<NSubmenuButton>();
		}
		if (info.TryGetProperty(PropertyName._loadButton, out var value2))
		{
			_loadButton = value2.As<NSubmenuButton>();
		}
		if (info.TryGetProperty(PropertyName._abandonButton, out var value3))
		{
			_abandonButton = value3.As<NSubmenuButton>();
		}
		if (info.TryGetProperty(PropertyName._joinButton, out var value4))
		{
			_joinButton = value4.As<NSubmenuButton>();
		}
		if (info.TryGetProperty(PropertyName._loadingOverlay, out var value5))
		{
			_loadingOverlay = value5.As<Control>();
		}
	}
}
