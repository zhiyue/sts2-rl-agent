using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Nodes;

[ScriptPath("res://src/Core/Nodes/NBackgroundModeHandler.cs")]
public class NBackgroundModeHandler : Node
{
	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _Notification = "_Notification";

		public static readonly StringName EnterBackgroundMode = "EnterBackgroundMode";

		public static readonly StringName ExitBackgroundMode = "ExitBackgroundMode";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName _savedMaxFps = "_savedMaxFps";

		public static readonly StringName _isBackgrounded = "_isBackgrounded";
	}

	public new class SignalName : Node.SignalName
	{
	}

	private const int _backgroundFps = 30;

	private int _savedMaxFps;

	private bool _isBackgrounded;

	private static bool IsHeadless => DisplayServer.GetName().Equals("headless", StringComparison.OrdinalIgnoreCase);

	private static bool IsEditor => OS.HasFeature("editor");

	public override void _Notification(int what)
	{
		if (!IsHeadless && !IsEditor && !NonInteractiveMode.IsActive)
		{
			if ((long)what == 1005)
			{
				EnterBackgroundMode();
			}
			else if ((long)what == 1004)
			{
				ExitBackgroundMode();
			}
		}
	}

	private void EnterBackgroundMode()
	{
		if (!_isBackgrounded && SaveManager.Instance.SettingsSave.LimitFpsInBackground)
		{
			INetGameService netService = RunManager.Instance.NetService;
			if (netService == null || !netService.Type.IsMultiplayer())
			{
				_isBackgrounded = true;
				_savedMaxFps = Engine.MaxFps;
				Engine.MaxFps = 30;
				Log.Info($"Limiting background FPS to {30}");
			}
		}
	}

	private void ExitBackgroundMode()
	{
		if (_isBackgrounded)
		{
			_isBackgrounded = false;
			Engine.MaxFps = _savedMaxFps;
			Log.Info("Restored foreground FPS");
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName._Notification, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "what", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.EnterBackgroundMode, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ExitBackgroundMode, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.EnterBackgroundMode && args.Count == 0)
		{
			EnterBackgroundMode();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ExitBackgroundMode && args.Count == 0)
		{
			ExitBackgroundMode();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Notification)
		{
			return true;
		}
		if (method == MethodName.EnterBackgroundMode)
		{
			return true;
		}
		if (method == MethodName.ExitBackgroundMode)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._savedMaxFps)
		{
			_savedMaxFps = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._isBackgrounded)
		{
			_isBackgrounded = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._savedMaxFps)
		{
			value = VariantUtils.CreateFrom(in _savedMaxFps);
			return true;
		}
		if (name == PropertyName._isBackgrounded)
		{
			value = VariantUtils.CreateFrom(in _isBackgrounded);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._savedMaxFps, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isBackgrounded, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._savedMaxFps, Variant.From(in _savedMaxFps));
		info.AddProperty(PropertyName._isBackgrounded, Variant.From(in _isBackgrounded));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._savedMaxFps, out var value))
		{
			_savedMaxFps = value.As<int>();
		}
		if (info.TryGetProperty(PropertyName._isBackgrounded, out var value2))
		{
			_isBackgrounded = value2.As<bool>();
		}
	}
}
