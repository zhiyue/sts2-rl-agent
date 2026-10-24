using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace MegaCrit.Sts2.Core.Nodes.GodotExtensions;

/// <summary>
/// Manages scrolling of a container.
/// This should be put on an object that doesn't scroll, and then the content to be scrolled should be a child named
/// Content or Mask/Content.
/// </summary>
[ScriptPath("res://src/Core/Nodes/GodotExtensions/NScrollableContainer.cs")]
public class NScrollableContainer : Control
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Control.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'SetContent' method.
		/// </summary>
		public static readonly StringName SetContent = "SetContent";

		/// <summary>
		/// Cached name for the 'UpdatePadding' method.
		/// </summary>
		public static readonly StringName UpdatePadding = "UpdatePadding";

		/// <summary>
		/// Cached name for the 'DisableScrollingIfContentFits' method.
		/// </summary>
		public static readonly StringName DisableScrollingIfContentFits = "DisableScrollingIfContentFits";

		/// <summary>
		/// Cached name for the '_EnterTree' method.
		/// </summary>
		public new static readonly StringName _EnterTree = "_EnterTree";

		/// <summary>
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";

		/// <summary>
		/// Cached name for the 'UpdateScrollLimitBottom' method.
		/// </summary>
		public static readonly StringName UpdateScrollLimitBottom = "UpdateScrollLimitBottom";

		/// <summary>
		/// Cached name for the '_GuiInput' method.
		/// </summary>
		public new static readonly StringName _GuiInput = "_GuiInput";

		/// <summary>
		/// Cached name for the '_Input' method.
		/// </summary>
		public new static readonly StringName _Input = "_Input";

		/// <summary>
		/// Cached name for the 'ProcessControllerEvent' method.
		/// </summary>
		public static readonly StringName ProcessControllerEvent = "ProcessControllerEvent";

		/// <summary>
		/// Cached name for the 'ProcessMouseEvent' method.
		/// </summary>
		public static readonly StringName ProcessMouseEvent = "ProcessMouseEvent";

		/// <summary>
		/// Cached name for the 'ProcessScrollEvent' method.
		/// </summary>
		public static readonly StringName ProcessScrollEvent = "ProcessScrollEvent";

		/// <summary>
		/// Cached name for the '_Process' method.
		/// </summary>
		public new static readonly StringName _Process = "_Process";

		/// <summary>
		/// Cached name for the 'InstantlyScrollToTop' method.
		/// </summary>
		public static readonly StringName InstantlyScrollToTop = "InstantlyScrollToTop";

		/// <summary>
		/// Cached name for the 'ProcessGuiFocus' method.
		/// </summary>
		public static readonly StringName ProcessGuiFocus = "ProcessGuiFocus";

		/// <summary>
		/// Cached name for the 'UpdateScrollPosition' method.
		/// </summary>
		public static readonly StringName UpdateScrollPosition = "UpdateScrollPosition";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
		/// <summary>
		/// Cached name for the 'ScrollViewportTop' property.
		/// </summary>
		public static readonly StringName ScrollViewportTop = "ScrollViewportTop";

		/// <summary>
		/// Cached name for the 'ScrollViewportSize' property.
		/// </summary>
		public static readonly StringName ScrollViewportSize = "ScrollViewportSize";

		/// <summary>
		/// Cached name for the 'ScrollLimitBottom' property.
		/// </summary>
		public static readonly StringName ScrollLimitBottom = "ScrollLimitBottom";

		/// <summary>
		/// Cached name for the 'Scrollbar' property.
		/// </summary>
		public static readonly StringName Scrollbar = "Scrollbar";

		/// <summary>
		/// Cached name for the '_controllerScrollAmount' field.
		/// </summary>
		public static readonly StringName _controllerScrollAmount = "_controllerScrollAmount";

		/// <summary>
		/// Cached name for the '_startDragPosY' field.
		/// </summary>
		public static readonly StringName _startDragPosY = "_startDragPosY";

		/// <summary>
		/// Cached name for the '_targetDragPosY' field.
		/// </summary>
		public static readonly StringName _targetDragPosY = "_targetDragPosY";

		/// <summary>
		/// Cached name for the '_isDragging' field.
		/// </summary>
		public static readonly StringName _isDragging = "_isDragging";

		/// <summary>
		/// Cached name for the '_paddingTop' field.
		/// </summary>
		public static readonly StringName _paddingTop = "_paddingTop";

		/// <summary>
		/// Cached name for the '_paddingBottom' field.
		/// </summary>
		public static readonly StringName _paddingBottom = "_paddingBottom";

		/// <summary>
		/// Cached name for the '_content' field.
		/// </summary>
		public static readonly StringName _content = "_content";

		/// <summary>
		/// Cached name for the '_scrollbarPressed' field.
		/// </summary>
		public static readonly StringName _scrollbarPressed = "_scrollbarPressed";

		/// <summary>
		/// Cached name for the '_disableScrollingIfContentFits' field.
		/// </summary>
		public static readonly StringName _disableScrollingIfContentFits = "_disableScrollingIfContentFits";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
	}

	private float _controllerScrollAmount = 400f;

	private float _startDragPosY;

	private float _targetDragPosY;

	private bool _isDragging;

	private float _paddingTop;

	private float _paddingBottom;

	private Control? _content;

	private bool _scrollbarPressed;

	private bool _disableScrollingIfContentFits;

	private float ScrollViewportTop
	{
		get
		{
			if (_content != null)
			{
				return _content.GetParent<Control>().Position.Y;
			}
			return 0f;
		}
	}

	private float ScrollViewportSize
	{
		get
		{
			if (_content != null)
			{
				return _content.GetParent<Control>().Size.Y;
			}
			return 0f;
		}
	}

	private float ScrollLimitBottom
	{
		get
		{
			if (_content != null)
			{
				return 0f - (_paddingBottom + _paddingTop + _content.Size.Y) + _content.GetParent<Control>().Size.Y;
			}
			return 0f;
		}
	}

	public NScrollbar Scrollbar { get; private set; }

	public override void _Ready()
	{
		_content = GetNodeOrNull<Control>("Content") ?? GetNodeOrNull<Control>("Mask/Content");
		Scrollbar = GetNode<NScrollbar>("Scrollbar");
		SetContent(_content);
		Scrollbar.Visible = false;
		Scrollbar.Connect(NScrollbar.SignalName.MousePressed, Callable.From<InputEvent>(delegate
		{
			_scrollbarPressed = true;
		}));
		Scrollbar.Connect(NScrollbar.SignalName.MouseReleased, Callable.From<InputEvent>(delegate
		{
			_scrollbarPressed = false;
		}));
	}

	/// <summary>
	/// If the content to be scrolled is dynamic (e.g. a tab in the settings menu, then you can change it using this method.
	/// </summary>
	public void SetContent(Control? content, float paddingTop = 0f, float paddingBottom = 0f)
	{
		Callable callable = Callable.From(UpdateScrollLimitBottom);
		if (_content != null && _content.IsConnected(CanvasItem.SignalName.ItemRectChanged, callable))
		{
			_content.Disconnect(CanvasItem.SignalName.ItemRectChanged, callable);
		}
		_content = content;
		if (_content != null)
		{
			_content.Connect(CanvasItem.SignalName.ItemRectChanged, Callable.From(UpdateScrollLimitBottom));
			UpdatePadding(paddingTop, paddingBottom);
		}
	}

	public void UpdatePadding(float paddingTop = 0f, float paddingBottom = 0f)
	{
		_paddingTop = paddingTop;
		_paddingBottom = paddingBottom;
		UpdateScrollLimitBottom();
	}

	/// <summary>
	/// If this is called, then scrolling is disabled when the content fits entirely within the scroll container.
	/// </summary>
	public void DisableScrollingIfContentFits()
	{
		_disableScrollingIfContentFits = true;
	}

	public override void _EnterTree()
	{
		GetViewport().Connect(Viewport.SignalName.GuiFocusChanged, Callable.From<Control>(ProcessGuiFocus));
	}

	public override void _ExitTree()
	{
		GetViewport().Disconnect(Viewport.SignalName.GuiFocusChanged, Callable.From<Control>(ProcessGuiFocus));
	}

	private void UpdateScrollLimitBottom()
	{
		if (_content != null)
		{
			Scrollbar.Visible = _content.Size.Y + _paddingTop + _paddingBottom > ScrollViewportSize;
			Scrollbar.MouseFilter = (MouseFilterEnum)(Scrollbar.Visible ? 0 : 2);
		}
	}

	public override void _GuiInput(InputEvent inputEvent)
	{
		if (IsVisibleInTree())
		{
			ProcessMouseEvent(inputEvent);
			ProcessScrollEvent(inputEvent);
		}
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (IsVisibleInTree())
		{
			Viewport viewport = GetViewport();
			if (viewport == null || viewport.GuiGetFocusOwner() == null)
			{
				ProcessControllerEvent(inputEvent);
			}
		}
	}

	private void ProcessControllerEvent(InputEvent inputEvent)
	{
		if (inputEvent.IsActionPressed(MegaInput.up))
		{
			_targetDragPosY += _controllerScrollAmount;
		}
		else if (inputEvent.IsActionPressed(MegaInput.down))
		{
			_targetDragPosY += 0f - _controllerScrollAmount;
		}
	}

	/// <summary>
	/// Detects mouse click up/down and updates our scroll target accordingly.
	/// </summary>
	private void ProcessMouseEvent(InputEvent inputEvent)
	{
		if (_content == null)
		{
			return;
		}
		if (!(inputEvent is InputEventMouseMotion inputEventMouseMotion))
		{
			if (!(inputEvent is InputEventMouseButton inputEventMouseButton))
			{
				return;
			}
			if (inputEventMouseButton.ButtonIndex == MouseButton.Left)
			{
				_isDragging = inputEventMouseButton.Pressed;
				if (inputEventMouseButton.Pressed)
				{
					_startDragPosY = _content.Position.Y - _paddingTop;
					_targetDragPosY = _startDragPosY;
				}
			}
			else if (!inputEventMouseButton.Pressed)
			{
				_isDragging = false;
			}
		}
		else if (_isDragging)
		{
			_targetDragPosY += inputEventMouseMotion.Relative.Y;
		}
	}

	/// <summary>
	/// Detects mouse wheel up/down and updates our scroll target accordingly
	/// </summary>
	/// <param name="inputEvent"></param>
	private void ProcessScrollEvent(InputEvent inputEvent)
	{
		_targetDragPosY += ScrollHelper.GetDragForScrollEvent(inputEvent);
	}

	public override void _Process(double delta)
	{
		if (IsVisibleInTree() && (!_disableScrollingIfContentFits || Scrollbar.Visible))
		{
			UpdateScrollPosition(delta);
		}
	}

	public void InstantlyScrollToTop()
	{
		if (_content == null)
		{
			throw new InvalidOperationException("No content to scroll!");
		}
		_targetDragPosY = 0f;
		Control? content = _content;
		Vector2 position = _content.Position;
		position.Y = _paddingTop;
		content.Position = position;
		Scrollbar.SetValueWithoutAnimation(0.0);
	}

	private void ProcessGuiFocus(Control focusedControl)
	{
		if (_content != null && IsVisibleInTree() && NControllerManager.Instance.IsUsingController && !(focusedControl is NDropdownItem) && _content.IsAncestorOf(focusedControl))
		{
			float num = _content.GlobalPosition.Y - focusedControl.GlobalPosition.Y;
			float value = num + ScrollViewportSize * 0.5f;
			float max = Mathf.Max(ScrollLimitBottom, 0f);
			float min = Mathf.Min(ScrollLimitBottom, 0f);
			value = Mathf.Clamp(value, min, max);
			_targetDragPosY = value;
		}
	}

	private void UpdateScrollPosition(double delta)
	{
		if (_content == null)
		{
			return;
		}
		float num = _paddingTop + _targetDragPosY;
		if (!Mathf.IsEqualApprox(_content.Position.Y, num))
		{
			float y = Mathf.Lerp(_content.Position.Y, num, (float)delta * 15f);
			Control? content = _content;
			Vector2 position = _content.Position;
			position.Y = y;
			content.Position = position;
			if (Mathf.Abs(_content.Position.Y - num) < 0.5f)
			{
				Control? content2 = _content;
				position = _content.Position;
				position.Y = num;
				content2.Position = position;
			}
			if (!_scrollbarPressed && ScrollLimitBottom < 0f)
			{
				Scrollbar.SetValueWithoutAnimation(Mathf.Clamp((_content.Position.Y - _paddingTop) / ScrollLimitBottom, 0f, 1f) * 100f);
			}
		}
		if (_scrollbarPressed)
		{
			_targetDragPosY = Mathf.Lerp(0f, ScrollLimitBottom, (float)Scrollbar.Value * 0.01f);
		}
		if (!_isDragging)
		{
			if (_targetDragPosY < Mathf.Min(ScrollLimitBottom, 0f))
			{
				_targetDragPosY = Mathf.Lerp(_targetDragPosY, ScrollLimitBottom, (float)delta * 12f);
			}
			else if (_targetDragPosY > Mathf.Max(ScrollLimitBottom, 0f))
			{
				_targetDragPosY = Mathf.Lerp(_targetDragPosY, 0f, (float)delta * 12f);
			}
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
		List<MethodInfo> list = new List<MethodInfo>(16);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetContent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "content", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false),
			new PropertyInfo(Variant.Type.Float, "paddingTop", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Float, "paddingBottom", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdatePadding, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "paddingTop", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Float, "paddingBottom", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.DisableScrollingIfContentFits, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateScrollLimitBottom, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._GuiInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ProcessControllerEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
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
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.InstantlyScrollToTop, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ProcessGuiFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "focusedControl", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateScrollPosition, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
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
		if (method == MethodName.SetContent && args.Count == 3)
		{
			SetContent(VariantUtils.ConvertTo<Control>(in args[0]), VariantUtils.ConvertTo<float>(in args[1]), VariantUtils.ConvertTo<float>(in args[2]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdatePadding && args.Count == 2)
		{
			UpdatePadding(VariantUtils.ConvertTo<float>(in args[0]), VariantUtils.ConvertTo<float>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DisableScrollingIfContentFits && args.Count == 0)
		{
			DisableScrollingIfContentFits();
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
		if (method == MethodName.UpdateScrollLimitBottom && args.Count == 0)
		{
			UpdateScrollLimitBottom();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._GuiInput && args.Count == 1)
		{
			_GuiInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Input && args.Count == 1)
		{
			_Input(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ProcessControllerEvent && args.Count == 1)
		{
			ProcessControllerEvent(VariantUtils.ConvertTo<InputEvent>(in args[0]));
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
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.InstantlyScrollToTop && args.Count == 0)
		{
			InstantlyScrollToTop();
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
		if (method == MethodName.SetContent)
		{
			return true;
		}
		if (method == MethodName.UpdatePadding)
		{
			return true;
		}
		if (method == MethodName.DisableScrollingIfContentFits)
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
		if (method == MethodName.UpdateScrollLimitBottom)
		{
			return true;
		}
		if (method == MethodName._GuiInput)
		{
			return true;
		}
		if (method == MethodName._Input)
		{
			return true;
		}
		if (method == MethodName.ProcessControllerEvent)
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
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName.InstantlyScrollToTop)
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
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.Scrollbar)
		{
			Scrollbar = VariantUtils.ConvertTo<NScrollbar>(in value);
			return true;
		}
		if (name == PropertyName._controllerScrollAmount)
		{
			_controllerScrollAmount = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._startDragPosY)
		{
			_startDragPosY = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._targetDragPosY)
		{
			_targetDragPosY = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._isDragging)
		{
			_isDragging = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._paddingTop)
		{
			_paddingTop = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._paddingBottom)
		{
			_paddingBottom = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._content)
		{
			_content = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._scrollbarPressed)
		{
			_scrollbarPressed = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._disableScrollingIfContentFits)
		{
			_disableScrollingIfContentFits = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		float from;
		if (name == PropertyName.ScrollViewportTop)
		{
			from = ScrollViewportTop;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.ScrollViewportSize)
		{
			from = ScrollViewportSize;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.ScrollLimitBottom)
		{
			from = ScrollLimitBottom;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.Scrollbar)
		{
			value = VariantUtils.CreateFrom<NScrollbar>(Scrollbar);
			return true;
		}
		if (name == PropertyName._controllerScrollAmount)
		{
			value = VariantUtils.CreateFrom(in _controllerScrollAmount);
			return true;
		}
		if (name == PropertyName._startDragPosY)
		{
			value = VariantUtils.CreateFrom(in _startDragPosY);
			return true;
		}
		if (name == PropertyName._targetDragPosY)
		{
			value = VariantUtils.CreateFrom(in _targetDragPosY);
			return true;
		}
		if (name == PropertyName._isDragging)
		{
			value = VariantUtils.CreateFrom(in _isDragging);
			return true;
		}
		if (name == PropertyName._paddingTop)
		{
			value = VariantUtils.CreateFrom(in _paddingTop);
			return true;
		}
		if (name == PropertyName._paddingBottom)
		{
			value = VariantUtils.CreateFrom(in _paddingBottom);
			return true;
		}
		if (name == PropertyName._content)
		{
			value = VariantUtils.CreateFrom(in _content);
			return true;
		}
		if (name == PropertyName._scrollbarPressed)
		{
			value = VariantUtils.CreateFrom(in _scrollbarPressed);
			return true;
		}
		if (name == PropertyName._disableScrollingIfContentFits)
		{
			value = VariantUtils.CreateFrom(in _disableScrollingIfContentFits);
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
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._controllerScrollAmount, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._startDragPosY, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._targetDragPosY, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isDragging, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName.ScrollViewportTop, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName.ScrollViewportSize, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName.ScrollLimitBottom, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._paddingTop, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._paddingBottom, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._content, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._scrollbarPressed, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._disableScrollingIfContentFits, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Scrollbar, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.Scrollbar, Variant.From<NScrollbar>(Scrollbar));
		info.AddProperty(PropertyName._controllerScrollAmount, Variant.From(in _controllerScrollAmount));
		info.AddProperty(PropertyName._startDragPosY, Variant.From(in _startDragPosY));
		info.AddProperty(PropertyName._targetDragPosY, Variant.From(in _targetDragPosY));
		info.AddProperty(PropertyName._isDragging, Variant.From(in _isDragging));
		info.AddProperty(PropertyName._paddingTop, Variant.From(in _paddingTop));
		info.AddProperty(PropertyName._paddingBottom, Variant.From(in _paddingBottom));
		info.AddProperty(PropertyName._content, Variant.From(in _content));
		info.AddProperty(PropertyName._scrollbarPressed, Variant.From(in _scrollbarPressed));
		info.AddProperty(PropertyName._disableScrollingIfContentFits, Variant.From(in _disableScrollingIfContentFits));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.Scrollbar, out var value))
		{
			Scrollbar = value.As<NScrollbar>();
		}
		if (info.TryGetProperty(PropertyName._controllerScrollAmount, out var value2))
		{
			_controllerScrollAmount = value2.As<float>();
		}
		if (info.TryGetProperty(PropertyName._startDragPosY, out var value3))
		{
			_startDragPosY = value3.As<float>();
		}
		if (info.TryGetProperty(PropertyName._targetDragPosY, out var value4))
		{
			_targetDragPosY = value4.As<float>();
		}
		if (info.TryGetProperty(PropertyName._isDragging, out var value5))
		{
			_isDragging = value5.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._paddingTop, out var value6))
		{
			_paddingTop = value6.As<float>();
		}
		if (info.TryGetProperty(PropertyName._paddingBottom, out var value7))
		{
			_paddingBottom = value7.As<float>();
		}
		if (info.TryGetProperty(PropertyName._content, out var value8))
		{
			_content = value8.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._scrollbarPressed, out var value9))
		{
			_scrollbarPressed = value9.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._disableScrollingIfContentFits, out var value10))
		{
			_disableScrollingIfContentFits = value10.As<bool>();
		}
	}
}
