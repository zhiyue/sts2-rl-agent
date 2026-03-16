using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace MegaCrit.Sts2.Core.Nodes.CommonUi;

[ScriptPath("res://src/Core/Nodes/CommonUi/NTickbox.cs")]
public class NTickbox : NButton
{
	[Signal]
	public delegate void ToggledEventHandler(NTickbox tickbox);

	public new class MethodName : NButton.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName ConnectSignals = "ConnectSignals";

		public new static readonly StringName OnRelease = "OnRelease";

		public static readonly StringName OnUntick = "OnUntick";

		public static readonly StringName OnTick = "OnTick";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";

		public new static readonly StringName OnPress = "OnPress";

		public new static readonly StringName OnDisable = "OnDisable";

		public new static readonly StringName OnEnable = "OnEnable";

		public static readonly StringName UpdateShaderV = "UpdateShaderV";
	}

	public new class PropertyName : NButton.PropertyName
	{
		public static readonly StringName IsTicked = "IsTicked";

		public static readonly StringName _isTicked = "_isTicked";

		public static readonly StringName _imageContainer = "_imageContainer";

		public static readonly StringName _tickedImage = "_tickedImage";

		public static readonly StringName _notTickedImage = "_notTickedImage";

		public static readonly StringName _hsv = "_hsv";

		public static readonly StringName _tween = "_tween";

		public static readonly StringName _baseScale = "_baseScale";

		public static readonly StringName _hoverScale = "_hoverScale";

		public static readonly StringName _pressDownScale = "_pressDownScale";

		public static readonly StringName _hoverV = "_hoverV";
	}

	public new class SignalName : NButton.SignalName
	{
		public static readonly StringName Toggled = "Toggled";
	}

	private static readonly StringName _v = new StringName("v");

	private bool _isTicked = true;

	private Control _imageContainer;

	private Control _tickedImage;

	private Control _notTickedImage;

	private ShaderMaterial _hsv;

	private Tween? _tween;

	private Vector2 _baseScale;

	private float _hoverScale = 1.05f;

	private float _pressDownScale = 0.95f;

	private float _hoverV = 1.2f;

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
			_tickedImage.Visible = _isTicked;
			_notTickedImage.Visible = !_isTicked;
		}
	}

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
		if (GetType() != typeof(NTickbox))
		{
			Log.Error($"{GetType()}");
			throw new InvalidOperationException("Don't call base._Ready()! Call ConnectSignals() instead.");
		}
		ConnectSignals();
	}

	protected override void ConnectSignals()
	{
		base.ConnectSignals();
		_imageContainer = GetNode<Control>("%TickboxVisuals");
		_baseScale = _imageContainer.Scale;
		_hsv = (ShaderMaterial)_imageContainer.Material;
		_tickedImage = GetNode<Control>("%TickboxVisuals/Ticked");
		_notTickedImage = GetNode<Control>("%TickboxVisuals/NotTicked");
	}

	protected override void OnRelease()
	{
		base.OnRelease();
		IsTicked = !IsTicked;
		if (IsTicked)
		{
			SfxCmd.Play("event:/sfx/ui/clicks/ui_checkbox_on");
			OnTick();
		}
		else
		{
			SfxCmd.Play("event:/sfx/ui/clicks/ui_checkbox_off");
			OnUntick();
		}
		EmitSignal(SignalName.Toggled, this);
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_imageContainer, "scale", _baseScale * _hoverScale, 0.05);
		_tween.TweenMethod(Callable.From<float>(UpdateShaderV), _hsv.GetShaderParameter(_v), _hoverV, 0.05);
	}

	protected virtual void OnUntick()
	{
	}

	protected virtual void OnTick()
	{
	}

	protected override void OnFocus()
	{
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_imageContainer, "scale", _baseScale * _hoverScale, 0.05);
		_tween.TweenMethod(Callable.From<float>(UpdateShaderV), _hsv.GetShaderParameter(_v), _hoverV, 0.05);
	}

	protected override void OnUnfocus()
	{
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_imageContainer, "scale", _baseScale, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_tween.TweenMethod(Callable.From<float>(UpdateShaderV), _hsv.GetShaderParameter(_v), 1f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	protected override void OnPress()
	{
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_imageContainer, "scale", _baseScale * _pressDownScale, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_tween.TweenMethod(Callable.From<float>(UpdateShaderV), _hsv.GetShaderParameter(_v), 0.8f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		base.Modulate = StsColors.gray;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		base.Modulate = Colors.White;
	}

	private void UpdateShaderV(float value)
	{
		_hsv.SetShaderParameter(_v, value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(11);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ConnectSignals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnRelease, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUntick, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnTick, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPress, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnDisable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnEnable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.ConnectSignals && args.Count == 0)
		{
			ConnectSignals();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnRelease && args.Count == 0)
		{
			OnRelease();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnUntick && args.Count == 0)
		{
			OnUntick();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnTick && args.Count == 0)
		{
			OnTick();
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
		if (method == MethodName.OnDisable && args.Count == 0)
		{
			OnDisable();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnEnable && args.Count == 0)
		{
			OnEnable();
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
		if (method == MethodName.ConnectSignals)
		{
			return true;
		}
		if (method == MethodName.OnRelease)
		{
			return true;
		}
		if (method == MethodName.OnUntick)
		{
			return true;
		}
		if (method == MethodName.OnTick)
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
		if (method == MethodName.OnDisable)
		{
			return true;
		}
		if (method == MethodName.OnEnable)
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
		if (name == PropertyName._imageContainer)
		{
			_imageContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._tickedImage)
		{
			_tickedImage = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._notTickedImage)
		{
			_notTickedImage = VariantUtils.ConvertTo<Control>(in value);
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
		if (name == PropertyName._hoverV)
		{
			_hoverV = VariantUtils.ConvertTo<float>(in value);
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
		if (name == PropertyName._imageContainer)
		{
			value = VariantUtils.CreateFrom(in _imageContainer);
			return true;
		}
		if (name == PropertyName._tickedImage)
		{
			value = VariantUtils.CreateFrom(in _tickedImage);
			return true;
		}
		if (name == PropertyName._notTickedImage)
		{
			value = VariantUtils.CreateFrom(in _notTickedImage);
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
		if (name == PropertyName._hoverV)
		{
			value = VariantUtils.CreateFrom(in _hoverV);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isTicked, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsTicked, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._imageContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tickedImage, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._notTickedImage, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hsv, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._baseScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._hoverScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._pressDownScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._hoverV, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.IsTicked, Variant.From<bool>(IsTicked));
		info.AddProperty(PropertyName._isTicked, Variant.From(in _isTicked));
		info.AddProperty(PropertyName._imageContainer, Variant.From(in _imageContainer));
		info.AddProperty(PropertyName._tickedImage, Variant.From(in _tickedImage));
		info.AddProperty(PropertyName._notTickedImage, Variant.From(in _notTickedImage));
		info.AddProperty(PropertyName._hsv, Variant.From(in _hsv));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._baseScale, Variant.From(in _baseScale));
		info.AddProperty(PropertyName._hoverScale, Variant.From(in _hoverScale));
		info.AddProperty(PropertyName._pressDownScale, Variant.From(in _pressDownScale));
		info.AddProperty(PropertyName._hoverV, Variant.From(in _hoverV));
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
		if (info.TryGetProperty(PropertyName._imageContainer, out var value3))
		{
			_imageContainer = value3.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._tickedImage, out var value4))
		{
			_tickedImage = value4.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._notTickedImage, out var value5))
		{
			_notTickedImage = value5.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._hsv, out var value6))
		{
			_hsv = value6.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value7))
		{
			_tween = value7.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._baseScale, out var value8))
		{
			_baseScale = value8.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._hoverScale, out var value9))
		{
			_hoverScale = value9.As<float>();
		}
		if (info.TryGetProperty(PropertyName._pressDownScale, out var value10))
		{
			_pressDownScale = value10.As<float>();
		}
		if (info.TryGetProperty(PropertyName._hoverV, out var value11))
		{
			_hoverV = value11.As<float>();
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

	protected void EmitSignalToggled(NTickbox tickbox)
	{
		EmitSignal(SignalName.Toggled, tickbox);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.Toggled && args.Count == 1)
		{
			backing_Toggled?.Invoke(VariantUtils.ConvertTo<NTickbox>(in args[0]));
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
