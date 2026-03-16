using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.CommonUi;

[ScriptPath("res://src/Core/Nodes/CommonUi/NPopupYesNoButton.cs")]
public class NPopupYesNoButton : NButton
{
	public new class MethodName : NButton.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName DisconnectHotkeys = "DisconnectHotkeys";

		public static readonly StringName SetText = "SetText";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";

		public new static readonly StringName OnPress = "OnPress";

		public new static readonly StringName OnRelease = "OnRelease";

		public static readonly StringName UpdateShaderS = "UpdateShaderS";

		public static readonly StringName UpdateShaderV = "UpdateShaderV";
	}

	public new class PropertyName : NButton.PropertyName
	{
		public static readonly StringName IsYes = "IsYes";

		public new static readonly StringName Hotkeys = "Hotkeys";

		public static readonly StringName _visuals = "_visuals";

		public static readonly StringName _outline = "_outline";

		public static readonly StringName _image = "_image";

		public static readonly StringName _label = "_label";

		public static readonly StringName _tween = "_tween";

		public static readonly StringName _baseS = "_baseS";

		public static readonly StringName _baseV = "_baseV";

		public static readonly StringName _hsv = "_hsv";

		public static readonly StringName _outlineMaterial = "_outlineMaterial";

		public static readonly StringName _isFocused = "_isFocused";

		public static readonly StringName _isYes = "_isYes";
	}

	public new class SignalName : NButton.SignalName
	{
	}

	private static readonly StringName _v = new StringName("v");

	private static readonly StringName _s = new StringName("s");

	private Control _visuals;

	private Control _outline;

	private Control _image;

	private MegaLabel _label;

	private Tween? _tween;

	private float _baseS;

	private float _baseV;

	private ShaderMaterial _hsv;

	private CanvasItemMaterial _outlineMaterial;

	private bool _isFocused;

	private static readonly Color _goldOutline = new Color("f0b400");

	private bool _isYes;

	public bool IsYes
	{
		get
		{
			return _isYes;
		}
		set
		{
			DisconnectHotkeys();
			_isYes = value;
			Callable.From(base.RegisterHotkeys).CallDeferred();
			UpdateControllerButton();
		}
	}

	protected override string[] Hotkeys => new string[1] { _isYes ? MegaInput.select : MegaInput.cancel };

	public override void _Ready()
	{
		ConnectSignals();
		_visuals = GetNode<Control>("%Visuals");
		_outline = GetNode<Control>("%Outline");
		_image = GetNode<Control>("%Image");
		_label = GetNode<MegaLabel>("%Label");
		_hsv = (ShaderMaterial)_image.GetMaterial();
		_outlineMaterial = (CanvasItemMaterial)_outline.GetMaterial();
		_baseS = (float)_hsv.GetShaderParameter(_s);
		_baseV = (float)_hsv.GetShaderParameter(_v);
	}

	public override void _ExitTree()
	{
		DisconnectHotkeys();
	}

	public void DisconnectHotkeys()
	{
		string[] hotkeys = Hotkeys;
		foreach (string hotkey in hotkeys)
		{
			NHotkeyManager.Instance.RemoveHotkeyPressedBinding(hotkey, base.OnPressHandler);
			NHotkeyManager.Instance.RemoveHotkeyReleasedBinding(hotkey, base.OnReleaseHandler);
		}
	}

	public void SetText(string text)
	{
		_label.SetTextAutoSize(text);
	}

	protected override void OnFocus()
	{
		base.OnFocus();
		_isFocused = true;
		_outlineMaterial.BlendMode = CanvasItemMaterial.BlendModeEnum.Add;
		_outline.Modulate = Colors.White;
		_outline.SelfModulate = _goldOutline;
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_visuals, "scale", Vector2.One * 1.025f, 0.05);
		_tween.TweenMethod(Callable.From<float>(UpdateShaderS), _hsv.GetShaderParameter(_s), _baseS + 0.25f, 0.05).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_tween.TweenMethod(Callable.From<float>(UpdateShaderV), _hsv.GetShaderParameter(_v), _baseV + 0.25f, 0.05).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	protected override void OnUnfocus()
	{
		base.OnUnfocus();
		_isFocused = false;
		_outlineMaterial.BlendMode = CanvasItemMaterial.BlendModeEnum.Mix;
		_outline.Modulate = StsColors.halfTransparentWhite;
		_outline.SelfModulate = Colors.Black;
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_visuals, "scale", Vector2.One, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_tween.TweenMethod(Callable.From<float>(UpdateShaderS), _hsv.GetShaderParameter(_s), _baseS, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_tween.TweenMethod(Callable.From<float>(UpdateShaderV), _hsv.GetShaderParameter(_v), _baseV, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	protected override void OnPress()
	{
		base.OnPress();
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_visuals, "scale", Vector2.One * 0.975f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_tween.TweenMethod(Callable.From<float>(UpdateShaderS), _hsv.GetShaderParameter(_s), _baseS - 0.1f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_tween.TweenMethod(Callable.From<float>(UpdateShaderV), _hsv.GetShaderParameter(_v), _baseV - 0.1f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	protected override void OnRelease()
	{
		_isFocused = false;
		_outlineMaterial.BlendMode = CanvasItemMaterial.BlendModeEnum.Mix;
		_outline.Modulate = StsColors.halfTransparentWhite;
		_outline.SelfModulate = Colors.Black;
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_visuals, "scale", Vector2.One, 0.05);
		_tween.TweenMethod(Callable.From<float>(UpdateShaderS), _hsv.GetShaderParameter(_s), _baseS, 0.05);
		_tween.TweenMethod(Callable.From<float>(UpdateShaderV), _hsv.GetShaderParameter(_v), _baseV, 0.05);
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
		List<MethodInfo> list = new List<MethodInfo>(10);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DisconnectHotkeys, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetText, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "text", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPress, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnRelease, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DisconnectHotkeys && args.Count == 0)
		{
			DisconnectHotkeys();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetText && args.Count == 1)
		{
			SetText(VariantUtils.ConvertTo<string>(in args[0]));
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
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.DisconnectHotkeys)
		{
			return true;
		}
		if (method == MethodName.SetText)
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
		if (name == PropertyName.IsYes)
		{
			IsYes = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._visuals)
		{
			_visuals = VariantUtils.ConvertTo<Control>(in value);
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
		if (name == PropertyName._label)
		{
			_label = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
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
		if (name == PropertyName._hsv)
		{
			_hsv = VariantUtils.ConvertTo<ShaderMaterial>(in value);
			return true;
		}
		if (name == PropertyName._outlineMaterial)
		{
			_outlineMaterial = VariantUtils.ConvertTo<CanvasItemMaterial>(in value);
			return true;
		}
		if (name == PropertyName._isFocused)
		{
			_isFocused = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._isYes)
		{
			_isYes = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.IsYes)
		{
			value = VariantUtils.CreateFrom<bool>(IsYes);
			return true;
		}
		if (name == PropertyName.Hotkeys)
		{
			value = VariantUtils.CreateFrom<string[]>(Hotkeys);
			return true;
		}
		if (name == PropertyName._visuals)
		{
			value = VariantUtils.CreateFrom(in _visuals);
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
		if (name == PropertyName._label)
		{
			value = VariantUtils.CreateFrom(in _label);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
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
		if (name == PropertyName._hsv)
		{
			value = VariantUtils.CreateFrom(in _hsv);
			return true;
		}
		if (name == PropertyName._outlineMaterial)
		{
			value = VariantUtils.CreateFrom(in _outlineMaterial);
			return true;
		}
		if (name == PropertyName._isFocused)
		{
			value = VariantUtils.CreateFrom(in _isFocused);
			return true;
		}
		if (name == PropertyName._isYes)
		{
			value = VariantUtils.CreateFrom(in _isYes);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._visuals, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._outline, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._image, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._label, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._baseS, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._baseV, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hsv, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._outlineMaterial, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isFocused, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isYes, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsYes, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.PackedStringArray, PropertyName.Hotkeys, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.IsYes, Variant.From<bool>(IsYes));
		info.AddProperty(PropertyName._visuals, Variant.From(in _visuals));
		info.AddProperty(PropertyName._outline, Variant.From(in _outline));
		info.AddProperty(PropertyName._image, Variant.From(in _image));
		info.AddProperty(PropertyName._label, Variant.From(in _label));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._baseS, Variant.From(in _baseS));
		info.AddProperty(PropertyName._baseV, Variant.From(in _baseV));
		info.AddProperty(PropertyName._hsv, Variant.From(in _hsv));
		info.AddProperty(PropertyName._outlineMaterial, Variant.From(in _outlineMaterial));
		info.AddProperty(PropertyName._isFocused, Variant.From(in _isFocused));
		info.AddProperty(PropertyName._isYes, Variant.From(in _isYes));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.IsYes, out var value))
		{
			IsYes = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._visuals, out var value2))
		{
			_visuals = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._outline, out var value3))
		{
			_outline = value3.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._image, out var value4))
		{
			_image = value4.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._label, out var value5))
		{
			_label = value5.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value6))
		{
			_tween = value6.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._baseS, out var value7))
		{
			_baseS = value7.As<float>();
		}
		if (info.TryGetProperty(PropertyName._baseV, out var value8))
		{
			_baseV = value8.As<float>();
		}
		if (info.TryGetProperty(PropertyName._hsv, out var value9))
		{
			_hsv = value9.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._outlineMaterial, out var value10))
		{
			_outlineMaterial = value10.As<CanvasItemMaterial>();
		}
		if (info.TryGetProperty(PropertyName._isFocused, out var value11))
		{
			_isFocused = value11.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._isYes, out var value12))
		{
			_isYes = value12.As<bool>();
		}
	}
}
