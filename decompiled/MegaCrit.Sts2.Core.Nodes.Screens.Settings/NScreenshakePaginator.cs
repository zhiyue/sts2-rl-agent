using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Settings;

[ScriptPath("res://src/Core/Nodes/Screens/Settings/NScreenshakePaginator.cs")]
public class NScreenshakePaginator : NPaginator, IResettableSettingNode
{
	public new class MethodName : NPaginator.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName SetFromSettings = "SetFromSettings";

		public new static readonly StringName OnIndexChanged = "OnIndexChanged";

		public static readonly StringName GetShakeMultiplier = "GetShakeMultiplier";
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
		_options.Add(new LocString("settings_ui", "SCREENSHAKE_NONE").GetFormattedText());
		_options.Add(new LocString("settings_ui", "SCREENSHAKE_SOME").GetFormattedText());
		_options.Add(new LocString("settings_ui", "SCREENSHAKE_NORMAL").GetFormattedText());
		_options.Add(new LocString("settings_ui", "SCREENSHAKE_LOTS").GetFormattedText());
		_options.Add(new LocString("settings_ui", "SCREENSHAKE_CAAAW").GetFormattedText());
		SetFromSettings();
	}

	public void SetFromSettings()
	{
		_currentIndex = SaveManager.Instance.PrefsSave.ScreenShakeOptionIndex;
		if (_currentIndex >= _options.Count)
		{
			_label.SetTextAutoSize(">:P");
		}
		else
		{
			_label.SetTextAutoSize(_options[_currentIndex]);
		}
		NGame.Instance.SetScreenshakeMultiplier(GetShakeMultiplier(_currentIndex));
	}

	protected override void OnIndexChanged(int index)
	{
		if (_currentIndex >= _options.Count)
		{
			_currentIndex = 2;
		}
		_currentIndex = index;
		_label.SetTextAutoSize(_options[index]);
		SaveManager.Instance.PrefsSave.ScreenShakeOptionIndex = _currentIndex;
		Log.Info($"Screenshake set to: {_currentIndex}");
		NGame.Instance.SetScreenshakeMultiplier(GetShakeMultiplier(_currentIndex));
		NGame.Instance.ScreenShakeTrauma(ShakeStrength.Medium);
	}

	public static float GetShakeMultiplier(int index)
	{
		return index switch
		{
			0 => 0f, 
			1 => 0.5f, 
			2 => 1f, 
			3 => 2f, 
			4 => 4f, 
			_ => index, 
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetFromSettings, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnIndexChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "index", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetShakeMultiplier, new PropertyInfo(Variant.Type.Float, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
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
		if (method == MethodName.OnIndexChanged && args.Count == 1)
		{
			OnIndexChanged(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetShakeMultiplier && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<float>(GetShakeMultiplier(VariantUtils.ConvertTo<int>(in args[0])));
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.GetShakeMultiplier && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<float>(GetShakeMultiplier(VariantUtils.ConvertTo<int>(in args[0])));
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
		if (method == MethodName.OnIndexChanged)
		{
			return true;
		}
		if (method == MethodName.GetShakeMultiplier)
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
