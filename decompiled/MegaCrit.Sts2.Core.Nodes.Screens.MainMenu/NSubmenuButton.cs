using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.Fonts;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

[ScriptPath("res://src/Core/Nodes/Screens/MainMenu/NSubmenuButton.cs")]
public class NSubmenuButton : NButton
{
	public new class MethodName : NButton.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName ConnectSignals = "ConnectSignals";

		public static readonly StringName SetIconAndLocalization = "SetIconAndLocalization";

		public new static readonly StringName _Notification = "_Notification";

		public static readonly StringName RefreshLabels = "RefreshLabels";

		public new static readonly StringName OnEnable = "OnEnable";

		public new static readonly StringName OnDisable = "OnDisable";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";

		public static readonly StringName UpdateShaderParam = "UpdateShaderParam";
	}

	public new class PropertyName : NButton.PropertyName
	{
		public static readonly StringName _hsv = "_hsv";

		public static readonly StringName _bgPanel = "_bgPanel";

		public static readonly StringName _icon = "_icon";

		public static readonly StringName _title = "_title";

		public static readonly StringName _description = "_description";

		public static readonly StringName _locKeyPrefix = "_locKeyPrefix";

		public static readonly StringName _defaultV = "_defaultV";

		public static readonly StringName _scaleTween = "_scaleTween";
	}

	public new class SignalName : NButton.SignalName
	{
	}

	private static readonly StringName _v = new StringName("v");

	private static readonly StringName _s = new StringName("s");

	private ShaderMaterial _hsv;

	private Control _bgPanel;

	private TextureRect _icon;

	private MegaLabel _title;

	private MegaRichTextLabel _description;

	private string? _locKeyPrefix;

	private float _defaultV;

	private const float _hoverV = 1f;

	private Tween? _scaleTween;

	private static readonly Vector2 _hoverScale = Vector2.One * 1.025f;

	public override void _Ready()
	{
		if (GetType() != typeof(NSubmenuButton))
		{
			throw new InvalidOperationException("Don't call base._Ready(). Use ConnectSignals() instead.");
		}
		ConnectSignals();
	}

	protected override void ConnectSignals()
	{
		base.ConnectSignals();
		_bgPanel = GetNode<Control>("BgPanel");
		_hsv = (ShaderMaterial)_bgPanel.Material;
		_icon = GetNode<TextureRect>("Icon");
		_title = GetNode<MegaLabel>("%Title");
		_description = GetNode<MegaRichTextLabel>("%Description");
		_defaultV = (float)_hsv.GetShaderParameter(_v);
	}

	public void SetIconAndLocalization(string locKeyPrefix)
	{
		_locKeyPrefix = locKeyPrefix;
		RefreshLabels();
	}

	public override void _Notification(int what)
	{
		if ((long)what == 2010 && _locKeyPrefix != null && IsNodeReady())
		{
			RefreshLabels();
		}
	}

	public void RefreshLabels()
	{
		LocString locString = new LocString("main_menu_ui", _locKeyPrefix + ".title");
		_title.SetTextAutoSize(locString.GetFormattedText());
		_title.ApplyLocaleFontSubstitution(FontType.Regular, ThemeConstants.Label.font);
		LocString locString2;
		if (base.IsEnabled)
		{
			locString2 = new LocString("main_menu_ui", _locKeyPrefix + ".description");
		}
		else
		{
			locString2 = new LocString("main_menu_ui", _locKeyPrefix + ".LOCKED.description");
			if (!locString2.Exists())
			{
				Log.Warn($"Submenu button {base.Name} tried to find locked description for {_locKeyPrefix} but couldn't");
				locString2 = new LocString("main_menu_ui", _locKeyPrefix + ".description");
			}
		}
		_description.Text = locString2.GetFormattedText();
		_description.ApplyLocaleFontSubstitution(FontType.Regular, ThemeConstants.RichTextLabel.normalFont);
		_description.ApplyLocaleFontSubstitution(FontType.Bold, ThemeConstants.RichTextLabel.boldFont);
		_description.ApplyLocaleFontSubstitution(FontType.Italic, ThemeConstants.RichTextLabel.italicsFont);
	}

	protected override void OnEnable()
	{
		base.Modulate = Colors.White;
		GetNode<TextureRect>("%Lock").Visible = false;
		_hsv.SetShaderParameter(_s, 1f);
		_icon.Modulate = Colors.White;
	}

	protected override void OnDisable()
	{
		base.Modulate = Colors.DarkGray;
		GetNode<TextureRect>("%Lock").Visible = true;
		_hsv.SetShaderParameter(_s, 0f);
		_icon.Modulate = new Color(0.5f, 0.5f, 0.5f, 0.5f);
	}

	protected override void OnFocus()
	{
		base.OnFocus();
		_scaleTween?.Kill();
		base.Scale = _hoverScale;
		_hsv.SetShaderParameter(_v, 1f);
	}

	protected override void OnUnfocus()
	{
		_scaleTween?.Kill();
		_scaleTween = CreateTween().SetParallel();
		_scaleTween.TweenProperty(this, "scale", Vector2.One, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		_scaleTween.TweenMethod(Callable.From<float>(UpdateShaderParam), 1f, _defaultV, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	private void UpdateShaderParam(float newV)
	{
		_hsv.SetShaderParameter(_v, newV);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(10);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ConnectSignals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetIconAndLocalization, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "locKeyPrefix", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Notification, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "what", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RefreshLabels, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnEnable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnDisable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.ConnectSignals && args.Count == 0)
		{
			ConnectSignals();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetIconAndLocalization && args.Count == 1)
		{
			SetIconAndLocalization(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Notification && args.Count == 1)
		{
			_Notification(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshLabels && args.Count == 0)
		{
			RefreshLabels();
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
		if (method == MethodName.ConnectSignals)
		{
			return true;
		}
		if (method == MethodName.SetIconAndLocalization)
		{
			return true;
		}
		if (method == MethodName._Notification)
		{
			return true;
		}
		if (method == MethodName.RefreshLabels)
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
		if (method == MethodName.UpdateShaderParam)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._hsv)
		{
			_hsv = VariantUtils.ConvertTo<ShaderMaterial>(in value);
			return true;
		}
		if (name == PropertyName._bgPanel)
		{
			_bgPanel = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._icon)
		{
			_icon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._title)
		{
			_title = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._description)
		{
			_description = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._locKeyPrefix)
		{
			_locKeyPrefix = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._defaultV)
		{
			_defaultV = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._scaleTween)
		{
			_scaleTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._hsv)
		{
			value = VariantUtils.CreateFrom(in _hsv);
			return true;
		}
		if (name == PropertyName._bgPanel)
		{
			value = VariantUtils.CreateFrom(in _bgPanel);
			return true;
		}
		if (name == PropertyName._icon)
		{
			value = VariantUtils.CreateFrom(in _icon);
			return true;
		}
		if (name == PropertyName._title)
		{
			value = VariantUtils.CreateFrom(in _title);
			return true;
		}
		if (name == PropertyName._description)
		{
			value = VariantUtils.CreateFrom(in _description);
			return true;
		}
		if (name == PropertyName._locKeyPrefix)
		{
			value = VariantUtils.CreateFrom(in _locKeyPrefix);
			return true;
		}
		if (name == PropertyName._defaultV)
		{
			value = VariantUtils.CreateFrom(in _defaultV);
			return true;
		}
		if (name == PropertyName._scaleTween)
		{
			value = VariantUtils.CreateFrom(in _scaleTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hsv, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bgPanel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._icon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._title, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._description, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName._locKeyPrefix, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._defaultV, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._scaleTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._hsv, Variant.From(in _hsv));
		info.AddProperty(PropertyName._bgPanel, Variant.From(in _bgPanel));
		info.AddProperty(PropertyName._icon, Variant.From(in _icon));
		info.AddProperty(PropertyName._title, Variant.From(in _title));
		info.AddProperty(PropertyName._description, Variant.From(in _description));
		info.AddProperty(PropertyName._locKeyPrefix, Variant.From(in _locKeyPrefix));
		info.AddProperty(PropertyName._defaultV, Variant.From(in _defaultV));
		info.AddProperty(PropertyName._scaleTween, Variant.From(in _scaleTween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._hsv, out var value))
		{
			_hsv = value.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._bgPanel, out var value2))
		{
			_bgPanel = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._icon, out var value3))
		{
			_icon = value3.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._title, out var value4))
		{
			_title = value4.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._description, out var value5))
		{
			_description = value5.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._locKeyPrefix, out var value6))
		{
			_locKeyPrefix = value6.As<string>();
		}
		if (info.TryGetProperty(PropertyName._defaultV, out var value7))
		{
			_defaultV = value7.As<float>();
		}
		if (info.TryGetProperty(PropertyName._scaleTween, out var value8))
		{
			_scaleTween = value8.As<Tween>();
		}
	}
}
