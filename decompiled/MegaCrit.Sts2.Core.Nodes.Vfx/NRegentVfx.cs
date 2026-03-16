using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[GlobalClass]
[ScriptPath("res://src/Core/Nodes/Vfx/NRegentVfx.cs")]
public class NRegentVfx : Node
{
	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnAnimationEvent = "OnAnimationEvent";

		public static readonly StringName TurnOnDying = "TurnOnDying";

		public static readonly StringName TurnOnDying2 = "TurnOnDying2";

		public static readonly StringName TurnOffDying = "TurnOffDying";

		public static readonly StringName Explode = "Explode";

		public static readonly StringName DisableExplode = "DisableExplode";

		public static readonly StringName Attack = "Attack";

		public static readonly StringName OnAnimationStart = "OnAnimationStart";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName _deathParticlesArm = "_deathParticlesArm";

		public static readonly StringName _deathParticlesChest = "_deathParticlesChest";

		public static readonly StringName _deathParticlesBack = "_deathParticlesBack";

		public static readonly StringName _deathParticlesLeg = "_deathParticlesLeg";

		public static readonly StringName _deathParticlesLegL = "_deathParticlesLegL";

		public static readonly StringName _explosionParticles = "_explosionParticles";

		public static readonly StringName _attackParticlesSmall = "_attackParticlesSmall";

		public static readonly StringName _attackParticlesSmall2 = "_attackParticlesSmall2";

		public static readonly StringName _attackParticlesLarge = "_attackParticlesLarge";

		public static readonly StringName _curWeapon = "_curWeapon";

