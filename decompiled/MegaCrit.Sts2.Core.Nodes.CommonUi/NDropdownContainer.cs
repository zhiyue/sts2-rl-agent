using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;

namespace MegaCrit.Sts2.Core.Nodes.CommonUi;

[ScriptPath("res://src/Core/Nodes/CommonUi/NDropdownContainer.cs")]
public class NDropdownContainer : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName OnVisibilityChange = "OnVisibilityChange";

		public static readonly StringName RefreshLayout = "RefreshLayout";

		public static readonly StringName IsScrollbarNeeded = "IsScrollbarNeeded";

		public new static readonly StringName _Process = "_Process";

		public static readonly StringName ProcessGuiFocus = "ProcessGuiFocus";

		public static readonly StringName UpdateScrollPosition = "UpdateScrollPosition";

		public static readonly StringName UpdateScrollbar = "UpdateScrollbar";

		public static readonly StringName UpdatePositionBasedOnTrain = "UpdatePositionBasedOnTrain";

		public new static readonly StringName _GuiInput = "_GuiInput";

		public static readonly StringName ProcessMouseEvent = "ProcessMouseEvent";

		public static readonly StringName ProcessScrollEvent = "ProcessScrollEvent";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _scrollbar = "_scrollbar";

		public static readonly StringName _scrollbarTrain = "_scrollbarTrain";

		public static readonly StringName _dropdownItems = "_dropdownItems";

		public static readonly StringName _maxHeight = "_maxHeight";

		public static readonly StringName _contentHeight = "_contentHeight";

		public static readonly StringName _startDragPos = "_startDragPos";

		public static readonly StringName _targetDragPos = "_targetDragPos";

		public static readonly StringName _scrollLimitBottom = "_scrollLimitBottom";

		public static readonly StringName _isDragging = "_isDragging";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private NDropdownScrollbar _scrollbar;

	private Control _scrollbarTrain;

	private VBoxContainer _dropdownItems;

	private float _maxHeight;

	private float _contentHeight;

	private Vector2 _startDragPos;

	private Vector2 _targetDragPos = Vector2.Zero;

	private const float _scrollLimitTop = 0f;

	private float _scrollLimitBottom;

	private bool _isDragging;

	public override void _Ready()
	{
		_scrollbar = GetNode<NDropdownScrollbar>("Scrollbar");
		_scrollbarTrain = GetNode<Control>("Scrollbar/Train");
		_dropdownItems = GetNode<VBoxContainer>("VBoxContainer");
		Connect(CanvasItem.SignalName.VisibilityChanged, Callable.From(OnVisibilityChange));
		_maxHeight = base.Size.Y;
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		GetViewport().Connect(Viewport.SignalName.GuiFocusChanged, Callable.From<Control>(ProcessGuiFocus));
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		GetViewport().Disconnect(Viewport.SignalName.GuiFocusChanged, Callable.From<Control>(ProcessGuiFocus));
	}

	private void OnVisibilityChange()
	{
		if (base.Visible)
		{
			_isDragging = false;
		}
	}

	public void RefreshLayout()
	{
		_scrollbar.Visible = IsScrollbarNeeded();
	}

	private bool IsScrollbarNeeded()
	{
		_contentHeight = 0f;
		foreach (Node child in _dropdownItems.GetChildren())
		{
			if (child is Control control)
			{
				_contentHeight += control.Size.Y;
			}
		}
		_scrollLimitBottom = 0f - _contentHeight + _maxHeight;
		if (_contentHeight > _maxHeight)
		{
			base.Size = new Vector2(base.Size.X, _maxHeight);
			_scrollbar.RefreshTrainBounds();
			return true;
		}
		base.Size = new Vector2(base.Size.X, _contentHeight);
		return false;
	}

	public override void _Process(double delta)
	{
		if (IsVisibleInTree())
		{
			UpdateScrollPosition(delta);
			UpdateScrollbar();
		}
	}

	private void ProcessGuiFocus(Control focusedControl)
	{
		if (IsVisibleInTree() && _scrollbar.Visible && NControllerManager.Instance.IsUsingController && _dropdownItems.IsAncestorOf(focusedControl))
		{
			float num = _dropdownItems.GlobalPosition.Y - focusedControl.GlobalPosition.Y;
			float value = num + base.Size.Y * 0.5f;
			value = Mathf.Clamp(value, _scrollLimitBottom, 0f);
			_targetDragPos = new Vector2(_targetDragPos.X, value);
		}
	}

	private void UpdateScrollPosition(double delta)
	{
		if (!_scrollbar.Visible)
		{
			return;
		}
		if (_dropdownItems.Position != _targetDragPos)
		{
			_dropdownItems.Position = _dropdownItems.Position.Lerp(_targetDragPos, (float)delta * 15f);
			if (_dropdownItems.Position.DistanceTo(_targetDragPos) < 0.5f)
			{
				_dropdownItems.Position = _targetDragPos;
			}
		}
		if (!_isDragging)
		{
			if (_targetDragPos.Y < _scrollLimitBottom)
			{
				_targetDragPos = _targetDragPos.Lerp(new Vector2(0f, _scrollLimitBottom), (float)delta * 12f);
			}
			else if (_targetDragPos.Y > 0f)
			{
				_targetDragPos = _targetDragPos.Lerp(new Vector2(0f, 0f), (float)delta * 12f);
			}
		}
	}

	private void UpdateScrollbar()
	{
		if (_scrollbar.Visible && !_scrollbar.hasControl)
		{
			float num = (_dropdownItems.Position.Y - _scrollLimitBottom) / (0f - _scrollLimitBottom);
			_scrollbar.SetTrainPositionFromPercentage(Mathf.Clamp(1f - num, 0f, 1f));
		}
	}

	public void UpdatePositionBasedOnTrain(float trainPosition)
	{
		_targetDragPos = new Vector2(_targetDragPos.X, _scrollLimitBottom + trainPosition * (0f - _scrollLimitBottom));
	}

	public override void _GuiInput(InputEvent inputEvent)
	{
		ProcessMouseEvent(inputEvent);
		ProcessScrollEvent(inputEvent);
	}

	private void ProcessMouseEvent(InputEvent inputEvent)
	{
		if (_isDragging && inputEvent is InputEventMouseMotion inputEventMouseMotion)
		{
			_targetDragPos += new Vector2(0f, inputEventMouseMotion.Relative.Y);
		}
		else
		{
			if (!(inputEvent is InputEventMouseButton inputEventMouseButton))
			{
				return;
			}
			if (inputEventMouseButton.ButtonIndex == MouseButton.Left)
			{
				if (inputEventMouseButton.Pressed)
				{
					_isDragging = true;
					_startDragPos = _dropdownItems.Position;
					_targetDragPos = _startDragPos;
				}
				else
				{
					_isDragging = false;
				}
			}
			else if (!inputEventMouseButton.Pressed)
			{
				_isDragging = false;
			}
		}
	}

	private void ProcessScrollEvent(InputEvent inputEvent)
	{
		_targetDragPos += new Vector2(0f, ScrollHelper.GetDragForScrollEvent(inputEvent));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(14);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnVisibilityChange, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshLayout, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.IsScrollbarNeeded, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ProcessGuiFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "focusedControl", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateScrollPosition, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateScrollbar, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdatePositionBasedOnTrain, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "trainPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._GuiInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ProcessMouseEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ProcessScrollEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
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
		if (method == MethodName.OnVisibilityChange && args.Count == 0)
		{
			OnVisibilityChange();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshLayout && args.Count == 0)
		{
			RefreshLayout();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.IsScrollbarNeeded && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(IsScrollbarNeeded());
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ProcessGuiFocus && args.Count == 1)
		{
			ProcessGuiFocus(VariantUtils.ConvertTo<Control>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateScrollPosition && args.Count == 1)
		{
			UpdateScrollPosition(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateScrollbar && args.Count == 0)
		{
			UpdateScrollbar();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdatePositionBasedOnTrain && args.Count == 1)
		{
			UpdatePositionBasedOnTrain(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._GuiInput && args.Count == 1)
		{
			_GuiInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ProcessMouseEvent && args.Count == 1)
		{
			ProcessMouseEvent(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ProcessScrollEvent && args.Count == 1)
		{
			ProcessScrollEvent(VariantUtils.ConvertTo<InputEvent>(in args[0]));
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
		if (method == MethodName.OnVisibilityChange)
		{
			return true;
		}
		if (method == MethodName.RefreshLayout)
		{
			return true;
		}
		if (method == MethodName.IsScrollbarNeeded)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName.ProcessGuiFocus)
		{
			return true;
		}
		if (method == MethodName.UpdateScrollPosition)
		{
			return true;
		}
		if (method == MethodName.UpdateScrollbar)
		{
			return true;
		}
		if (method == MethodName.UpdatePositionBasedOnTrain)
		{
			return true;
		}
		if (method == MethodName._GuiInput)
		{
			return true;
		}
		if (method == MethodName.ProcessMouseEvent)
		{
			return true;
		}
		if (method == MethodName.ProcessScrollEvent)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._scrollbar)
		{
			_scrollbar = VariantUtils.ConvertTo<NDropdownScrollbar>(in value);
			return true;
		}
		if (name == PropertyName._scrollbarTrain)
		{
			_scrollbarTrain = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._dropdownItems)
		{
			_dropdownItems = VariantUtils.ConvertTo<VBoxContainer>(in value);
			return true;
		}
		if (name == PropertyName._maxHeight)
		{
			_maxHeight = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._contentHeight)
		{
			_contentHeight = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._startDragPos)
		{
			_startDragPos = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._targetDragPos)
		{
			_targetDragPos = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._scrollLimitBottom)
		{
			_scrollLimitBottom = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._isDragging)
		{
			_isDragging = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._scrollbar)
		{
			value = VariantUtils.CreateFrom(in _scrollbar);
			return true;
		}
		if (name == PropertyName._scrollbarTrain)
		{
			value = VariantUtils.CreateFrom(in _scrollbarTrain);
			return true;
		}
		if (name == PropertyName._dropdownItems)
		{
			value = VariantUtils.CreateFrom(in _dropdownItems);
			return true;
		}
		if (name == PropertyName._maxHeight)
		{
			value = VariantUtils.CreateFrom(in _maxHeight);
			return true;
		}
		if (name == PropertyName._contentHeight)
		{
			value = VariantUtils.CreateFrom(in _contentHeight);
			return true;
		}
		if (name == PropertyName._startDragPos)
		{
			value = VariantUtils.CreateFrom(in _startDragPos);
			return true;
		}
		if (name == PropertyName._targetDragPos)
		{
			value = VariantUtils.CreateFrom(in _targetDragPos);
			return true;
		}
		if (name == PropertyName._scrollLimitBottom)
		{
			value = VariantUtils.CreateFrom(in _scrollLimitBottom);
			return true;
		}
		if (name == PropertyName._isDragging)
		{
			value = VariantUtils.CreateFrom(in _isDragging);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._scrollbar, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._scrollbarTrain, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._dropdownItems, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._maxHeight, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._contentHeight, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._startDragPos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._targetDragPos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._scrollLimitBottom, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isDragging, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._scrollbar, Variant.From(in _scrollbar));
		info.AddProperty(PropertyName._scrollbarTrain, Variant.From(in _scrollbarTrain));
		info.AddProperty(PropertyName._dropdownItems, Variant.From(in _dropdownItems));
		info.AddProperty(PropertyName._maxHeight, Variant.From(in _maxHeight));
		info.AddProperty(PropertyName._contentHeight, Variant.From(in _contentHeight));
		info.AddProperty(PropertyName._startDragPos, Variant.From(in _startDragPos));
		info.AddProperty(PropertyName._targetDragPos, Variant.From(in _targetDragPos));
		info.AddProperty(PropertyName._scrollLimitBottom, Variant.From(in _scrollLimitBottom));
		info.AddProperty(PropertyName._isDragging, Variant.From(in _isDragging));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._scrollbar, out var value))
		{
			_scrollbar = value.As<NDropdownScrollbar>();
		}
		if (info.TryGetProperty(PropertyName._scrollbarTrain, out var value2))
		{
			_scrollbarTrain = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._dropdownItems, out var value3))
		{
			_dropdownItems = value3.As<VBoxContainer>();
		}
		if (info.TryGetProperty(PropertyName._maxHeight, out var value4))
		{
			_maxHeight = value4.As<float>();
		}
		if (info.TryGetProperty(PropertyName._contentHeight, out var value5))
		{
			_contentHeight = value5.As<float>();
		}
		if (info.TryGetProperty(PropertyName._startDragPos, out var value6))
		{
			_startDragPos = value6.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._targetDragPos, out var value7))
		{
			_targetDragPos = value7.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._scrollLimitBottom, out var value8))
		{
			_scrollLimitBottom = value8.As<float>();
		}
		if (info.TryGetProperty(PropertyName._isDragging, out var value9))
		{
			_isDragging = value9.As<bool>();
		}
	}
}
