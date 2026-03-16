using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Nodes.Reaction;

[ScriptPath("res://src/Core/Nodes/Reaction/NReactionWheel.cs")]
public class NReactionWheel : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public new static readonly StringName _Notification = "_Notification";

		public new static readonly StringName _Input = "_Input";

		public static readonly StringName HideWheel = "HideWheel";

		public static readonly StringName WarpMouseBackToOriginalPosition = "WarpMouseBackToOriginalPosition";

		public static readonly StringName React = "React";

		public static readonly StringName MoveMarker = "MoveMarker";

		public static readonly StringName GetSelectedWedge = "GetSelectedWedge";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _rightWedge = "_rightWedge";

		public static readonly StringName _downRightWedge = "_downRightWedge";

		public static readonly StringName _downWedge = "_downWedge";

		public static readonly StringName _downLeftWedge = "_downLeftWedge";

		public static readonly StringName _leftWedge = "_leftWedge";

		public static readonly StringName _upLeftWedge = "_upLeftWedge";

		public static readonly StringName _upWedge = "_upWedge";

		public static readonly StringName _upRightWedge = "_upRightWedge";

		public static readonly StringName _marker = "_marker";

		public static readonly StringName _ignoreNextMouseInput = "_ignoreNextMouseInput";

		public static readonly StringName _centerPosition = "_centerPosition";

		public static readonly StringName _selectedWedge = "_selectedWedge";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private static readonly StringName _reactWheel = new StringName("react_wheel");

	private const float _centerRadius = 70f;

	private NReactionWheelWedge _rightWedge;

	private NReactionWheelWedge _downRightWedge;

	private NReactionWheelWedge _downWedge;

	private NReactionWheelWedge _downLeftWedge;

	private NReactionWheelWedge _leftWedge;

	private NReactionWheelWedge _upLeftWedge;

	private NReactionWheelWedge _upWedge;

	private NReactionWheelWedge _upRightWedge;

	private TextureRect _marker;

	private bool _ignoreNextMouseInput;

	private Vector2 _centerPosition;

	private NReactionWheelWedge? _selectedWedge;

	private Player? _localPlayer;

	public override void _Ready()
	{
		_rightWedge = GetNode<NReactionWheelWedge>("RightWedge");
		_downRightWedge = GetNode<NReactionWheelWedge>("DownRightWedge");
		_downWedge = GetNode<NReactionWheelWedge>("DownWedge");
		_downLeftWedge = GetNode<NReactionWheelWedge>("DownLeftWedge");
		_leftWedge = GetNode<NReactionWheelWedge>("LeftWedge");
		_upLeftWedge = GetNode<NReactionWheelWedge>("UpLeftWedge");
		_upWedge = GetNode<NReactionWheelWedge>("UpWedge");
		_upRightWedge = GetNode<NReactionWheelWedge>("UpRightWedge");
		_marker = GetNode<TextureRect>("Marker");
		base.Visible = false;
	}

	public override void _EnterTree()
	{
		RunManager.Instance.RunStarted += OnRunStarted;
	}

	public override void _ExitTree()
	{
		RunManager.Instance.RunStarted -= OnRunStarted;
	}

	public override void _Notification(int what)
	{
		if (base.Visible && (long)what == 2017)
		{
			base.Visible = false;
		}
	}

	public override void _Input(InputEvent inputEvent)
	{
		Control control = GetViewport().GuiGetFocusOwner();
		bool flag = ((control is TextEdit || control is LineEdit) ? true : false);
		bool flag2 = flag;
		if (!NGame.Instance.ReactionContainer.InMultiplayer)
		{
			if (base.Visible)
			{
				HideWheel();
			}
		}
		else if (inputEvent is InputEventMouseMotion inputEventMouseMotion)
		{
			if (_ignoreNextMouseInput)
			{
				_ignoreNextMouseInput = false;
			}
			else if (base.Visible)
			{
				MoveMarker(inputEventMouseMotion.Relative);
				_ignoreNextMouseInput = true;
				WarpMouseBackToOriginalPosition();
			}
		}
		else if (inputEvent.IsActionPressed(_reactWheel) && !flag2)
		{
			base.Visible = true;
			if (_localPlayer != null)
			{
				_marker.Texture = _localPlayer.Character.MapMarker;
			}
			_centerPosition = GetViewport().GetMousePosition();
			_marker.Position = (base.Size - _marker.Size) * 0.5f;
			base.GlobalPosition = _centerPosition - base.Size * base.Scale * 0.5f;
			Input.MouseMode = Input.MouseModeEnum.Hidden;
		}
		else if (inputEvent.IsActionReleased(_reactWheel) && base.Visible)
		{
			HideWheel();
			React();
		}
	}

	private void OnRunStarted(RunState runState)
	{
		_localPlayer = LocalContext.GetMe(runState);
	}

	private void HideWheel()
	{
		Input.MouseMode = Input.MouseModeEnum.Visible;
		WarpMouseBackToOriginalPosition();
		base.Visible = false;
	}

	private void WarpMouseBackToOriginalPosition()
	{
		Transform2D viewportTransform = GetViewportTransform();
		Input.WarpMouse(viewportTransform * _centerPosition);
	}

	private void React()
	{
		if (_selectedWedge != null)
		{
			NGame.Instance.ReactionContainer.DoLocalReaction(_selectedWedge.Reaction, _centerPosition);
		}
	}

	private void MoveMarker(Vector2 relative)
	{
		Vector2 vector = (base.Size - _marker.Size) * 0.5f;
		Vector2 vector2 = _marker.Position - vector;
		vector2 = (vector2 + relative).LimitLength(70f);
		_marker.Position = vector + vector2;
		float num = Mathf.Atan2(vector2.Y, vector2.X);
		_marker.Rotation = num - (float)Math.PI / 2f;
		NReactionWheelWedge selectedWedge = GetSelectedWedge(num);
		if (_selectedWedge != selectedWedge)
		{
			_selectedWedge?.OnDeselected();
			_selectedWedge = selectedWedge;
			_selectedWedge?.OnSelected();
		}
	}

	private NReactionWheelWedge GetSelectedWedge(float angle)
	{
		float num = Mathf.Wrap(angle + (float)Math.PI / 8f, 0f, (float)Math.PI * 2f);
		float num2 = (float)Math.PI / 4f;
		return (int)(num / num2) switch
		{
			0 => _rightWedge, 
			1 => _downRightWedge, 
			2 => _downWedge, 
			3 => _downLeftWedge, 
			4 => _leftWedge, 
			5 => _upLeftWedge, 
			6 => _upWedge, 
			7 => _upRightWedge, 
			_ => throw new InvalidOperationException(), 
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(10);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Notification, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "what", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.HideWheel, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.WarpMouseBackToOriginalPosition, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.React, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.MoveMarker, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "relative", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetSelectedWedge, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("TextureRect"), exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "angle", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName._Notification && args.Count == 1)
		{
			_Notification(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Input && args.Count == 1)
		{
			_Input(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HideWheel && args.Count == 0)
		{
			HideWheel();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.WarpMouseBackToOriginalPosition && args.Count == 0)
		{
			WarpMouseBackToOriginalPosition();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.React && args.Count == 0)
		{
			React();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.MoveMarker && args.Count == 1)
		{
			MoveMarker(VariantUtils.ConvertTo<Vector2>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetSelectedWedge && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NReactionWheelWedge>(GetSelectedWedge(VariantUtils.ConvertTo<float>(in args[0])));
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
		if (method == MethodName._Notification)
		{
			return true;
		}
		if (method == MethodName._Input)
		{
			return true;
		}
		if (method == MethodName.HideWheel)
		{
			return true;
		}
		if (method == MethodName.WarpMouseBackToOriginalPosition)
		{
			return true;
		}
		if (method == MethodName.React)
		{
			return true;
		}
		if (method == MethodName.MoveMarker)
		{
			return true;
		}
		if (method == MethodName.GetSelectedWedge)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._rightWedge)
		{
			_rightWedge = VariantUtils.ConvertTo<NReactionWheelWedge>(in value);
			return true;
		}
		if (name == PropertyName._downRightWedge)
		{
			_downRightWedge = VariantUtils.ConvertTo<NReactionWheelWedge>(in value);
			return true;
		}
		if (name == PropertyName._downWedge)
		{
			_downWedge = VariantUtils.ConvertTo<NReactionWheelWedge>(in value);
			return true;
		}
		if (name == PropertyName._downLeftWedge)
		{
			_downLeftWedge = VariantUtils.ConvertTo<NReactionWheelWedge>(in value);
			return true;
		}
		if (name == PropertyName._leftWedge)
		{
			_leftWedge = VariantUtils.ConvertTo<NReactionWheelWedge>(in value);
			return true;
		}
		if (name == PropertyName._upLeftWedge)
		{
			_upLeftWedge = VariantUtils.ConvertTo<NReactionWheelWedge>(in value);
			return true;
		}
		if (name == PropertyName._upWedge)
		{
			_upWedge = VariantUtils.ConvertTo<NReactionWheelWedge>(in value);
			return true;
		}
		if (name == PropertyName._upRightWedge)
		{
			_upRightWedge = VariantUtils.ConvertTo<NReactionWheelWedge>(in value);
			return true;
		}
		if (name == PropertyName._marker)
		{
			_marker = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._ignoreNextMouseInput)
		{
			_ignoreNextMouseInput = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._centerPosition)
		{
			_centerPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._selectedWedge)
		{
			_selectedWedge = VariantUtils.ConvertTo<NReactionWheelWedge>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._rightWedge)
		{
			value = VariantUtils.CreateFrom(in _rightWedge);
			return true;
		}
		if (name == PropertyName._downRightWedge)
		{
			value = VariantUtils.CreateFrom(in _downRightWedge);
			return true;
		}
		if (name == PropertyName._downWedge)
		{
			value = VariantUtils.CreateFrom(in _downWedge);
			return true;
		}
		if (name == PropertyName._downLeftWedge)
		{
			value = VariantUtils.CreateFrom(in _downLeftWedge);
			return true;
		}
		if (name == PropertyName._leftWedge)
		{
			value = VariantUtils.CreateFrom(in _leftWedge);
			return true;
		}
		if (name == PropertyName._upLeftWedge)
		{
			value = VariantUtils.CreateFrom(in _upLeftWedge);
			return true;
		}
		if (name == PropertyName._upWedge)
		{
			value = VariantUtils.CreateFrom(in _upWedge);
			return true;
		}
		if (name == PropertyName._upRightWedge)
		{
			value = VariantUtils.CreateFrom(in _upRightWedge);
			return true;
		}
		if (name == PropertyName._marker)
		{
			value = VariantUtils.CreateFrom(in _marker);
			return true;
		}
		if (name == PropertyName._ignoreNextMouseInput)
		{
			value = VariantUtils.CreateFrom(in _ignoreNextMouseInput);
			return true;
		}
		if (name == PropertyName._centerPosition)
		{
			value = VariantUtils.CreateFrom(in _centerPosition);
			return true;
		}
		if (name == PropertyName._selectedWedge)
		{
			value = VariantUtils.CreateFrom(in _selectedWedge);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._rightWedge, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._downRightWedge, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._downWedge, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._downLeftWedge, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._leftWedge, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._upLeftWedge, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._upWedge, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._upRightWedge, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._marker, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._ignoreNextMouseInput, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._centerPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._selectedWedge, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._rightWedge, Variant.From(in _rightWedge));
		info.AddProperty(PropertyName._downRightWedge, Variant.From(in _downRightWedge));
		info.AddProperty(PropertyName._downWedge, Variant.From(in _downWedge));
		info.AddProperty(PropertyName._downLeftWedge, Variant.From(in _downLeftWedge));
		info.AddProperty(PropertyName._leftWedge, Variant.From(in _leftWedge));
		info.AddProperty(PropertyName._upLeftWedge, Variant.From(in _upLeftWedge));
		info.AddProperty(PropertyName._upWedge, Variant.From(in _upWedge));
		info.AddProperty(PropertyName._upRightWedge, Variant.From(in _upRightWedge));
		info.AddProperty(PropertyName._marker, Variant.From(in _marker));
		info.AddProperty(PropertyName._ignoreNextMouseInput, Variant.From(in _ignoreNextMouseInput));
		info.AddProperty(PropertyName._centerPosition, Variant.From(in _centerPosition));
		info.AddProperty(PropertyName._selectedWedge, Variant.From(in _selectedWedge));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._rightWedge, out var value))
		{
			_rightWedge = value.As<NReactionWheelWedge>();
		}
		if (info.TryGetProperty(PropertyName._downRightWedge, out var value2))
		{
			_downRightWedge = value2.As<NReactionWheelWedge>();
		}
		if (info.TryGetProperty(PropertyName._downWedge, out var value3))
		{
			_downWedge = value3.As<NReactionWheelWedge>();
		}
		if (info.TryGetProperty(PropertyName._downLeftWedge, out var value4))
		{
			_downLeftWedge = value4.As<NReactionWheelWedge>();
		}
		if (info.TryGetProperty(PropertyName._leftWedge, out var value5))
		{
			_leftWedge = value5.As<NReactionWheelWedge>();
		}
		if (info.TryGetProperty(PropertyName._upLeftWedge, out var value6))
		{
			_upLeftWedge = value6.As<NReactionWheelWedge>();
		}
		if (info.TryGetProperty(PropertyName._upWedge, out var value7))
		{
			_upWedge = value7.As<NReactionWheelWedge>();
		}
		if (info.TryGetProperty(PropertyName._upRightWedge, out var value8))
		{
			_upRightWedge = value8.As<NReactionWheelWedge>();
		}
		if (info.TryGetProperty(PropertyName._marker, out var value9))
		{
			_marker = value9.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._ignoreNextMouseInput, out var value10))
		{
			_ignoreNextMouseInput = value10.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._centerPosition, out var value11))
		{
			_centerPosition = value11.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._selectedWedge, out var value12))
		{
			_selectedWedge = value12.As<NReactionWheelWedge>();
		}
	}
}
