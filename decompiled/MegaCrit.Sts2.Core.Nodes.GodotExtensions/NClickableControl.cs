using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.GodotExtensions;

/// <summary>
/// The "clickable"
/// </summary>
[ScriptPath("res://src/Core/Nodes/GodotExtensions/NClickableControl.cs")]
public class NClickableControl : Control
{
	[Signal]
	public delegate void ReleasedEventHandler(NClickableControl button);

	[Signal]
	public delegate void FocusedEventHandler(NClickableControl button);

	[Signal]
	public delegate void UnfocusedEventHandler(NClickableControl button);

	[Signal]
	public delegate void MouseReleasedEventHandler(InputEvent inputEvent);

	[Signal]
	public delegate void MousePressedEventHandler(InputEvent inputEvent);

	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Control.MethodName
	{
		/// <summary>
		/// Cached name for the 'ConnectSignals' method.
		/// </summary>
		public static readonly StringName ConnectSignals = "ConnectSignals";

		/// <summary>
		/// Cached name for the 'OnVisibilityChanged' method.
		/// </summary>
		public static readonly StringName OnVisibilityChanged = "OnVisibilityChanged";

		/// <summary>
		/// Cached name for the 'OnFocusHandler' method.
		/// </summary>
		public static readonly StringName OnFocusHandler = "OnFocusHandler";

		/// <summary>
		/// Cached name for the 'OnUnFocusHandler' method.
		/// </summary>
		public static readonly StringName OnUnFocusHandler = "OnUnFocusHandler";

		/// <summary>
		/// Cached name for the 'HandleMousePress' method.
		/// </summary>
		public static readonly StringName HandleMousePress = "HandleMousePress";

		/// <summary>
		/// Cached name for the 'HandleMouseRelease' method.
		/// </summary>
		public static readonly StringName HandleMouseRelease = "HandleMouseRelease";

		/// <summary>
		/// Cached name for the 'OnHoverHandler' method.
		/// </summary>
		public static readonly StringName OnHoverHandler = "OnHoverHandler";

		/// <summary>
		/// Cached name for the 'OnUnhoverHandler' method.
		/// </summary>
		public static readonly StringName OnUnhoverHandler = "OnUnhoverHandler";

		/// <summary>
		/// Cached name for the 'OnPressHandler' method.
		/// </summary>
		public static readonly StringName OnPressHandler = "OnPressHandler";

		/// <summary>
		/// Cached name for the 'OnReleaseHandler' method.
		/// </summary>
		public static readonly StringName OnReleaseHandler = "OnReleaseHandler";

		/// <summary>
		/// Cached name for the 'RefreshFocus' method.
		/// </summary>
		public static readonly StringName RefreshFocus = "RefreshFocus";

		/// <summary>
		/// Cached name for the 'OnFocus' method.
		/// </summary>
		public static readonly StringName OnFocus = "OnFocus";

		/// <summary>
		/// Cached name for the 'OnUnfocus' method.
		/// </summary>
		public static readonly StringName OnUnfocus = "OnUnfocus";

		/// <summary>
		/// Cached name for the 'OnPress' method.
		/// </summary>
		public static readonly StringName OnPress = "OnPress";

		/// <summary>
		/// Cached name for the 'OnRelease' method.
		/// </summary>
		public static readonly StringName OnRelease = "OnRelease";

		/// <summary>
		/// Cached name for the '_GuiInput' method.
		/// </summary>
		public new static readonly StringName _GuiInput = "_GuiInput";

		/// <summary>
		/// Cached name for the 'CheckMouseDragThreshold' method.
		/// </summary>
		public static readonly StringName CheckMouseDragThreshold = "CheckMouseDragThreshold";

		/// <summary>
		/// Cached name for the 'DebugPress' method.
		/// </summary>
		public static readonly StringName DebugPress = "DebugPress";

		/// <summary>
		/// Cached name for the 'DebugRelease' method.
		/// </summary>
		public static readonly StringName DebugRelease = "DebugRelease";

		/// <summary>
		/// Cached name for the 'ForceClick' method.
		/// </summary>
		public static readonly StringName ForceClick = "ForceClick";

		/// <summary>
		/// Cached name for the 'SetEnabled' method.
		/// </summary>
		public static readonly StringName SetEnabled = "SetEnabled";

		/// <summary>
		/// Cached name for the 'Enable' method.
		/// </summary>
		public static readonly StringName Enable = "Enable";

		/// <summary>
		/// Cached name for the 'Disable' method.
		/// </summary>
		public static readonly StringName Disable = "Disable";

		/// <summary>
		/// Cached name for the 'OnEnable' method.
		/// </summary>
		public static readonly StringName OnEnable = "OnEnable";

		/// <summary>
		/// Cached name for the 'OnDisable' method.
		/// </summary>
		public static readonly StringName OnDisable = "OnDisable";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
		/// <summary>
		/// Cached name for the 'AllowFocusWhileDisabled' property.
		/// </summary>
		public static readonly StringName AllowFocusWhileDisabled = "AllowFocusWhileDisabled";

		/// <summary>
		/// Cached name for the 'IsFocused' property.
		/// </summary>
		public static readonly StringName IsFocused = "IsFocused";

		/// <summary>
		/// Cached name for the 'IsEnabled' property.
		/// </summary>
		public static readonly StringName IsEnabled = "IsEnabled";

		/// <summary>
		/// Cached name for the '_ignoreDragThreshold' field.
		/// </summary>
		public static readonly StringName _ignoreDragThreshold = "_ignoreDragThreshold";

		/// <summary>
		/// Cached name for the '_isEnabled' field.
		/// </summary>
		public static readonly StringName _isEnabled = "_isEnabled";

		/// <summary>
		/// Cached name for the '_isHovered' field.
		/// </summary>
		public static readonly StringName _isHovered = "_isHovered";

		/// <summary>
		/// Cached name for the '_isControllerFocused' field.
		/// </summary>
		public static readonly StringName _isControllerFocused = "_isControllerFocused";

		/// <summary>
		/// Cached name for the '_isControllerNavigable' field.
		/// </summary>
		public static readonly StringName _isControllerNavigable = "_isControllerNavigable";

		/// <summary>
		/// Cached name for the '_beginDragPosition' field.
		/// </summary>
		public static readonly StringName _beginDragPosition = "_beginDragPosition";

		/// <summary>
		/// Cached name for the '_isPressed' field.
		/// </summary>
		public static readonly StringName _isPressed = "_isPressed";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
		/// <summary>
		/// Cached name for the 'Released' signal.
		/// </summary>
		public static readonly StringName Released = "Released";

		/// <summary>
		/// Cached name for the 'Focused' signal.
		/// </summary>
		public static readonly StringName Focused = "Focused";

		/// <summary>
		/// Cached name for the 'Unfocused' signal.
		/// </summary>
		public static readonly StringName Unfocused = "Unfocused";

		/// <summary>
		/// Cached name for the 'MouseReleased' signal.
		/// </summary>
		public static readonly StringName MouseReleased = "MouseReleased";

		/// <summary>
		/// Cached name for the 'MousePressed' signal.
		/// </summary>
		public static readonly StringName MousePressed = "MousePressed";
	}

