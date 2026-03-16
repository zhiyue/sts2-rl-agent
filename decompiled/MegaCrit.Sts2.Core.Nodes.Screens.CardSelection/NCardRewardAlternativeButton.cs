using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;

[ScriptPath("res://src/Core/Nodes/Screens/CardSelection/NCardRewardAlternativeButton.cs")]
public class NCardRewardAlternativeButton : NButton
{
	public new class MethodName : NButton.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName Create = "Create";

		public static readonly StringName AnimateIn = "AnimateIn";

		public new static readonly StringName OnPress = "OnPress";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";

		public static readonly StringName UpdateShaderParam = "UpdateShaderParam";
	}

	public new class PropertyName : NButton.PropertyName
	{
		public new static readonly StringName Hotkeys = "Hotkeys";

		public static readonly StringName _image = "_image";

		public static readonly StringName _label = "_label";

		public static readonly StringName _optionName = "_optionName";

		public static readonly StringName _animInTween = "_animInTween";

		public static readonly StringName _showPosition = "_showPosition";

		public static readonly StringName _currentTween = "_currentTween";

		public static readonly StringName _hsv = "_hsv";

		public static readonly StringName _hsvDefault = "_hsvDefault";

		public static readonly StringName _hsvHover = "_hsvHover";

		public static readonly StringName _hsvDown = "_hsvDown";

		public static readonly StringName _hotkeys = "_hotkeys";
	}

	public new class SignalName : NButton.SignalName
	{
	}

	private static readonly StringName _v = new StringName("v");

	private TextureRect _image;

	private MegaLabel _label;

	private string? _optionName;

	private Tween? _animInTween;

	private Vector2 _showPosition;

	private static readonly Vector2 _animOffsetPosition = new Vector2(0f, -50f);

	private Tween? _currentTween;

	private ShaderMaterial _hsv;

	private Variant _hsvDefault = 0.9;

	private Variant _hsvHover = 1.1;

	private Variant _hsvDown = 0.7;

	private static readonly Vector2 _defaultScale = Vector2.One;

	private static readonly Vector2 _hoverScale = Vector2.One * 1.05f;

	private static readonly Vector2 _downScale = Vector2.One * 0.95f;

	private string[] _hotkeys;

	private static string ScenePath => SceneHelper.GetScenePath("/ui/card_reward_alternative_button");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	protected override string[] Hotkeys => _hotkeys;

	public override void _Ready()
	{
		ConnectSignals();
		_image = GetNode<TextureRect>("Image");
		_label = GetNode<MegaLabel>("Label");
		_hsv = (ShaderMaterial)_image.Material;
		_showPosition = base.Position;
		if (_optionName != null)
		{
			_label.SetTextAutoSize(_optionName);
		}
		_controllerHotkeyIcon.Texture = NInputManager.Instance.GetHotkeyIcon(Hotkeys.First());
	}

	public static NCardRewardAlternativeButton Create(string optionName, string hotkey)
	{
		NCardRewardAlternativeButton nCardRewardAlternativeButton = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NCardRewardAlternativeButton>(PackedScene.GenEditState.Disabled);
		nCardRewardAlternativeButton._optionName = optionName;
		nCardRewardAlternativeButton._hotkeys = new string[1] { hotkey };
		return nCardRewardAlternativeButton;
	}

	public void AnimateIn()
	{
		_animInTween?.Kill();
		_animInTween = CreateTween().SetParallel();
		_animInTween.TweenProperty(this, "modulate:a", 1f, 0.4).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
			.From(0f);
		_animInTween.TweenProperty(this, "position", _showPosition, 0.4).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back)
			.From(_showPosition + _animOffsetPosition);
	}

	protected override void OnPress()
	{
		_currentTween?.Kill();
		_currentTween = CreateTween().SetParallel();
		_currentTween.TweenMethod(Callable.From<float>(UpdateShaderParam), _hsvHover, _hsvDown, 0.20000000298023224).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_currentTween.TweenProperty(this, "scale", _downScale, 0.20000000298023224).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	protected override void OnFocus()
	{
		_currentTween?.Kill();
		base.Scale = _hoverScale;
		_hsv.SetShaderParameter(_v, _hsvHover);
	}

	protected override void OnUnfocus()
	{
		_currentTween?.Kill();
		_currentTween = CreateTween().SetParallel();
		_currentTween.TweenMethod(Callable.From<float>(UpdateShaderParam), _hsv.GetShaderParameter(_v), _hsvDefault, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_currentTween.TweenProperty(this, "scale", _defaultScale, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	private void UpdateShaderParam(float value)
	{
		_hsv.SetShaderParameter(_v, value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(7);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "optionName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.String, "hotkey", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AnimateIn, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPress, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateShaderParam, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
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
		if (method == MethodName.Create && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<NCardRewardAlternativeButton>(Create(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<string>(in args[1])));
			return true;
		}
		if (method == MethodName.AnimateIn && args.Count == 0)
		{
			AnimateIn();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPress && args.Count == 0)
		{
			OnPress();
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
		if (method == MethodName.UpdateShaderParam && args.Count == 1)
		{
			UpdateShaderParam(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<NCardRewardAlternativeButton>(Create(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<string>(in args[1])));
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.Create)
		{
			return true;
		}
		if (method == MethodName.AnimateIn)
		{
			return true;
		}
		if (method == MethodName.OnPress)
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
		if (method == MethodName.UpdateShaderParam)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._image)
		{
			_image = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._label)
		{
			_label = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._optionName)
		{
			_optionName = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._animInTween)
		{
			_animInTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._showPosition)
		{
			_showPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._currentTween)
		{
			_currentTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._hsv)
		{
			_hsv = VariantUtils.ConvertTo<ShaderMaterial>(in value);
			return true;
		}
		if (name == PropertyName._hsvDefault)
		{
			_hsvDefault = VariantUtils.ConvertTo<Variant>(in value);
			return true;
		}
		if (name == PropertyName._hsvHover)
		{
			_hsvHover = VariantUtils.ConvertTo<Variant>(in value);
			return true;
		}
		if (name == PropertyName._hsvDown)
		{
			_hsvDown = VariantUtils.ConvertTo<Variant>(in value);
			return true;
		}
		if (name == PropertyName._hotkeys)
		{
			_hotkeys = VariantUtils.ConvertTo<string[]>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.Hotkeys)
		{
			value = VariantUtils.CreateFrom<string[]>(Hotkeys);
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
		if (name == PropertyName._optionName)
		{
			value = VariantUtils.CreateFrom(in _optionName);
			return true;
		}
		if (name == PropertyName._animInTween)
		{
			value = VariantUtils.CreateFrom(in _animInTween);
			return true;
		}
		if (name == PropertyName._showPosition)
		{
			value = VariantUtils.CreateFrom(in _showPosition);
			return true;
		}
		if (name == PropertyName._currentTween)
		{
			value = VariantUtils.CreateFrom(in _currentTween);
			return true;
		}
		if (name == PropertyName._hsv)
		{
			value = VariantUtils.CreateFrom(in _hsv);
			return true;
		}
		if (name == PropertyName._hsvDefault)
		{
			value = VariantUtils.CreateFrom(in _hsvDefault);
			return true;
		}
		if (name == PropertyName._hsvHover)
		{
			value = VariantUtils.CreateFrom(in _hsvHover);
			return true;
		}
		if (name == PropertyName._hsvDown)
		{
			value = VariantUtils.CreateFrom(in _hsvDown);
			return true;
		}
		if (name == PropertyName._hotkeys)
		{
			value = VariantUtils.CreateFrom(in _hotkeys);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._image, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._label, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName._optionName, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._animInTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._showPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._currentTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hsv, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Nil, PropertyName._hsvDefault, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Nil, PropertyName._hsvHover, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Nil, PropertyName._hsvDown, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.PackedStringArray, PropertyName._hotkeys, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.PackedStringArray, PropertyName.Hotkeys, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._image, Variant.From(in _image));
		info.AddProperty(PropertyName._label, Variant.From(in _label));
		info.AddProperty(PropertyName._optionName, Variant.From(in _optionName));
		info.AddProperty(PropertyName._animInTween, Variant.From(in _animInTween));
		info.AddProperty(PropertyName._showPosition, Variant.From(in _showPosition));
		info.AddProperty(PropertyName._currentTween, Variant.From(in _currentTween));
		info.AddProperty(PropertyName._hsv, Variant.From(in _hsv));
		info.AddProperty(PropertyName._hsvDefault, Variant.From(in _hsvDefault));
		info.AddProperty(PropertyName._hsvHover, Variant.From(in _hsvHover));
		info.AddProperty(PropertyName._hsvDown, Variant.From(in _hsvDown));
		info.AddProperty(PropertyName._hotkeys, Variant.From(in _hotkeys));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._image, out var value))
		{
			_image = value.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._label, out var value2))
		{
			_label = value2.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._optionName, out var value3))
		{
			_optionName = value3.As<string>();
		}
		if (info.TryGetProperty(PropertyName._animInTween, out var value4))
		{
			_animInTween = value4.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._showPosition, out var value5))
		{
			_showPosition = value5.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._currentTween, out var value6))
		{
			_currentTween = value6.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._hsv, out var value7))
		{
			_hsv = value7.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._hsvDefault, out var value8))
		{
			_hsvDefault = value8.As<Variant>();
		}
		if (info.TryGetProperty(PropertyName._hsvHover, out var value9))
		{
			_hsvHover = value9.As<Variant>();
		}
		if (info.TryGetProperty(PropertyName._hsvDown, out var value10))
		{
			_hsvDown = value10.As<Variant>();
		}
		if (info.TryGetProperty(PropertyName._hotkeys, out var value11))
		{
			_hotkeys = value11.As<string[]>();
		}
	}
}
