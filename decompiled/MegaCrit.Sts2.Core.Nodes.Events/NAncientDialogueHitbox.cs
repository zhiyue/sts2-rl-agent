using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Events;

[ScriptPath("res://src/Core/Nodes/Events/NAncientDialogueHitbox.cs")]
public class NAncientDialogueHitbox : NButton
{
	public new class MethodName : NButton.MethodName
	{
		public static readonly StringName GetHotkey = "GetHotkey";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName OnRelease = "OnRelease";

		public new static readonly StringName OnPress = "OnPress";

		public new static readonly StringName OnFocus = "OnFocus";
	}

	public new class PropertyName : NButton.PropertyName
	{
		public new static readonly StringName Hotkeys = "Hotkeys";

		public static readonly StringName _label = "_label";

		public static readonly StringName _arrow = "_arrow";

		public static readonly StringName _loopTween = "_loopTween";

		public static readonly StringName _tween = "_tween";

		public static readonly StringName _isAnimating = "_isAnimating";
	}

	public new class SignalName : NButton.SignalName
	{
	}

	private MegaLabel _label;

	private TextureRect _arrow;

	private Tween? _loopTween;

	private Tween? _tween;

	private bool _isAnimating;

	protected override string[] Hotkeys => new string[1] { MegaInput.accept };

	public string? GetHotkey()
	{
		return Hotkeys.FirstOrDefault();
	}

	public override void _Ready()
	{
		ConnectSignals();
		_label = GetNode<MegaLabel>("%Label");
		_label.SelfModulate = StsColors.transparentWhite;
		_label.Text = string.Empty;
		_arrow = GetNode<TextureRect>("%Arrow");
		_arrow.SelfModulate = StsColors.transparentWhite;
		_loopTween = CreateTween().SetParallel().SetLoops();
		_loopTween.TweenProperty(_arrow, "position:x", _arrow.Position.X + 4f, 0.4).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
		_loopTween.Chain().TweenProperty(_arrow, "position:x", _arrow.Position.X - 4f, 0.6).SetEase(Tween.EaseType.InOut)
			.SetTrans(Tween.TransitionType.Sine);
		Tween tween = CreateTween().SetParallel();
		tween.TweenProperty(_label, "self_modulate:a", 1f, 1.0).SetDelay(0.5);
		tween.TweenProperty(_arrow, "self_modulate:a", 1f, 1.0).SetDelay(0.5);
	}

	protected override void OnRelease()
	{
		base.OnRelease();
		_loopTween?.Play();
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_label, "scale", Vector2.One, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_tween.TweenProperty(_arrow, "scale", Vector2.One, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	protected override void OnPress()
	{
		base.OnPress();
		_loopTween?.Pause();
		_label.PivotOffset = _label.Size * 0.5f;
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_label, "scale", Vector2.One * 0.9f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_tween.TweenProperty(_arrow, "scale", Vector2.One * 0.9f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_isAnimating = false;
	}

	protected override void OnFocus()
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName.GetHotkey, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnRelease, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPress, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.GetHotkey && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<string>(GetHotkey());
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnRelease && args.Count == 0)
		{
			OnRelease();
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
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.GetHotkey)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.OnRelease)
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
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._label)
		{
			_label = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._arrow)
		{
			_arrow = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._loopTween)
		{
			_loopTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._isAnimating)
		{
			_isAnimating = VariantUtils.ConvertTo<bool>(in value);
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
		if (name == PropertyName._label)
		{
			value = VariantUtils.CreateFrom(in _label);
			return true;
		}
		if (name == PropertyName._arrow)
		{
			value = VariantUtils.CreateFrom(in _arrow);
			return true;
		}
		if (name == PropertyName._loopTween)
		{
			value = VariantUtils.CreateFrom(in _loopTween);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._isAnimating)
		{
			value = VariantUtils.CreateFrom(in _isAnimating);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.PackedStringArray, PropertyName.Hotkeys, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._label, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._arrow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._loopTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isAnimating, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._label, Variant.From(in _label));
		info.AddProperty(PropertyName._arrow, Variant.From(in _arrow));
		info.AddProperty(PropertyName._loopTween, Variant.From(in _loopTween));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._isAnimating, Variant.From(in _isAnimating));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._label, out var value))
		{
			_label = value.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._arrow, out var value2))
		{
			_arrow = value2.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._loopTween, out var value3))
		{
			_loopTween = value3.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value4))
		{
			_tween = value4.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._isAnimating, out var value5))
		{
			_isAnimating = value5.As<bool>();
		}
	}
}
