using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

/// <summary>
/// A simple vfx which renders a colorRect to tint the entire screen using an additive blend.
/// </summary>
[ScriptPath("res://src/Core/Nodes/Vfx/NAdditiveOverlayVfx.cs")]
public class NAdditiveOverlayVfx : ColorRect
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : ColorRect.MethodName
	{
		/// <summary>
		/// Cached name for the 'Create' method.
		/// </summary>
		public static readonly StringName Create = "Create";

		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'SetVfxColor' method.
		/// </summary>
		public static readonly StringName SetVfxColor = "SetVfxColor";

		/// <summary>
		/// Cached name for the 'OnTweenFinished' method.
		/// </summary>
		public static readonly StringName OnTweenFinished = "OnTweenFinished";

		/// <summary>
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : ColorRect.PropertyName
	{
		/// <summary>
		/// Cached name for the '_vfxColor' field.
		/// </summary>
		public static readonly StringName _vfxColor = "_vfxColor";

		/// <summary>
		/// Cached name for the '_tween' field.
		/// </summary>
		public static readonly StringName _tween = "_tween";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : ColorRect.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("vfx/additive_overlay_vfx");

	private VfxColor _vfxColor;

	private Tween? _tween;

	public static NAdditiveOverlayVfx? Create(VfxColor vfxColor = VfxColor.Red)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NAdditiveOverlayVfx nAdditiveOverlayVfx = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NAdditiveOverlayVfx>(PackedScene.GenEditState.Disabled);
		nAdditiveOverlayVfx._vfxColor = vfxColor;
		return nAdditiveOverlayVfx;
	}

	public override void _Ready()
	{
		SetVfxColor();
		_tween = CreateTween();
		_tween.TweenProperty(this, "modulate:a", 0.1f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_tween.TweenInterval(0.5);
		_tween.TweenProperty(this, "modulate:a", 0f, 0.5);
		_tween.Finished += OnTweenFinished;
	}

	private void SetVfxColor()
	{
		switch (_vfxColor)
		{
		case VfxColor.Green:
			base.Modulate = new Color("00ff1500");
			break;
		case VfxColor.Blue:
			base.Modulate = new Color("001aff00");
			break;
		case VfxColor.Purple:
			base.Modulate = new Color("b300ff00");
			break;
		case VfxColor.White:
			base.Modulate = new Color("ffffff00");
			break;
		case VfxColor.Cyan:
			base.Modulate = new Color("00fffb00");
			break;
		case VfxColor.Gold:
			base.Modulate = new Color("b17e0000");
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case VfxColor.Red:
		case VfxColor.Black:
			break;
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

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("ColorRect"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "vfxColor", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetVfxColor, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnTweenFinished, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NAdditiveOverlayVfx>(Create(VariantUtils.ConvertTo<VfxColor>(in args[0])));
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetVfxColor && args.Count == 0)
		{
			SetVfxColor();
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
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NAdditiveOverlayVfx>(Create(VariantUtils.ConvertTo<VfxColor>(in args[0])));
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	/// <inheritdoc />
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
		if (method == MethodName.SetVfxColor)
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

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._vfxColor)
		{
			_vfxColor = VariantUtils.ConvertTo<VfxColor>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._vfxColor)
		{
			value = VariantUtils.CreateFrom(in _vfxColor);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
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
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._vfxColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._vfxColor, Variant.From(in _vfxColor));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._vfxColor, out var value))
		{
			_vfxColor = value.As<VfxColor>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value2))
		{
			_tween = value2.As<Tween>();
		}
	}
}