	[Export(PropertyHint.None, "")]
	protected float _ignoreDragThreshold = -1f;

	protected bool _isEnabled = true;

	private bool _isHovered;

	private bool _isControllerFocused;

	private bool _isControllerNavigable;

	private Vector2 _beginDragPosition;

	private bool _isPressed;

	private static readonly StyleBoxEmpty _blankFocusStyle = new StyleBoxEmpty();

	private ReleasedEventHandler backing_Released;

	private FocusedEventHandler backing_Focused;

	private UnfocusedEventHandler backing_Unfocused;

	private MouseReleasedEventHandler backing_MouseReleased;

	private MousePressedEventHandler backing_MousePressed;

	protected virtual bool AllowFocusWhileDisabled => false;

	protected bool IsFocused { get; private set; }

	public bool IsEnabled => _isEnabled;

	/// <inheritdoc cref="T:MegaCrit.Sts2.Core.Nodes.GodotExtensions.NClickableControl.ReleasedEventHandler" />
	public event ReleasedEventHandler Released
	{
		add
		{
			backing_Released = (ReleasedEventHandler)Delegate.Combine(backing_Released, value);
		}
		remove
		{
			backing_Released = (ReleasedEventHandler)Delegate.Remove(backing_Released, value);
		}
	}

	/// <inheritdoc cref="T:MegaCrit.Sts2.Core.Nodes.GodotExtensions.NClickableControl.FocusedEventHandler" />
	public event FocusedEventHandler Focused
	{
		add
		{
			backing_Focused = (FocusedEventHandler)Delegate.Combine(backing_Focused, value);
		}
		remove
		{
			backing_Focused = (FocusedEventHandler)Delegate.Remove(backing_Focused, value);
		}
	}

