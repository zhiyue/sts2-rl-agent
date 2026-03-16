using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace MegaCrit.Sts2.Core.Nodes.GodotExtensions;

[ScriptPath("res://src/Core/Nodes/GodotExtensions/NButton.cs")]
public class NButton : NClickableControl
{
	public new class MethodName : NClickableControl.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName ConnectSignals = "ConnectSignals";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _Input = "_Input";

		public new static readonly StringName OnPress = "OnPress";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnEnable = "OnEnable";

		public new static readonly StringName OnDisable = "OnDisable";

		public static readonly StringName UpdateControllerButton = "UpdateControllerButton";

		public static readonly StringName RegisterHotkeys = "RegisterHotkeys";

		public static readonly StringName UnregisterHotkeys = "UnregisterHotkeys";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : NClickableControl.PropertyName
	{
		public static readonly StringName ClickedSfx = "ClickedSfx";

		public static readonly StringName HoveredSfx = "HoveredSfx";

		public static readonly StringName Hotkeys = "Hotkeys";

		public static readonly StringName ControllerIconHotkey = "ControllerIconHotkey";

		public static readonly StringName HasControllerHotkey = "HasControllerHotkey";

		public static readonly StringName _controllerHotkeyIcon = "_controllerHotkeyIcon";
	}

	public new class SignalName : NClickableControl.SignalName
	{
	}

	protected TextureRect? _controllerHotkeyIcon;

	protected virtual string? ClickedSfx => "event:/sfx/ui/clicks/ui_click";

	protected virtual string HoveredSfx => "event:/sfx/ui/clicks/ui_hover";

	protected virtual string[] Hotkeys => Array.Empty<string>();

	protected virtual string? ControllerIconHotkey
	{
		get
		{
			if (Hotkeys.Length == 0)
			{
				return null;
			}
			return Hotkeys[0];
		}
	}

	private bool HasControllerHotkey => Hotkeys.Length != 0;

	public override void _Ready()
	{
		if (GetType() != typeof(NButton))
		{
			Log.Error($"{GetType()}");
			throw new InvalidOperationException("Don't call base._Ready()! Call ConnectSignals() instead.");
		}
		ConnectSignals();
	}

	protected override void ConnectSignals()
	{
		base.ConnectSignals();
		if (HasControllerHotkey)
		{
			RegisterHotkeys();
		}
		_controllerHotkeyIcon = GetNodeOrNull<TextureRect>("%ControllerIcon");
		UpdateControllerButton();
	}

	public override void _EnterTree()
	{
		if (NControllerManager.Instance != null)
		{
			NControllerManager.Instance.Connect(NControllerManager.SignalName.MouseDetected, Callable.From(UpdateControllerButton));
			NControllerManager.Instance.Connect(NControllerManager.SignalName.ControllerDetected, Callable.From(UpdateControllerButton));
		}
		if (NInputManager.Instance != null)
		{
			NInputManager.Instance.Connect(NInputManager.SignalName.InputRebound, Callable.From(UpdateControllerButton));
		}
	}

	public override void _Input(InputEvent inputEvent)
	{
		CheckMouseDragThreshold(inputEvent);
	}

	protected override void OnPress()
	{
		if (ClickedSfx != null)
		{
			SfxCmd.Play(ClickedSfx);
		}
	}

	protected override void OnFocus()
	{
		SfxCmd.Play(HoveredSfx);
	}

	protected override void OnEnable()
	{
		Callable.From(RegisterHotkeys).CallDeferred();
		UpdateControllerButton();
	}

	protected override void OnDisable()
	{
		UnregisterHotkeys();
		UpdateControllerButton();
	}

	protected void UpdateControllerButton()
	{
		if (_controllerHotkeyIcon == null)
		{
			return;
		}
		NControllerManager instance = NControllerManager.Instance;
		if (instance == null)
		{
			return;
		}
		_controllerHotkeyIcon.Visible = instance.IsUsingController && _isEnabled;
		if (ControllerIconHotkey != null)
		{
			Texture2D hotkeyIcon = NInputManager.Instance.GetHotkeyIcon(ControllerIconHotkey);
			if (hotkeyIcon != null)
			{
				_controllerHotkeyIcon.Texture = hotkeyIcon;
			}
		}
	}

