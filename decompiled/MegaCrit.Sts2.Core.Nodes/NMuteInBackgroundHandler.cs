using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Nodes;

[ScriptPath("res://src/Core/Nodes/NMuteInBackgroundHandler.cs")]
public class NMuteInBackgroundHandler : Node
{
	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _Notification = "_Notification";

		public static readonly StringName Mute = "Mute";

		public static readonly StringName Unmute = "Unmute";

		public static readonly StringName SetMasterVolume = "SetMasterVolume";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName _tween = "_tween";
	}

	public new class SignalName : Node.SignalName
	{
	}

	private Tween? _tween;

	public override void _Notification(int what)
	{
		if ((long)what == 1005)
		{
			Mute();
		}
		else if ((long)what == 1004)
		{
			Unmute();
		}
	}

	private void Mute()
	{
		PrefsSave prefsSave = SaveManager.Instance.PrefsSave;
		SettingsSave settingsSave = SaveManager.Instance.SettingsSave;
		if (prefsSave != null && settingsSave != null && prefsSave.MuteInBackground)
		{
			_tween = CreateTween();
			_tween.TweenMethod(Callable.From<float>(SetMasterVolume), settingsSave.VolumeMaster, 0f, 1.0);
		}
	}

	private void Unmute()
	{
		if (_tween != null)
		{
			_tween?.Kill();
			_tween = null;
			SetMasterVolume(SaveManager.Instance.SettingsSave.VolumeMaster);
		}
	}

	private static void SetMasterVolume(float volume)
	{
		NGame.Instance.AudioManager.SetMasterVol(volume);
		NGame.Instance.DebugAudio.SetMasterAudioVolume(volume);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName._Notification, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "what", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.Mute, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Unmute, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetMasterVolume, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "volume", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._Notification && args.Count == 1)
		{
			_Notification(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Mute && args.Count == 0)
		{
			Mute();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Unmute && args.Count == 0)
		{
			Unmute();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetMasterVolume && args.Count == 1)
		{
			SetMasterVolume(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.SetMasterVolume && args.Count == 1)
		{
			SetMasterVolume(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Notification)
		{
			return true;
		}
		if (method == MethodName.Mute)
		{
			return true;
		}
		if (method == MethodName.Unmute)
		{
			return true;
		}
		if (method == MethodName.SetMasterVolume)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._tween, out var value))
		{
			_tween = value.As<Tween>();
		}
	}
}