	/// <inheritdoc cref="T:MegaCrit.Sts2.Core.Nodes.GodotExtensions.NClickableControl.UnfocusedEventHandler" />
	public event UnfocusedEventHandler Unfocused
	{
		add
		{
			backing_Unfocused = (UnfocusedEventHandler)Delegate.Combine(backing_Unfocused, value);
		}
		remove
		{
			backing_Unfocused = (UnfocusedEventHandler)Delegate.Remove(backing_Unfocused, value);
		}
	}

	/// <inheritdoc cref="T:MegaCrit.Sts2.Core.Nodes.GodotExtensions.NClickableControl.MouseReleasedEventHandler" />
	public event MouseReleasedEventHandler MouseReleased
	{
		add
		{
			backing_MouseReleased = (MouseReleasedEventHandler)Delegate.Combine(backing_MouseReleased, value);
		}
		remove
		{
			backing_MouseReleased = (MouseReleasedEventHandler)Delegate.Remove(backing_MouseReleased, value);
		}
	}

	/// <inheritdoc cref="T:MegaCrit.Sts2.Core.Nodes.GodotExtensions.NClickableControl.MousePressedEventHandler" />
	public event MousePressedEventHandler MousePressed
	{
		add
		{
			backing_MousePressed = (MousePressedEventHandler)Delegate.Combine(backing_MousePressed, value);
		}
		remove
		{
			backing_MousePressed = (MousePressedEventHandler)Delegate.Remove(backing_MousePressed, value);
		}
	}

	protected virtual void ConnectSignals()
	{
		Connect(Control.SignalName.FocusEntered, Callable.From(OnFocusHandler));
		Connect(Control.SignalName.FocusExited, Callable.From(OnUnFocusHandler));
		Connect(Control.SignalName.MouseEntered, Callable.From(OnHoverHandler));
		Connect(Control.SignalName.MouseExited, Callable.From(OnUnhoverHandler));
		Connect(SignalName.MousePressed, Callable.From<InputEvent>(HandleMousePress));
		Connect(SignalName.MouseReleased, Callable.From<InputEvent>(HandleMouseRelease));
		Connect(CanvasItem.SignalName.VisibilityChanged, Callable.From(OnVisibilityChanged));
		AddThemeStyleboxOverride(ThemeConstants.Control.Focus, _blankFocusStyle);
		_isControllerNavigable = base.FocusMode == FocusModeEnum.All;
		if (HasFocus())
		{
			OnFocusHandler();
		}
	}

	private void OnVisibilityChanged()
	{
		if (!IsVisibleInTree())
		{
			OnUnFocusHandler();
		}
	}

	private void OnFocusHandler()
	{
		_isControllerFocused = true;
		RefreshFocus();
	}

	private void OnUnFocusHandler()
	{
		_isControllerFocused = false;
		RefreshFocus();
	}

	/// <summary>
	/// Called when we start pressing down on the mouse button
	/// </summary>
	/// <param name="inputEvent"></param>
	private void HandleMousePress(InputEvent inputEvent)
	{
		if (_isEnabled && IsVisibleInTree() && IsFocused && inputEvent is InputEventMouseButton inputEventMouseButton && inputEventMouseButton.ButtonIndex == MouseButton.Left)
		{
			_isControllerFocused = false;
			_beginDragPosition = inputEventMouseButton.GlobalPosition;
			OnPressHandler();
		}
	}

