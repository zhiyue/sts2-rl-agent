using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Map;

[ScriptPath("res://src/Core/Nodes/Screens/Map/NControllerMapDrawingInput.cs")]
public class NControllerMapDrawingInput : NMapDrawingInput
{
	public new class MethodName : NMapDrawingInput.MethodName
	{
		public new static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _Process = "_Process";

		public new static readonly StringName _Input = "_Input";
	}

	public new class PropertyName : NMapDrawingInput.PropertyName
	{
		public static readonly StringName _eraserIconPos = "_eraserIconPos";

		public static readonly StringName _drawingIconPos = "_drawingIconPos";

		public static readonly StringName _isPressed = "_isPressed";

		public static readonly StringName _cursorTex = "_cursorTex";

		public static readonly StringName _cursorTiltedTex = "_cursorTiltedTex";

		public static readonly StringName _cursor = "_cursor";

		public static readonly StringName _direction = "_direction";
	}

	public new class SignalName : NMapDrawingInput.SignalName
	{
	}

	private const string _scenePath = "res://scenes/screens/map/controller_map_drawing_input.tscn";

	private Vector2 _eraserIconPos = new Vector2(-34f, -76f);

	private Vector2 _drawingIconPos = new Vector2(-10f, -76f);

	private bool _isPressed;

	private Texture2D _cursorTex;

	private Texture2D _cursorTiltedTex;

	private Control _cursor;

	private Vector2 _direction;

	public static NMapDrawingInput Create()
	{
		return PreloadManager.Cache.GetScene("res://scenes/screens/map/controller_map_drawing_input.tscn").Instantiate<NControllerMapDrawingInput>(PackedScene.GenEditState.Disabled);
	}

