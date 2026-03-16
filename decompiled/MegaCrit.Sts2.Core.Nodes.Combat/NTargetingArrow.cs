using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace MegaCrit.Sts2.Core.Nodes.Combat;

[ScriptPath("res://src/Core/Nodes/Combat/NTargetingArrow.cs")]
public class NTargetingArrow : Node2D
{
	public new class MethodName : Node2D.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _Process = "_Process";

		public static readonly StringName UpdateArrowPosition = "UpdateArrowPosition";

		public static readonly StringName SetHighlightingOn = "SetHighlightingOn";

		public static readonly StringName SetHighlightingOff = "SetHighlightingOff";

		public static readonly StringName UpdateSegments = "UpdateSegments";

		public static readonly StringName StartDrawingFrom = "StartDrawingFrom";

		public static readonly StringName StopDrawing = "StopDrawing";

		public static readonly StringName UpdateDrawingTo = "UpdateDrawingTo";
	}

	public new class PropertyName : Node2D.PropertyName
	{
		public static readonly StringName From = "From";

		public static readonly StringName _fromPos = "_fromPos";

		public static readonly StringName _fromControl = "_fromControl";

		public static readonly StringName _toPosition = "_toPosition";

		public static readonly StringName _segments = "_segments";

		public static readonly StringName _arrowHead = "_arrowHead";

		public static readonly StringName _arrowHeadTween = "_arrowHeadTween";

		public static readonly StringName _initialized = "_initialized";

		public static readonly StringName _followMouse = "_followMouse";
	}

	public new class SignalName : Node2D.SignalName
	{
	}

	private const int _segmentCount = 19;

	private Vector2 _fromPos;

	private Control? _fromControl;

	private Vector2 _toPosition;

	private Vector2? _currentArrowPos;

	private static readonly string _segmentHeadPath = ImageHelper.GetImagePath("ui/combat/targeting_arrow_head.png");

	private static readonly string _segmentBlockPath = ImageHelper.GetImagePath("ui/combat/targeting_arrow_segment.png");

	private Sprite2D[] _segments = new Sprite2D[19];

	private Sprite2D _arrowHead;

	private Tween? _arrowHeadTween;

	private bool _initialized;

	private bool _followMouse;

	private const float _segmentScaleStart = 0.28f;

	private const float _segmentScaleEnd = 0.42f;

	private static readonly Vector2 _arrowHeadDefaultScale = Vector2.One * 0.95f;

	private static readonly Vector2 _arrowHeadHoverScale = Vector2.One * 1.05f;

	private Vector2 From => _fromControl?.GlobalPosition ?? _fromPos;

	private static Texture2D SegmentHead => PreloadManager.Cache.GetTexture2D(_segmentHeadPath);

	private static Texture2D SegmentBlock => PreloadManager.Cache.GetTexture2D(_segmentBlockPath);

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { _segmentHeadPath, _segmentBlockPath });

	public override void _Ready()
	{
		if (!_initialized)
		{
			_initialized = true;
			for (int i = 0; i < 19; i++)
			{
				_segments[i] = new Sprite2D();
				_segments[i].Texture = SegmentBlock;
				this.AddChildSafely(_segments[i]);
			}
			_arrowHead = new Sprite2D();
			_arrowHead.Texture = SegmentHead;
			this.AddChildSafely(_arrowHead);
			StopDrawing();
		}
	}

	public override void _Process(double delta)
	{
		if (base.Visible)
		{
			if (_followMouse)
			{
				UpdateDrawingTo(GetViewport().GetMousePosition());
				UpdateArrowPosition(_toPosition);
			}
			else
			{
				_currentArrowPos = _currentArrowPos.Value.Lerp(_toPosition, (float)delta * 14f);
				UpdateArrowPosition(_currentArrowPos.Value);
			}
		}
	}

	private void UpdateArrowPosition(Vector2 targetPos)
	{
		_arrowHead.Position = targetPos + new Vector2(0f, 88f).Rotated(_arrowHead.Rotation);
		Vector2 finalPos = targetPos + new Vector2(0f, 40f).Rotated(_arrowHead.Rotation);
		Vector2 zero = Vector2.Zero;
		zero.X = From.X - (_arrowHead.Position.X - From.X) * 0.25f;
		if (From.Y > 540f)
		{
			zero.Y = _arrowHead.Position.Y + (_arrowHead.Position.Y - From.Y) * 0.5f;
		}
		else
		{
			zero.Y = _arrowHead.Position.Y * 0.75f + From.Y * 0.25f;
		}
		_arrowHead.Rotation = (targetPos - zero).Angle() + (float)Math.PI / 2f;
		UpdateSegments(From, finalPos, zero);
	}

	public void SetHighlightingOn(bool isEnemy)
	{
		_arrowHeadTween?.Kill();
		_arrowHeadTween = CreateTween();
		_arrowHeadTween.TweenProperty(_arrowHead, "scale", _arrowHeadHoverScale, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Elastic);
		base.Modulate = (isEnemy ? StsColors.targetingArrowEnemy : StsColors.targetingArrowAlly);
	}

	public void SetHighlightingOff()
	{
		_arrowHeadTween?.Kill();
		_arrowHead.Scale = _arrowHeadDefaultScale;
		base.Modulate = Colors.White;
	}

	private void UpdateSegments(Vector2 initialPos, Vector2 finalPos, Vector2 controlPoint)
	{
		for (int i = 0; i < 19; i++)
		{
			_segments[i].Scale = Vector2.One * Mathf.Lerp(0.28f, 0.42f, (float)i * 2f / 19f);
			_segments[i].Position = MathHelper.BezierCurve(initialPos, finalPos, controlPoint, (float)i / 20f);
			if (i == 0)
			{
				_segments[i].Rotation = ((_segments[i].GlobalPosition.Y > 540f) ? 0f : ((float)Math.PI));
			}
			else
			{
				_segments[i].Rotation = (_segments[i].Position - _segments[i - 1].Position).Angle() + (float)Math.PI / 2f;
			}
		}
		_segments[0].Rotation = (_segments[0].Position - _segments[1].Position).Angle() - (float)Math.PI / 2f;
	}

	public void StartDrawingFrom(Vector2 from, bool usingController)
	{
		_followMouse = !usingController;
		if (!NControllerManager.Instance.IsUsingController)
		{
			Input.MouseMode = Input.MouseModeEnum.Hidden;
		}
		_fromPos = from;
		base.Visible = !NCombatUi.IsDebugHideTargetingUi;
	}

	public void StartDrawingFrom(Control control, bool usingController)
	{
		_followMouse = !usingController;
		base.ZIndex = control.ZIndex + 1;
		Input.MouseMode = Input.MouseModeEnum.Hidden;
		_fromControl = control;
		base.Visible = !NCombatUi.IsDebugHideTargetingUi;
	}

	public void StopDrawing()
	{
		if (!NControllerManager.Instance.IsUsingController)
		{
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}
		_fromControl = null;
		_currentArrowPos = null;
		base.Visible = false;
		SetHighlightingOff();
	}

	public void UpdateDrawingTo(Vector2 position)
	{
		_toPosition = position;
		if (!_followMouse && !_currentArrowPos.HasValue)
		{
			_currentArrowPos = _toPosition;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(9);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateArrowPosition, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "targetPos", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetHighlightingOn, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "isEnemy", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetHighlightingOff, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateSegments, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "initialPos", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Vector2, "finalPos", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Vector2, "controlPoint", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.StartDrawingFrom, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "from", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Bool, "usingController", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.StopDrawing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateDrawingTo, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "position", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateArrowPosition && args.Count == 1)
		{
			UpdateArrowPosition(VariantUtils.ConvertTo<Vector2>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetHighlightingOn && args.Count == 1)
		{
			SetHighlightingOn(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetHighlightingOff && args.Count == 0)
		{
			SetHighlightingOff();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateSegments && args.Count == 3)
		{
			UpdateSegments(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<Vector2>(in args[1]), VariantUtils.ConvertTo<Vector2>(in args[2]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartDrawingFrom && args.Count == 2)
		{
			StartDrawingFrom(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StopDrawing && args.Count == 0)
		{
			StopDrawing();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateDrawingTo && args.Count == 1)
		{
			UpdateDrawingTo(VariantUtils.ConvertTo<Vector2>(in args[0]));
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
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName.UpdateArrowPosition)
		{
			return true;
		}
		if (method == MethodName.SetHighlightingOn)
		{
			return true;
		}
		if (method == MethodName.SetHighlightingOff)
		{
			return true;
		}
		if (method == MethodName.UpdateSegments)
		{
			return true;
		}
		if (method == MethodName.StartDrawingFrom)
		{
			return true;
		}
		if (method == MethodName.StopDrawing)
		{
			return true;
		}
		if (method == MethodName.UpdateDrawingTo)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._fromPos)
		{
			_fromPos = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._fromControl)
		{
			_fromControl = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._toPosition)
		{
			_toPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._segments)
		{
			_segments = VariantUtils.ConvertToSystemArrayOfGodotObject<Sprite2D>(in value);
			return true;
		}
		if (name == PropertyName._arrowHead)
		{
			_arrowHead = VariantUtils.ConvertTo<Sprite2D>(in value);
			return true;
		}
		if (name == PropertyName._arrowHeadTween)
		{
			_arrowHeadTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._initialized)
		{
			_initialized = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._followMouse)
		{
			_followMouse = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.From)
		{
			value = VariantUtils.CreateFrom<Vector2>(From);
			return true;
		}
		if (name == PropertyName._fromPos)
		{
			value = VariantUtils.CreateFrom(in _fromPos);
			return true;
		}
		if (name == PropertyName._fromControl)
		{
			value = VariantUtils.CreateFrom(in _fromControl);
			return true;
		}
		if (name == PropertyName._toPosition)
		{
			value = VariantUtils.CreateFrom(in _toPosition);
			return true;
		}
		if (name == PropertyName._segments)
		{
			GodotObject[] segments = _segments;
			value = VariantUtils.CreateFromSystemArrayOfGodotObject(segments);
			return true;
		}
		if (name == PropertyName._arrowHead)
		{
			value = VariantUtils.CreateFrom(in _arrowHead);
			return true;
		}
		if (name == PropertyName._arrowHeadTween)
		{
			value = VariantUtils.CreateFrom(in _arrowHeadTween);
			return true;
		}
		if (name == PropertyName._initialized)
		{
			value = VariantUtils.CreateFrom(in _initialized);
			return true;
		}
		if (name == PropertyName._followMouse)
		{
			value = VariantUtils.CreateFrom(in _followMouse);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._fromPos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._fromControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._toPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName.From, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._segments, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._arrowHead, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._arrowHeadTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._initialized, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._followMouse, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._fromPos, Variant.From(in _fromPos));
		info.AddProperty(PropertyName._fromControl, Variant.From(in _fromControl));
		info.AddProperty(PropertyName._toPosition, Variant.From(in _toPosition));
		StringName segments = PropertyName._segments;
		GodotObject[] segments2 = _segments;
		info.AddProperty(segments, Variant.CreateFrom(segments2));
		info.AddProperty(PropertyName._arrowHead, Variant.From(in _arrowHead));
		info.AddProperty(PropertyName._arrowHeadTween, Variant.From(in _arrowHeadTween));
		info.AddProperty(PropertyName._initialized, Variant.From(in _initialized));
		info.AddProperty(PropertyName._followMouse, Variant.From(in _followMouse));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._fromPos, out var value))
		{
			_fromPos = value.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._fromControl, out var value2))
		{
			_fromControl = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._toPosition, out var value3))
		{
			_toPosition = value3.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._segments, out var value4))
		{
			_segments = value4.AsGodotObjectArray<Sprite2D>();
		}
		if (info.TryGetProperty(PropertyName._arrowHead, out var value5))
		{
			_arrowHead = value5.As<Sprite2D>();
		}
		if (info.TryGetProperty(PropertyName._arrowHeadTween, out var value6))
		{
			_arrowHeadTween = value6.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._initialized, out var value7))
		{
			_initialized = value7.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._followMouse, out var value8))
		{
			_followMouse = value8.As<bool>();
		}
	}
}
