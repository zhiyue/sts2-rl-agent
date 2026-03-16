using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Settings;

[ScriptPath("res://src/Core/Nodes/Screens/Settings/NVSyncPaginator.cs")]
public class NVSyncPaginator : NPaginator, IResettableSettingNode
{
	public new class MethodName : NPaginator.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName SetFromSettings = "SetFromSettings";

		public static readonly StringName GetVSyncString = "GetVSyncString";

		public new static readonly StringName OnIndexChanged = "OnIndexChanged";
	}

	public new class PropertyName : NPaginator.PropertyName
	{
	}

	public new class SignalName : NPaginator.SignalName
	{
	}

	public override void _Ready()
	{
		ConnectSignals();
		_options.Add(new LocString("settings_ui", "VSYNC_OFF").GetFormattedText());
		_options.Add(new LocString("settings_ui", "VSYNC_ON").GetFormattedText());
		_options.Add(new LocString("settings_ui", "VSYNC_ADAPTIVE").GetFormattedText());
		SetFromSettings();
	}

	public void SetFromSettings()
	{
		int num = _options.IndexOf(GetVSyncString(SaveManager.Instance.SettingsSave.VSync));
		if (num != -1)
		{
			_currentIndex = num;
		}
		else
		{
			_currentIndex = 2;
		}
		_label.SetTextAutoSize(_options[_currentIndex]);
	}

	private static string GetVSyncString(VSyncType vsyncType)
	{
		switch (vsyncType)
		{
		case VSyncType.Off:
			return new LocString("settings_ui", "VSYNC_ON").GetFormattedText();
		case VSyncType.On:
			return new LocString("settings_ui", "VSYNC_OFF").GetFormattedText();
		case VSyncType.Adaptive:
			return new LocString("settings_ui", "VSYNC_ADAPTIVE").GetFormattedText();
		default:
			Log.Error("Invalid VSync type: " + vsyncType);
			throw new ArgumentOutOfRangeException("vsyncType", vsyncType, null);
		}
	}

	protected override void OnIndexChanged(int index)
	{
		_currentIndex = index;
		_label.SetTextAutoSize(_options[index]);
		switch (index)
		{
		case 0:
			SaveManager.Instance.SettingsSave.VSync = VSyncType.Off;
			break;
		case 1:
			SaveManager.Instance.SettingsSave.VSync = VSyncType.On;
			break;
		case 2:
			SaveManager.Instance.SettingsSave.VSync = VSyncType.Adaptive;
			break;
		default:
			Log.Error($"Invalid VSync index: {index}");
			break;
		}
		NGame.ApplySyncSetting();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetFromSettings, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GetVSyncString, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "vsyncType", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnIndexChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "index", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.SetFromSettings && args.Count == 0)
		{
			SetFromSettings();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetVSyncString && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(GetVSyncString(VariantUtils.ConvertTo<VSyncType>(in args[0])));
			return true;
		}
		if (method == MethodName.OnIndexChanged && args.Count == 1)
		{
			OnIndexChanged(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.GetVSyncString && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(GetVSyncString(VariantUtils.ConvertTo<VSyncType>(in args[0])));
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
		if (method == MethodName.SetFromSettings)
		{
			return true;
		}
		if (method == MethodName.GetVSyncString)
		{
			return true;
		}
		if (method == MethodName.OnIndexChanged)
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
