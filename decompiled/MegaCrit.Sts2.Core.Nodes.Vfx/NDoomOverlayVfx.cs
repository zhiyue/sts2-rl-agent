using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NDoomOverlayVfx.cs")]
public class NDoomOverlayVfx : BackBufferCopy
{
	public new class MethodName : BackBufferCopy.MethodName
	{
		public static readonly StringName GetOrCreate = "GetOrCreate";

		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName PlayVfx = "PlayVfx";

		public static readonly StringName OnTweenFinished = "OnTweenFinished";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : BackBufferCopy.PropertyName
	{
		public static readonly StringName _tween = "_tween";
	}

	public new class SignalName : BackBufferCopy.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("vfx/doom_overlay_vfx");

	private static NDoomOverlayVfx? _instance;

	private Tween? _tween;

	public static NDoomOverlayVfx? GetOrCreate()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		if (_instance != null)
		{
			_instance.PlayVfx();
		}
		else
		{
			_instance = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NDoomOverlayVfx>(PackedScene.GenEditState.Disabled);
		}
		if (!GodotObject.IsInstanceValid(_instance))
		{
			return null;
		}
		return _instance;
	}

	public override void _Ready()
	{
		base.Modulate = Colors.Transparent;
		GetNode<Control>("%Rect").SetDeferred(Control.PropertyName.Size, GetViewportRect().Size);
		PlayVfx();
	}

	private void PlayVfx()
	{
		if (GodotObject.IsInstanceValid(this))
		{
			_tween?.Kill();
			_tween = CreateTween();
			_tween.TweenProperty(this, "modulate:a", 1f, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
			_tween.TweenInterval(1.2000000476837158);
			_tween.TweenProperty(this, "modulate:a", 0f, 0.5);
			_tween.TweenCallback(Callable.From(delegate
			{
				_instance = null;
			}));
			_tween.Finished += OnTweenFinished;
		}
	}

	private void OnTweenFinished()
	{
		this.QueueFreeSafely();
	}

	public override void _ExitTree()
	{
		_tween?.Kill();
		if (_tween != null)
		{
			_tween.Finished -= OnTweenFinished;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName.GetOrCreate, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("BackBufferCopy"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.PlayVfx, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnTweenFinished, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.GetOrCreate && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NDoomOverlayVfx>(GetOrCreate());
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PlayVfx && args.Count == 0)
		{
			PlayVfx();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnTweenFinished && args.Count == 0)
		{
			OnTweenFinished();
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
		if (method == MethodName.GetOrCreate && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NDoomOverlayVfx>(GetOrCreate());
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.GetOrCreate)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.PlayVfx)
		{
			return true;
		}
		if (method == MethodName.OnTweenFinished)
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._tween, out var value))
		{
			_tween = value.As<Tween>();
		}
	}
}
