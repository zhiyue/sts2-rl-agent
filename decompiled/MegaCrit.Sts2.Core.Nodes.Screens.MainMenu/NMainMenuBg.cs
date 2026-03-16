using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

namespace MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

[ScriptPath("res://src/Core/Nodes/Screens/MainMenu/NMainMenuBg.cs")]
public class NMainMenuBg : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnWindowChange = "OnWindowChange";

		public static readonly StringName ScaleBgIfNarrow = "ScaleBgIfNarrow";

		public static readonly StringName HideLogo = "HideLogo";

		public static readonly StringName ShowLogo = "ShowLogo";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _window = "_window";

		public static readonly StringName _bg = "_bg";

		public static readonly StringName _logo = "_logo";

		public static readonly StringName _logoTween = "_logoTween";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private Window _window;

	private Control _bg;

	private Node2D _logo;

	private Tween? _logoTween;

	private static readonly Vector2 _defaultBgScale = Vector2.One * 1.01f;

	private const float _bgScaleRatioThreshold = 1.5f;

	public override void _Ready()
	{
		_bg = GetNode<Control>("BgContainer");
		_logo = GetNode<Node2D>("%Logo");
		_window = GetTree().Root;
		_window.Connect(Viewport.SignalName.SizeChanged, Callable.From(OnWindowChange));
	}

	private void OnWindowChange()
	{
		ScaleBgIfNarrow((float)_window.Size.X / (float)_window.Size.Y);
	}

	private void ScaleBgIfNarrow(float ratio)
	{
		if (ratio < 1.5f)
		{
			_bg.Scale = Vector2.One * 1.04f;
		}
		else
		{
			_bg.Scale = _defaultBgScale;
		}
	}

	public void HideLogo()
	{
		_logoTween?.Kill();
		_logoTween = CreateTween();
		_logoTween.TweenProperty(_logo, "modulate:a", 0f, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
	}

	public void ShowLogo()
	{
		_logoTween?.Kill();
		_logoTween = CreateTween();
		_logoTween.TweenProperty(_logo, "modulate:a", 1f, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnWindowChange, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ScaleBgIfNarrow, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "ratio", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.HideLogo, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ShowLogo, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.ScaleBgIfNarrow && args.Count == 1)
		{
			ScaleBgIfNarrow(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HideLogo && args.Count == 0)
		{
			HideLogo();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ShowLogo && args.Count == 0)
		{
			ShowLogo();
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
		if (method == MethodName.ScaleBgIfNarrow)
		{
			return true;
		}
		if (method == MethodName.HideLogo)
		{
			return true;
		}
		if (method == MethodName.ShowLogo)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._window)
		{
			_window = VariantUtils.ConvertTo<Window>(in value);
			return true;
		}
		if (name == PropertyName._bg)
		{
			_bg = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._logo)
		{
			_logo = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._logoTween)
		{
			_logoTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._window)
		{
			value = VariantUtils.CreateFrom(in _window);
			return true;
		}
		if (name == PropertyName._bg)
		{
			value = VariantUtils.CreateFrom(in _bg);
			return true;
		}
		if (name == PropertyName._logo)
		{
			value = VariantUtils.CreateFrom(in _logo);
			return true;
		}
		if (name == PropertyName._logoTween)
		{
			value = VariantUtils.CreateFrom(in _logoTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._window, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bg, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._logo, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._logoTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._window, Variant.From(in _window));
		info.AddProperty(PropertyName._bg, Variant.From(in _bg));
		info.AddProperty(PropertyName._logo, Variant.From(in _logo));
		info.AddProperty(PropertyName._logoTween, Variant.From(in _logoTween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._window, out var value))
		{
			_window = value.As<Window>();
		}
		if (info.TryGetProperty(PropertyName._bg, out var value2))
		{
			_bg = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._logo, out var value3))
		{
			_logo = value3.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._logoTween, out var value4))
		{
			_logoTween = value4.As<Tween>();
		}
	}
}
