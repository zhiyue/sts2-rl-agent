using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace MegaCrit.Sts2.Core.Nodes.CommonUi;

[ScriptPath("res://src/Core/Nodes/CommonUi/NGoldArrowButton.cs")]
public class NGoldArrowButton : NButton
{
	public new class MethodName : NButton.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";

		public new static readonly StringName OnPress = "OnPress";

		public new static readonly StringName OnRelease = "OnRelease";

		public static readonly StringName UpdateShaderParam = "UpdateShaderParam";
	}

	public new class PropertyName : NButton.PropertyName
	{
		public static readonly StringName _icon = "_icon";

		public static readonly StringName _hsv = "_hsv";

		public static readonly StringName _animTween = "_animTween";

		public static readonly StringName _valueDefault = "_valueDefault";

		public static readonly StringName _valueHovered = "_valueHovered";

		public static readonly StringName _hoverScale = "_hoverScale";
	}

	public new class SignalName : NButton.SignalName
	{
	}

	private static readonly StringName _v = new StringName("v");

	protected TextureRect _icon;

	private ShaderMaterial _hsv;

	private Tween? _animTween;

	private float _valueDefault = 0.9f;

	private float _valueHovered = 1.2f;

	private Vector2 _hoverScale = Vector2.One * 1.1f;

	public override void _Ready()
	{
		ConnectSignals();
		_icon = GetNode<TextureRect>("TextureRect");
		_hsv = (ShaderMaterial)_icon.Material;
		_hsv.SetShaderParameter(_v, _valueDefault);
	}

	protected override void OnFocus()
	{
		base.OnFocus();
		_animTween?.Kill();
		_hsv.SetShaderParameter(_v, _valueHovered);
		_icon.Scale = _hoverScale;
	}

	protected override void OnUnfocus()
	{
		base.OnUnfocus();
		_animTween?.Kill();
		_animTween = CreateTween().SetParallel();
		_animTween.TweenMethod(Callable.From<float>(UpdateShaderParam), _valueHovered, _valueDefault, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_animTween.TweenProperty(_icon, "scale", Vector2.One, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	protected override void OnPress()
	{
		base.OnPress();
		_animTween?.Kill();
		_animTween = CreateTween();
		_hsv.SetShaderParameter(_v, 0.7f);
		_animTween.TweenProperty(_icon, "scale", Vector2.One, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	protected override void OnRelease()
	{
		if (base.IsFocused)
		{
			_animTween?.Kill();
			_hsv.SetShaderParameter(_v, _valueHovered);
			_icon.Scale = _hoverScale;
		}
	}

	private void UpdateShaderParam(float newV)
	{
		_hsv.SetShaderParameter(_v, newV);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(6);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPress, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnRelease, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateShaderParam, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "newV", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.UpdateShaderParam && args.Count == 1)
		{
			UpdateShaderParam(VariantUtils.ConvertTo<float>(in args[0]));
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
		if (method == MethodName.UpdateShaderParam)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._icon)
		{
			_icon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._hsv)
		{
			_hsv = VariantUtils.ConvertTo<ShaderMaterial>(in value);
			return true;
		}
		if (name == PropertyName._animTween)
		{
			_animTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._valueDefault)
		{
			_valueDefault = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._valueHovered)
		{
			_valueHovered = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._hoverScale)
		{
			_hoverScale = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._icon)
		{
			value = VariantUtils.CreateFrom(in _icon);
			return true;
		}
		if (name == PropertyName._hsv)
		{
			value = VariantUtils.CreateFrom(in _hsv);
			return true;
		}
		if (name == PropertyName._animTween)
		{
			value = VariantUtils.CreateFrom(in _animTween);
			return true;
		}
		if (name == PropertyName._valueDefault)
		{
			value = VariantUtils.CreateFrom(in _valueDefault);
			return true;
		}
		if (name == PropertyName._valueHovered)
		{
			value = VariantUtils.CreateFrom(in _valueHovered);
			return true;
		}
		if (name == PropertyName._hoverScale)
		{
			value = VariantUtils.CreateFrom(in _hoverScale);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._icon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hsv, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._animTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._valueDefault, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._valueHovered, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._hoverScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._icon, Variant.From(in _icon));
		info.AddProperty(PropertyName._hsv, Variant.From(in _hsv));
		info.AddProperty(PropertyName._animTween, Variant.From(in _animTween));
		info.AddProperty(PropertyName._valueDefault, Variant.From(in _valueDefault));
		info.AddProperty(PropertyName._valueHovered, Variant.From(in _valueHovered));
		info.AddProperty(PropertyName._hoverScale, Variant.From(in _hoverScale));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._icon, out var value))
		{
			_icon = value.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._hsv, out var value2))
		{
			_hsv = value2.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._animTween, out var value3))
		{
			_animTween = value3.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._valueDefault, out var value4))
		{
			_valueDefault = value4.As<float>();
		}
		if (info.TryGetProperty(PropertyName._valueHovered, out var value5))
		{
			_valueHovered = value5.As<float>();
		}
		if (info.TryGetProperty(PropertyName._hoverScale, out var value6))
		{
			_hoverScale = value6.As<Vector2>();
		}
	}
}
