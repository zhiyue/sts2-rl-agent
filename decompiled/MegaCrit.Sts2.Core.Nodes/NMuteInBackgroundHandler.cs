using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Nodes;

/// <summary>
/// Responsible for muting audio when the game enters the background, if the player has the setting set.
/// </summary>
[ScriptPath("res://src/Core/Nodes/NMuteInBackgroundHandler.cs")]
public class NMuteInBackgroundHandler : Node
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Node.MethodName
	{
		/// <summary>
		/// Cached name for the '_Notification' method.
		/// </summary>
		public new static readonly StringName _Notification = "_Notification";

		/// <summary>
		/// Cached name for the 'Mute' method.
		/// </summary>
		public static readonly StringName Mute = "Mute";

		/// <summary>
		/// Cached name for the 'Unmute' method.
		/// </summary>
		public static readonly StringName Unmute = "Unmute";

		/// <summary>
		/// Cached name for the 'SetMasterVolume' method.
		/// </summary>
		public static readonly StringName SetMasterVolume = "SetMasterVolume";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Node.PropertyName
	{
		/// <summary>
		/// Cached name for the '_tween' field.
		/// </summary>
		public static readonly StringName _tween = "_tween";

		/// <summary>
		/// Cached name for the '_lastFocusOutMsec' field.
		/// </summary>
		public static readonly StringName _lastFocusOutMsec = "_lastFocusOutMsec";

		/// <summary>
		/// Cached name for the '_loggedEnvironment' field.
		/// </summary>
		public static readonly StringName _loggedEnvironment = "_loggedEnvironment";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Node.SignalName
	{
	}

	private Tween? _tween;

	private ulong _lastFocusOutMsec;

	private bool _loggedEnvironment;

	public override void _Notification(int what)
	{
		if ((long)what == 1005)
		{
			if (!_loggedEnvironment)
			{
				_loggedEnvironment = true;
				Log.Info($"MuteInBackground: environment [displayServer={DisplayServer.GetName()}, os={OS.GetName()}]");
			}
			_lastFocusOutMsec = Time.GetTicksMsec();
			Log.Info($"MuteInBackground: FocusOut received [tween={_tween != null}, windowFocused={GetWindow().HasFocus()}]");
			Mute();
		}
		else if ((long)what == 1004)
		{
			ulong value = Time.GetTicksMsec() - _lastFocusOutMsec;
			Log.Info($"MuteInBackground: FocusIn received [tween={_tween != null}, msSinceFocusOut={value}, windowFocused={GetWindow().HasFocus()}]");
			Unmute();
		}
	}

	private void Mute()
	{
		PrefsSave prefsSave = SaveManager.Instance.PrefsSave;
		SettingsSave settingsSave = SaveManager.Instance.SettingsSave;
		if (prefsSave != null && settingsSave != null && prefsSave.MuteInBackground)
		{
			bool flag = _tween != null;
			_tween?.Kill();
			_tween = CreateTween();
			_tween.TweenMethod(Callable.From<float>(SetMasterVolume), settingsSave.VolumeMaster, 0f, 1.0);
			if (flag)
			{
				Log.Info("MuteInBackground: Muting (replaced existing tween)");
			}
		}
		else
		{
			Log.Info("MuteInBackground: FocusOut ignored (setting off or saves null)");
		}
	}

	private void Unmute()
	{
		if (_tween != null)
		{
			_tween?.Kill();
			_tween = null;
			float volumeMaster = SaveManager.Instance.SettingsSave.VolumeMaster;
			SetMasterVolume(volumeMaster);
			Log.Info($"MuteInBackground: Unmuted, restored volume to {volumeMaster}");
		}
	}

	private static void SetMasterVolume(float volume)
	{
		NGame.Instance.AudioManager.SetMasterVol(volume);
		NGame.Instance.DebugAudio.SetMasterAudioVolume(volume);
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
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

	/// <inheritdoc />
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

	/// <inheritdoc />
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

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._lastFocusOutMsec)
		{
			_lastFocusOutMsec = VariantUtils.ConvertTo<ulong>(in value);
			return true;
		}
		if (name == PropertyName._loggedEnvironment)
		{
			_loggedEnvironment = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._lastFocusOutMsec)
		{
			value = VariantUtils.CreateFrom(in _lastFocusOutMsec);
			return true;
		}
		if (name == PropertyName._loggedEnvironment)
		{
			value = VariantUtils.CreateFrom(in _loggedEnvironment);
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
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._lastFocusOutMsec, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._loggedEnvironment, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._lastFocusOutMsec, Variant.From(in _lastFocusOutMsec));
		info.AddProperty(PropertyName._loggedEnvironment, Variant.From(in _loggedEnvironment));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._tween, out var value))
		{
			_tween = value.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._lastFocusOutMsec, out var value2))
		{
			_lastFocusOutMsec = value2.As<ulong>();
		}
		if (info.TryGetProperty(PropertyName._loggedEnvironment, out var value3))
		{
			_loggedEnvironment = value3.As<bool>();
		}
	}
}
