using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;

namespace MegaCrit.Sts2.Core.Nodes.GodotExtensions;

[GlobalClass]
[ScriptPath("res://src/Core/Nodes/GodotExtensions/NScrollbar.cs")]
public class NScrollbar : Godot.Range
{
	[Signal]
	public delegate void MouseReleasedEventHandler(InputEvent inputEvent);

	[Signal]
	public delegate void MousePressedEventHandler(InputEvent inputEvent);

	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Godot.Range.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the '_GuiInput' method.
		/// </summary>
		public new static readonly StringName _GuiInput = "_GuiInput";

		/// <summary>
		/// Cached name for the 'SetValueBasedOnMousePosition' method.
		/// </summary>
		public static readonly StringName SetValueBasedOnMousePosition = "SetValueBasedOnMousePosition";

		/// <summary>
		/// Cached name for the 'SetValueWithoutAnimation' method.
		/// </summary>
		public static readonly StringName SetValueWithoutAnimation = "SetValueWithoutAnimation";

		/// <summary>
		/// Cached name for the '_Process' method.
		/// </summary>
		public new static readonly StringName _Process = "_Process";

		/// <summary>
		/// Cached name for the 'UpdateHandlePosition' method.
		/// </summary>
		public static readonly StringName UpdateHandlePosition = "UpdateHandlePosition";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Godot.Range.PropertyName
	{
		/// <summary>
		/// Cached name for the '_handle' field.
		/// </summary>
		public static readonly StringName _handle = "_handle";

		/// <summary>
		/// Cached name for the '_currentHandlePosition' field.
		/// </summary>
		public static readonly StringName _currentHandlePosition = "_currentHandlePosition";

		/// <summary>
		/// Cached name for the '_currentVelocity' field.
		/// </summary>
		public static readonly StringName _currentVelocity = "_currentVelocity";

		/// <summary>
		/// Cached name for the '_isDragging' field.
		/// </summary>
		public static readonly StringName _isDragging = "_isDragging";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Godot.Range.SignalName
	{
		/// <summary>
		/// Cached name for the 'MouseReleased' signal.
		/// </summary>
		public static readonly StringName MouseReleased = "MouseReleased";

		/// <summary>
		/// Cached name for the 'MousePressed' signal.
		/// </summary>
		public static readonly StringName MousePressed = "MousePressed";
	}

	private Control _handle;

	private float _currentHandlePosition;

	private float _currentVelocity;

	private bool _isDragging;

	private MouseReleasedEventHandler backing_MouseReleased;

	private MousePressedEventHandler backing_MousePressed;

	/// <inheritdoc cref="T:MegaCrit.Sts2.Core.Nodes.GodotExtensions.NScrollbar.MouseReleasedEventHandler" />
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

	/// <inheritdoc cref="T:MegaCrit.Sts2.Core.Nodes.GodotExtensions.NScrollbar.MousePressedEventHandler" />
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

	public override void _Ready()
	{
		_handle = GetNode<Control>("%Handle");
	}

	/// <summary>
	/// WARNING: If overriding, be sure to call the base function to retain
	/// OnPressDown and OnRelease functionality.
	/// </summary>
	/// <param name="inputEvent"></param>
	public override void _GuiInput(InputEvent inputEvent)
	{
		base._GuiInput(inputEvent);
		if (!(inputEvent is InputEventMouseButton { ButtonIndex: var buttonIndex } inputEventMouseButton))
		{
			if (inputEvent is InputEventMouseMotion inputEventMouseMotion && _isDragging)
			{
				SetValueBasedOnMousePosition(inputEventMouseMotion.Position);
			}
		}
		else if (((ulong)(buttonIndex - 1) <= 1uL) ? true : false)
		{
			_isDragging = inputEventMouseButton.IsPressed();
			SetValueBasedOnMousePosition(inputEventMouseButton.Position);
			EmitSignal(inputEventMouseButton.IsPressed() ? SignalName.MousePressed : SignalName.MouseReleased, inputEvent);
		}
	}

	private void SetValueBasedOnMousePosition(Vector2 mousePosition)
	{
		base.Value = (double)(mousePosition.Y / base.Size.Y) * base.MaxValue;
	}

	public void SetValueWithoutAnimation(double value)
	{
		_currentHandlePosition = (float)value;
		base.Value = value;
		UpdateHandlePosition();
	}

	public override void _Process(double delta)
	{
		_currentHandlePosition = (float)base.Value;
		UpdateHandlePosition();
	}

	private void UpdateHandlePosition()
	{
		float y = MathHelper.SmoothDamp(_handle.Position.Y, base.Size.Y * (float)((double)_currentHandlePosition / base.MaxValue) - _handle.Size.Y * 0.5f, ref _currentVelocity, 0.05f, (float)GetProcessDeltaTime());
		_handle.Position = new Vector2((base.Size.X - _handle.Size.X) * 0.5f, y);
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(6);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._GuiInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetValueBasedOnMousePosition, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "mousePosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetValueWithoutAnimation, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "value", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateHandlePosition, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._GuiInput && args.Count == 1)
		{
			_GuiInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetValueBasedOnMousePosition && args.Count == 1)
		{
			SetValueBasedOnMousePosition(VariantUtils.ConvertTo<Vector2>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetValueWithoutAnimation && args.Count == 1)
		{
			SetValueWithoutAnimation(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateHandlePosition && args.Count == 0)
		{
			UpdateHandlePosition();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName._GuiInput)
		{
			return true;
		}
		if (method == MethodName.SetValueBasedOnMousePosition)
		{
			return true;
		}
		if (method == MethodName.SetValueWithoutAnimation)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName.UpdateHandlePosition)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._handle)
		{
			_handle = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._currentHandlePosition)
		{
			_currentHandlePosition = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._currentVelocity)
		{
			_currentVelocity = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._isDragging)
		{
			_isDragging = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._handle)
		{
			value = VariantUtils.CreateFrom(in _handle);
			return true;
		}
		if (name == PropertyName._currentHandlePosition)
		{
			value = VariantUtils.CreateFrom(in _currentHandlePosition);
			return true;
		}
		if (name == PropertyName._currentVelocity)
		{
			value = VariantUtils.CreateFrom(in _currentVelocity);
			return true;
		}
		if (name == PropertyName._isDragging)
		{
			value = VariantUtils.CreateFrom(in _isDragging);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._handle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._currentHandlePosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._currentVelocity, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isDragging, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._handle, Variant.From(in _handle));
		info.AddProperty(PropertyName._currentHandlePosition, Variant.From(in _currentHandlePosition));
		info.AddProperty(PropertyName._currentVelocity, Variant.From(in _currentVelocity));
		info.AddProperty(PropertyName._isDragging, Variant.From(in _isDragging));
		info.AddSignalEventDelegate(SignalName.MouseReleased, backing_MouseReleased);
		info.AddSignalEventDelegate(SignalName.MousePressed, backing_MousePressed);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._handle, out var value))
		{
			_handle = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._currentHandlePosition, out var value2))
		{
			_currentHandlePosition = value2.As<float>();
		}
		if (info.TryGetProperty(PropertyName._currentVelocity, out var value3))
		{
			_currentVelocity = value3.As<float>();
		}
		if (info.TryGetProperty(PropertyName._isDragging, out var value4))
		{
			_isDragging = value4.As<bool>();
		}
		if (info.TryGetSignalEventDelegate<MouseReleasedEventHandler>(SignalName.MouseReleased, out var value5))
		{
			backing_MouseReleased = value5;
		}
		if (info.TryGetSignalEventDelegate<MousePressedEventHandler>(SignalName.MousePressed, out var value6))
		{
			backing_MousePressed = value6;
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
		List<MethodInfo> list = new List<MethodInfo>(2);
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
		if (signal == SignalName.MouseReleased && args.Count == 1)
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
