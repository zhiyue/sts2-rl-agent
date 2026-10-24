using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Helpers;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NLaserVfx.cs")]
public class NLaserVfx : Node2D
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Node2D.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'ExtendLaser' method.
		/// </summary>
		public static readonly StringName ExtendLaser = "ExtendLaser";

		/// <summary>
		/// Cached name for the 'RetractLaser' method.
		/// </summary>
		public static readonly StringName RetractLaser = "RetractLaser";

		/// <summary>
		/// Cached name for the 'ResetLaser' method.
		/// </summary>
		public static readonly StringName ResetLaser = "ResetLaser";

		/// <summary>
		/// Cached name for the 'SetLaserColor' method.
		/// </summary>
		public static readonly StringName SetLaserColor = "SetLaserColor";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Node2D.PropertyName
	{
		/// <summary>
		/// Cached name for the '_animNode' field.
		/// </summary>
		public static readonly StringName _animNode = "_animNode";

		/// <summary>
		/// Cached name for the '_targetingBone' field.
		/// </summary>
		public static readonly StringName _targetingBone = "_targetingBone";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Node2D.SignalName
	{
	}

	private static readonly StringName _color = new StringName("Color");

	private Node2D _animNode;

	private MegaSprite _animController;

	private Node2D _targetingBone;

	public override void _Ready()
	{
		_animNode = GetNode<Node2D>("SpineSprite");
		_animController = new MegaSprite(_animNode);
		_targetingBone = GetNode<Node2D>("SpineSprite/TargetingBone");
		this.RunWhenSpineReady(_animController, delegate(MegaAnimationState animState)
		{
			animState.SetAnimation("animation");
		});
		_animNode.Visible = false;
	}

	public void ExtendLaser(Vector2 targetPos)
	{
		_animNode.Visible = true;
		_animController.GetAnimationState().SetAnimation("animation");
		_targetingBone.GlobalPosition = base.GlobalPosition;
		Tween tween = CreateTween();
		tween.TweenProperty(_targetingBone, "position", targetPos, 0.15000000596046448).SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.Out);
		tween.Chain().TweenProperty(_animNode, "modulate", Colors.Red, 0.20000000298023224);
	}

	public void RetractLaser()
	{
		Tween tween = CreateTween();
		tween.TweenProperty(_targetingBone, "position", base.Position, 0.15000000596046448).SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.In);
		tween.Chain().TweenProperty(_animNode, "visible", false, 0.0);
	}

	public void ResetLaser()
	{
		_targetingBone.Position = base.Position;
	}

	private void SetLaserColor(Color color)
	{
		((ShaderMaterial)_animController.GetAdditiveMaterial()).SetShaderParameter(_color, color);
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
		list.Add(new MethodInfo(MethodName.ExtendLaser, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "targetPos", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RetractLaser, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ResetLaser, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetLaserColor, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Color, "color", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.ExtendLaser && args.Count == 1)
		{
			ExtendLaser(VariantUtils.ConvertTo<Vector2>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RetractLaser && args.Count == 0)
		{
			RetractLaser();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ResetLaser && args.Count == 0)
		{
			ResetLaser();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetLaserColor && args.Count == 1)
		{
			SetLaserColor(VariantUtils.ConvertTo<Color>(in args[0]));
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
		if (method == MethodName.ExtendLaser)
		{
			return true;
		}
		if (method == MethodName.RetractLaser)
		{
			return true;
		}
		if (method == MethodName.ResetLaser)
		{
			return true;
		}
		if (method == MethodName.SetLaserColor)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._animNode)
		{
			_animNode = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._targetingBone)
		{
			_targetingBone = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._animNode)
		{
			value = VariantUtils.CreateFrom(in _animNode);
			return true;
		}
		if (name == PropertyName._targetingBone)
		{
			value = VariantUtils.CreateFrom(in _targetingBone);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._animNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._targetingBone, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._animNode, Variant.From(in _animNode));
		info.AddProperty(PropertyName._targetingBone, Variant.From(in _targetingBone));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._animNode, out var value))
		{
			_animNode = value.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._targetingBone, out var value2))
		{
			_targetingBone = value2.As<Node2D>();
		}
	}
}