		public static readonly StringName _parent = "_parent";
	}

	public new class SignalName : Node.SignalName
	{
	}

	private GpuParticles2D _deathParticlesArm;

	private GpuParticles2D _deathParticlesChest;

	private GpuParticles2D _deathParticlesBack;

	private GpuParticles2D _deathParticlesLeg;

	private GpuParticles2D _deathParticlesLegL;

	private GpuParticles2D _explosionParticles;

	private MegaSprite _weapon;

	private MegaSprite _weapon2;

	private GpuParticles2D _attackParticlesSmall;

	private GpuParticles2D _attackParticlesSmall2;

	private GpuParticles2D _attackParticlesLarge;

	private MegaAnimationState _weaponAnimState;

	private MegaAnimationState _weaponAnimState2;

	private int _curWeapon = 1;

	private Node2D _parent;

	private MegaSprite _animController;

	public override void _Ready()
	{
		_parent = GetParent<Node2D>();
		_animController = new MegaSprite(_parent);
		_animController.ConnectAnimationEvent(Callable.From<GodotObject, GodotObject, GodotObject, GodotObject>(OnAnimationEvent));
		_animController.ConnectAnimationStarted(Callable.From<GodotObject, GodotObject, GodotObject>(OnAnimationStart));
		_deathParticlesArm = _parent.GetNode<GpuParticles2D>("SpineArmBone/Particles");
		_deathParticlesChest = _parent.GetNode<GpuParticles2D>("SpineChestBone/Particles");
		_deathParticlesBack = _parent.GetNode<GpuParticles2D>("SpineChestBone/ParticlesBack");
		_deathParticlesLeg = _parent.GetNode<GpuParticles2D>("SpineLegBone/Particles");
		_deathParticlesLegL = _parent.GetNode<GpuParticles2D>("SpineLegBoneL/Particles");
		_explosionParticles = _parent.GetNode<GpuParticles2D>("Explosion");
		_weapon = new MegaSprite(_parent.GetNode("Weapons/WeaponAnim1"));
		_weapon2 = new MegaSprite(_parent.GetNode("Weapons/WeaponAnim2"));
		_weaponAnimState = _weapon.GetAnimationState();
		_weaponAnimState2 = _weapon2.GetAnimationState();
		_deathParticlesArm.Emitting = false;
		_deathParticlesChest.Emitting = false;
		_deathParticlesBack.Emitting = false;
		_deathParticlesLeg.Emitting = false;
		_deathParticlesLegL.Emitting = false;
		_explosionParticles.Emitting = false;
	}

	private void OnAnimationEvent(GodotObject _, GodotObject __, GodotObject ___, GodotObject spineEvent)
	{
		switch (new MegaEvent(spineEvent).GetData().GetEventName())
		{
		case "death_particles_start":
			TurnOnDying();
			break;
		case "death_particles_start2":
			TurnOnDying2();
			break;
		case "death_particles_end":
			TurnOffDying();
			break;
		case "explode_dead":
			Explode();
			break;
		case "explode_end":
			DisableExplode();
			break;
		case "attack1":
			Attack();
			break;
		}
	}

	private void TurnOnDying()
	{
		_deathParticlesArm.Restart();
		_deathParticlesLeg.Restart();
		_deathParticlesLegL.Restart();
	}

	private void TurnOnDying2()
	{
		_deathParticlesChest.Restart();
		_deathParticlesBack.Restart();
	}

	private void TurnOffDying()
	{
		_deathParticlesArm.Emitting = false;
		_deathParticlesChest.Emitting = false;
		_deathParticlesBack.Emitting = false;
		_deathParticlesLeg.Emitting = false;
		_deathParticlesLegL.Emitting = false;
	}

	private void Explode()
	{
		_explosionParticles.Restart();
	}

	private void DisableExplode()
	{
		_explosionParticles.Emitting = false;
	}

	private void Attack()
	{
		if (_curWeapon == 1)
		{
			_weaponAnimState.SetAnimation("attack", loop: false);
			_curWeapon = 2;
		}
		else
		{
			_weaponAnimState2.SetAnimation("attack2", loop: false);
			_curWeapon = 1;
		}
	}

	private void OnAnimationStart(GodotObject spineSprite, GodotObject animationState, GodotObject trackEntry)
	{
		if (new MegaAnimationState(animationState).GetCurrent(0).GetAnimation().GetName() != "die")
		{
			DisableExplode();
			TurnOffDying();
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(9);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnAnimationEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "__", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "___", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "spineEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.TurnOnDying, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TurnOnDying2, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TurnOffDying, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Explode, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DisableExplode, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Attack, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnAnimationStart, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "spineSprite", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "animationState", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "trackEntry", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false)
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
		if (method == MethodName.OnAnimationEvent && args.Count == 4)
		{
			OnAnimationEvent(VariantUtils.ConvertTo<GodotObject>(in args[0]), VariantUtils.ConvertTo<GodotObject>(in args[1]), VariantUtils.ConvertTo<GodotObject>(in args[2]), VariantUtils.ConvertTo<GodotObject>(in args[3]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TurnOnDying && args.Count == 0)
		{
			TurnOnDying();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TurnOnDying2 && args.Count == 0)
		{
			TurnOnDying2();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TurnOffDying && args.Count == 0)
		{
			TurnOffDying();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Explode && args.Count == 0)
		{
			Explode();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DisableExplode && args.Count == 0)
		{
			DisableExplode();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Attack && args.Count == 0)
		{
			Attack();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnAnimationStart && args.Count == 3)
		{
			OnAnimationStart(VariantUtils.ConvertTo<GodotObject>(in args[0]), VariantUtils.ConvertTo<GodotObject>(in args[1]), VariantUtils.ConvertTo<GodotObject>(in args[2]));
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
		if (method == MethodName.TurnOnDying)
		{
			return true;
		}
		if (method == MethodName.TurnOnDying2)
		{
			return true;
		}
		if (method == MethodName.TurnOffDying)
		{
			return true;
		}
		if (method == MethodName.Explode)
		{
			return true;
		}
		if (method == MethodName.DisableExplode)
		{
			return true;
		}
		if (method == MethodName.Attack)
		{
			return true;
		}
		if (method == MethodName.OnAnimationStart)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._deathParticlesArm)
		{
			_deathParticlesArm = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._deathParticlesChest)
		{
			_deathParticlesChest = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._deathParticlesBack)
		{
			_deathParticlesBack = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._deathParticlesLeg)
		{
			_deathParticlesLeg = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._deathParticlesLegL)
		{
			_deathParticlesLegL = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._explosionParticles)
		{
			_explosionParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._attackParticlesSmall)
		{
			_attackParticlesSmall = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._attackParticlesSmall2)
		{
			_attackParticlesSmall2 = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._attackParticlesLarge)
		{
			_attackParticlesLarge = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._curWeapon)
		{
			_curWeapon = VariantUtils.ConvertTo<int>(in value);
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
		if (name == PropertyName._deathParticlesArm)
		{
			value = VariantUtils.CreateFrom(in _deathParticlesArm);
			return true;
		}
		if (name == PropertyName._deathParticlesChest)
		{
			value = VariantUtils.CreateFrom(in _deathParticlesChest);
			return true;
		}
		if (name == PropertyName._deathParticlesBack)
		{
			value = VariantUtils.CreateFrom(in _deathParticlesBack);
			return true;
		}
		if (name == PropertyName._deathParticlesLeg)
		{
			value = VariantUtils.CreateFrom(in _deathParticlesLeg);
			return true;
		}
		if (name == PropertyName._deathParticlesLegL)
		{
			value = VariantUtils.CreateFrom(in _deathParticlesLegL);
			return true;
		}
		if (name == PropertyName._explosionParticles)
		{
			value = VariantUtils.CreateFrom(in _explosionParticles);
			return true;
		}
		if (name == PropertyName._attackParticlesSmall)
		{
			value = VariantUtils.CreateFrom(in _attackParticlesSmall);
			return true;
		}
		if (name == PropertyName._attackParticlesSmall2)
		{
			value = VariantUtils.CreateFrom(in _attackParticlesSmall2);
			return true;
		}
		if (name == PropertyName._attackParticlesLarge)
		{
			value = VariantUtils.CreateFrom(in _attackParticlesLarge);
			return true;
		}
		if (name == PropertyName._curWeapon)
		{
			value = VariantUtils.CreateFrom(in _curWeapon);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._deathParticlesArm, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._deathParticlesChest, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._deathParticlesBack, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._deathParticlesLeg, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._deathParticlesLegL, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._explosionParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._attackParticlesSmall, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._attackParticlesSmall2, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._attackParticlesLarge, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._curWeapon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._parent, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._deathParticlesArm, Variant.From(in _deathParticlesArm));
		info.AddProperty(PropertyName._deathParticlesChest, Variant.From(in _deathParticlesChest));
		info.AddProperty(PropertyName._deathParticlesBack, Variant.From(in _deathParticlesBack));
		info.AddProperty(PropertyName._deathParticlesLeg, Variant.From(in _deathParticlesLeg));
		info.AddProperty(PropertyName._deathParticlesLegL, Variant.From(in _deathParticlesLegL));
		info.AddProperty(PropertyName._explosionParticles, Variant.From(in _explosionParticles));
		info.AddProperty(PropertyName._attackParticlesSmall, Variant.From(in _attackParticlesSmall));
		info.AddProperty(PropertyName._attackParticlesSmall2, Variant.From(in _attackParticlesSmall2));
		info.AddProperty(PropertyName._attackParticlesLarge, Variant.From(in _attackParticlesLarge));
		info.AddProperty(PropertyName._curWeapon, Variant.From(in _curWeapon));
		info.AddProperty(PropertyName._parent, Variant.From(in _parent));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._deathParticlesArm, out var value))
		{
			_deathParticlesArm = value.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._deathParticlesChest, out var value2))
		{
			_deathParticlesChest = value2.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._deathParticlesBack, out var value3))
		{
			_deathParticlesBack = value3.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._deathParticlesLeg, out var value4))
		{
			_deathParticlesLeg = value4.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._deathParticlesLegL, out var value5))
		{
			_deathParticlesLegL = value5.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._explosionParticles, out var value6))
		{
			_explosionParticles = value6.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._attackParticlesSmall, out var value7))
		{
			_attackParticlesSmall = value7.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._attackParticlesSmall2, out var value8))
		{
			_attackParticlesSmall2 = value8.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._attackParticlesLarge, out var value9))
		{
			_attackParticlesLarge = value9.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._curWeapon, out var value10))
		{
			_curWeapon = value10.As<int>();
		}
		if (info.TryGetProperty(PropertyName._parent, out var value11))
		{
			_parent = value11.As<Node2D>();
		}
	}
}
