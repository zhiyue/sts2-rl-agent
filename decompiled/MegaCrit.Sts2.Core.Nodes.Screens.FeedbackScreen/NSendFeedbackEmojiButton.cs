using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace MegaCrit.Sts2.Core.Nodes.Screens.FeedbackScreen;

[ScriptPath("res://src/Core/Nodes/Screens/FeedbackScreen/NSendFeedbackEmojiButton.cs")]
public class NSendFeedbackEmojiButton : NButton
{
	public new class MethodName : NButton.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";

		public static readonly StringName SetSelected = "SetSelected";

		public static readonly StringName FlashError = "FlashError";
	}

	public new class PropertyName : NButton.PropertyName
	{
		public static readonly StringName _tween = "_tween";

		public static readonly StringName _isSelected = "_isSelected";

		public static readonly StringName _selectionReticle = "_selectionReticle";
	}

	public new class SignalName : NButton.SignalName
	{
	}

	private static readonly Color _defaultColor = new Color(0.1f, 0.1f, 0.1f);

	private Tween? _tween;

	private bool _isSelected;

	private NSelectionReticle _selectionReticle;

	public override void _Ready()
	{
		ConnectSignals();
		_selectionReticle = GetNode<NSelectionReticle>("SelectionReticle");
		base.PivotOffset = base.Size * 0.5f;
	}

	protected override void OnFocus()
	{
		base.OnFocus();
		base.Scale = Vector2.One * 1.2f;
		if (NControllerManager.Instance.IsUsingController)
		{
			_selectionReticle.OnSelect();
		}
	}

	protected override void OnUnfocus()
	{
		base.OnFocus();
		if (!_isSelected)
		{
			_selectionReticle.OnDeselect();
			base.Scale = Vector2.One;
		}
	}

	public void SetSelected(bool isSelected)
	{
		if (isSelected)
		{
			base.Scale = Vector2.One * 1.2f;
			base.SelfModulate = StsColors.blueGlow;
		}
		else
		{
			base.Scale = Vector2.One;
			base.SelfModulate = _defaultColor;
		}
		_isSelected = isSelected;
	}

	public void FlashError()
	{
		_tween?.Kill();
		_tween = CreateTween();
		_tween.TweenProperty(this, "self_modulate", StsColors.redGlow, 0.10000000149011612);
		_tween.TweenProperty(this, "self_modulate", _defaultColor, 0.10000000149011612);
		_tween.TweenProperty(this, "self_modulate", StsColors.redGlow, 0.10000000149011612);
		_tween.TweenProperty(this, "self_modulate", _defaultColor, 0.10000000149011612);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetSelected, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "isSelected", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.FlashError, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.SetSelected && args.Count == 1)
		{
			SetSelected(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FlashError && args.Count == 0)
		{
			FlashError();
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
		if (method == MethodName.OnFocus)
		{
			return true;
		}
		if (method == MethodName.OnUnfocus)
		{
			return true;
		}
		if (method == MethodName.SetSelected)
		{
			return true;
		}
		if (method == MethodName.FlashError)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._isSelected)
		{
			_isSelected = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._selectionReticle)
		{
			_selectionReticle = VariantUtils.ConvertTo<NSelectionReticle>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._isSelected)
		{
			value = VariantUtils.CreateFrom(in _isSelected);
			return true;
		}
		if (name == PropertyName._selectionReticle)
		{
			value = VariantUtils.CreateFrom(in _selectionReticle);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isSelected, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._selectionReticle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._isSelected, Variant.From(in _isSelected));
		info.AddProperty(PropertyName._selectionReticle, Variant.From(in _selectionReticle));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._tween, out var value))
		{
			_tween = value.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._isSelected, out var value2))
		{
			_isSelected = value2.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._selectionReticle, out var value3))
		{
			_selectionReticle = value3.As<NSelectionReticle>();
		}
	}
}
