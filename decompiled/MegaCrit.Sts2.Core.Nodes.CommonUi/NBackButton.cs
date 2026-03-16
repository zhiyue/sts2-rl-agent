using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace MegaCrit.Sts2.Core.Nodes.CommonUi;

[ScriptPath("res://src/Core/Nodes/CommonUi/NBackButton.cs")]
public class NBackButton : NButton
{
	public new class MethodName : NButton.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnWindowChange = "OnWindowChange";

		public static readonly StringName MoveToHidePosition = "MoveToHidePosition";

		public new static readonly StringName OnEnable = "OnEnable";

		public new static readonly StringName OnDisable = "OnDisable";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";

		public new static readonly StringName OnPress = "OnPress";
	}

	public new class PropertyName : NButton.PropertyName
	{
		public new static readonly StringName ClickedSfx = "ClickedSfx";

		public new static readonly StringName Hotkeys = "Hotkeys";

		public new static readonly StringName ControllerIconHotkey = "ControllerIconHotkey";

		public static readonly StringName _outline = "_outline";

		public static readonly StringName _buttonImage = "_buttonImage";

		public static readonly StringName _defaultOutlineColor = "_defaultOutlineColor";

		public static readonly StringName _hoveredOutlineColor = "_hoveredOutlineColor";

		public static readonly StringName _downColor = "_downColor";

		public static readonly StringName _outlineColor = "_outlineColor";

		public static readonly StringName _outlineTransparentColor = "_outlineTransparentColor";

		public static readonly StringName _posOffset = "_posOffset";

		public static readonly StringName _showPos = "_showPos";

		public static readonly StringName _hidePos = "_hidePos";

		public static readonly StringName _hoverTween = "_hoverTween";

		public static readonly StringName _moveTween = "_moveTween";
	}

	public new class SignalName : NButton.SignalName
	{
	}

	private Control _outline;

	private Control _buttonImage;

	private Color _defaultOutlineColor = StsColors.cream;

	private Color _hoveredOutlineColor = StsColors.gold;

	private Color _downColor = Colors.Gray;

	private Color _outlineColor = new Color("F0B400");

	private Color _outlineTransparentColor = new Color("FF000000");

	private static readonly Vector2 _hoverScale = Vector2.One * 1.05f;

	private static readonly Vector2 _downScale = Vector2.One;

	private const double _animInOutDur = 0.35;

	private Vector2 _posOffset;

	private Vector2 _showPos;

	private Vector2 _hidePos;

	private static readonly Vector2 _hideOffset = new Vector2(-180f, 0f);

	private Tween? _hoverTween;

	private Tween? _moveTween;

	protected override string ClickedSfx => "event:/sfx/ui/clicks/ui_back";

	protected override string[] Hotkeys => new string[3]
	{
		MegaInput.cancel,
		MegaInput.pauseAndBack,
		MegaInput.back
	};

	protected override string ControllerIconHotkey => MegaInput.cancel;

	public override void _Ready()
	{
		ConnectSignals();
		_outline = GetNode<Control>("Outline");
		_buttonImage = GetNode<Control>("Image");
		_isEnabled = false;
		_posOffset = new Vector2(base.OffsetLeft + 80f, 0f - base.OffsetBottom + 110f);
		GetTree().Root.Connect(Viewport.SignalName.SizeChanged, Callable.From(OnWindowChange));
		OnWindowChange();
		OnDisable();
	}

	private void OnWindowChange()
	{
		_showPos = new Vector2(0f, GetWindow().ContentScaleSize.Y) - _posOffset;
		_hidePos = _showPos + _hideOffset;
		base.Position = (_isEnabled ? _showPos : _hidePos);
	}

	public void MoveToHidePosition()
	{
		base.GlobalPosition = _hidePos;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		_isEnabled = true;
		_outline.Modulate = Colors.Transparent;
		_buttonImage.Modulate = Colors.White;
		base.Scale = Vector2.One;
		_moveTween?.Kill();
		_moveTween = CreateTween();
		_moveTween.TweenProperty(this, "global_position", _showPos, 0.35).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_isEnabled = false;
		_moveTween?.Kill();
		_moveTween = CreateTween();
		_moveTween.TweenProperty(this, "global_position", _hidePos, 0.35).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	protected override void OnFocus()
	{
		base.OnFocus();
		_hoverTween?.Kill();
		_hoverTween = CreateTween().SetParallel();
		_hoverTween.TweenProperty(this, "scale", _hoverScale, 0.05);
		_hoverTween.TweenProperty(_outline, "modulate", _outlineColor, 0.05);
	}

	protected override void OnUnfocus()
	{
		_hoverTween?.Kill();
		_hoverTween = CreateTween().SetParallel();
		_hoverTween.TweenProperty(this, "scale", _hoverScale, 0.5).SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.Out);
		_hoverTween.TweenProperty(_outline, "modulate", _outlineTransparentColor, 0.5);
	}

	protected override void OnPress()
	{
		base.OnPress();
		_hoverTween?.Kill();
		_hoverTween = CreateTween().SetParallel();
		_hoverTween.TweenProperty(this, "scale", _downScale, 0.25).SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.Out);
		_hoverTween.TweenProperty(_buttonImage, "modulate", _downColor, 0.25).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
		_hoverTween.TweenProperty(_outline, "modulate", _outlineTransparentColor, 0.25).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(8);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnWindowChange, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.MoveToHidePosition, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnEnable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnDisable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPress, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.OnWindowChange && args.Count == 0)
		{
			OnWindowChange();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.MoveToHidePosition && args.Count == 0)
		{
			MoveToHidePosition();
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
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.OnWindowChange)
		{
			return true;
		}
		if (method == MethodName.MoveToHidePosition)
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
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._outline)
		{
			_outline = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._buttonImage)
		{
			_buttonImage = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._defaultOutlineColor)
		{
			_defaultOutlineColor = VariantUtils.ConvertTo<Color>(in value);
			return true;
		}
		if (name == PropertyName._hoveredOutlineColor)
		{
			_hoveredOutlineColor = VariantUtils.ConvertTo<Color>(in value);
			return true;
		}
		if (name == PropertyName._downColor)
		{
			_downColor = VariantUtils.ConvertTo<Color>(in value);
			return true;
		}
		if (name == PropertyName._outlineColor)
		{
			_outlineColor = VariantUtils.ConvertTo<Color>(in value);
			return true;
		}
		if (name == PropertyName._outlineTransparentColor)
		{
			_outlineTransparentColor = VariantUtils.ConvertTo<Color>(in value);
			return true;
		}
		if (name == PropertyName._posOffset)
		{
			_posOffset = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._showPos)
		{
			_showPos = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._hidePos)
		{
			_hidePos = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._hoverTween)
		{
			_hoverTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._moveTween)
		{
			_moveTween = VariantUtils.ConvertTo<Tween>(in value);
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
		if (name == PropertyName._outline)
		{
			value = VariantUtils.CreateFrom(in _outline);
			return true;
		}
		if (name == PropertyName._buttonImage)
		{
			value = VariantUtils.CreateFrom(in _buttonImage);
			return true;
		}
		if (name == PropertyName._defaultOutlineColor)
		{
			value = VariantUtils.CreateFrom(in _defaultOutlineColor);
			return true;
		}
		if (name == PropertyName._hoveredOutlineColor)
		{
			value = VariantUtils.CreateFrom(in _hoveredOutlineColor);
			return true;
		}
		if (name == PropertyName._downColor)
		{
			value = VariantUtils.CreateFrom(in _downColor);
			return true;
		}
		if (name == PropertyName._outlineColor)
		{
			value = VariantUtils.CreateFrom(in _outlineColor);
			return true;
		}
		if (name == PropertyName._outlineTransparentColor)
		{
			value = VariantUtils.CreateFrom(in _outlineTransparentColor);
			return true;
		}
		if (name == PropertyName._posOffset)
		{
			value = VariantUtils.CreateFrom(in _posOffset);
			return true;
		}
		if (name == PropertyName._showPos)
		{
			value = VariantUtils.CreateFrom(in _showPos);
			return true;
		}
		if (name == PropertyName._hidePos)
		{
			value = VariantUtils.CreateFrom(in _hidePos);
			return true;
		}
		if (name == PropertyName._hoverTween)
		{
			value = VariantUtils.CreateFrom(in _hoverTween);
			return true;
		}
		if (name == PropertyName._moveTween)
		{
			value = VariantUtils.CreateFrom(in _moveTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName.ClickedSfx, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.PackedStringArray, PropertyName.Hotkeys, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName.ControllerIconHotkey, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._outline, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._buttonImage, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName._defaultOutlineColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName._hoveredOutlineColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName._downColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName._outlineColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName._outlineTransparentColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._posOffset, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._showPos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._hidePos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hoverTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._moveTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._outline, Variant.From(in _outline));
		info.AddProperty(PropertyName._buttonImage, Variant.From(in _buttonImage));
		info.AddProperty(PropertyName._defaultOutlineColor, Variant.From(in _defaultOutlineColor));
		info.AddProperty(PropertyName._hoveredOutlineColor, Variant.From(in _hoveredOutlineColor));
		info.AddProperty(PropertyName._downColor, Variant.From(in _downColor));
		info.AddProperty(PropertyName._outlineColor, Variant.From(in _outlineColor));
		info.AddProperty(PropertyName._outlineTransparentColor, Variant.From(in _outlineTransparentColor));
		info.AddProperty(PropertyName._posOffset, Variant.From(in _posOffset));
		info.AddProperty(PropertyName._showPos, Variant.From(in _showPos));
		info.AddProperty(PropertyName._hidePos, Variant.From(in _hidePos));
		info.AddProperty(PropertyName._hoverTween, Variant.From(in _hoverTween));
		info.AddProperty(PropertyName._moveTween, Variant.From(in _moveTween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._outline, out var value))
		{
			_outline = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._buttonImage, out var value2))
		{
			_buttonImage = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._defaultOutlineColor, out var value3))
		{
			_defaultOutlineColor = value3.As<Color>();
		}
		if (info.TryGetProperty(PropertyName._hoveredOutlineColor, out var value4))
		{
			_hoveredOutlineColor = value4.As<Color>();
		}
		if (info.TryGetProperty(PropertyName._downColor, out var value5))
		{
			_downColor = value5.As<Color>();
		}
		if (info.TryGetProperty(PropertyName._outlineColor, out var value6))
		{
			_outlineColor = value6.As<Color>();
		}
		if (info.TryGetProperty(PropertyName._outlineTransparentColor, out var value7))
		{
			_outlineTransparentColor = value7.As<Color>();
		}
		if (info.TryGetProperty(PropertyName._posOffset, out var value8))
		{
			_posOffset = value8.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._showPos, out var value9))
		{
			_showPos = value9.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._hidePos, out var value10))
		{
			_hidePos = value10.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._hoverTween, out var value11))
		{
			_hoverTween = value11.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._moveTween, out var value12))
		{
			_moveTween = value12.As<Tween>();
		}
	}
}
