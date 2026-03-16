using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NSmokyVignetteVfx.cs")]
public class NSmokyVignetteVfx : Control
{
	public new class MethodName : Control.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName Reset = "Reset";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _highlights = "_highlights";

		public static readonly StringName _targetColor = "_targetColor";

		public static readonly StringName _highlightColor = "_highlightColor";

		public static readonly StringName _tween = "_tween";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private const string _path = "res://scenes/vfx/whole_screen/vfx_smoky_vignette.tscn";

	private Control _highlights;

	private Color _targetColor;

	private Color _highlightColor;

	private Tween? _tween;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>("res://scenes/vfx/whole_screen/vfx_smoky_vignette.tscn");

	public static NSmokyVignetteVfx? Create(Color tint, Color highlightColor)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NSmokyVignetteVfx nSmokyVignetteVfx = PreloadManager.Cache.GetScene("res://scenes/vfx/whole_screen/vfx_smoky_vignette.tscn").Instantiate<NSmokyVignetteVfx>(PackedScene.GenEditState.Disabled);
		nSmokyVignetteVfx.Modulate = tint;
		nSmokyVignetteVfx._targetColor = tint;
		nSmokyVignetteVfx._highlightColor = highlightColor;
		return nSmokyVignetteVfx;
	}

	public override void _Ready()
	{
		_highlights = GetNode<Control>("Highlights");
		_highlights.Modulate = _highlightColor;
		TaskHelper.RunSafely(Animate(fadeIn: true));
	}

	public void Reset(Color tint, Color highlightColor)
	{
		_targetColor = tint;
		_highlightColor = highlightColor;
		_tween?.Kill();
		TaskHelper.RunSafely(Animate(fadeIn: false));
	}

	private async Task Animate(bool fadeIn)
	{
		_tween = CreateTween().SetParallel();
		if (fadeIn)
		{
			_tween.TweenProperty(this, "modulate:a", _targetColor.A, 0.1).From(0f);
			_tween.TweenProperty(_highlights, "modulate:a", _highlightColor.A, 0.1).From(0f);
		}
		_tween.Chain();
		_tween.TweenProperty(this, "modulate:a", 0f, 1.0).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Cubic);
		_tween.TweenProperty(_highlights, "modulate:a", 0f, 1.0);
		while (_tween.IsValid() && _tween.IsRunning())
		{
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}
		if (_tween.IsValid())
		{
			this.QueueFreeSafely();
		}
	}

	public override void _ExitTree()
	{
		_tween?.Kill();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Color, "tint", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Color, "highlightColor", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Reset, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Color, "tint", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Color, "highlightColor", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<NSmokyVignetteVfx>(Create(VariantUtils.ConvertTo<Color>(in args[0]), VariantUtils.ConvertTo<Color>(in args[1])));
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Reset && args.Count == 2)
		{
			Reset(VariantUtils.ConvertTo<Color>(in args[0]), VariantUtils.ConvertTo<Color>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
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
			ret = VariantUtils.CreateFrom<NSmokyVignetteVfx>(Create(VariantUtils.ConvertTo<Color>(in args[0]), VariantUtils.ConvertTo<Color>(in args[1])));
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.Create)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.Reset)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._highlights)
		{
			_highlights = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._targetColor)
		{
			_targetColor = VariantUtils.ConvertTo<Color>(in value);
			return true;
		}
		if (name == PropertyName._highlightColor)
		{
			_highlightColor = VariantUtils.ConvertTo<Color>(in value);
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
		if (name == PropertyName._highlights)
		{
			value = VariantUtils.CreateFrom(in _highlights);
			return true;
		}
		if (name == PropertyName._targetColor)
		{
			value = VariantUtils.CreateFrom(in _targetColor);
			return true;
		}
		if (name == PropertyName._highlightColor)
		{
			value = VariantUtils.CreateFrom(in _highlightColor);
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
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._highlights, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName._targetColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName._highlightColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._highlights, Variant.From(in _highlights));
		info.AddProperty(PropertyName._targetColor, Variant.From(in _targetColor));
		info.AddProperty(PropertyName._highlightColor, Variant.From(in _highlightColor));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._highlights, out var value))
		{
			_highlights = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._targetColor, out var value2))
		{
			_targetColor = value2.As<Color>();
		}
		if (info.TryGetProperty(PropertyName._highlightColor, out var value3))
		{
			_highlightColor = value3.As<Color>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value4))
		{
			_tween = value4.As<Tween>();
		}
	}
}
