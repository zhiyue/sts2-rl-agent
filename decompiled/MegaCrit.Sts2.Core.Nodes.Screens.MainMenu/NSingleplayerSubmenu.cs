using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Nodes.Screens.CustomRun;
using MegaCrit.Sts2.Core.Nodes.Screens.DailyRun;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.Timeline.Epochs;

namespace MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

[ScriptPath("res://src/Core/Nodes/Screens/MainMenu/NSingleplayerSubmenu.cs")]
public class NSingleplayerSubmenu : NSubmenu
{
	public new class MethodName : NSubmenu.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName RefreshButtons = "RefreshButtons";

		public new static readonly StringName OnSubmenuOpened = "OnSubmenuOpened";

		public static readonly StringName OpenCharacterSelect = "OpenCharacterSelect";

		public static readonly StringName OpenDailyScreen = "OpenDailyScreen";

		public static readonly StringName OpenCustomScreen = "OpenCustomScreen";
	}

	public new class PropertyName : NSubmenu.PropertyName
	{
		public new static readonly StringName InitialFocusedControl = "InitialFocusedControl";

		public static readonly StringName _standardButton = "_standardButton";

		public static readonly StringName _dailyButton = "_dailyButton";

		public static readonly StringName _customButton = "_customButton";
	}

	public new class SignalName : NSubmenu.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("screens/singleplayer_submenu");

	private NSubmenuButton _standardButton;

	private NSubmenuButton _dailyButton;

	private NSubmenuButton _customButton;

	private const string _keyStandard = "STANDARD";

	private const string _keyDaily = "DAILY";

	private const string _keyCustom = "CUSTOM";

	protected override Control InitialFocusedControl => _standardButton;

	public static NSingleplayerSubmenu? Create()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		return PreloadManager.Cache.GetScene(_scenePath).Instantiate<NSingleplayerSubmenu>(PackedScene.GenEditState.Disabled);
	}

	public override void _Ready()
	{
		ConnectSignals();
		_standardButton = GetNode<NSubmenuButton>("StandardButton");
		_standardButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OpenCharacterSelect));
		_standardButton.SetIconAndLocalization("STANDARD");
		_dailyButton = GetNode<NSubmenuButton>("DailyButton");
		_dailyButton.Connect(NClickableControl.SignalName.Released, Callable.From((Action<NButton>)OpenDailyScreen));
		_dailyButton.SetIconAndLocalization("DAILY");
		_customButton = GetNode<NSubmenuButton>("CustomRunButton");
		_customButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OpenCustomScreen));
		_customButton.SetIconAndLocalization("CUSTOM");
	}

	private void RefreshButtons()
	{
		_dailyButton.SetEnabled(SaveManager.Instance.IsEpochRevealed<DailyRunEpoch>());
		_customButton.SetEnabled(SaveManager.Instance.IsEpochRevealed<CustomAndSeedsEpoch>());
		_dailyButton.RefreshLabels();
		_customButton.RefreshLabels();
	}

	public override void OnSubmenuOpened()
	{
		RefreshButtons();
	}

	private void OpenCharacterSelect(NButton _)
	{
		NCharacterSelectScreen submenuType = _stack.GetSubmenuType<NCharacterSelectScreen>();
		submenuType.InitializeSingleplayer();
		_stack.Push(submenuType);
	}

	private void OpenDailyScreen(NButton _)
	{
		OpenDailyScreen();
	}

	private void OpenDailyScreen()
	{
		NDailyRunScreen submenuType = _stack.GetSubmenuType<NDailyRunScreen>();
		submenuType.InitializeSingleplayer();
		_stack.Push(submenuType);
	}

	private void OpenCustomScreen(NButton _)
	{
		NCustomRunScreen submenuType = _stack.GetSubmenuType<NCustomRunScreen>();
		submenuType.InitializeSingleplayer();
		_stack.Push(submenuType);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(8);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshButtons, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuOpened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OpenCharacterSelect, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OpenDailyScreen, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OpenDailyScreen, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OpenCustomScreen, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NSingleplayerSubmenu>(Create());
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
		if (method == MethodName.OpenCharacterSelect && args.Count == 1)
		{
			OpenCharacterSelect(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenDailyScreen && args.Count == 1)
		{
			OpenDailyScreen(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenDailyScreen && args.Count == 0)
		{
			OpenDailyScreen();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenCustomScreen && args.Count == 1)
		{
			OpenCustomScreen(VariantUtils.ConvertTo<NButton>(in args[0]));
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
			ret = VariantUtils.CreateFrom<NSingleplayerSubmenu>(Create());
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
		if (method == MethodName.OpenCharacterSelect)
		{
			return true;
		}
		if (method == MethodName.OpenDailyScreen)
		{
			return true;
		}
		if (method == MethodName.OpenCustomScreen)
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
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._standardButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._dailyButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._customButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
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
	}
}
