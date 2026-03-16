using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Nodes.Screens.CustomRun;
using MegaCrit.Sts2.Core.Nodes.Screens.DailyRun;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Platform.Steam;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.Timeline.Epochs;

namespace MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

[ScriptPath("res://src/Core/Nodes/Screens/MainMenu/NMultiplayerHostSubmenu.cs")]
public class NMultiplayerHostSubmenu : NSubmenu
{
	public new class MethodName : NSubmenu.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName RefreshButtons = "RefreshButtons";

		public new static readonly StringName OnSubmenuOpened = "OnSubmenuOpened";

		public static readonly StringName OnStandardPressed = "OnStandardPressed";

		public static readonly StringName OnDailyPressed = "OnDailyPressed";

		public static readonly StringName OnCustomPressed = "OnCustomPressed";

		public static readonly StringName StartHost = "StartHost";
	}

	public new class PropertyName : NSubmenu.PropertyName
	{
		public new static readonly StringName InitialFocusedControl = "InitialFocusedControl";

		public static readonly StringName _standardButton = "_standardButton";

		public static readonly StringName _dailyButton = "_dailyButton";

		public static readonly StringName _customButton = "_customButton";

		public static readonly StringName _loadingOverlay = "_loadingOverlay";
	}

	public new class SignalName : NSubmenu.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("screens/multiplayer_host_submenu");

	private NSubmenuButton _standardButton;

	private NSubmenuButton _dailyButton;

	private NSubmenuButton _customButton;

	private const string _keyStandard = "STANDARD_MP";

	private const string _keyDaily = "DAILY_MP";

	private const string _keyCustom = "CUSTOM_MP";

	private Control _loadingOverlay;

	protected override Control InitialFocusedControl => _standardButton;

	public static NMultiplayerHostSubmenu? Create()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		return PreloadManager.Cache.GetScene(_scenePath).Instantiate<NMultiplayerHostSubmenu>(PackedScene.GenEditState.Disabled);
	}

	public override void _Ready()
	{
		ConnectSignals();
		_loadingOverlay = GetNode<Control>("%LoadingOverlay");
		_standardButton = GetNode<NSubmenuButton>("StandardButton");
		_standardButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnStandardPressed));
		_standardButton.SetIconAndLocalization("STANDARD_MP");
		_dailyButton = GetNode<NSubmenuButton>("DailyButton");
		_dailyButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnDailyPressed));
		_dailyButton.SetIconAndLocalization("DAILY_MP");
		_customButton = GetNode<NSubmenuButton>("CustomRunButton");
		_customButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnCustomPressed));
		_customButton.SetIconAndLocalization("CUSTOM_MP");
	}

	private void RefreshButtons()
	{
		_dailyButton.SetEnabled(SaveManager.Instance.IsEpochRevealed<DailyRunEpoch>());
		_customButton.SetEnabled(SaveManager.Instance.IsEpochRevealed<CustomAndSeedsEpoch>());
	}

	public override void OnSubmenuOpened()
	{
		RefreshButtons();
	}

	private void OnStandardPressed(NButton _)
	{
		StartHost(GameMode.Standard);
	}

	private void OnDailyPressed(NButton _)
	{
		StartHost(GameMode.Daily);
	}

	private void OnCustomPressed(NButton _)
	{
		StartHost(GameMode.Custom);
	}

	public void StartHost(GameMode gameMode)
	{
		TaskHelper.RunSafely(StartHostAsync(gameMode, _loadingOverlay, _stack));
	}

	public static async Task StartHostAsync(GameMode gameMode, Control loadingOverlay, NSubmenuStack stack)
	{
		PlatformType platformType = ((SteamInitializer.Initialized && !CommandLineHelper.HasArg("fastmp")) ? PlatformType.Steam : PlatformType.None);
		loadingOverlay.Visible = true;
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
				switch (gameMode)
				{
				case GameMode.Standard:
				{
					NCharacterSelectScreen submenuType3 = stack.GetSubmenuType<NCharacterSelectScreen>();
					submenuType3.InitializeMultiplayerAsHost(netService, 4);
					stack.Push(submenuType3);
					break;
				}
				case GameMode.Daily:
				{
					NDailyRunScreen submenuType2 = stack.GetSubmenuType<NDailyRunScreen>();
					submenuType2.InitializeMultiplayerAsHost(netService);
					stack.Push(submenuType2);
					break;
				}
				default:
				{
					NCustomRunScreen submenuType = stack.GetSubmenuType<NCustomRunScreen>();
					submenuType.InitializeMultiplayerAsHost(netService, 4);
					stack.Push(submenuType);
					break;
				}
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
		catch
		{
			NErrorPopup nErrorPopup2 = NErrorPopup.Create(new NetErrorInfo(NetError.InternalError, selfInitiated: false));
			if (nErrorPopup2 != null)
			{
				NModalContainer.Instance.Add(nErrorPopup2);
			}
			throw;
		}
		finally
		{
			loadingOverlay.Visible = false;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(8);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshButtons, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuOpened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnStandardPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnDailyPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnCustomPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.StartHost, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "gameMode", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NMultiplayerHostSubmenu>(Create());
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshButtons && args.Count == 0)
		{
			RefreshButtons();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSubmenuOpened && args.Count == 0)
		{
			OnSubmenuOpened();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnStandardPressed && args.Count == 1)
		{
			OnStandardPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnDailyPressed && args.Count == 1)
		{
			OnDailyPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnCustomPressed && args.Count == 1)
		{
			OnCustomPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartHost && args.Count == 1)
		{
			StartHost(VariantUtils.ConvertTo<GameMode>(in args[0]));
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
			ret = VariantUtils.CreateFrom<NMultiplayerHostSubmenu>(Create());
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
		if (method == MethodName.RefreshButtons)
		{
			return true;
		}
		if (method == MethodName.OnSubmenuOpened)
		{
			return true;
		}
		if (method == MethodName.OnStandardPressed)
		{
			return true;
		}
		if (method == MethodName.OnDailyPressed)
		{
			return true;
		}
		if (method == MethodName.OnCustomPressed)
		{
			return true;
		}
		if (method == MethodName.StartHost)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._standardButton)
		{
			_standardButton = VariantUtils.ConvertTo<NSubmenuButton>(in value);
			return true;
		}
		if (name == PropertyName._dailyButton)
		{
			_dailyButton = VariantUtils.ConvertTo<NSubmenuButton>(in value);
			return true;
		}
		if (name == PropertyName._customButton)
		{
			_customButton = VariantUtils.ConvertTo<NSubmenuButton>(in value);
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
		if (name == PropertyName._standardButton)
		{
			value = VariantUtils.CreateFrom(in _standardButton);
			return true;
		}
		if (name == PropertyName._dailyButton)
		{
			value = VariantUtils.CreateFrom(in _dailyButton);
			return true;
		}
		if (name == PropertyName._customButton)
		{
			value = VariantUtils.CreateFrom(in _customButton);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._standardButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._dailyButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._customButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._loadingOverlay, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.InitialFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._standardButton, Variant.From(in _standardButton));
		info.AddProperty(PropertyName._dailyButton, Variant.From(in _dailyButton));
		info.AddProperty(PropertyName._customButton, Variant.From(in _customButton));
		info.AddProperty(PropertyName._loadingOverlay, Variant.From(in _loadingOverlay));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._standardButton, out var value))
		{
			_standardButton = value.As<NSubmenuButton>();
		}
		if (info.TryGetProperty(PropertyName._dailyButton, out var value2))
		{
			_dailyButton = value2.As<NSubmenuButton>();
		}
		if (info.TryGetProperty(PropertyName._customButton, out var value3))
		{
			_customButton = value3.As<NSubmenuButton>();
		}
		if (info.TryGetProperty(PropertyName._loadingOverlay, out var value4))
		{
			_loadingOverlay = value4.As<Control>();
		}
	}
}
