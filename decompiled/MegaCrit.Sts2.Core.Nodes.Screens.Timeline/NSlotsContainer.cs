using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Timeline;

[ScriptPath("res://src/Core/Nodes/Screens/Timeline/NSlotsContainer.cs")]
public class NSlotsContainer : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public new static readonly StringName _GuiInput = "_GuiInput";

		public static readonly StringName ProcessPanEvent = "ProcessPanEvent";

		public static readonly StringName ProcessGuiFocus = "ProcessGuiFocus";

		public static readonly StringName ProcessScrollEvent = "ProcessScrollEvent";

		public new static readonly StringName _Process = "_Process";

		public static readonly StringName OnToggleVisibility = "OnToggleVisibility";

		public static readonly StringName Reset = "Reset";

		public static readonly StringName SetEnabled = "SetEnabled";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName GetInitX = "GetInitX";

		public static readonly StringName _whatsMoved = "_whatsMoved";

		public static readonly StringName _dragStartPosition = "_dragStartPosition";

		public static readonly StringName _targetPosition = "_targetPosition";

		public static readonly StringName _isDragging = "_isDragging";

		public static readonly StringName _tween = "_tween";

		public static readonly StringName _epochSlots = "_epochSlots";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private Control _whatsMoved;

	private Vector2 _dragStartPosition;

	private Vector2 _targetPosition;

	private bool _isDragging;

	private const float _scrollSpeed = 50f;

	private const float _trackpadScrollSpeed = 20f;

	private const float _bounceBackStrength = 36f;

	private const float _lerpSmoothness = 20f;

	private Tween? _tween;

	private Control _epochSlots;

	public float GetInitX => _whatsMoved.GlobalPosition.X;

	public override void _Ready()
	{
		_whatsMoved = GetNode<Control>("%WhatsMoved");
		_epochSlots = GetNode<Control>("%EpochSlots");
		_targetPosition = _whatsMoved.Position;
		Connect(CanvasItem.SignalName.VisibilityChanged, Callable.From(OnToggleVisibility));
	}

	public override void _EnterTree()
	{
		GetViewport().Connect(Viewport.SignalName.GuiFocusChanged, Callable.From<Control>(ProcessGuiFocus));
	}

	public override void _ExitTree()
	{
		GetViewport().Disconnect(Viewport.SignalName.GuiFocusChanged, Callable.From<Control>(ProcessGuiFocus));
	}

	public override void _GuiInput(InputEvent inputEvent)
	{
		ProcessPanEvent(inputEvent);
		ProcessScrollEvent(inputEvent);
	}

	private void ProcessPanEvent(InputEvent inputEvent)
	{
		if (inputEvent is InputEventMouseButton inputEventMouseButton && inputEventMouseButton.ButtonIndex == MouseButton.Left)
		{
			if (inputEventMouseButton.Pressed)
			{
				_isDragging = true;
				_dragStartPosition = inputEventMouseButton.Position;
			}
			else
			{
				_isDragging = false;
			}
		}
		else if (inputEvent is InputEventMouseMotion inputEventMouseMotion && _isDragging)
		{
			_targetPosition += new Vector2((inputEventMouseMotion.Position - _dragStartPosition).X, 0f);
			_dragStartPosition = inputEventMouseMotion.Position;
		}
	}

	private void ProcessGuiFocus(Control focusedControl)
	{
		if (IsVisibleInTree() && NControllerManager.Instance.IsUsingController && IsAncestorOf(focusedControl))
		{
			float x = _whatsMoved.GlobalPosition.X - focusedControl.GetParent<Control>().GlobalPosition.X;
			_targetPosition = new Vector2(x, _targetPosition.Y);
		}
	}

	private void ProcessScrollEvent(InputEvent inputEvent)
	{
		if (inputEvent is InputEventMouseButton inputEventMouseButton)
		{
			if (inputEventMouseButton.ButtonIndex == MouseButton.WheelUp)
			{
				_targetPosition -= new Vector2(50f, 0f);
			}
			else if (inputEventMouseButton.ButtonIndex == MouseButton.WheelDown)
			{
				_targetPosition += new Vector2(50f, 0f);
			}
			else if (inputEventMouseButton.ButtonIndex == MouseButton.WheelRight)
			{
				_targetPosition -= new Vector2(50f, 0f);
			}
			else if (inputEventMouseButton.ButtonIndex == MouseButton.WheelLeft)
			{
				_targetPosition += new Vector2(50f, 0f);
			}
		}
		else if (inputEvent is InputEventPanGesture inputEventPanGesture)
		{
			_targetPosition += new Vector2((0f - inputEventPanGesture.Delta.X) * 20f, 0f);
		}
	}

	public override void _Process(double delta)
	{
		_whatsMoved.Position = _whatsMoved.Position.Lerp(_targetPosition, (float)delta * 20f);
		if (!_isDragging)
		{
			float num = _targetPosition.X;
			float num2 = _epochSlots.Position.X - _whatsMoved.Size.X;
			float num3 = _epochSlots.Position.X + _epochSlots.Size.X - _whatsMoved.Size.X;
			if (num < num2)
			{
				num = Mathf.Lerp(num, num2, (float)delta * 36f);
			}
			else if (num > num3)
			{
				num = Mathf.Lerp(num, num3, (float)delta * 36f);
			}
			_targetPosition = new Vector2(num, _targetPosition.Y);
		}
	}

	private void OnToggleVisibility()
	{
		if (base.Visible)
		{
			_targetPosition = _whatsMoved.Position;
			_dragStartPosition = Vector2.Zero;
		}
	}

	public void Reset()
	{
		_whatsMoved.Position = new Vector2(-960f, _whatsMoved.Position.Y);
	}

	public async Task LerpToSlot(float slotPositionX)
	{
		float num = _whatsMoved.GlobalPosition.X - slotPositionX + 960f - 96f + (base.Size.X - 1920f) * 0.5f;
		_tween?.Kill();
		_tween = CreateTween();
		_tween.TweenProperty(_whatsMoved, "global_position:x", num, 2.5).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Cubic);
		await ToSignal(_tween, Tween.SignalName.Finished);
		_targetPosition = _whatsMoved.Position;
	}

	public void SetEnabled(bool enabled)
	{
		base.FocusBehaviorRecursive = (FocusBehaviorRecursiveEnum)(enabled ? 2 : 1);
		_isDragging = false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(11);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._GuiInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ProcessPanEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ProcessGuiFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "focusedControl", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ProcessScrollEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnToggleVisibility, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Reset, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetEnabled, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "enabled", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName._EnterTree && args.Count == 0)
		{
			_EnterTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._GuiInput && args.Count == 1)
		{
			_GuiInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ProcessPanEvent && args.Count == 1)
		{
			ProcessPanEvent(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ProcessGuiFocus && args.Count == 1)
		{
			ProcessGuiFocus(VariantUtils.ConvertTo<Control>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ProcessScrollEvent && args.Count == 1)
		{
			ProcessScrollEvent(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnToggleVisibility && args.Count == 0)
		{
			OnToggleVisibility();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Reset && args.Count == 0)
		{
			Reset();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetEnabled && args.Count == 1)
		{
			SetEnabled(VariantUtils.ConvertTo<bool>(in args[0]));
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
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName._GuiInput)
		{
			return true;
		}
		if (method == MethodName.ProcessPanEvent)
		{
			return true;
		}
		if (method == MethodName.ProcessGuiFocus)
		{
			return true;
		}
		if (method == MethodName.ProcessScrollEvent)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName.OnToggleVisibility)
		{
			return true;
		}
		if (method == MethodName.Reset)
		{
			return true;
		}
		if (method == MethodName.SetEnabled)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._whatsMoved)
		{
			_whatsMoved = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._dragStartPosition)
		{
			_dragStartPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._targetPosition)
		{
			_targetPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._isDragging)
		{
			_isDragging = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._epochSlots)
		{
			_epochSlots = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.GetInitX)
		{
			value = VariantUtils.CreateFrom<float>(GetInitX);
			return true;
		}
		if (name == PropertyName._whatsMoved)
		{
			value = VariantUtils.CreateFrom(in _whatsMoved);
			return true;
		}
		if (name == PropertyName._dragStartPosition)
		{
			value = VariantUtils.CreateFrom(in _dragStartPosition);
			return true;
		}
		if (name == PropertyName._targetPosition)
		{
			value = VariantUtils.CreateFrom(in _targetPosition);
			return true;
		}
		if (name == PropertyName._isDragging)
		{
			value = VariantUtils.CreateFrom(in _isDragging);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._epochSlots)
		{
			value = VariantUtils.CreateFrom(in _epochSlots);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._whatsMoved, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._dragStartPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._targetPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isDragging, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._epochSlots, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName.GetInitX, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._whatsMoved, Variant.From(in _whatsMoved));
		info.AddProperty(PropertyName._dragStartPosition, Variant.From(in _dragStartPosition));
		info.AddProperty(PropertyName._targetPosition, Variant.From(in _targetPosition));
		info.AddProperty(PropertyName._isDragging, Variant.From(in _isDragging));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._epochSlots, Variant.From(in _epochSlots));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._whatsMoved, out var value))
		{
			_whatsMoved = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._dragStartPosition, out var value2))
		{
			_dragStartPosition = value2.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._targetPosition, out var value3))
		{
			_targetPosition = value3.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._isDragging, out var value4))
		{
			_isDragging = value4.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value5))
		{
			_tween = value5.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._epochSlots, out var value6))
		{
			_epochSlots = value6.As<Control>();
		}
	}
}
