using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Settings;

[ScriptPath("res://src/Core/Nodes/Screens/Settings/NSfxVolumeSlider.cs")]
public class NSfxVolumeSlider : NSettingsSlider
{
	public new class MethodName : NSettingsSlider.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName OnValueChanged = "OnValueChanged";

		public static readonly StringName OnDragEnded = "OnDragEnded";
	}

	public new class PropertyName : NSettingsSlider.PropertyName
	{
	}

	public new class SignalName : NSettingsSlider.SignalName
	{
	}

	public override void _Ready()
	{
		ConnectSignals();
		_slider.SetValueWithoutAnimation(SaveManager.Instance.SettingsSave.VolumeSfx * 100f);
		_slider.Connect(Range.SignalName.ValueChanged, Callable.From<double>(OnValueChanged));
		_slider.Connect(NSlider.SignalName.MouseReleased, Callable.From<bool>(OnDragEnded));
	}

	private static void OnValueChanged(double value)
	{
		float num = (float)value / 100f;
		SaveManager.Instance.SettingsSave.VolumeSfx = num;
		NAudioManager.Instance?.SetSfxVol(num);
		NDebugAudioManager.Instance?.SetSfxAudioVolume(num);
	}

	private static void OnDragEnded(bool valueChanged)
	{
		if (valueChanged)
		{
			NDebugAudioManager.Instance?.Play("dagger_throw.mp3");
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnValueChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "value", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnDragEnded, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "valueChanged", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.OnValueChanged && args.Count == 1)
		{
			OnValueChanged(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnDragEnded && args.Count == 1)
		{
			OnDragEnded(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.OnValueChanged && args.Count == 1)
		{
			OnValueChanged(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnDragEnded && args.Count == 1)
		{
			OnDragEnded(VariantUtils.ConvertTo<bool>(in args[0]));
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
		if (method == MethodName.OnValueChanged)
		{
			return true;
		}
		if (method == MethodName.OnDragEnded)
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
