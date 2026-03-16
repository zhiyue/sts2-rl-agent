using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.ControllerInput.ControllerConfigs;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.CommonUi;

[ScriptPath("res://src/Core/Nodes/CommonUi/NControllerManager.cs")]
public class NControllerManager : Node
{
	[Signal]
	public delegate void ControllerDetectedEventHandler();

	[Signal]
	public delegate void MouseDetectedEventHandler();

	[Signal]
	public delegate void ControllerTypeChangedEventHandler();

	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _ExitTree = "_ExitTree";

		public new static readonly StringName _Process = "_Process";

		public new static readonly StringName _Input = "_Input";

		public static readonly StringName OnControllerTypeChanged = "OnControllerTypeChanged";

		public static readonly StringName CheckForMouseInput = "CheckForMouseInput";

		public static readonly StringName CheckForControllerInput = "CheckForControllerInput";

		public static readonly StringName ControlModeChanged = "ControlModeChanged";

		public static readonly StringName OnScreenContextChanged = "OnScreenContextChanged";

		public static readonly StringName GetHotkeyIcon = "GetHotkeyIcon";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName ShouldAllowControllerRebinding = "ShouldAllowControllerRebinding";

		public static readonly StringName IsUsingController = "IsUsingController";

		public static readonly StringName ControllerMappingType = "ControllerMappingType";

		public static readonly StringName _lastMousePosition = "_lastMousePosition";

		public static readonly StringName _label = "_label";