	/// <summary>
	/// Called when the mouse button is released on this control (had to start the click in this Control too)
	/// </summary>
	/// <param name="inputEvent"></param>
	private void HandleMouseRelease(InputEvent inputEvent)
	{
		if (_isEnabled && IsVisibleInTree() && IsFocused && inputEvent is InputEventMouseButton inputEventMouseButton && inputEventMouseButton.ButtonIndex == MouseButton.Left)
		{
			OnReleaseHandler();
		}
	}

	private void OnHoverHandler()
	{
		_isHovered = true;
		if (!GetTree().Paused || NGame.IsReleaseGame())
		{
			RefreshFocus();
		}
	}

	private void OnUnhoverHandler()
	{
		_isHovered = false;
		if (!GetTree().Paused || NGame.IsReleaseGame())
		{
			RefreshFocus();
		}
	}

	protected void OnPressHandler()
	{
		_isPressed = true;
		OnPress();
	}

	/// <summary>
	/// Middle-man function to prevent base.OnRelease() and to
	/// allow handling of various input types simultaneously.
	/// Required because we invoke OnButtonReleased
	/// </summary>
	protected void OnReleaseHandler()
	{
		if (_isPressed)
		{
			_isPressed = false;
			OnRelease();
			EmitSignal(SignalName.Released, this);
		}
	}

	private void RefreshFocus()
	{
		bool flag = (_isEnabled || AllowFocusWhileDisabled) && IsVisibleInTree() && (_isHovered || _isControllerFocused);
		if (IsFocused != flag)
		{
			IsFocused = flag;
			if (IsFocused)
			{
				EmitSignal(SignalName.Focused, this);
				OnFocus();
			}
			else
			{
				EmitSignal(SignalName.Unfocused, this);
				OnUnfocus();
			}
		}
	}

	/// <summary>
	/// Called when a user hovers this button or navigates to/focuses on this button (controller).
	/// </summary>
	protected virtual void OnFocus()
	{
	}

	/// <summary>
	/// Called when a user unhovers or navigates away from this button (controller).
	/// </summary>
	protected virtual void OnUnfocus()
	{
	}

	/// <summary>
	/// Called when a user starts a click or button press.
	/// </summary>
	protected virtual void OnPress()
	{
	}

	/// <summary>
	/// Called when the player releases a click or button press.
	/// NOTE: Doesn't trigger unless the user triggered OnPressDown on this button.
	/// </summary>
	protected virtual void OnRelease()
	{
	}

	/// <summary>
	/// WARNING: If overriding, be sure to call the base function to retain
	/// OnPressDown and OnRelease functionality.
	/// </summary>
	/// <param name="inputEvent"></param>
	public override void _GuiInput(InputEvent inputEvent)
	{
		if (inputEvent is InputEventMouseButton inputEventMouseButton && _isEnabled)
		{
			MouseButton buttonIndex = inputEventMouseButton.ButtonIndex;
			if (((ulong)(buttonIndex - 1) <= 1uL) ? true : false)
			{
				EmitSignal(inputEventMouseButton.IsPressed() ? SignalName.MousePressed : SignalName.MouseReleased, inputEvent);
			}
		}
		if (inputEvent.IsActionPressed(MegaInput.select))
		{
			OnPressHandler();
		}
		else if (inputEvent.IsActionReleased(MegaInput.select))
		{
			OnReleaseHandler();
		}
	}

	protected void CheckMouseDragThreshold(InputEvent inputEvent)
	{
		if (!(_ignoreDragThreshold <= 0f) && _isPressed && inputEvent is InputEventMouseMotion { GlobalPosition: var globalPosition } && globalPosition.DistanceTo(_beginDragPosition) >= _ignoreDragThreshold)
		{
			_isPressed = false;
		}
	}

	public void DebugPress()
	{
		EmitSignal(SignalName.MousePressed, new InputEventMouseButton
		{
			ButtonIndex = MouseButton.Left,
			Pressed = true
		});
	}

	public void DebugRelease()
	{
		EmitSignal(SignalName.MouseReleased, new InputEventMouseButton
		{
			ButtonIndex = MouseButton.Left,
			Pressed = false
		});
	}

