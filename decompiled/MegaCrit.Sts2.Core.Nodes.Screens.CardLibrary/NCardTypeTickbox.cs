using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;

namespace MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;

[ScriptPath("res://src/Core/Nodes/Screens/CardLibrary/NCardTypeTickbox.cs")]
public class NCardTypeTickbox : NButton
{
	[Signal]
	public delegate void ToggledEventHandler(NCardTypeTickbox tickbox);

	public new class MethodName : NButton.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnToggle = "OnToggle";

		public new static readonly StringName OnRelease = "OnRelease";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";

		public new static readonly StringName OnPress = "OnPress";

		public static readonly StringName UpdateShaderS = "UpdateShaderS";

		public static readonly StringName UpdateShaderV = "UpdateShaderV";
	}

	public new class PropertyName : NButton.PropertyName
	{
		public static readonly StringName IsTicked = "IsTicked";

		public static readonly StringName _isTicked = "_isTicked";

		public static readonly StringName _baseS = "_baseS";

		public static readonly StringName _baseV = "_baseV";

		public static readonly StringName _outline = "_outline";

		public static readonly StringName _image = "_image";

		public static readonly StringName _hsv = "_hsv";

		public static readonly StringName _tween = "_tween";

		public static readonly StringName _baseScale = "_baseScale";

		public static readonly StringName _hoverScale = "_hoverScale";

		public static readonly StringName _pressDownScale = "_pressDownScale";
	}

	public new class SignalName : NButton.SignalName
	{
		public static readonly StringName Toggled = "Toggled";
	}

	private static readonly StringName _v = new StringName("v");

	private static readonly StringName _s = new StringName("s");

	private bool _isTicked = true;

	private float _baseS = 1f;

	private float _baseV = 1.2f;

	private Control _outline;

	private Control _image;

	private ShaderMaterial _hsv;

	private Tween? _tween;

	private Vector2 _baseScale;

	private float _hoverScale = 1.2f;

	private float _pressDownScale = 0.9f;

	private ToggledEventHandler backing_Toggled;

	public bool IsTicked
	{
		get
		{
			return _isTicked;
		}
		set
		{
			_isTicked = value;
			_outline.Visible = _isTicked;
			OnToggle();
		}
	}

	public LocString Loc { get; set; }

	public event ToggledEventHandler Toggled
	{
		add
		{
			backing_Toggled = (ToggledEventHandler)Delegate.Combine(backing_Toggled, value);
		}
		remove
		{
			backing_Toggled = (ToggledEventHandler)Delegate.Remove(backing_Toggled, value);
		}
	}

	public override void _Ready()
	{
		ConnectSignals();
		_baseScale = base.Scale;
		_image = GetNode<Control>("%Image");
		_outline = GetNode<Control>("%Outline");
		_hsv = (ShaderMaterial)_image.Material;
	}

	private void OnToggle()
	{
		_baseS = (_isTicked ? 1f : 0.65f);
		_baseV = (_isTicked ? 1.2f : 0.7f);
		UpdateShaderS(_baseS);
		UpdateShaderV(_baseV);
	}

	protected override void OnRelease()
	{
		base.OnRelease();
		IsTicked = !IsTicked;
		EmitSignal(SignalName.Toggled, this);
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_image, "scale", _baseScale * _hoverScale, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
		_tween.TweenMethod(Callable.From<float>(UpdateShaderV), _hsv.GetShaderParameter(_s), _baseS, 0.05);
		_tween.TweenMethod(Callable.From<float>(UpdateShaderS), _hsv.GetShaderParameter(_v), _baseV, 0.05);
	}

	protected override void OnFocus()
	{
		base.OnFocus();
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_image, "scale", _baseScale * _hoverScale, 0.05);
		_tween.TweenMethod(Callable.From<float>(UpdateShaderS), _hsv.GetShaderParameter(_s), _isTicked ? 1.4f : 1f, 0.05);
		_tween.TweenMethod(Callable.From<float>(UpdateShaderV), _hsv.GetShaderParameter(_v), _isTicked ? 1.4f : 1f, 0.05);
		NHoverTipSet nHoverTipSet = NHoverTipSet.CreateAndShow(this, new HoverTip(Loc));
		nHoverTipSet.GlobalPosition = new Vector2(310f, base.GlobalPosition.Y);
	}

	protected override void OnUnfocus()
	{
		base.OnUnfocus();
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_image, "scale", _baseScale, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_tween.TweenMethod(Callable.From<float>(UpdateShaderS), _hsv.GetShaderParameter(_s), _baseS, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_tween.TweenMethod(Callable.From<float>(UpdateShaderV), _hsv.GetShaderParameter(_v), _baseV, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		NHoverTipSet.Remove(this);
	}

	protected override void OnPress()
	{
		base.OnPress();
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_image, "scale", _baseScale * _pressDownScale, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_tween.TweenMethod(Callable.From<float>(UpdateShaderS), _hsv.GetShaderParameter(_s), _baseS, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_tween.TweenMethod(Callable.From<float>(UpdateShaderV), _hsv.GetShaderParameter(_v), _baseV, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	private void UpdateShaderS(float value)
	{
		_hsv.SetShaderParameter(_s, value);
	}

	private void UpdateShaderV(float value)
	{
		_hsv.SetShaderParameter(_v, value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(8);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnToggle, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnRelease, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPress, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateShaderS, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "value", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateShaderV, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "value", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.OnToggle && args.Count == 0)
		{
			OnToggle();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnRelease && args.Count == 0)
		{
			OnRelease();
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
		if (method == MethodName.UpdateShaderS && args.Count == 1)
		{
			UpdateShaderS(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateShaderV && args.Count == 1)
		{
			UpdateShaderV(VariantUtils.ConvertTo<float>(in args[0]));
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
		if (method == MethodName.OnToggle)
		{
			return true;
		}
		if (method == MethodName.OnRelease)
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
		if (method == MethodName.UpdateShaderS)
		{
			return true;
		}
		if (method == MethodName.UpdateShaderV)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.IsTicked)
		{
			IsTicked = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._isTicked)
		{
			_isTicked = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._baseS)
		{
			_baseS = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._baseV)
		{
			_baseV = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._outline)
		{
			_outline = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._image)
		{
			_image = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._hsv)
		{
			_hsv = VariantUtils.ConvertTo<ShaderMaterial>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._baseScale)
		{
			_baseScale = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._hoverScale)
		{
			_hoverScale = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._pressDownScale)
		{
			_pressDownScale = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.IsTicked)
		{
			value = VariantUtils.CreateFrom<bool>(IsTicked);
			return true;
		}
		if (name == PropertyName._isTicked)
		{
			value = VariantUtils.CreateFrom(in _isTicked);
			return true;
		}
		if (name == PropertyName._baseS)
		{
			value = VariantUtils.CreateFrom(in _baseS);
			return true;
		}
		if (name == PropertyName._baseV)
		{
			value = VariantUtils.CreateFrom(in _baseV);
			return true;
		}
		if (name == PropertyName._outline)
		{
			value = VariantUtils.CreateFrom(in _outline);
			return true;
		}
		if (name == PropertyName._image)
		{
			value = VariantUtils.CreateFrom(in _image);
			return true;
		}
		if (name == PropertyName._hsv)
		{
			value = VariantUtils.CreateFrom(in _hsv);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._baseScale)
		{
			value = VariantUtils.CreateFrom(in _baseScale);
			return true;
		}
		if (name == PropertyName._hoverScale)
		{
			value = VariantUtils.CreateFrom(in _hoverScale);
			return true;
		}
		if (name == PropertyName._pressDownScale)
		{
			value = VariantUtils.CreateFrom(in _pressDownScale);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isTicked, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._baseS, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._baseV, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsTicked, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._outline, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._image, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hsv, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._baseScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._hoverScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._pressDownScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.IsTicked, Variant.From<bool>(IsTicked));
		info.AddProperty(PropertyName._isTicked, Variant.From(in _isTicked));
		info.AddProperty(PropertyName._baseS, Variant.From(in _baseS));
		info.AddProperty(PropertyName._baseV, Variant.From(in _baseV));
		info.AddProperty(PropertyName._outline, Variant.From(in _outline));
		info.AddProperty(PropertyName._image, Variant.From(in _image));
		info.AddProperty(PropertyName._hsv, Variant.From(in _hsv));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._baseScale, Variant.From(in _baseScale));
		info.AddProperty(PropertyName._hoverScale, Variant.From(in _hoverScale));
		info.AddProperty(PropertyName._pressDownScale, Variant.From(in _pressDownScale));
		info.AddSignalEventDelegate(SignalName.Toggled, backing_Toggled);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.IsTicked, out var value))
		{
			IsTicked = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._isTicked, out var value2))
		{
			_isTicked = value2.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._baseS, out var value3))
		{
			_baseS = value3.As<float>();
		}
		if (info.TryGetProperty(PropertyName._baseV, out var value4))
		{
			_baseV = value4.As<float>();
		}
		if (info.TryGetProperty(PropertyName._outline, out var value5))
		{
			_outline = value5.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._image, out var value6))
		{
			_image = value6.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._hsv, out var value7))
		{
			_hsv = value7.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value8))
		{
			_tween = value8.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._baseScale, out var value9))
		{
			_baseScale = value9.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._hoverScale, out var value10))
		{
			_hoverScale = value10.As<float>();
		}
		if (info.TryGetProperty(PropertyName._pressDownScale, out var value11))
		{
			_pressDownScale = value11.As<float>();
		}
		if (info.TryGetSignalEventDelegate<ToggledEventHandler>(SignalName.Toggled, out var value12))
		{
			backing_Toggled = value12;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotSignalList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(SignalName.Toggled, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "tickbox", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		return list;
	}

	protected void EmitSignalToggled(NCardTypeTickbox tickbox)
	{
		EmitSignal(SignalName.Toggled, tickbox);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.Toggled && args.Count == 1)
		{
			backing_Toggled?.Invoke(VariantUtils.ConvertTo<NCardTypeTickbox>(in args[0]));
		}
		else
		{
			base.RaiseGodotClassSignalCallbacks(in signal, args);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassSignal(in godot_string_name signal)
	{
		if (signal == SignalName.Toggled)
		{
			return true;
		}
		return base.HasGodotClassSignal(in signal);
	}
}
