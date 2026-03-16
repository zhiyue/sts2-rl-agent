using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Settings;

[ScriptPath("res://src/Core/Nodes/Screens/Settings/NFullscreenTickbox.cs")]
public class NFullscreenTickbox : NSettingsTickbox
{
	public new class MethodName : NSettingsTickbox.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName OnTick = "OnTick";

		public new static readonly StringName OnUntick = "OnUntick";

		public static readonly StringName OnWindowChange = "OnWindowChange";

		public static readonly StringName SetFullscreen = "SetFullscreen";
	}

	public new class PropertyName : NSettingsTickbox.PropertyName
	{
	}

	public new class SignalName : NSettingsTickbox.SignalName
	{
	}

	public override void _Ready()
	{
		ConnectSignals();
		NGame.Instance.Connect(NGame.SignalName.WindowChange, Callable.From<bool>(OnWindowChange));
		OnWindowChange(SaveManager.Instance.SettingsSave.AspectRatioSetting == AspectRatioSetting.Auto);
		if (PlatformUtil.GetSupportedWindowMode().ShouldForceFullscreen())
		{
			Disable();
		}
	}

	protected override void OnTick()
	{
		SetFullscreen(fullscreen: true);
	}

	protected override void OnUntick()
	{
		SetFullscreen(fullscreen: false);
	}

	private void OnWindowChange(bool _)
	{
		base.IsTicked = SaveManager.Instance.SettingsSave.Fullscreen;
	}

	public static void SetFullscreen(bool fullscreen)
	{
		if (PlatformUtil.GetSupportedWindowMode().ShouldForceFullscreen() && !fullscreen)
		{
			Log.Warn($"Tried to go to windowed mode, but the current platform doesn't support it ({PlatformUtil.GetSupportedWindowMode()})");
			return;
		}
		int num = DisplayServer.WindowGetCurrentScreen();
		SettingsSave settingsSave = SaveManager.Instance.SettingsSave;
		if (fullscreen)
		{
			Log.Info($"Setting FULLSCREEN for display [{num}]");
			settingsSave.TargetDisplay = num;
			settingsSave.Fullscreen = true;
			settingsSave.WindowSize = DisplayServer.WindowGetSize();
			settingsSave.WindowPosition = new Vector2I(-1, -1);
		}
		else
		{
			Log.Info($"Exiting FULLSCREEN for display [{num}]");
			if (settingsSave.WindowSize >= DisplayServer.ScreenGetSize(num))
			{
				settingsSave.WindowSize = DisplayServer.ScreenGetSize(num) - new Vector2I(8, 48);
				settingsSave.WindowPosition = new Vector2I(4, 44);
			}
			settingsSave.Fullscreen = false;
		}
		NGame.Instance.ApplyDisplaySettings();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnTick, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUntick, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnWindowChange, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "_", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetFullscreen, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "fullscreen", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnTick && args.Count == 0)
		{
			OnTick();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnUntick && args.Count == 0)
		{
			OnUntick();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnWindowChange && args.Count == 1)
		{
			OnWindowChange(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetFullscreen && args.Count == 1)
		{
			SetFullscreen(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.SetFullscreen && args.Count == 1)
		{
			SetFullscreen(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.OnTick)
		{
			return true;
		}
		if (method == MethodName.OnUntick)
		{
			return true;
		}
		if (method == MethodName.OnWindowChange)
		{
			return true;
		}
		if (method == MethodName.SetFullscreen)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
	}
}