	/// <summary>
	/// Force emits the Released signal and calls OnRelease(), bypassing all hover/focus/pause checks.
	/// Used by AutoSlay for automated testing.
	/// </summary>
	public void ForceClick()
	{
		OnRelease();
		EmitSignal(SignalName.Released, this);
	}

	/// <summary>
	/// Helper function to enable/disable this button similar to how Unity handles it
	/// </summary>
	public void SetEnabled(bool enabled)
	{
		if (enabled)
		{
			Enable();
		}
		else
		{
			Disable();
		}
	}

	/// <summary>
	/// Helper function to enable this button similar to how Unity handles it
	/// </summary>
	public void Enable()
	{
		if (!_isEnabled)
		{
			_isEnabled = true;
			base.FocusMode = (FocusModeEnum)(_isControllerNavigable ? 2 : 0);
			OnEnable();
			RefreshFocus();
			Callable.From(delegate
			{
				SetProcessInput(enable: true);
			}).CallDeferred();
		}
	}

	/// <summary>
	/// Helper function to disable this button similar to how Unity handles it
	/// </summary>
	public void Disable()
	{
		if (_isEnabled)
		{
			_isEnabled = false;
			_isPressed = false;
			base.FocusMode = FocusModeEnum.None;
			OnDisable();
			RefreshFocus();
			SetProcessInput(enable: false);
		}
	}

	/// <summary>
	/// Called whenever this button becomes enabled
	/// </summary>
	protected virtual void OnEnable()
	{
	}

