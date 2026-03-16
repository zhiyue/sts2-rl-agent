using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.CommonUi;

[ScriptPath("res://src/Core/Nodes/CommonUi/NCommonBanner.cs")]
public class NCommonBanner : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnWindowChange = "OnWindowChange";

		public static readonly StringName AnimateIn = "AnimateIn";

		public static readonly StringName AnimateOut = "AnimateOut";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName label = "label";

		public static readonly StringName _tween = "_tween";

		public static readonly StringName _showPos = "_showPos";

		public static readonly StringName _hidePos = "_hidePos";

		public static readonly StringName _imgOffset = "_imgOffset";
	}

	public new class SignalName : Control.SignalName
	{
	}

	public MegaLabel label;

	private Tween? _tween;

	private Vector2 _showPos;

	private Vector2 _hidePos;

	private static readonly Vector2 _hideOffset = new Vector2(0f, 50f);

	private Vector2 _imgOffset;

	public override void _Ready()
	{
		label = GetNode<MegaLabel>("MegaLabel");
		base.Modulate = StsColors.transparentWhite;
		_imgOffset = new Vector2(GetViewportRect().Size.X * 0.5f, GetViewportRect().Size.Y * 0.5f) - base.GlobalPosition;
		GetTree().Root.Connect(Viewport.SignalName.SizeChanged, Callable.From(OnWindowChange));
		OnWindowChange();
	}

	private void OnWindowChange()
	{
		_showPos = new Vector2(GetViewportRect().Size.X * 0.5f, GetViewportRect().Size.Y * 0.5f) - _imgOffset;
		_hidePos = _showPos + _hideOffset;
		base.Position = _showPos;
	}

	public void AnimateIn()
	{
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(this, "modulate:a", 1f, 0.4).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_tween.TweenProperty(this, "global_position", _showPos, 0.4).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back)
			.From(_hidePos);
	}

	public void AnimateOut()
	{
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(this, "modulate:a", 0f, 0.4).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnWindowChange, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimateIn, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimateOut, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.AnimateIn && args.Count == 0)
		{
			AnimateIn();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimateOut && args.Count == 0)
		{
			AnimateOut();
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
		if (method == MethodName.AnimateIn)
		{
			return true;
		}
		if (method == MethodName.AnimateOut)
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
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._showPos)
		{
			_showPos = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._hidePos)
		{
			_hidePos = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._imgOffset)
		{
			_imgOffset = VariantUtils.ConvertTo<Vector2>(in value);
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
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._showPos)
		{
			value = VariantUtils.CreateFrom(in _showPos);
			return true;
		}
		if (name == PropertyName._hidePos)
		{
			value = VariantUtils.CreateFrom(in _hidePos);
			return true;
		}
		if (name == PropertyName._imgOffset)
		{
			value = VariantUtils.CreateFrom(in _imgOffset);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.label, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._showPos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._hidePos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._imgOffset, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.label, Variant.From(in label));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._showPos, Variant.From(in _showPos));
		info.AddProperty(PropertyName._hidePos, Variant.From(in _hidePos));
		info.AddProperty(PropertyName._imgOffset, Variant.From(in _imgOffset));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.label, out var value))
		{
			label = value.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value2))
		{
			_tween = value2.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._showPos, out var value3))
		{
			_showPos = value3.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._hidePos, out var value4))
		{
			_hidePos = value4.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._imgOffset, out var value5))
		{
			_imgOffset = value5.As<Vector2>();
		}
	}
}