	public override void _Ready()
	{
		base._Ready();
		_cursor = GetNode<Control>("%Cursor");
		this.TryGrabFocus();
		GetViewport().Connect(Viewport.SignalName.GuiFocusChanged, Callable.From<Control>(delegate
		{
			StopDrawing();
		}));
		if (base.DrawingMode == DrawingMode.Drawing)
		{
			_cursorTex = ImageTexture.CreateFromImage(PreloadManager.Cache.GetAsset<Image>("res://images/packed/common_ui/cursor_quill.png"));
			_cursorTiltedTex = ImageTexture.CreateFromImage(PreloadManager.Cache.GetAsset<Image>("res://images/packed/common_ui/cursor_quill_tilted.png"));
		}
		else
		{
			_cursorTex = ImageTexture.CreateFromImage(PreloadManager.Cache.GetAsset<Image>("res://images/packed/common_ui/cursor_eraser.png"));
			_cursorTiltedTex = ImageTexture.CreateFromImage(PreloadManager.Cache.GetAsset<Image>("res://images/packed/common_ui/cursor_eraser_tilted.png"));
		}
		_cursor.GetNode<TextureRect>("TextureRect").Texture = _cursorTex;
		_cursor.GetNode<TextureRect>("TextureRect").Position = ((base.DrawingMode == DrawingMode.Drawing) ? _drawingIconPos : _eraserIconPos);
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionPressed(MegaInput.select))
		{
			if (!_isPressed)
			{
				_drawings.BeginLineLocal(_drawings.GetGlobalTransform().Inverse() * _cursor.GlobalPosition, null);
				_cursor.GetNode<TextureRect>("TextureRect").Texture = _cursorTiltedTex;
				_isPressed = true;
			}
		}
		else if (_isPressed)
		{
			_drawings.StopLineLocal();
			_cursor.GetNode<TextureRect>("TextureRect").Texture = _cursorTex;
			_isPressed = false;
		}
		_direction = Input.GetVector(Controller.joystickLeft, Controller.joystickRight, Controller.joystickUp, Controller.joystickDown);
		if (_direction.Length() < 0.1f)
		{
			_direction += Input.GetVector(Controller.dPadWest, Controller.dPadEast, Controller.dPadNorth, Controller.dPadSouth);
		}
		if (_direction.Length() > 0f)
		{
			_cursor.GlobalPosition += _direction * 700f * (float)delta;
			_cursor.GlobalPosition = _cursor.GlobalPosition.Clamp(NGame.Instance.GlobalPosition, NGame.Instance.GlobalPosition + NGame.Instance.Size);
			if (_drawings.IsLocalDrawing())
			{
				_drawings.UpdateCurrentLinePositionLocal(_drawings.GetGlobalTransform().Inverse() * _cursor.GlobalPosition);
			}
		}
	}

	public override void _Input(InputEvent input)
	{
		if (IsVisibleInTree() && input.IsActionPressed(MegaInput.cancel))
		{
			StopDrawing();
			ActiveScreenContext.Instance.FocusOnDefaultControl();
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "input", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NMapDrawingInput>(Create());
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
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
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NMapDrawingInput>(Create());
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.Create)
		{
			return true;
		}
		if (method == MethodName._Ready)
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
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._eraserIconPos)
		{
			_eraserIconPos = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._drawingIconPos)
		{
			_drawingIconPos = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._isPressed)
		{
			_isPressed = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._cursorTex)
		{
			_cursorTex = VariantUtils.ConvertTo<Texture2D>(in value);
			return true;
		}
		if (name == PropertyName._cursorTiltedTex)
		{
			_cursorTiltedTex = VariantUtils.ConvertTo<Texture2D>(in value);
			return true;
		}
		if (name == PropertyName._cursor)
		{
			_cursor = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._direction)
		{
			_direction = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._eraserIconPos)
		{
			value = VariantUtils.CreateFrom(in _eraserIconPos);
			return true;
		}
		if (name == PropertyName._drawingIconPos)
		{
			value = VariantUtils.CreateFrom(in _drawingIconPos);
			return true;
		}
		if (name == PropertyName._isPressed)
		{
			value = VariantUtils.CreateFrom(in _isPressed);
			return true;
		}
		if (name == PropertyName._cursorTex)
		{
			value = VariantUtils.CreateFrom(in _cursorTex);
			return true;
		}
		if (name == PropertyName._cursorTiltedTex)
		{
			value = VariantUtils.CreateFrom(in _cursorTiltedTex);
			return true;
		}
		if (name == PropertyName._cursor)
		{
			value = VariantUtils.CreateFrom(in _cursor);
			return true;
		}
		if (name == PropertyName._direction)
		{
			value = VariantUtils.CreateFrom(in _direction);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._eraserIconPos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._drawingIconPos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isPressed, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cursorTex, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cursorTiltedTex, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cursor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._direction, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._eraserIconPos, Variant.From(in _eraserIconPos));
		info.AddProperty(PropertyName._drawingIconPos, Variant.From(in _drawingIconPos));
		info.AddProperty(PropertyName._isPressed, Variant.From(in _isPressed));
		info.AddProperty(PropertyName._cursorTex, Variant.From(in _cursorTex));
		info.AddProperty(PropertyName._cursorTiltedTex, Variant.From(in _cursorTiltedTex));
		info.AddProperty(PropertyName._cursor, Variant.From(in _cursor));
		info.AddProperty(PropertyName._direction, Variant.From(in _direction));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._eraserIconPos, out var value))
		{
			_eraserIconPos = value.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._drawingIconPos, out var value2))
		{
			_drawingIconPos = value2.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._isPressed, out var value3))
		{
			_isPressed = value3.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._cursorTex, out var value4))
		{
			_cursorTex = value4.As<Texture2D>();
		}
		if (info.TryGetProperty(PropertyName._cursorTiltedTex, out var value5))
		{
			_cursorTiltedTex = value5.As<Texture2D>();
		}
		if (info.TryGetProperty(PropertyName._cursor, out var value6))
		{
			_cursor = value6.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._direction, out var value7))
		{
			_direction = value7.As<Vector2>();
		}
	}
}
