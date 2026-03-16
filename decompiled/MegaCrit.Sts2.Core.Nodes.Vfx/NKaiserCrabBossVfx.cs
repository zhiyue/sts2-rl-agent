using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NKaiserCrabBossVfx.cs")]
public class NKaiserCrabBossVfx : Node
{
	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnAnimationEvent = "OnAnimationEvent";

		public static readonly StringName OnAnimationStart = "OnAnimationStart";

		public static readonly StringName OnChargeSteamStart = "OnChargeSteamStart";

		public static readonly StringName OnChargeSteamEnd = "OnChargeSteamEnd";

		public static readonly StringName OnDeathSpitStart = "OnDeathSpitStart";

		public static readonly StringName OnDeathSpitEnd = "OnDeathSpitEnd";

		public static readonly StringName OnLeftEmbersStart = "OnLeftEmbersStart";

		public static readonly StringName OnPlowChunksStart = "OnPlowChunksStart";

		public static readonly StringName OnPlowChunksEnd = "OnPlowChunksEnd";

		public static readonly StringName OnRegenSplatsStart = "OnRegenSplatsStart";

		public static readonly StringName OnRegenSplatsEnd = "OnRegenSplatsEnd";

		public static readonly StringName OnRocketThrustStart = "OnRocketThrustStart";

		public static readonly StringName OnRocketThrustEnd = "OnRocketThrustEnd";

		public static readonly StringName OnClawLExplode = "OnClawLExplode";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName _regenSplatParticles = "_regenSplatParticles";

		public static readonly StringName _plowChunkParticles = "_plowChunkParticles";

		public static readonly StringName _steamParticles1 = "_steamParticles1";

		public static readonly StringName _steamParticles2 = "_steamParticles2";

		public static readonly StringName _steamParticles3 = "_steamParticles3";

		public static readonly StringName _smokeParticles = "_smokeParticles";

		public static readonly StringName _sparkParticles = "_sparkParticles";

		public static readonly StringName _spittleParticles = "_spittleParticles";

		public static readonly StringName _leftArmExplosionPosition = "_leftArmExplosionPosition";

