using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NEpochOffscreenVfx.cs")]
public class NEpochOffscreenVfx : Control
{
	public new class MethodName : Control.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _Process = "_Process";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _slot = "_slot";

		public static readonly StringName _tween = "_tween";

		public static readonly StringName _showVfx = "_showVfx";

		public static readonly StringName _viewportSizeX = "_viewportSizeX";
	}

	public new class SignalName : Control.SignalName
	{
	}

	public static readonly string scenePath = SceneHelper.GetScenePath("timeline_screen/epoch_offscreen_vfx");

	private NEpochSlot _slot;

	private Tween? _tween;

	private bool _showVfx;

	private float _viewportSizeX;

	public static NEpochOffscreenVfx Create(NEpochSlot slot)
	{
		NEpochOffscreenVfx nEpochOffscreenVfx = PreloadManager.Cache.GetScene(scenePath).Instantiate<NEpochOffscreenVfx>(PackedScene.GenEditState.Disabled);
		nEpochOffscreenVfx._slot = slot;
		return nEpochOffscreenVfx;
	}

	public override void _Ready()
	{
		_tween = CreateTween().SetParallel();
		_viewportSizeX = GetViewportRect().Size.X;
	}

	public override void _Process(double delta)
	{
		if (!_showVfx)
		{
			if (_slot.GlobalPosition.X < 0f)
			{
				_showVfx = true;
				_tween?.Kill();
				_tween = CreateTween();
				_tween.TweenProperty(this, "modulate:a", 0.75f, 0.5);
				base.GlobalPosition = new Vector2(0f, _slot.GlobalPosition.Y + 60f);
			}
			else if (_slot.GlobalPosition.X > _viewportSizeX)
			{
				_showVfx = true;
				_tween?.Kill();
				_tween = CreateTween();
				_tween.TweenProperty(this, "modulate:a", 0.75f, 0.5);
				base.GlobalPosition = new Vector2(_viewportSizeX, _slot.GlobalPosition.Y + 60f);
			}
		}
		else if (_slot.GlobalPosition.X > 0f && _slot.GlobalPosition.X < _viewportSizeX)
		{
			_showVfx = false;
			_tween?.Kill();
			_tween = CreateTween();
			_tween.TweenProperty(this, "modulate:a", 0f, 0.2);
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
			new PropertyInfo(Variant.Type.Object, "slot", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NEpochOffscreenVfx>(Create(VariantUtils.ConvertTo<NEpochSlot>(in args[0])));
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
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
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NEpochOffscreenVfx>(Create(VariantUtils.ConvertTo<NEpochSlot>(in args[0])));
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
		if (method == MethodName._Process)
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
		if (name == PropertyName._slot)
		{
			_slot = VariantUtils.ConvertTo<NEpochSlot>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._showVfx)
		{
			_showVfx = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._viewportSizeX)
		{
			_viewportSizeX = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._slot)
		{
			value = VariantUtils.CreateFrom(in _slot);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._showVfx)
		{
			value = VariantUtils.CreateFrom(in _showVfx);
			return true;
		}
		if (name == PropertyName._viewportSizeX)
		{
			value = VariantUtils.CreateFrom(in _viewportSizeX);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._slot, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._showVfx, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._viewportSizeX, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._slot, Variant.From(in _slot));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._showVfx, Variant.From(in _showVfx));
		info.AddProperty(PropertyName._viewportSizeX, Variant.From(in _viewportSizeX));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._slot, out var value))
		{
			_slot = value.As<NEpochSlot>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value2))
		{
			_tween = value2.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._showVfx, out var value3))
		{
			_showVfx = value3.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._viewportSizeX, out var value4))
		{
			_viewportSizeX = value4.As<float>();
		}
	}
}
