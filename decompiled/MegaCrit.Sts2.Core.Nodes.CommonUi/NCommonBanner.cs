using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.CommonUi;

/// <summary>
/// The beige banner which appears on many many screens. Choose a Card!
/// </summary>
[ScriptPath("res://src/Core/Nodes/CommonUi/NCommonBanner.cs")]
public class NCommonBanner : Control
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Control.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'OnWindowChange' method.
		/// </summary>
		public static readonly StringName OnWindowChange = "OnWindowChange";

		/// <summary>
		/// Cached name for the 'AnimateIn' method.
		/// </summary>
		public static readonly StringName AnimateIn = "AnimateIn";

		/// <summary>
		/// Cached name for the 'AnimateOut' method.
		/// </summary>
		public static readonly StringName AnimateOut = "AnimateOut";

		/// <summary>
		/// Cached name for the 'ChangeText' method.
		/// </summary>
		public static readonly StringName ChangeText = "ChangeText";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
		/// <summary>
		/// Cached name for the 'label' field.
		/// </summary>
		public static readonly StringName label = "label";

		/// <summary>
		/// Cached name for the '_labelTween' field.
		/// </summary>
		public static readonly StringName _labelTween = "_labelTween";

		/// <summary>
		/// Cached name for the '_tween' field.
		/// </summary>
		public static readonly StringName _tween = "_tween";

		/// <summary>
		/// Cached name for the '_showPos' field.
		/// </summary>
		public static readonly StringName _showPos = "_showPos";

		/// <summary>
		/// Cached name for the '_hidePos' field.
		/// </summary>
		public static readonly StringName _hidePos = "_hidePos";

		/// <summary>
		/// Cached name for the '_imgOffset' field.
		/// </summary>
		public static readonly StringName _imgOffset = "_imgOffset";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
	}

	/// NOTE: This is set to public for modders as it's a common UI element.
	public MegaLabel label;

	private Tween? _labelTween;

	private Tween? _tween;

	private Vector2 _showPos;

	private Vector2 _hidePos;

	private static readonly Vector2 _hideOffset = new Vector2(0f, 50f);

	private Vector2 _imgOffset;

	public override void _Ready()
	{
		label = GetNode<MegaLabel>("%Label");
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

	/// <summary>
	/// Call this when you want to change the text on an existing banner without animating the banner again.
	/// </summary>
	public void ChangeText(string text)
	{
		label.SetTextAutoSize(text);
		label.Modulate = StsColors.transparentWhite;
		_labelTween?.Kill();
		_labelTween = CreateTween();
		_labelTween.TweenProperty(label, "modulate:a", 1f, 0.25);
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnWindowChange, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimateIn, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimateOut, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ChangeText, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "text", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	/// <inheritdoc />
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
		if (method == MethodName.ChangeText && args.Count == 1)
		{
			ChangeText(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	/// <inheritdoc />
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
		if (method == MethodName.ChangeText)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.label)
		{
			label = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._labelTween)
		{
			_labelTween = VariantUtils.ConvertTo<Tween>(in value);
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

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.label)
		{
			value = VariantUtils.CreateFrom(in label);
			return true;
		}
		if (name == PropertyName._labelTween)
		{
			value = VariantUtils.CreateFrom(in _labelTween);
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

	/// <summary>
	/// Get the property information for all the properties declared in this class.
	/// This method is used by Godot to register the available properties in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.label, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._labelTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._showPos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._hidePos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._imgOffset, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.label, Variant.From(in label));
		info.AddProperty(PropertyName._labelTween, Variant.From(in _labelTween));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._showPos, Variant.From(in _showPos));
		info.AddProperty(PropertyName._hidePos, Variant.From(in _hidePos));
		info.AddProperty(PropertyName._imgOffset, Variant.From(in _imgOffset));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.label, out var value))
		{
			label = value.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._labelTween, out var value2))
		{
			_labelTween = value2.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value3))
		{
			_tween = value3.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._showPos, out var value4))
		{
			_showPos = value4.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._hidePos, out var value5))
		{
			_hidePos = value5.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._imgOffset, out var value6))
		{
			_imgOffset = value6.As<Vector2>();
		}
	}
}
