using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;

namespace MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;

[ScriptPath("res://src/Core/Nodes/Screens/CardLibrary/NCardPoolFilter.cs")]
public class NCardPoolFilter : NButton
{
	[Signal]
	public delegate void ToggledEventHandler(NCardPoolFilter filter);

	public new class MethodName : NButton.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnToggle = "OnToggle";

		public new static readonly StringName OnRelease = "OnRelease";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";

		public new static readonly StringName OnPress = "OnPress";
	}

	public new class PropertyName : NButton.PropertyName
	{
		public static readonly StringName IsSelected = "IsSelected";

		public static readonly StringName _isSelected = "_isSelected";

		public static readonly StringName _image = "_image";

		public static readonly StringName _hsv = "_hsv";

		public static readonly StringName _controllerSelectionReticle = "_controllerSelectionReticle";

		public static readonly StringName _tween = "_tween";
	}

	public new class SignalName : NButton.SignalName
	{
		public static readonly StringName Toggled = "Toggled";
	}

	private static readonly StringName _v = new StringName("v");

	private static readonly StringName _s = new StringName("s");

	private bool _isSelected;

	private Control _image;

	private ShaderMaterial _hsv;

	private NSelectionReticle _controllerSelectionReticle;

	private Tween? _tween;

	private const float _focusedMultiplier = 1.2f;

	private const float _pressDownMultiplier = 0.8f;

	private static readonly Vector2 _enabledScale = Vector2.One * 1.1f;

	private static readonly Vector2 _disabledScale = Vector2.One * 0.95f;

	private ToggledEventHandler backing_Toggled;

	public LocString? Loc { get; set; }

	public bool IsSelected
	{
		get
		{
			return _isSelected;
		}
		set
		{
			_isSelected = value;
			OnToggle();
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
		ConnectSignals();
		_image = GetNode<Control>("Image");
		_controllerSelectionReticle = GetNode<NSelectionReticle>("%SelectionReticle");
		_hsv = (ShaderMaterial)_image.GetMaterial();
	}

	private void OnToggle()
	{
		_tween?.Kill();
		_hsv.SetShaderParameter(_s, _isSelected ? 1f : 0.3f);
		_hsv.SetShaderParameter(_v, _isSelected ? 1f : 0.55f);
		if (!_isSelected)
		{
			_tween = CreateTween().SetParallel();
			_tween.TweenProperty(_image, "scale", _disabledScale, 0.3).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		}
		else
		{
			_tween = CreateTween().SetParallel();
			_tween.TweenProperty(_image, "scale", _enabledScale, 0.2).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
		}
	}

	protected override void OnRelease()
	{
		base.OnRelease();
		IsSelected = !IsSelected;
		EmitSignal(SignalName.Toggled, this);
	}

	protected override void OnFocus()
	{
		base.OnFocus();
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_image, "scale", (_isSelected ? _enabledScale : _disabledScale) * 1.2f, 0.05);
		if (NControllerManager.Instance.IsUsingController)
		{
			_controllerSelectionReticle.OnSelect();
		}
		if (Loc != null)
		{
			NHoverTipSet nHoverTipSet = NHoverTipSet.CreateAndShow(this, new HoverTip(Loc));
			nHoverTipSet.GlobalPosition = new Vector2(310f, base.GlobalPosition.Y);
		}
	}

	protected override void OnUnfocus()
	{
		base.OnUnfocus();
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_image, "scale", _isSelected ? _enabledScale : _disabledScale, 0.3);
		_controllerSelectionReticle.OnDeselect();
		if (Loc != null)
		{
			NHoverTipSet.Remove(this);
		}
	}

	protected override void OnPress()
	{
		if (!_isSelected)
		{
			base.OnPress();
			_tween?.Kill();
			_tween = CreateTween().SetParallel();
			_tween.TweenProperty(_image, "scale", (_isSelected ? _enabledScale : _disabledScale) * 0.8f, 0.3).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(6);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnToggle, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnRelease, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.IsSelected)
		{
			IsSelected = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._isSelected)
		{
			_isSelected = VariantUtils.ConvertTo<bool>(in value);
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
		if (name == PropertyName._controllerSelectionReticle)
		{
			_controllerSelectionReticle = VariantUtils.ConvertTo<NSelectionReticle>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.IsSelected)
		{
			value = VariantUtils.CreateFrom<bool>(IsSelected);
			return true;
		}
		if (name == PropertyName._isSelected)
		{
			value = VariantUtils.CreateFrom(in _isSelected);
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
		if (name == PropertyName._controllerSelectionReticle)
		{
			value = VariantUtils.CreateFrom(in _controllerSelectionReticle);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isSelected, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._image, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hsv, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._controllerSelectionReticle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsSelected, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.IsSelected, Variant.From<bool>(IsSelected));
		info.AddProperty(PropertyName._isSelected, Variant.From(in _isSelected));
		info.AddProperty(PropertyName._image, Variant.From(in _image));
		info.AddProperty(PropertyName._hsv, Variant.From(in _hsv));
		info.AddProperty(PropertyName._controllerSelectionReticle, Variant.From(in _controllerSelectionReticle));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddSignalEventDelegate(SignalName.Toggled, backing_Toggled);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.IsSelected, out var value))
		{
			IsSelected = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._isSelected, out var value2))
		{
			_isSelected = value2.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._image, out var value3))
		{
			_image = value3.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._hsv, out var value4))
		{
			_hsv = value4.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._controllerSelectionReticle, out var value5))
		{
			_controllerSelectionReticle = value5.As<NSelectionReticle>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value6))
		{
			_tween = value6.As<Tween>();
		}
		if (info.TryGetSignalEventDelegate<ToggledEventHandler>(SignalName.Toggled, out var value7))
		{
			backing_Toggled = value7;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotSignalList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(SignalName.Toggled, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "filter", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		return list;
	}

	protected void EmitSignalToggled(NCardPoolFilter filter)
	{
		EmitSignal(SignalName.Toggled, filter);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.Toggled && args.Count == 1)
		{
			backing_Toggled?.Invoke(VariantUtils.ConvertTo<NCardPoolFilter>(in args[0]));
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
