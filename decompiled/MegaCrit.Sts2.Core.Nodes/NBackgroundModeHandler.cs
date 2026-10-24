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

/// <summary>
/// Limits FPS to 30 when the game window loses focus to reduce GPU load and save battery.
/// </summary>
[ScriptPath("res://src/Core/Nodes/NBackgroundModeHandler.cs")]
public class NBackgroundModeHandler : Node
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
		/// Cached name for the 'EnterBackgroundMode' method.
		/// </summary>
		public static readonly StringName EnterBackgroundMode = "EnterBackgroundMode";

		/// <summary>
		/// Cached name for the 'ExitBackgroundMode' method.
		/// </summary>
		public static readonly StringName ExitBackgroundMode = "ExitBackgroundMode";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Node.PropertyName
	{
		/// <summary>
		/// Cached name for the '_savedMaxFps' field.
		/// </summary>
		public static readonly StringName _savedMaxFps = "_savedMaxFps";

		/// <summary>
		/// Cached name for the '_isBackgrounded' field.
		/// </summary>
		public static readonly StringName _isBackgrounded = "_isBackgrounded";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
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
		if (_isBackgrounded)
		{
			Log.Info("BackgroundMode: duplicate FocusOut (already backgrounded)");
		}
		else if (SaveManager.Instance.SettingsSave.LimitFpsInBackground)
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

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
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

	/// <inheritdoc />
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

	/// <inheritdoc />
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

	/// <inheritdoc />
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

	/// <summary>
	/// Get the property information for all the properties declared in this class.
	/// This method is used by Godot to register the available properties in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._savedMaxFps, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isBackgrounded, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._savedMaxFps, Variant.From(in _savedMaxFps));
		info.AddProperty(PropertyName._isBackgrounded, Variant.From(in _isBackgrounded));
	}

	/// <inheritdoc />
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
