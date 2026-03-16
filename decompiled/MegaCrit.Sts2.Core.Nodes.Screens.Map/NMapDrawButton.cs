using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Map;

[ScriptPath("res://src/Core/Nodes/Screens/Map/NMapDrawButton.cs")]
public class NMapDrawButton : NButton
{
	public new class MethodName : NButton.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName SetIsDrawing = "SetIsDrawing";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";

		public static readonly StringName OnControllerUpdated = "OnControllerUpdated";
	}

	public new class PropertyName : NButton.PropertyName
	{
		public static readonly StringName _drawingToolHolder = "_drawingToolHolder";

		public static readonly StringName _icon = "_icon";

		public static readonly StringName _tween = "_tween";

		public static readonly StringName _isDrawing = "_isDrawing";
	}

	public new class SignalName : NButton.SignalName
	{
	}

	private static readonly StringName _imagePath = "res://images/packed/map/drawing_quill.png";

	private static readonly StringName _glowImagePath = "res://images/packed/map/drawing_quill_glow.png";

	private Control _drawingToolHolder;

	private TextureRect _icon;

	private HoverTip _hoverTip;

	private Tween? _tween;

	private bool _isDrawing;

	private static readonly Color _activeColor = new Color("57C4FFFF");

	private static readonly Color _inactiveColor = new Color("FFFFFF80");

	public static IEnumerable<string> AssetPaths
	{
		get
		{
			List<string> list = new List<string>();
			list.Add(_imagePath);
			list.Add(_glowImagePath);
			list.AddRange(NMapEraseButton.AssetPaths);
			return new _003C_003Ez__ReadOnlyList<string>(list);
		}
	}

	public override void _Ready()
	{
		ConnectSignals();
		_drawingToolHolder = (Control)GetParent();
		_icon = GetNode<TextureRect>("Icon");
		NControllerManager.Instance.Connect(NControllerManager.SignalName.ControllerDetected, Callable.From(OnControllerUpdated));
		NControllerManager.Instance.Connect(NControllerManager.SignalName.MouseDetected, Callable.From(OnControllerUpdated));
		OnControllerUpdated();
	}

	public void SetIsDrawing(bool isDrawing)
	{
		_isDrawing = isDrawing;
		_icon.Texture = PreloadManager.Cache.GetTexture2D(isDrawing ? _glowImagePath : _imagePath);
		_icon.SelfModulate = (isDrawing ? _activeColor : _inactiveColor);
	}

	protected override void OnFocus()
	{
		base.OnFocus();
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_icon, "scale", Vector2.One * 1.2f, 0.05);
		_tween.TweenProperty(_icon, "self_modulate", _activeColor, 0.05);
		NHoverTipSet nHoverTipSet = NHoverTipSet.CreateAndShow(_drawingToolHolder, _hoverTip);
		nHoverTipSet.GlobalPosition = _drawingToolHolder.GlobalPosition + new Vector2(10f, -132f);
	}

	protected override void OnUnfocus()
	{
		base.OnUnfocus();
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_icon, "scale", Vector2.One * 1.1f, 0.05);
		_tween.TweenProperty(_icon, "self_modulate", _isDrawing ? _activeColor : _inactiveColor, 0.05);
		NHoverTipSet.Remove(_drawingToolHolder);
	}

	private void OnControllerUpdated()
	{
		LocString description = new LocString("map", "DRAWING_BUTTON.description");
		LocString title = ((!NControllerManager.Instance.IsUsingController) ? new LocString("map", "DRAWING_BUTTON.title_mkb") : new LocString("map", "DRAWING_BUTTON.title_controller"));
		_hoverTip = new HoverTip(title, description);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetIsDrawing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "isDrawing", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnControllerUpdated, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.SetIsDrawing && args.Count == 1)
		{
			SetIsDrawing(VariantUtils.ConvertTo<bool>(in args[0]));
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
		if (method == MethodName.OnControllerUpdated && args.Count == 0)
		{
			OnControllerUpdated();
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
		if (method == MethodName.SetIsDrawing)
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
		if (method == MethodName.OnControllerUpdated)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._drawingToolHolder)
		{
			_drawingToolHolder = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._icon)
		{
			_icon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._isDrawing)
		{
			_isDrawing = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._drawingToolHolder)
		{
			value = VariantUtils.CreateFrom(in _drawingToolHolder);
			return true;
		}
		if (name == PropertyName._icon)
		{
			value = VariantUtils.CreateFrom(in _icon);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._isDrawing)
		{
			value = VariantUtils.CreateFrom(in _isDrawing);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._drawingToolHolder, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._icon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isDrawing, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._drawingToolHolder, Variant.From(in _drawingToolHolder));
		info.AddProperty(PropertyName._icon, Variant.From(in _icon));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._isDrawing, Variant.From(in _isDrawing));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._drawingToolHolder, out var value))
		{
			_drawingToolHolder = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._icon, out var value2))
		{
			_icon = value2.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value3))
		{
			_tween = value3.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._isDrawing, out var value4))
		{
			_isDrawing = value4.As<bool>();
		}
	}
}
