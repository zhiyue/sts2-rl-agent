using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.Fonts;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

[ScriptPath("res://src/Core/Nodes/Screens/MainMenu/NMainMenuTextButton.cs")]
public class NMainMenuTextButton : NButton
{
	public new class MethodName : NButton.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName ConnectSignals = "ConnectSignals";

		public static readonly StringName SetLocalization = "SetLocalization";

		public new static readonly StringName _Notification = "_Notification";

		public static readonly StringName RefreshLabel = "RefreshLabel";

		public new static readonly StringName OnPress = "OnPress";

		public new static readonly StringName OnRelease = "OnRelease";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";

		public static readonly StringName AnimUnhover = "AnimUnhover";

		public static readonly StringName AnimPressDown = "AnimPressDown";

		public static readonly StringName AnimRelease = "AnimRelease";

		public new static readonly StringName OnDisable = "OnDisable";

		public new static readonly StringName OnEnable = "OnEnable";
	}

	public new class PropertyName : NButton.PropertyName
	{
		public static readonly StringName label = "label";

		public static readonly StringName _defaultColor = "_defaultColor";

		public static readonly StringName _hoveredColor = "_hoveredColor";

		public static readonly StringName _downColor = "_downColor";

		public static readonly StringName _tween = "_tween";
	}

	public new class SignalName : NButton.SignalName
	{
	}

	public MegaLabel? label;

	private Color _defaultColor = StsColors.cream;

	private Color _hoveredColor = StsColors.gold;

	private Color _downColor = StsColors.halfTransparentWhite;

	private static readonly Vector2 _hoverScale = new Vector2(1.05f, 1.05f);

	private static readonly Vector2 _downScale = new Vector2(0.95f, 0.95f);

	private static readonly StyleBoxEmpty _emptyStyleBox = new StyleBoxEmpty();

	private const double _pressDownDur = 0.2;

	private const double _unhoverAnimDur = 0.5;

	private Tween? _tween;

	private LocString? _locString;

	public override void _Ready()
	{
		if (GetType() != typeof(NMainMenuTextButton))
		{
			throw new InvalidOperationException("Don't call base._Ready()!");
		}
		ConnectSignals();
	}

	protected override void ConnectSignals()
	{
		base.ConnectSignals();
		label = GetChild<MegaLabel>(0);
		label.AddThemeStyleboxOverride(ThemeConstants.Control.focus, _emptyStyleBox);
		label.FocusMode = FocusModeEnum.None;
	}

	public void SetLocalization(string locKey)
	{
		_locString = new LocString("main_menu_ui", locKey);
		RefreshLabel();
	}

	public override void _Notification(int what)
	{
		if ((long)what == 2010)
		{
			RefreshLabel();
		}
	}

	private void RefreshLabel()
	{
		if (label != null && _locString != null)
		{
			label.Text = _locString.GetFormattedText();
			label.ApplyLocaleFontSubstitution(FontType.Regular, ThemeConstants.Label.font);
			TaskHelper.RunSafely(UpdatePivotOffset());
		}
	}

	private async Task UpdatePivotOffset()
	{
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		if (label != null)
		{
			label.PivotOffset = label.Size * 0.5f;
		}
	}

	protected override void OnPress()
	{
		base.OnPress();
		AnimPressDown();
	}

	protected override void OnRelease()
	{
		AnimRelease();
	}

	protected override void OnFocus()
	{
		base.OnFocus();
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(label, "scale", _hoverScale, 0.05);
		_tween.TweenProperty(label, "self_modulate", _hoveredColor, 0.05);
	}

	protected override void OnUnfocus()
	{
		base.OnUnfocus();
		AnimUnhover();
	}

	private void AnimUnhover()
	{
		_tween?.Kill();
		if (label != null)
		{
			label.SelfModulate = _defaultColor;
			_tween = CreateTween();
			_tween.TweenProperty(label, "scale", Vector2.One, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		}
	}

	private void AnimPressDown()
	{
		_tween?.Kill();
		if (label != null)
		{
			label.SelfModulate = _hoveredColor;
			label.Scale = _hoverScale;
			_tween = CreateTween();
			_tween.TweenProperty(label, "scale", _downScale, 0.2).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
			_tween.TweenProperty(label, "self_modulate", _downColor, 0.2).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
		}
	}

	private void AnimRelease()
	{
		_tween?.Kill();
		if (label != null)
		{
			_tween = CreateTween();
			_tween.TweenProperty(label, "scale", base.IsFocused ? _hoverScale : Vector2.One, 0.2).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
			_tween.TweenProperty(label, "self_modulate", _defaultColor, 0.2).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
		}
	}

	protected override void OnDisable()
	{
		if (label != null)
		{
			label.Modulate = StsColors.quarterTransparentWhite;
		}
	}

	protected override void OnEnable()
	{
		if (label != null)
		{
			label.Modulate = Colors.White;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(14);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ConnectSignals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetLocalization, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "locKey", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Notification, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "what", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RefreshLabel, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPress, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnRelease, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimUnhover, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimPressDown, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimRelease, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnDisable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnEnable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.SetLocalization && args.Count == 1)
		{
			SetLocalization(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Notification && args.Count == 1)
		{
			_Notification(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshLabel && args.Count == 0)
		{
			RefreshLabel();
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
		if (method == MethodName.AnimUnhover && args.Count == 0)
		{
			AnimUnhover();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimPressDown && args.Count == 0)
		{
			AnimPressDown();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimRelease && args.Count == 0)
		{
			AnimRelease();
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
		if (method == MethodName.SetLocalization)
		{
			return true;
		}
		if (method == MethodName._Notification)
		{
			return true;
		}
		if (method == MethodName.RefreshLabel)
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
		if (method == MethodName.OnFocus)
		{
			return true;
		}
		if (method == MethodName.OnUnfocus)
		{
			return true;
		}
		if (method == MethodName.AnimUnhover)
		{
			return true;
		}
		if (method == MethodName.AnimPressDown)
		{
			return true;
		}
		if (method == MethodName.AnimRelease)
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
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.label)
		{
			label = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._defaultColor)
		{
			_defaultColor = VariantUtils.ConvertTo<Color>(in value);
			return true;
		}
		if (name == PropertyName._hoveredColor)
		{
			_hoveredColor = VariantUtils.ConvertTo<Color>(in value);
			return true;
		}
		if (name == PropertyName._downColor)
		{
			_downColor = VariantUtils.ConvertTo<Color>(in value);
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
		if (name == PropertyName.label)
		{
			value = VariantUtils.CreateFrom(in label);
			return true;
		}
		if (name == PropertyName._defaultColor)
		{
			value = VariantUtils.CreateFrom(in _defaultColor);
			return true;
		}
		if (name == PropertyName._hoveredColor)
		{
			value = VariantUtils.CreateFrom(in _hoveredColor);
			return true;
		}
		if (name == PropertyName._downColor)
		{
			value = VariantUtils.CreateFrom(in _downColor);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.label, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName._defaultColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName._hoveredColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName._downColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.label, Variant.From(in label));
		info.AddProperty(PropertyName._defaultColor, Variant.From(in _defaultColor));
		info.AddProperty(PropertyName._hoveredColor, Variant.From(in _hoveredColor));
		info.AddProperty(PropertyName._downColor, Variant.From(in _downColor));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.label, out var value))
		{
			label = value.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._defaultColor, out var value2))
		{
			_defaultColor = value2.As<Color>();
		}
		if (info.TryGetProperty(PropertyName._hoveredColor, out var value3))
		{
			_hoveredColor = value3.As<Color>();
		}
		if (info.TryGetProperty(PropertyName._downColor, out var value4))
		{
			_downColor = value4.As<Color>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value5))
		{
			_tween = value5.As<Tween>();
		}
	}
}
