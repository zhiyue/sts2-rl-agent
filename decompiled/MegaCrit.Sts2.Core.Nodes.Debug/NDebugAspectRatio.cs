using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;

namespace MegaCrit.Sts2.Core.Nodes.Debug;

[ScriptPath("res://src/Core/Nodes/Debug/NDebugAspectRatio.cs")]
public class NDebugAspectRatio : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnWindowChange = "OnWindowChange";

		public static readonly StringName ScaleBgIfNarrow = "ScaleBgIfNarrow";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _window = "_window";

		public static readonly StringName _infoLabel = "_infoLabel";

		public static readonly StringName _bg = "_bg";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private Window _window;

	private Label _infoLabel;

	private TextureRect _bg;

	private const float _maxNarrowRatio = 1.3333334f;

	private const float _maxWideRatio = 2.3888888f;

	private static readonly Vector2 _defaultBgScale = Vector2.One * 1.01f;

	private const float _bgScaleRatioThreshold = 1.5f;

	public override void _Ready()
	{
		_bg = GetNode<TextureRect>("EventBg");
		_infoLabel = GetNode<Label>("Anchors/Label");
		_window = GetTree().Root;
		_window.Connect(Viewport.SignalName.SizeChanged, Callable.From(OnWindowChange));
	}

	private void OnWindowChange()
	{
		float num = (float)_window.Size.X / (float)_window.Size.Y;
		string value = num.ToString("0.000");
		ScaleBgIfNarrow(num);
		if (num > 2.3888888f)
		{
			_window.ContentScaleAspect = Window.ContentScaleAspectEnum.KeepWidth;
			_window.ContentScaleSize = new Vector2I(2580, 1080);
			_infoLabel.Text = $"{value}: {_window.Size}";
			_infoLabel.Modulate = StsColors.red;
		}
		else if (num < 1.3333334f)
		{
			_window.ContentScaleAspect = Window.ContentScaleAspectEnum.KeepHeight;
			_window.ContentScaleSize = new Vector2I(1680, 1260);
			_infoLabel.Text = $"{value}: {_window.Size}";
			_infoLabel.Modulate = StsColors.red;
		}
		else
		{
			_window.ContentScaleAspect = Window.ContentScaleAspectEnum.Expand;
			_window.ContentScaleSize = new Vector2I(1680, 1080);
			_infoLabel.Text = $"{value}: {_window.Size}";
			_infoLabel.Modulate = StsColors.cream;
		}
	}

	private void ScaleBgIfNarrow(float ratio)
	{
		if (ratio < 1.5f)
		{
			_bg.Scale = Vector2.One * 1.05f;
		}
		else
		{
			_bg.Scale = _defaultBgScale;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnWindowChange, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ScaleBgIfNarrow, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "ratio", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (name == PropertyName._infoLabel)
		{
			_infoLabel = VariantUtils.ConvertTo<Label>(in value);
			return true;
		}
		if (name == PropertyName._bg)
		{
			_bg = VariantUtils.ConvertTo<TextureRect>(in value);
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
		if (name == PropertyName._infoLabel)
		{
			value = VariantUtils.CreateFrom(in _infoLabel);
			return true;
		}
		if (name == PropertyName._bg)
		{
			value = VariantUtils.CreateFrom(in _bg);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._window, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._infoLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bg, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._window, Variant.From(in _window));
		info.AddProperty(PropertyName._infoLabel, Variant.From(in _infoLabel));
		info.AddProperty(PropertyName._bg, Variant.From(in _bg));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._window, out var value))
		{
			_window = value.As<Window>();
		}
		if (info.TryGetProperty(PropertyName._infoLabel, out var value2))
		{
			_infoLabel = value2.As<Label>();
		}
		if (info.TryGetProperty(PropertyName._bg, out var value3))
		{
			_bg = value3.As<TextureRect>();
		}
	}
}