	protected void RegisterHotkeys()
	{
		if (HasControllerHotkey && _isEnabled)
		{
			string[] hotkeys = Hotkeys;
			foreach (string hotkey in hotkeys)
			{
				NHotkeyManager.Instance.PushHotkeyPressedBinding(hotkey, base.OnPressHandler);
				NHotkeyManager.Instance.PushHotkeyReleasedBinding(hotkey, base.OnReleaseHandler);
			}
		}
	}

	protected void UnregisterHotkeys()
	{
		if (HasControllerHotkey)
		{
			string[] hotkeys = Hotkeys;
			foreach (string hotkey in hotkeys)
			{
				NHotkeyManager.Instance.RemoveHotkeyPressedBinding(hotkey, base.OnPressHandler);
				NHotkeyManager.Instance.RemoveHotkeyReleasedBinding(hotkey, base.OnReleaseHandler);
			}
		}
	}

	public override void _ExitTree()
	{
		if (NControllerManager.Instance != null)
		{
			NControllerManager.Instance.Disconnect(NControllerManager.SignalName.ControllerDetected, Callable.From(UpdateControllerButton));
			NControllerManager.Instance.Disconnect(NControllerManager.SignalName.MouseDetected, Callable.From(UpdateControllerButton));
		}
		if (NInputManager.Instance != null)
		{
			NInputManager.Instance.Disconnect(NInputManager.SignalName.InputRebound, Callable.From(UpdateControllerButton));
		}
		UnregisterHotkeys();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(12);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ConnectSignals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnPress, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnEnable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnDisable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateControllerButton, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RegisterHotkeys, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UnregisterHotkeys, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.ConnectSignals && args.Count == 0)
		{
			ConnectSignals();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._EnterTree && args.Count == 0)
		{
			_EnterTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Input && args.Count == 1)
		{
			_Input(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPress && args.Count == 0)
		{
			OnPress();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnFocus && args.Count == 0)
		{
			OnFocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnEnable && args.Count == 0)
		{
			OnEnable();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnDisable && args.Count == 0)
		{
			OnDisable();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateControllerButton && args.Count == 0)
		{
			UpdateControllerButton();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RegisterHotkeys && args.Count == 0)
		{
			RegisterHotkeys();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UnregisterHotkeys && args.Count == 0)
		{
			UnregisterHotkeys();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.ConnectSignals)
		{
			return true;
		}
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName._Input)
		{
			return true;
		}
		if (method == MethodName.OnPress)
		{
			return true;
		}
		if (method == MethodName.OnFocus)
		{
			return true;
		}
		if (method == MethodName.OnEnable)
		{
			return true;
		}
		if (method == MethodName.OnDisable)
		{
			return true;
		}
		if (method == MethodName.UpdateControllerButton)
		{
			return true;
		}
		if (method == MethodName.RegisterHotkeys)
		{
			return true;
		}
		if (method == MethodName.UnregisterHotkeys)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._controllerHotkeyIcon)
		{
			_controllerHotkeyIcon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		string from;
		if (name == PropertyName.ClickedSfx)
		{
			from = ClickedSfx;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.HoveredSfx)
		{
			from = HoveredSfx;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.Hotkeys)
		{
			value = VariantUtils.CreateFrom<string[]>(Hotkeys);
			return true;
		}
		if (name == PropertyName.ControllerIconHotkey)
		{
			from = ControllerIconHotkey;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.HasControllerHotkey)
		{
			value = VariantUtils.CreateFrom<bool>(HasControllerHotkey);
			return true;
		}
		if (name == PropertyName._controllerHotkeyIcon)
		{
			value = VariantUtils.CreateFrom(in _controllerHotkeyIcon);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName.ClickedSfx, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName.HoveredSfx, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.PackedStringArray, PropertyName.Hotkeys, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName.ControllerIconHotkey, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.HasControllerHotkey, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._controllerHotkeyIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._controllerHotkeyIcon, Variant.From(in _controllerHotkeyIcon));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._controllerHotkeyIcon, out var value))
		{
			_controllerHotkeyIcon = value.As<TextureRect>();
		}
	}
}