		public static readonly StringName _parent = "_parent";
	}

	public new class SignalName : Node.SignalName
	{
	}

	private MegaSprite _megaSprite;

	private GpuParticles2D _regenSplatParticles;

	private GpuParticles2D _plowChunkParticles;

	private GpuParticles2D _steamParticles1;

	private GpuParticles2D _steamParticles2;

	private GpuParticles2D _steamParticles3;

	private GpuParticles2D _smokeParticles;

	private GpuParticles2D _sparkParticles;

	private GpuParticles2D _spittleParticles;

	private Node2D _leftArmExplosionPosition;

	private Node2D _parent;

	public override void _Ready()
	{
		_parent = GetParent<Node2D>();
		_regenSplatParticles = _parent.GetNode<GpuParticles2D>("RegenSplatSlot/RegenSplatParticles");
		_plowChunkParticles = _parent.GetNode<GpuParticles2D>("PlowChunkSlot/PlowChunkParticles");
		_steamParticles1 = _parent.GetNode<GpuParticles2D>("RocketSlot/SteamParticles1");
		_steamParticles2 = _parent.GetNode<GpuParticles2D>("RocketSlot/SteamParticles2");
		_steamParticles3 = _parent.GetNode<GpuParticles2D>("RocketSlot/SteamParticles3");
		_sparkParticles = _parent.GetNode<GpuParticles2D>("RocketSlot/SparkParticles");
		_smokeParticles = _parent.GetNode<GpuParticles2D>("RocketSlot/SmokeParticles");
		_leftArmExplosionPosition = _parent.GetNode<Node2D>("%LeftArmExplosionPosition");
		_spittleParticles = _parent.GetNode<GpuParticles2D>("SpittleSlot/SpittleParticles");
		_megaSprite = new MegaSprite(GetParent<Node2D>());
		_megaSprite.ConnectAnimationEvent(Callable.From<GodotObject, GodotObject, GodotObject, GodotObject>(OnAnimationEvent));
		_megaSprite.ConnectAnimationStarted(Callable.From<GodotObject, GodotObject, GodotObject>(OnAnimationStart));
		_regenSplatParticles.Emitting = false;
		_plowChunkParticles.Emitting = false;
		_steamParticles1.Emitting = false;
		_steamParticles2.Emitting = false;
		_steamParticles3.Emitting = false;
		_spittleParticles.Emitting = false;
		_sparkParticles.Emitting = false;
	}

	private void OnAnimationEvent(GodotObject _, GodotObject __, GodotObject ___, GodotObject spineEvent)
	{
		string eventName = new MegaEvent(spineEvent).GetData().GetEventName();
		if (eventName == null)
		{
			return;
		}
		switch (eventName.Length)
		{
		case 18:
			switch (eventName[0])
			{
			case 'c':
				if (eventName == "charge_steam_start")
				{
					OnChargeSteamStart();
				}
				break;
			case 'r':
				if (eventName == "regen_splats_start")
				{
					OnRegenSplatsStart();
				}
				break;
			}
			break;
		case 16:
			switch (eventName[0])
			{
			case 'c':
				if (eventName == "charge_steam_end")
				{
					OnChargeSteamEnd();
				}
				break;
			case 'd':
				if (eventName == "death_spit_start")
				{
					OnDeathSpitStart();
				}
				break;
			case 'r':
				if (eventName == "regen_splats_end")
				{
					OnRegenSplatsEnd();
				}
				break;
			}
			break;
		case 14:
			switch (eventName[0])
			{
			case 'd':
				if (eventName == "death_spit_end")
				{
					OnDeathSpitEnd();
				}
				break;
			case 'c':
				if (eventName == "claw_explode_l")
				{
					OnClawLExplode();
				}
				break;
			}
			break;
		case 17:
			switch (eventName[0])
			{
			case 'p':
				if (eventName == "plow_chunks_start")
				{
					OnPlowChunksStart();
				}
				break;
			case 'r':
				if (eventName == "rocket_thrust_end")
				{
					OnRocketThrustEnd();
				}
				break;
			}
			break;
		case 15:
			if (eventName == "plow_chunks_end")
			{
				OnPlowChunksEnd();
			}
			break;
		case 19:
			if (eventName == "rocket_thrust_start")
			{
				OnRocketThrustStart();
			}
			break;
		}
	}

	private void OnAnimationStart(GodotObject spineSprite, GodotObject animationState, GodotObject trackEntry)
	{
		string name = new MegaAnimationState(animationState).GetCurrent(2).GetAnimation().GetName();
		if (name != "right/charged_loop" && name != "right/charge_up")
		{
			OnChargeSteamEnd();
		}
		if (name != "right/attack_heavy")
		{
			OnRocketThrustEnd();
		}
	}

	private void OnChargeSteamStart()
	{
		_steamParticles1.Restart();
		_steamParticles2.Restart();
		_steamParticles3.Restart();
	}

	private void OnChargeSteamEnd()
	{
		_steamParticles1.Emitting = false;
		_steamParticles2.Emitting = false;
		_steamParticles3.Emitting = false;
	}

	private void OnDeathSpitStart()
	{
		_spittleParticles.Restart();
	}

	private void OnDeathSpitEnd()
	{
		_spittleParticles.Emitting = false;
	}

	private void OnLeftEmbersStart()
	{
	}

	private void OnPlowChunksStart()
	{
		_plowChunkParticles.Restart();
	}

	private void OnPlowChunksEnd()
	{
		_plowChunkParticles.Emitting = false;
	}

	private void OnRegenSplatsStart()
	{
		_regenSplatParticles.Restart();
	}

	private void OnRegenSplatsEnd()
	{
		_regenSplatParticles.Emitting = false;
	}

	private void OnRocketThrustStart()
	{
		_smokeParticles.Restart();
		_sparkParticles.Restart();
	}

	private void OnRocketThrustEnd()
	{
		_smokeParticles.Emitting = false;
		_sparkParticles.Emitting = false;
	}

	private void OnClawLExplode()
	{
		VfxCmd.PlayVfx(_leftArmExplosionPosition.GlobalPosition, "vfx/monsters/kaiser_crab_boss_explosion");
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(15);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnAnimationEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "__", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "___", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "spineEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnAnimationStart, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "spineSprite", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "animationState", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "trackEntry", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnChargeSteamStart, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnChargeSteamEnd, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnDeathSpitStart, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnDeathSpitEnd, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnLeftEmbersStart, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPlowChunksStart, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPlowChunksEnd, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnRegenSplatsStart, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnRegenSplatsEnd, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnRocketThrustStart, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnRocketThrustEnd, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnClawLExplode, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.OnAnimationStart && args.Count == 3)
		{
			OnAnimationStart(VariantUtils.ConvertTo<GodotObject>(in args[0]), VariantUtils.ConvertTo<GodotObject>(in args[1]), VariantUtils.ConvertTo<GodotObject>(in args[2]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnChargeSteamStart && args.Count == 0)
		{
			OnChargeSteamStart();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnChargeSteamEnd && args.Count == 0)
		{
			OnChargeSteamEnd();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnDeathSpitStart && args.Count == 0)
		{
			OnDeathSpitStart();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnDeathSpitEnd && args.Count == 0)
		{
			OnDeathSpitEnd();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnLeftEmbersStart && args.Count == 0)
		{
			OnLeftEmbersStart();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPlowChunksStart && args.Count == 0)
		{
			OnPlowChunksStart();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPlowChunksEnd && args.Count == 0)
		{
			OnPlowChunksEnd();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnRegenSplatsStart && args.Count == 0)
		{
			OnRegenSplatsStart();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnRegenSplatsEnd && args.Count == 0)
		{
			OnRegenSplatsEnd();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnRocketThrustStart && args.Count == 0)
		{
			OnRocketThrustStart();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnRocketThrustEnd && args.Count == 0)
		{
			OnRocketThrustEnd();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnClawLExplode && args.Count == 0)
		{
			OnClawLExplode();
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
		if (method == MethodName.OnAnimationStart)
		{
			return true;
		}
		if (method == MethodName.OnChargeSteamStart)
		{
			return true;
		}
		if (method == MethodName.OnChargeSteamEnd)
		{
			return true;
		}
		if (method == MethodName.OnDeathSpitStart)
		{
			return true;
		}
		if (method == MethodName.OnDeathSpitEnd)
		{
			return true;
		}
		if (method == MethodName.OnLeftEmbersStart)
		{
			return true;
		}
		if (method == MethodName.OnPlowChunksStart)
		{
			return true;
		}
		if (method == MethodName.OnPlowChunksEnd)
		{
			return true;
		}
		if (method == MethodName.OnRegenSplatsStart)
		{
			return true;
		}
		if (method == MethodName.OnRegenSplatsEnd)
		{
			return true;
		}
		if (method == MethodName.OnRocketThrustStart)
		{
			return true;
		}
		if (method == MethodName.OnRocketThrustEnd)
		{
			return true;
		}
		if (method == MethodName.OnClawLExplode)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._regenSplatParticles)
		{
			_regenSplatParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._plowChunkParticles)
		{
			_plowChunkParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._steamParticles1)
		{
			_steamParticles1 = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._steamParticles2)
		{
			_steamParticles2 = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._steamParticles3)
		{
			_steamParticles3 = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._smokeParticles)
		{
			_smokeParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._sparkParticles)
		{
			_sparkParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._spittleParticles)
		{
			_spittleParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._leftArmExplosionPosition)
		{
			_leftArmExplosionPosition = VariantUtils.ConvertTo<Node2D>(in value);
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
		if (name == PropertyName._regenSplatParticles)
		{
			value = VariantUtils.CreateFrom(in _regenSplatParticles);
			return true;
		}
		if (name == PropertyName._plowChunkParticles)
		{
			value = VariantUtils.CreateFrom(in _plowChunkParticles);
			return true;
		}
		if (name == PropertyName._steamParticles1)
		{
			value = VariantUtils.CreateFrom(in _steamParticles1);
			return true;
		}
		if (name == PropertyName._steamParticles2)
		{
			value = VariantUtils.CreateFrom(in _steamParticles2);
			return true;
		}
		if (name == PropertyName._steamParticles3)
		{
			value = VariantUtils.CreateFrom(in _steamParticles3);
			return true;
		}
		if (name == PropertyName._smokeParticles)
		{
			value = VariantUtils.CreateFrom(in _smokeParticles);
			return true;
		}
		if (name == PropertyName._sparkParticles)
		{
			value = VariantUtils.CreateFrom(in _sparkParticles);
			return true;
		}
		if (name == PropertyName._spittleParticles)
		{
			value = VariantUtils.CreateFrom(in _spittleParticles);
			return true;
		}
		if (name == PropertyName._leftArmExplosionPosition)
		{
			value = VariantUtils.CreateFrom(in _leftArmExplosionPosition);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._regenSplatParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._plowChunkParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._steamParticles1, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._steamParticles2, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._steamParticles3, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._smokeParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._sparkParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._spittleParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._leftArmExplosionPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._parent, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._regenSplatParticles, Variant.From(in _regenSplatParticles));
		info.AddProperty(PropertyName._plowChunkParticles, Variant.From(in _plowChunkParticles));
		info.AddProperty(PropertyName._steamParticles1, Variant.From(in _steamParticles1));
		info.AddProperty(PropertyName._steamParticles2, Variant.From(in _steamParticles2));
		info.AddProperty(PropertyName._steamParticles3, Variant.From(in _steamParticles3));
		info.AddProperty(PropertyName._smokeParticles, Variant.From(in _smokeParticles));
		info.AddProperty(PropertyName._sparkParticles, Variant.From(in _sparkParticles));
		info.AddProperty(PropertyName._spittleParticles, Variant.From(in _spittleParticles));
		info.AddProperty(PropertyName._leftArmExplosionPosition, Variant.From(in _leftArmExplosionPosition));
		info.AddProperty(PropertyName._parent, Variant.From(in _parent));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._regenSplatParticles, out var value))
		{
			_regenSplatParticles = value.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._plowChunkParticles, out var value2))
		{
			_plowChunkParticles = value2.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._steamParticles1, out var value3))
		{
			_steamParticles1 = value3.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._steamParticles2, out var value4))
		{
			_steamParticles2 = value4.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._steamParticles3, out var value5))
		{
			_steamParticles3 = value5.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._smokeParticles, out var value6))
		{
			_smokeParticles = value6.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._sparkParticles, out var value7))
		{
			_sparkParticles = value7.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._spittleParticles, out var value8))
		{
			_spittleParticles = value8.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._leftArmExplosionPosition, out var value9))
		{
			_leftArmExplosionPosition = value9.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._parent, out var value10))
		{
			_parent = value10.As<Node2D>();
		}
	}
}
