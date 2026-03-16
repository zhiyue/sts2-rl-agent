using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[GlobalClass]
[ScriptPath("res://src/Core/Nodes/Vfx/NSnappingJaxfruitVfx.cs")]
public class NSnappingJaxfruitVfx : Node
{
	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnAnimationEvent = "OnAnimationEvent";

		public static readonly StringName StartCast = "StartCast";

		public static readonly StringName ResetCast = "ResetCast";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName _projectileBone = "_projectileBone";

		public static readonly StringName _targetBone = "_targetBone";

		public static readonly StringName _glowParticles = "_glowParticles";

		public static readonly StringName _blobParticles = "_blobParticles";

		public static readonly StringName _trail = "_trail";

		public static readonly StringName _parent = "_parent";
	}

	public new class SignalName : Node.SignalName
	{
	}

	private const float _attackHeight = 130f;

	private Creature? _target;

	private Node2D _projectileBone;

	private Node2D _targetBone;

	private GpuParticles2D _glowParticles;

	private GpuParticles2D _blobParticles;

	private NBasicTrail _trail;

	private Node2D _parent;

	private MegaSprite _animController;

	public override void _Ready()
	{
		_parent = GetParent<Node2D>();
		_animController = new MegaSprite(_parent);
		_animController.ConnectAnimationEvent(Callable.From<GodotObject, GodotObject, GodotObject, GodotObject>(OnAnimationEvent));
		_projectileBone = _parent.GetNode<Node2D>("ProjectileAttachBone");
		_targetBone = _parent.GetNode<Node2D>("TargetBone");
		_glowParticles = _parent.GetNode<GpuParticles2D>("ProjectileAttachBone/GlowParticles");
		_blobParticles = _parent.GetNode<GpuParticles2D>("ProjectileAttachBone/BlobParticles");
		_trail = _parent.GetNode<NBasicTrail>("ProjectileAttachBone/Trail");
		ResetCast();
		_animController.GetAnimationState().SetAnimation("charged_loop");
	}

	public void SetTarget(Creature? target)
	{
		_target = target;
	}

	private void OnAnimationEvent(GodotObject _, GodotObject __, GodotObject ___, GodotObject spineEvent)
	{
		string eventName = new MegaEvent(spineEvent).GetData().GetEventName();
		if (!(eventName == "cast_start"))
		{
			if (eventName == "cast_end")
			{
				ResetCast();
			}
		}
		else
		{
			StartCast();
		}
	}

	private void StartCast()
	{
		NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(_target);
		if (nCreature != null)
		{
			_targetBone.GlobalPosition = new Vector2(nCreature.GlobalPosition.X, nCreature.GlobalPosition.Y - 130f);
		}
		_projectileBone.Visible = true;
		_trail.ClearPoints();
		_blobParticles.Restart();
		_glowParticles.Restart();
	}

	private void ResetCast()
	{
		_blobParticles.Emitting = false;
		_glowParticles.Emitting = false;
		_projectileBone.Visible = false;
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
		list.Add(new MethodInfo(MethodName.StartCast, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ResetCast, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.StartCast && args.Count == 0)
		{
			StartCast();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ResetCast && args.Count == 0)
		{
			ResetCast();
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
		if (method == MethodName.StartCast)
		{
			return true;
		}
		if (method == MethodName.ResetCast)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._projectileBone)
		{
			_projectileBone = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._targetBone)
		{
			_targetBone = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._glowParticles)
		{
			_glowParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._blobParticles)
		{
			_blobParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._trail)
		{
			_trail = VariantUtils.ConvertTo<NBasicTrail>(in value);
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
		if (name == PropertyName._projectileBone)
		{
			value = VariantUtils.CreateFrom(in _projectileBone);
			return true;
		}
		if (name == PropertyName._targetBone)
		{
			value = VariantUtils.CreateFrom(in _targetBone);
			return true;
		}
		if (name == PropertyName._glowParticles)
		{
			value = VariantUtils.CreateFrom(in _glowParticles);
			return true;
		}
		if (name == PropertyName._blobParticles)
		{
			value = VariantUtils.CreateFrom(in _blobParticles);
			return true;
		}
		if (name == PropertyName._trail)
		{
			value = VariantUtils.CreateFrom(in _trail);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._projectileBone, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._targetBone, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._glowParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._blobParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._trail, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._parent, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._projectileBone, Variant.From(in _projectileBone));
		info.AddProperty(PropertyName._targetBone, Variant.From(in _targetBone));
		info.AddProperty(PropertyName._glowParticles, Variant.From(in _glowParticles));
		info.AddProperty(PropertyName._blobParticles, Variant.From(in _blobParticles));
		info.AddProperty(PropertyName._trail, Variant.From(in _trail));
		info.AddProperty(PropertyName._parent, Variant.From(in _parent));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._projectileBone, out var value))
		{
			_projectileBone = value.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._targetBone, out var value2))
		{
			_targetBone = value2.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._glowParticles, out var value3))
		{
			_glowParticles = value3.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._blobParticles, out var value4))
		{
			_blobParticles = value4.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._trail, out var value5))
		{
			_trail = value5.As<NBasicTrail>();
		}
		if (info.TryGetProperty(PropertyName._parent, out var value6))
		{
			_parent = value6.As<Node2D>();
		}
	}
}
