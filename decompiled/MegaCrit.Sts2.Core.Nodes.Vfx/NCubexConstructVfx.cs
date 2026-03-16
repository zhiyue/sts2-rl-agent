using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[GlobalClass]
[ScriptPath("res://src/Core/Nodes/Vfx/NCubexConstructVfx.cs")]
public class NCubexConstructVfx : Node
{
	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnAnimationEvent = "OnAnimationEvent";

		public static readonly StringName StartLaser = "StartLaser";

		public static readonly StringName EndLaser = "EndLaser";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName _laser = "_laser";

		public static readonly StringName _rings = "_rings";

		public static readonly StringName _parent = "_parent";
	}

	public new class SignalName : Node.SignalName
	{
	}

	private NLaserVfx _laser;

	private Node2D _rings;

	private Node2D _parent;

	private MegaSprite _animController;

	public override void _Ready()
	{
		_parent = GetParent<Node2D>();
		_animController = new MegaSprite(_parent);
		_animController.ConnectAnimationEvent(Callable.From<GodotObject, GodotObject, GodotObject, GodotObject>(OnAnimationEvent));
		_laser = _parent.GetNode<NLaserVfx>("LaserBone/Laser");
		_rings = _parent.GetNode<Node2D>("LaserBone/Rings");
		MegaSkin megaSkin = _animController.NewSkin("newSkin");
		MegaSkeleton skeleton = _animController.GetSkeleton();
		MegaSkin skin = skeleton.GetData().FindSkin("moss3");
		megaSkin.AddSkin(skin);
		skeleton.SetSkin(megaSkin);
		_animController.GetAnimationState().SetAnimation("idle_loop");
		_rings.Visible = false;
		_laser.Scale = Vector2.One / _parent.Scale;
		_laser.Visible = false;
		_laser.ResetLaser();
	}

	private void OnAnimationEvent(GodotObject _, GodotObject __, GodotObject ___, GodotObject spineEvent)
	{
		string eventName = new MegaEvent(spineEvent).GetData().GetEventName();
		if (!(eventName == "laser_start"))
		{
			if (eventName == "laser_end")
			{
				EndLaser();
			}
		}
		else
		{
			StartLaser();
		}
	}

	private void StartLaser()
	{
		_laser.Visible = true;
		_laser.ExtendLaser(new Vector2(_laser.Position.X - 3000f, _parent.Position.Y));
		Vector2 scale = _rings.Scale;
		_rings.Scale = Vector2.Zero;
		_rings.Visible = true;
		Tween tween = CreateTween();
		tween.TweenProperty(_rings, "scale", scale, 0.4000000059604645).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	private void EndLaser()
	{
		_laser.RetractLaser();
		_rings.Visible = false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnAnimationEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "__", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "___", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "spineEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.StartLaser, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.EndLaser, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.OnAnimationEvent && args.Count == 4)
		{
			OnAnimationEvent(VariantUtils.ConvertTo<GodotObject>(in args[0]), VariantUtils.ConvertTo<GodotObject>(in args[1]), VariantUtils.ConvertTo<GodotObject>(in args[2]), VariantUtils.ConvertTo<GodotObject>(in args[3]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartLaser && args.Count == 0)
		{
			StartLaser();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EndLaser && args.Count == 0)
		{
			EndLaser();
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
		if (method == MethodName.OnAnimationEvent)
		{
			return true;
		}
		if (method == MethodName.StartLaser)
		{
			return true;
		}
		if (method == MethodName.EndLaser)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._laser)
		{
			_laser = VariantUtils.ConvertTo<NLaserVfx>(in value);
			return true;
		}
		if (name == PropertyName._rings)
		{
			_rings = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._parent)
		{
			_parent = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._laser)
		{
			value = VariantUtils.CreateFrom(in _laser);
			return true;
		}
		if (name == PropertyName._rings)
		{
			value = VariantUtils.CreateFrom(in _rings);
			return true;
		}
		if (name == PropertyName._parent)
		{
			value = VariantUtils.CreateFrom(in _parent);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._laser, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._rings, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._parent, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._laser, Variant.From(in _laser));
		info.AddProperty(PropertyName._rings, Variant.From(in _rings));
		info.AddProperty(PropertyName._parent, Variant.From(in _parent));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._laser, out var value))
		{
			_laser = value.As<NLaserVfx>();
		}
		if (info.TryGetProperty(PropertyName._rings, out var value2))
		{
			_rings = value2.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._parent, out var value3))
		{
			_parent = value3.As<Node2D>();
		}
	}
}