		public static readonly StringName _notifyTween = "_notifyTween";
	}

	public new class SignalName : Node.SignalName
	{
		public static readonly StringName ControllerDetected = "ControllerDetected";

		public static readonly StringName MouseDetected = "MouseDetected";

		public static readonly StringName ControllerTypeChanged = "ControllerTypeChanged";
	}

	private IControllerInputStrategy? _inputStrategy;

	private static readonly Vector2 _offscreenPos = Vector2.One * -1000f;

	private Vector2 _lastMousePosition;

	private MegaLabel _label;

	private Tween? _notifyTween;

	private ControllerDetectedEventHandler backing_ControllerDetected;

	private MouseDetectedEventHandler backing_MouseDetected;

	private ControllerTypeChangedEventHandler backing_ControllerTypeChanged;

	public static NControllerManager? Instance
	{
		get
		{
			if (NGame.Instance == null)
			{
				return null;
			}
			return NGame.Instance.InputManager.ControllerManager;
		}
	}

	public bool ShouldAllowControllerRebinding => _inputStrategy?.ShouldAllowControllerRebinding ?? true;

	public bool IsUsingController { get; private set; }

	public Dictionary<StringName, StringName> GetDefaultControllerInputMap
	{
		get
		{
			if (_inputStrategy == null)
			{
				return new SteamControllerConfig().DefaultControllerInputMap;
			}
			return _inputStrategy.GetDefaultControllerInputMap;
		}
	}

	public ControllerMappingType ControllerMappingType
	{
		get
		{
			if (_inputStrategy == null)
			{
				return ControllerMappingType.Default;
			}
			return _inputStrategy.ControllerConfig.ControllerMappingType;
		}
	}

	public event ControllerDetectedEventHandler ControllerDetected
	{
		add
		{
			backing_ControllerDetected = (ControllerDetectedEventHandler)Delegate.Combine(backing_ControllerDetected, value);
		}
		remove
		{
			backing_ControllerDetected = (ControllerDetectedEventHandler)Delegate.Remove(backing_ControllerDetected, value);
		}
	}

	public event MouseDetectedEventHandler MouseDetected
	{
		add
		{
			backing_MouseDetected = (MouseDetectedEventHandler)Delegate.Combine(backing_MouseDetected, value);
		}
		remove
		{
			backing_MouseDetected = (MouseDetectedEventHandler)Delegate.Remove(backing_MouseDetected, value);
		}
	}

	public event ControllerTypeChangedEventHandler ControllerTypeChanged
	{
		add
		{
			backing_ControllerTypeChanged = (ControllerTypeChangedEventHandler)Delegate.Combine(backing_ControllerTypeChanged, value);
		}
		remove
		{
			backing_ControllerTypeChanged = (ControllerTypeChangedEventHandler)Delegate.Remove(backing_ControllerTypeChanged, value);
		}
	}

	public async Task Init()
	{
		ActiveScreenContext.Instance.Updated += OnScreenContextChanged;
		_label = GetNode<MegaLabel>("Label");
		_label.Modulate = Colors.Transparent;
		_inputStrategy = new SteamControllerInputStrategy();
		await _inputStrategy.Init();
	}

	public override void _ExitTree()
	{
		ActiveScreenContext.Instance.Updated -= OnScreenContextChanged;
	}

	public override void _Process(double delta)
	{
		_inputStrategy?.ProcessInput();
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (IsUsingController)
		{
			CheckForMouseInput(inputEvent);
		}
		else
		{
			CheckForControllerInput(inputEvent);
		}
	}

	public void OnControllerTypeChanged()
	{
		EmitSignalControllerTypeChanged();
	}

	private void CheckForMouseInput(InputEvent inputEvent)
	{
		bool flag = inputEvent is InputEventMouseButton;
		bool flag2 = inputEvent is InputEventMouseMotion { Velocity: var velocity } && velocity.LengthSquared() > 100f;
		Viewport viewport = GetViewport();
		if (flag || flag2)
		{
			IsUsingController = false;
			Input.WarpMouse(_lastMousePosition);
			viewport?.GuiReleaseFocus();
			EmitSignal(SignalName.MouseDetected);
			ControlModeChanged();
		}
	}

	private void CheckForControllerInput(InputEvent inputEvent)
	{
		if (Controller.AllControllerInputs.Any((StringName i) => inputEvent.IsActionPressed(i)))
		{
			IsUsingController = true;
			Viewport viewport = GetViewport();
			if (viewport != null)
			{
				Vector2I vector2I = DisplayServer.MouseGetPosition();
				Vector2I vector2I2 = DisplayServer.WindowGetPosition();
				_lastMousePosition = new Vector2(vector2I.X - vector2I2.X, vector2I.Y - vector2I2.Y);
				viewport.WarpMouse(_offscreenPos);
			}
			ActiveScreenContext.Instance.FocusOnDefaultControl();
			EmitSignal(SignalName.ControllerDetected);
			ControlModeChanged();
			viewport?.SetInputAsHandled();
		}
	}

	private void ControlModeChanged()
	{
		_notifyTween?.Kill();
		_notifyTween = CreateTween();
		_notifyTween.TweenProperty(_label, "modulate", Colors.White, 0.25);
		_notifyTween.TweenInterval(0.5);
		_notifyTween.TweenProperty(_label, "modulate", Colors.Transparent, 0.75);
		if (IsUsingController)
		{
			_label.SetTextAutoSize(new LocString("main_menu_ui", "CONTROLLER_DETECTED").GetFormattedText());
		}
		else
		{
			_label.SetTextAutoSize(new LocString("main_menu_ui", "MOUSE_DETECTED").GetFormattedText());
		}
	}

	private void OnScreenContextChanged()
	{
		if (IsUsingController)
		{
			Callable.From(delegate
			{
				ActiveScreenContext.Instance.FocusOnDefaultControl();
			}).CallDeferred();
		}
		else
		{
			Vector2I vector2I = DisplayServer.MouseGetPosition();
			Vector2I vector2I2 = DisplayServer.WindowGetPosition();
			Input.WarpMouse(new Vector2(vector2I.X - vector2I2.X, vector2I.Y - vector2I2.Y));
		}
	}

	public Texture2D? GetHotkeyIcon(string hotkey)
	{
		return _inputStrategy?.GetHotkeyIcon(hotkey);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(9);
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnControllerTypeChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CheckForMouseInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.CheckForControllerInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ControlModeChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnScreenContextChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GetHotkeyIcon, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Texture2D"), exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "hotkey", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Input && args.Count == 1)
		{
			_Input(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnControllerTypeChanged && args.Count == 0)
		{
			OnControllerTypeChanged();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CheckForMouseInput && args.Count == 1)
		{
			CheckForMouseInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CheckForControllerInput && args.Count == 1)
		{
			CheckForControllerInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ControlModeChanged && args.Count == 0)
		{
			ControlModeChanged();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnScreenContextChanged && args.Count == 0)
		{
			OnScreenContextChanged();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetHotkeyIcon && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<Texture2D>(GetHotkeyIcon(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName._Input)
		{
			return true;
		}
		if (method == MethodName.OnControllerTypeChanged)
		{
			return true;
		}
		if (method == MethodName.CheckForMouseInput)
		{
			return true;
		}
		if (method == MethodName.CheckForControllerInput)
		{
			return true;
		}
		if (method == MethodName.ControlModeChanged)
		{
			return true;
		}
		if (method == MethodName.OnScreenContextChanged)
		{
			return true;
		}
		if (method == MethodName.GetHotkeyIcon)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.IsUsingController)
		{
			IsUsingController = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._lastMousePosition)
		{
			_lastMousePosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._label)
		{
			_label = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._notifyTween)
		{
			_notifyTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		bool from;
		if (name == PropertyName.ShouldAllowControllerRebinding)
		{
			from = ShouldAllowControllerRebinding;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.IsUsingController)
		{
			from = IsUsingController;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.ControllerMappingType)
		{
			value = VariantUtils.CreateFrom<ControllerMappingType>(ControllerMappingType);
			return true;
		}
		if (name == PropertyName._lastMousePosition)
		{
			value = VariantUtils.CreateFrom(in _lastMousePosition);
			return true;
		}
		if (name == PropertyName._label)
		{
			value = VariantUtils.CreateFrom(in _label);
			return true;
		}
		if (name == PropertyName._notifyTween)
		{
			value = VariantUtils.CreateFrom(in _notifyTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.ShouldAllowControllerRebinding, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._lastMousePosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._label, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._notifyTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsUsingController, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.ControllerMappingType, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.IsUsingController, Variant.From<bool>(IsUsingController));
		info.AddProperty(PropertyName._lastMousePosition, Variant.From(in _lastMousePosition));
		info.AddProperty(PropertyName._label, Variant.From(in _label));
		info.AddProperty(PropertyName._notifyTween, Variant.From(in _notifyTween));
		info.AddSignalEventDelegate(SignalName.ControllerDetected, backing_ControllerDetected);
		info.AddSignalEventDelegate(SignalName.MouseDetected, backing_MouseDetected);
		info.AddSignalEventDelegate(SignalName.ControllerTypeChanged, backing_ControllerTypeChanged);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.IsUsingController, out var value))
		{
			IsUsingController = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._lastMousePosition, out var value2))
		{
			_lastMousePosition = value2.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._label, out var value3))
		{
			_label = value3.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._notifyTween, out var value4))
		{
			_notifyTween = value4.As<Tween>();
		}
		if (info.TryGetSignalEventDelegate<ControllerDetectedEventHandler>(SignalName.ControllerDetected, out var value5))
		{
			backing_ControllerDetected = value5;
		}
		if (info.TryGetSignalEventDelegate<MouseDetectedEventHandler>(SignalName.MouseDetected, out var value6))
		{
			backing_MouseDetected = value6;
		}
		if (info.TryGetSignalEventDelegate<ControllerTypeChangedEventHandler>(SignalName.ControllerTypeChanged, out var value7))
		{
			backing_ControllerTypeChanged = value7;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotSignalList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(SignalName.ControllerDetected, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(SignalName.MouseDetected, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(SignalName.ControllerTypeChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	protected void EmitSignalControllerDetected()
	{
		EmitSignal(SignalName.ControllerDetected);
	}

	protected void EmitSignalMouseDetected()
	{
		EmitSignal(SignalName.MouseDetected);
	}

	protected void EmitSignalControllerTypeChanged()
	{
		EmitSignal(SignalName.ControllerTypeChanged);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.ControllerDetected && args.Count == 0)
		{
			backing_ControllerDetected?.Invoke();
		}
		else if (signal == SignalName.MouseDetected && args.Count == 0)
		{
			backing_MouseDetected?.Invoke();
		}
		else if (signal == SignalName.ControllerTypeChanged && args.Count == 0)
		{
			backing_ControllerTypeChanged?.Invoke();
		}
		else
		{
			base.RaiseGodotClassSignalCallbacks(in signal, args);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassSignal(in godot_string_name signal)
	{
		if (signal == SignalName.ControllerDetected)
		{
			return true;
		}
		if (signal == SignalName.MouseDetected)
		{
			return true;
		}
		if (signal == SignalName.ControllerTypeChanged)
		{
			return true;
		}
		return base.HasGodotClassSignal(in signal);
	}
}
