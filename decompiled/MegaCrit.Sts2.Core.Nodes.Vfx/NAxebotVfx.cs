using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[GlobalClass]
[ScriptPath("res://src/Core/Nodes/Vfx/NAxebotVfx.cs")]
public class NAxebotVfx : Node
{
	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnAnimationEvent = "OnAnimationEvent";

		public static readonly StringName TurnOnDeath1 = "TurnOnDeath1";

		public static readonly StringName TurnOnDeath2 = "TurnOnDeath2";

		public static readonly StringName TurnOnHurt = "TurnOnHurt";

		public static readonly StringName TurnOnLandingSmoke = "TurnOnLandingSmoke";

		public static readonly StringName TurnOffLandingSmoke = "TurnOffLandingSmoke";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName _hurtParticles1 = "_hurtParticles1";

		public static readonly StringName _hurtParticles2 = "_hurtParticles2";

		public static readonly StringName _smokeParticlesLeft = "_smokeParticlesLeft";

		public static readonly StringName _smokeParticlesRight = "_smokeParticlesRight";

		public static readonly StringName _parent = "_parent";

		public static readonly StringName _currentWeapon = "_currentWeapon";
	}

	public new class SignalName : Node.SignalName
	{
	}

	private GpuParticles2D _hurtParticles1;

	private GpuParticles2D _hurtParticles2;

	private GpuParticles2D _smokeParticlesLeft;

	private GpuParticles2D _smokeParticlesRight;

	private Node2D _parent;

	private MegaSprite _animController;

	private int _currentWeapon = 1;

	public override void _Ready()
	{
		_parent = GetParent<Node2D>();
		_animController = new MegaSprite(_parent);
		_animController.ConnectAnimationEvent(Callable.From<GodotObject, GodotObject, GodotObject, GodotObject>(OnAnimationEvent));
		_hurtParticles1 = _parent.GetNode<GpuParticles2D>("SparksBoneNode/HurtParticles1");
		_hurtParticles2 = _parent.GetNode<GpuParticles2D>("SparksBoneNode/HurtParticles2");
		_smokeParticlesLeft = _parent.GetNode<GpuParticles2D>("SmokeNodeLeft/SmokeParticles");
		_smokeParticlesRight = _parent.GetNode<GpuParticles2D>("SmokeNodeRight/SmokeParticles");
		_hurtParticles1.OneShot = true;
		_hurtParticles2.OneShot = true;
		_hurtParticles1.Emitting = false;
		_hurtParticles2.Emitting = false;
		_smokeParticlesLeft.Emitting = false;
		_smokeParticlesRight.Emitting = false;
	}

	private void OnAnimationEvent(GodotObject _, GodotObject __, GodotObject ___, GodotObject spineEvent)
	{
		switch (new MegaEvent(spineEvent).GetData().GetEventName())
		{
		case "start_hurt_sparks":
			TurnOnHurt();
			break;
		case "start_death_sparks1":
			TurnOnDeath1();
			break;
		case "start_death_sparks2":
			TurnOnDeath2();
			break;
		case "landing_smoke_start":
			TurnOnLandingSmoke();
			break;
		case "landing_smoke_end":
			TurnOffLandingSmoke();
			break;
		}
	}

	private void TurnOnDeath1()
	{
		_hurtParticles1.Restart();
	}

	private void TurnOnDeath2()
	{
		_hurtParticles2.Restart();
	}

	private void TurnOnHurt()
	{
		if (_currentWeapon == 1)
		{
			_hurtParticles1.Restart();
			_currentWeapon = 2;
		}
		else
		{
			_hurtParticles2.Restart();
			_currentWeapon = 1;
		}
	}

	private void TurnOnLandingSmoke()
	{
		_smokeParticlesLeft.Restart();
		_smokeParticlesRight.Restart();
	}

	private void TurnOffLandingSmoke()
	{
		_smokeParticlesLeft.Emitting = false;
		_smokeParticlesRight.Emitting = false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(7);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnAnimationEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "__", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "___", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "spineEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.TurnOnDeath1, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TurnOnDeath2, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TurnOnHurt, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TurnOnLandingSmoke, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TurnOffLandingSmoke, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.TurnOnDeath1 && args.Count == 0)
		{
			TurnOnDeath1();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TurnOnDeath2 && args.Count == 0)
		{
			TurnOnDeath2();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TurnOnHurt && args.Count == 0)
		{
			TurnOnHurt();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TurnOnLandingSmoke && args.Count == 0)
		{
			TurnOnLandingSmoke();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TurnOffLandingSmoke && args.Count == 0)
		{
			TurnOffLandingSmoke();
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
		if (method == MethodName.TurnOnDeath1)
		{
			return true;
		}
		if (method == MethodName.TurnOnDeath2)
		{
			return true;
		}
		if (method == MethodName.TurnOnHurt)
		{
			return true;
		}
		if (method == MethodName.TurnOnLandingSmoke)
		{
			return true;
		}
		if (method == MethodName.TurnOffLandingSmoke)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._hurtParticles1)
		{
			_hurtParticles1 = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._hurtParticles2)
		{
			_hurtParticles2 = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._smokeParticlesLeft)
		{
			_smokeParticlesLeft = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._smokeParticlesRight)
		{
			_smokeParticlesRight = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._parent)
		{
			_parent = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._currentWeapon)
		{
			_currentWeapon = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._hurtParticles1)
		{
			value = VariantUtils.CreateFrom(in _hurtParticles1);
			return true;
		}
		if (name == PropertyName._hurtParticles2)
		{
			value = VariantUtils.CreateFrom(in _hurtParticles2);
			return true;
		}
		if (name == PropertyName._smokeParticlesLeft)
		{
			value = VariantUtils.CreateFrom(in _smokeParticlesLeft);
			return true;
		}
		if (name == PropertyName._smokeParticlesRight)
		{
			value = VariantUtils.CreateFrom(in _smokeParticlesRight);
			return true;
		}
		if (name == PropertyName._parent)
		{
			value = VariantUtils.CreateFrom(in _parent);
			return true;
		}
		if (name == PropertyName._currentWeapon)
		{
			value = VariantUtils.CreateFrom(in _currentWeapon);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hurtParticles1, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hurtParticles2, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._smokeParticlesLeft, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._smokeParticlesRight, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._parent, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._currentWeapon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._hurtParticles1, Variant.From(in _hurtParticles1));
		info.AddProperty(PropertyName._hurtParticles2, Variant.From(in _hurtParticles2));
		info.AddProperty(PropertyName._smokeParticlesLeft, Variant.From(in _smokeParticlesLeft));
		info.AddProperty(PropertyName._smokeParticlesRight, Variant.From(in _smokeParticlesRight));
		info.AddProperty(PropertyName._parent, Variant.From(in _parent));
		info.AddProperty(PropertyName._currentWeapon, Variant.From(in _currentWeapon));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._hurtParticles1, out var value))
		{
			_hurtParticles1 = value.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._hurtParticles2, out var value2))
		{
			_hurtParticles2 = value2.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._smokeParticlesLeft, out var value3))
		{
			_smokeParticlesLeft = value3.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._smokeParticlesRight, out var value4))
		{
			_smokeParticlesRight = value4.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._parent, out var value5))
		{
			_parent = value5.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._currentWeapon, out var value6))
		{
			_currentWeapon = value6.As<int>();
		}
	}
}