	/// <summary>
	/// Called whenever this button becomes disabled
	/// </summary>
	protected virtual void OnDisable()
	{
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(25);
		list.Add(new MethodInfo(MethodName.ConnectSignals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnVisibilityChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocusHandler, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnFocusHandler, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.HandleMousePress, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.HandleMouseRelease, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnHoverHandler, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnhoverHandler, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPressHandler, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnReleaseHandler, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPress, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnRelease, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._GuiInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.CheckMouseDragThreshold, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.DebugPress, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DebugRelease, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ForceClick, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetEnabled, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "enabled", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.Enable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Disable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnEnable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnDisable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.ConnectSignals && args.Count == 0)
		{
			ConnectSignals();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnVisibilityChanged && args.Count == 0)
		{
			OnVisibilityChanged();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnFocusHandler && args.Count == 0)
		{
			OnFocusHandler();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnUnFocusHandler && args.Count == 0)
		{
			OnUnFocusHandler();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HandleMousePress && args.Count == 1)
		{
			HandleMousePress(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HandleMouseRelease && args.Count == 1)
		{
			HandleMouseRelease(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnHoverHandler && args.Count == 0)
		{
			OnHoverHandler();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnUnhoverHandler && args.Count == 0)
		{
			OnUnhoverHandler();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPressHandler && args.Count == 0)
		{
			OnPressHandler();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnReleaseHandler && args.Count == 0)
		{
			OnReleaseHandler();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshFocus && args.Count == 0)
		{
			RefreshFocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnFocus && args.Count == 0)
		{
			OnFocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnUnfocus && args.Count == 0)
		{
			OnUnfocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPress && args.Count == 0)
		{
			OnPress();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnRelease && args.Count == 0)
		{
			OnRelease();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._GuiInput && args.Count == 1)
		{
			_GuiInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CheckMouseDragThreshold && args.Count == 1)
		{
			CheckMouseDragThreshold(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DebugPress && args.Count == 0)
		{
			DebugPress();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DebugRelease && args.Count == 0)
		{
			DebugRelease();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ForceClick && args.Count == 0)
		{
			ForceClick();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetEnabled && args.Count == 1)
		{
			SetEnabled(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Enable && args.Count == 0)
		{
			Enable();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Disable && args.Count == 0)
		{
			Disable();
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
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.ConnectSignals)
		{
			return true;
		}
		if (method == MethodName.OnVisibilityChanged)
		{
			return true;
		}
		if (method == MethodName.OnFocusHandler)
		{
			return true;
		}
		if (method == MethodName.OnUnFocusHandler)
		{
			return true;
		}
		if (method == MethodName.HandleMousePress)
		{
			return true;
		}
		if (method == MethodName.HandleMouseRelease)
		{
			return true;
		}
		if (method == MethodName.OnHoverHandler)
		{
			return true;
		}
		if (method == MethodName.OnUnhoverHandler)
		{
			return true;
		}
		if (method == MethodName.OnPressHandler)
		{
			return true;
		}
		if (method == MethodName.OnReleaseHandler)
		{
			return true;
		}
		if (method == MethodName.RefreshFocus)
		{
			return true;
		}
		if (method == MethodName.OnFocus)
		{
			return true;
		}
		if (method == MethodName.OnUnfocus)
		{
			return true;
		}
		if (method == MethodName.OnPress)
		{
			return true;
		}
		if (method == MethodName.OnRelease)
		{
			return true;
		}
		if (method == MethodName._GuiInput)
		{
			return true;
		}
		if (method == MethodName.CheckMouseDragThreshold)
		{
			return true;
		}
		if (method == MethodName.DebugPress)
		{
			return true;
		}
		if (method == MethodName.DebugRelease)
		{
			return true;
		}
		if (method == MethodName.ForceClick)
		{
			return true;
		}
		if (method == MethodName.SetEnabled)
		{
			return true;
		}
		if (method == MethodName.Enable)
		{
			return true;
		}
		if (method == MethodName.Disable)
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
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.IsFocused)
		{
			IsFocused = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._ignoreDragThreshold)
		{
			_ignoreDragThreshold = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._isEnabled)
		{
			_isEnabled = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._isHovered)
		{
			_isHovered = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._isControllerFocused)
		{
			_isControllerFocused = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._isControllerNavigable)
		{
			_isControllerNavigable = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._beginDragPosition)
		{
			_beginDragPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._isPressed)
		{
			_isPressed = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		bool from;
		if (name == PropertyName.AllowFocusWhileDisabled)
		{
			from = AllowFocusWhileDisabled;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.IsFocused)
		{
			from = IsFocused;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.IsEnabled)
		{
			from = IsEnabled;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName._ignoreDragThreshold)
		{
			value = VariantUtils.CreateFrom(in _ignoreDragThreshold);
			return true;
		}
		if (name == PropertyName._isEnabled)
		{
			value = VariantUtils.CreateFrom(in _isEnabled);
			return true;
		}
		if (name == PropertyName._isHovered)
		{
			value = VariantUtils.CreateFrom(in _isHovered);
			return true;
		}
		if (name == PropertyName._isControllerFocused)
		{
			value = VariantUtils.CreateFrom(in _isControllerFocused);
			return true;
		}
		if (name == PropertyName._isControllerNavigable)
		{
			value = VariantUtils.CreateFrom(in _isControllerNavigable);
			return true;
		}
		if (name == PropertyName._beginDragPosition)
		{
			value = VariantUtils.CreateFrom(in _beginDragPosition);
			return true;
		}
		if (name == PropertyName._isPressed)
		{
			value = VariantUtils.CreateFrom(in _isPressed);
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
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._ignoreDragThreshold, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.AllowFocusWhileDisabled, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsFocused, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isEnabled, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsEnabled, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isHovered, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isControllerFocused, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isControllerNavigable, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._beginDragPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isPressed, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.IsFocused, Variant.From<bool>(IsFocused));
		info.AddProperty(PropertyName._ignoreDragThreshold, Variant.From(in _ignoreDragThreshold));
		info.AddProperty(PropertyName._isEnabled, Variant.From(in _isEnabled));
		info.AddProperty(PropertyName._isHovered, Variant.From(in _isHovered));
		info.AddProperty(PropertyName._isControllerFocused, Variant.From(in _isControllerFocused));
		info.AddProperty(PropertyName._isControllerNavigable, Variant.From(in _isControllerNavigable));
		info.AddProperty(PropertyName._beginDragPosition, Variant.From(in _beginDragPosition));
		info.AddProperty(PropertyName._isPressed, Variant.From(in _isPressed));
		info.AddSignalEventDelegate(SignalName.Released, backing_Released);
		info.AddSignalEventDelegate(SignalName.Focused, backing_Focused);
		info.AddSignalEventDelegate(SignalName.Unfocused, backing_Unfocused);
		info.AddSignalEventDelegate(SignalName.MouseReleased, backing_MouseReleased);
		info.AddSignalEventDelegate(SignalName.MousePressed, backing_MousePressed);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.IsFocused, out var value))
		{
			IsFocused = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._ignoreDragThreshold, out var value2))
		{
			_ignoreDragThreshold = value2.As<float>();
		}
		if (info.TryGetProperty(PropertyName._isEnabled, out var value3))
		{
			_isEnabled = value3.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._isHovered, out var value4))
		{
			_isHovered = value4.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._isControllerFocused, out var value5))
		{
			_isControllerFocused = value5.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._isControllerNavigable, out var value6))
		{
			_isControllerNavigable = value6.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._beginDragPosition, out var value7))
		{
			_beginDragPosition = value7.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._isPressed, out var value8))
		{
			_isPressed = value8.As<bool>();
		}
		if (info.TryGetSignalEventDelegate<ReleasedEventHandler>(SignalName.Released, out var value9))
		{
			backing_Released = value9;
		}
		if (info.TryGetSignalEventDelegate<FocusedEventHandler>(SignalName.Focused, out var value10))
		{
			backing_Focused = value10;
		}
		if (info.TryGetSignalEventDelegate<UnfocusedEventHandler>(SignalName.Unfocused, out var value11))
		{
			backing_Unfocused = value11;
		}
		if (info.TryGetSignalEventDelegate<MouseReleasedEventHandler>(SignalName.MouseReleased, out var value12))
		{
			backing_MouseReleased = value12;
		}
		if (info.TryGetSignalEventDelegate<MousePressedEventHandler>(SignalName.MousePressed, out var value13))
		{
			backing_MousePressed = value13;
		}
	}

	/// <summary>
	/// Get the signal information for all the signals declared in this class.
	/// This method is used by Godot to register the available signals in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotSignalList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(SignalName.Released, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "button", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(SignalName.Focused, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "button", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(SignalName.Unfocused, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "button", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(SignalName.MouseReleased, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(SignalName.MousePressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		return list;
	}

	protected void EmitSignalReleased(NClickableControl button)
	{
		EmitSignal(SignalName.Released, button);
	}

	protected void EmitSignalFocused(NClickableControl button)
	{
		EmitSignal(SignalName.Focused, button);
	}

	protected void EmitSignalUnfocused(NClickableControl button)
	{
		EmitSignal(SignalName.Unfocused, button);
	}

	protected void EmitSignalMouseReleased(InputEvent inputEvent)
	{
		EmitSignal(SignalName.MouseReleased, inputEvent);
	}

	protected void EmitSignalMousePressed(InputEvent inputEvent)
	{
		EmitSignal(SignalName.MousePressed, inputEvent);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.Released && args.Count == 1)
		{
			backing_Released?.Invoke(VariantUtils.ConvertTo<NClickableControl>(in args[0]));
		}
		else if (signal == SignalName.Focused && args.Count == 1)
		{
			backing_Focused?.Invoke(VariantUtils.ConvertTo<NClickableControl>(in args[0]));
		}
		else if (signal == SignalName.Unfocused && args.Count == 1)
		{
			backing_Unfocused?.Invoke(VariantUtils.ConvertTo<NClickableControl>(in args[0]));
		}
		else if (signal == SignalName.MouseReleased && args.Count == 1)
		{
			backing_MouseReleased?.Invoke(VariantUtils.ConvertTo<InputEvent>(in args[0]));
		}
		else if (signal == SignalName.MousePressed && args.Count == 1)
		{
			backing_MousePressed?.Invoke(VariantUtils.ConvertTo<InputEvent>(in args[0]));
		}
		else
		{
			base.RaiseGodotClassSignalCallbacks(in signal, args);
		}
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassSignal(in godot_string_name signal)
	{
		if (signal == SignalName.Released)
		{
			return true;
		}
		if (signal == SignalName.Focused)
		{
			return true;
		}
		if (signal == SignalName.Unfocused)
		{
			return true;
		}
		if (signal == SignalName.MouseReleased)
		{
			return true;
		}
		if (signal == SignalName.MousePressed)
		{
			return true;
		}
		return base.HasGodotClassSignal(in signal);
	}
}
