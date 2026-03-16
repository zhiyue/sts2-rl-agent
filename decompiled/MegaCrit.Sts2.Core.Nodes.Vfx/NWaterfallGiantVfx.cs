using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[GlobalClass]
[ScriptPath("res://src/Core/Nodes/Vfx/NWaterfallGiantVfx.cs")]
public class NWaterfallGiantVfx : Node
{
	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnAnimationEvent = "OnAnimationEvent";

		public static readonly StringName StartSteam1 = "StartSteam1";

		public static readonly StringName EndSteam1 = "EndSteam1";

		public static readonly StringName StartSteam2 = "StartSteam2";

		public static readonly StringName EndSteam2 = "EndSteam2";

		public static readonly StringName StartSteam3 = "StartSteam3";

		public static readonly StringName EndSteam3 = "EndSteam3";

		public static readonly StringName StartSteam5 = "StartSteam5";

		public static readonly StringName EndSteam5 = "EndSteam5";

		public static readonly StringName StartWaterfall = "StartWaterfall";

		public static readonly StringName EndWaterfall = "EndWaterfall";

		public static readonly StringName Explode = "Explode";

		public static readonly StringName Buildup1 = "Buildup1";

		public static readonly StringName Buildup2 = "Buildup2";

		public static readonly StringName Buildup3 = "Buildup3";

		public static readonly StringName EmitGracefully = "EmitGracefully";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName _steam1Particles = "_steam1Particles";

		public static readonly StringName _steam2Particles = "_steam2Particles";

		public static readonly StringName _steam3Particles = "_steam3Particles";

		public static readonly StringName _steam4Particles = "_steam4Particles";

		public static readonly StringName _steam5Particles = "_steam5Particles";

		public static readonly StringName _steam6Particles = "_steam6Particles";

		public static readonly StringName _steamLeakParticles1 = "_steamLeakParticles1";

		public static readonly StringName _steamLeakParticles2 = "_steamLeakParticles2";

		public static readonly StringName _steamLeakParticles3 = "_steamLeakParticles3";

		public static readonly StringName _mistParticles = "_mistParticles";

		public static readonly StringName _mouthParticles = "_mouthParticles";

		public static readonly StringName _dropletParticles = "_dropletParticles";

		public static readonly StringName _isDead = "_isDead";

		public static readonly StringName _parent = "_parent";
	}

	public new class SignalName : Node.SignalName
	{
	}

	private GpuParticles2D _steam1Particles;

	private GpuParticles2D _steam2Particles;

	private GpuParticles2D _steam3Particles;

	private GpuParticles2D _steam4Particles;

	private GpuParticles2D _steam5Particles;

	private GpuParticles2D _steam6Particles;

	private GpuParticles2D _steamLeakParticles1;

	private GpuParticles2D _steamLeakParticles2;

	private GpuParticles2D _steamLeakParticles3;

	private GpuParticles2D _mistParticles;

	private GpuParticles2D _mouthParticles;

	private GpuParticles2D _dropletParticles;

	private bool _isDead;

	private Node2D _parent;

	private MegaSprite _animController;

	public override void _Ready()
	{
		_parent = GetParent<Node2D>();
		_animController = new MegaSprite(_parent);
		_animController.ConnectAnimationEvent(Callable.From<GodotObject, GodotObject, GodotObject, GodotObject>(OnAnimationEvent));
		_steam1Particles = _parent.GetNode<GpuParticles2D>("SteamSlot1/steamParticles1");
		_steam2Particles = _parent.GetNode<GpuParticles2D>("SteamSlot2/steamParticles2");
		_steam3Particles = _parent.GetNode<GpuParticles2D>("SteamSlot3/steamParticles3");
		_steam4Particles = _parent.GetNode<GpuParticles2D>("SteamSlot4/steamParticles4");
		_steam5Particles = _parent.GetNode<GpuParticles2D>("SteamSlot5/steamParticles5");
		_steam6Particles = _parent.GetNode<GpuParticles2D>("SteamSlot6/steamParticles6");
		_steamLeakParticles1 = _parent.GetNode<GpuParticles2D>("SteamLeakSlot1/steamLeakParticles1");
		_steamLeakParticles2 = _parent.GetNode<GpuParticles2D>("SteamLeakSlot2/steamLeakParticles2");
		_steamLeakParticles3 = _parent.GetNode<GpuParticles2D>("SteamLeakSlot3/steamLeakParticles3");
		_mistParticles = _parent.GetNode<GpuParticles2D>("MistSlot/MistParticles");
		_dropletParticles = _parent.GetNode<GpuParticles2D>("MistSlot/Droplets");
		_mouthParticles = _parent.GetNode<GpuParticles2D>("MouthDropletsSlot/MouthDroplets");
		_steam1Particles.Emitting = false;
		_steam2Particles.Emitting = false;
		_steam3Particles.Emitting = false;
		_steam4Particles.Emitting = false;
		_steam5Particles.Emitting = false;
		_steam6Particles.Emitting = false;
		_steamLeakParticles1.Emitting = false;
		_steamLeakParticles2.Emitting = false;
		_steamLeakParticles3.Emitting = false;
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
		case 13:
			switch (eventName[6])
			{
			case '1':
				if (eventName == "steam_1_start")
				{
					StartSteam1();
				}
				break;
			case '2':
				if (eventName == "steam_2_start")
				{
					StartSteam2();
				}
				break;
			case '3':
				if (eventName == "steam_3_start")
				{
					StartSteam3();
				}
				break;
			case '5':
				if (eventName == "steam_5_start")
				{
					StartSteam5();
				}
				break;
			case 'a':
				if (eventName == "waterfall_end")
				{
					EndWaterfall();
				}
				break;
			}
			break;
		case 11:
			switch (eventName[6])
			{
			case '1':
				if (eventName == "steam_1_end")
				{
					EndSteam1();
				}
				break;
			case '2':
				if (eventName == "steam_2_end")
				{
					EndSteam2();
				}
				break;
			case '3':
				if (eventName == "steam_3_end")
				{
					EndSteam3();
				}
				break;
			case '5':
				if (eventName == "steam_5_end")
				{
					EndSteam5();
				}
				break;
			case '4':
				break;
			}
			break;
		case 8:
			switch (eventName[7])
			{
			case '1':
				if (eventName == "buildup1")
				{
					Buildup1();
				}
				break;
			case '2':
				if (eventName == "buildup2")
				{
					Buildup2();
				}
				break;
			case '3':
				if (eventName == "buildup3")
				{
					Buildup3();
				}
				break;
			}
			break;
		case 15:
			if (eventName == "waterfall_start")
			{
				StartWaterfall();
			}
			break;
		case 7:
			if (eventName == "explode")
			{
				Explode();
			}
			break;
		case 9:
		case 10:
		case 12:
		case 14:
			break;
		}
	}

	private void StartSteam1()
	{
		EmitGracefully(_steam1Particles);
	}

	private void EndSteam1()
	{
		_steam1Particles.Emitting = false;
	}

	private void StartSteam2()
	{
		EmitGracefully(_steam2Particles);
	}

	private void EndSteam2()
	{
		_steam2Particles.Emitting = false;
	}

	private void StartSteam3()
	{
		EmitGracefully(_steam3Particles);
		EmitGracefully(_steam4Particles);
	}

	private void EndSteam3()
	{
		_steam3Particles.Emitting = false;
		_steam4Particles.Emitting = false;
	}

	private void StartSteam5()
	{
		EmitGracefully(_steam5Particles);
		EmitGracefully(_steam6Particles);
	}

	private void EndSteam5()
	{
		_steam5Particles.Emitting = false;
		_steam6Particles.Emitting = false;
	}

	private void StartWaterfall()
	{
		_mouthParticles.Emitting = true;
		_dropletParticles.Emitting = true;
		_mistParticles.Emitting = true;
		_isDead = false;
	}

	private void EndWaterfall()
	{
		_mouthParticles.Emitting = false;
		_dropletParticles.Emitting = false;
		_mistParticles.Emitting = false;
	}

	private void Explode()
	{
		_steam1Particles.Visible = false;
		_steam2Particles.Visible = false;
		_steam3Particles.Visible = false;
		_steam4Particles.Visible = false;
		_steam5Particles.Visible = false;
		_steam6Particles.Visible = false;
		_steamLeakParticles1.Visible = false;
		_steamLeakParticles2.Visible = false;
		_steamLeakParticles3.Visible = false;
		_isDead = true;
	}

	private void Buildup1()
	{
		if (!_isDead)
		{
			EmitGracefully(_steamLeakParticles1);
			EmitGracefully(_steamLeakParticles2);
			EmitGracefully(_steamLeakParticles3);
			GpuParticles2D steamLeakParticles = _steamLeakParticles1;
			GpuParticles2D steamLeakParticles2 = _steamLeakParticles2;
			int num = (_steamLeakParticles3.Amount = 8);
			int amount = (steamLeakParticles2.Amount = num);
			steamLeakParticles.Amount = amount;
			GpuParticles2D steamLeakParticles3 = _steamLeakParticles1;
			GpuParticles2D steamLeakParticles4 = _steamLeakParticles2;
			double num4 = (_steamLeakParticles3.Lifetime = 0.3700000047683716);
			double lifetime = (steamLeakParticles4.Lifetime = num4);
			steamLeakParticles3.Lifetime = lifetime;
		}
	}

	private void Buildup2()
	{
		if (!_isDead)
		{
			EmitGracefully(_steamLeakParticles1);
			EmitGracefully(_steamLeakParticles2);
			EmitGracefully(_steamLeakParticles3);
			GpuParticles2D steamLeakParticles = _steamLeakParticles1;
			GpuParticles2D steamLeakParticles2 = _steamLeakParticles2;
			int num = (_steamLeakParticles3.Amount = 15);
			int amount = (steamLeakParticles2.Amount = num);
			steamLeakParticles.Amount = amount;
			GpuParticles2D steamLeakParticles3 = _steamLeakParticles1;
			GpuParticles2D steamLeakParticles4 = _steamLeakParticles2;
			double num4 = (_steamLeakParticles3.Lifetime = 0.44999998807907104);
			double lifetime = (steamLeakParticles4.Lifetime = num4);
			steamLeakParticles3.Lifetime = lifetime;
		}
	}

	private void Buildup3()
	{
		if (!_isDead)
		{
			EmitGracefully(_steamLeakParticles1);
			EmitGracefully(_steamLeakParticles2);
			EmitGracefully(_steamLeakParticles3);
			GpuParticles2D steamLeakParticles = _steamLeakParticles1;
			GpuParticles2D steamLeakParticles2 = _steamLeakParticles2;
			int num = (_steamLeakParticles3.Amount = 20);
			int amount = (steamLeakParticles2.Amount = num);
			steamLeakParticles.Amount = amount;
			GpuParticles2D steamLeakParticles3 = _steamLeakParticles1;
			GpuParticles2D steamLeakParticles4 = _steamLeakParticles2;
			double num4 = (_steamLeakParticles3.Lifetime = 0.6000000238418579);
			double lifetime = (steamLeakParticles4.Lifetime = num4);
			steamLeakParticles3.Lifetime = lifetime;
		}
	}

	private void EmitGracefully(GpuParticles2D emitter)
	{
		if (!emitter.Visible)
		{
			emitter.Visible = true;
			emitter.Restart();
		}
		else
		{
			emitter.Emitting = true;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(17);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnAnimationEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "__", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "___", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "spineEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.StartSteam1, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.EndSteam1, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.StartSteam2, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.EndSteam2, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.StartSteam3, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.EndSteam3, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.StartSteam5, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.EndSteam5, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.StartWaterfall, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.EndWaterfall, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Explode, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Buildup1, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Buildup2, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Buildup3, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.EmitGracefully, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "emitter", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("GPUParticles2D"), exported: false)
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
		if (method == MethodName.StartSteam1 && args.Count == 0)
		{
			StartSteam1();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EndSteam1 && args.Count == 0)
		{
			EndSteam1();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartSteam2 && args.Count == 0)
		{
			StartSteam2();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EndSteam2 && args.Count == 0)
		{
			EndSteam2();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartSteam3 && args.Count == 0)
		{
			StartSteam3();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EndSteam3 && args.Count == 0)
		{
			EndSteam3();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartSteam5 && args.Count == 0)
		{
			StartSteam5();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EndSteam5 && args.Count == 0)
		{
			EndSteam5();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartWaterfall && args.Count == 0)
		{
			StartWaterfall();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EndWaterfall && args.Count == 0)
		{
			EndWaterfall();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Explode && args.Count == 0)
		{
			Explode();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Buildup1 && args.Count == 0)
		{
			Buildup1();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Buildup2 && args.Count == 0)
		{
			Buildup2();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Buildup3 && args.Count == 0)
		{
			Buildup3();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EmitGracefully && args.Count == 1)
		{
			EmitGracefully(VariantUtils.ConvertTo<GpuParticles2D>(in args[0]));
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
		if (method == MethodName.StartSteam1)
		{
			return true;
		}
		if (method == MethodName.EndSteam1)
		{
			return true;
		}
		if (method == MethodName.StartSteam2)
		{
			return true;
		}
		if (method == MethodName.EndSteam2)
		{
			return true;
		}
		if (method == MethodName.StartSteam3)
		{
			return true;
		}
		if (method == MethodName.EndSteam3)
		{
			return true;
		}
		if (method == MethodName.StartSteam5)
		{
			return true;
		}
		if (method == MethodName.EndSteam5)
		{
			return true;
		}
		if (method == MethodName.StartWaterfall)
		{
			return true;
		}
		if (method == MethodName.EndWaterfall)
		{
			return true;
		}
		if (method == MethodName.Explode)
		{
			return true;
		}
		if (method == MethodName.Buildup1)
		{
			return true;
		}
		if (method == MethodName.Buildup2)
		{
			return true;
		}
		if (method == MethodName.Buildup3)
		{
			return true;
		}
		if (method == MethodName.EmitGracefully)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._steam1Particles)
		{
			_steam1Particles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._steam2Particles)
		{
			_steam2Particles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._steam3Particles)
		{
			_steam3Particles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._steam4Particles)
		{
			_steam4Particles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._steam5Particles)
		{
			_steam5Particles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._steam6Particles)
		{
			_steam6Particles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._steamLeakParticles1)
		{
			_steamLeakParticles1 = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._steamLeakParticles2)
		{
			_steamLeakParticles2 = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._steamLeakParticles3)
		{
			_steamLeakParticles3 = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._mistParticles)
		{
			_mistParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._mouthParticles)
		{
			_mouthParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._dropletParticles)
		{
			_dropletParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._isDead)
		{
			_isDead = VariantUtils.ConvertTo<bool>(in value);
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
		if (name == PropertyName._steam1Particles)
		{
			value = VariantUtils.CreateFrom(in _steam1Particles);
			return true;
		}
		if (name == PropertyName._steam2Particles)
		{
			value = VariantUtils.CreateFrom(in _steam2Particles);
			return true;
		}
		if (name == PropertyName._steam3Particles)
		{
			value = VariantUtils.CreateFrom(in _steam3Particles);
			return true;
		}
		if (name == PropertyName._steam4Particles)
		{
			value = VariantUtils.CreateFrom(in _steam4Particles);
			return true;
		}
		if (name == PropertyName._steam5Particles)
		{
			value = VariantUtils.CreateFrom(in _steam5Particles);
			return true;
		}
		if (name == PropertyName._steam6Particles)
		{
			value = VariantUtils.CreateFrom(in _steam6Particles);
			return true;
		}
		if (name == PropertyName._steamLeakParticles1)
		{
			value = VariantUtils.CreateFrom(in _steamLeakParticles1);
			return true;
		}
		if (name == PropertyName._steamLeakParticles2)
		{
			value = VariantUtils.CreateFrom(in _steamLeakParticles2);
			return true;
		}
		if (name == PropertyName._steamLeakParticles3)
		{
			value = VariantUtils.CreateFrom(in _steamLeakParticles3);
			return true;
		}
		if (name == PropertyName._mistParticles)
		{
			value = VariantUtils.CreateFrom(in _mistParticles);
			return true;
		}
		if (name == PropertyName._mouthParticles)
		{
			value = VariantUtils.CreateFrom(in _mouthParticles);
			return true;
		}
		if (name == PropertyName._dropletParticles)
		{
			value = VariantUtils.CreateFrom(in _dropletParticles);
			return true;
		}
		if (name == PropertyName._isDead)
		{
			value = VariantUtils.CreateFrom(in _isDead);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._steam1Particles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._steam2Particles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._steam3Particles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._steam4Particles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._steam5Particles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._steam6Particles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._steamLeakParticles1, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._steamLeakParticles2, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._steamLeakParticles3, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._mistParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._mouthParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._dropletParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isDead, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._parent, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._steam1Particles, Variant.From(in _steam1Particles));
		info.AddProperty(PropertyName._steam2Particles, Variant.From(in _steam2Particles));
		info.AddProperty(PropertyName._steam3Particles, Variant.From(in _steam3Particles));
		info.AddProperty(PropertyName._steam4Particles, Variant.From(in _steam4Particles));
		info.AddProperty(PropertyName._steam5Particles, Variant.From(in _steam5Particles));
		info.AddProperty(PropertyName._steam6Particles, Variant.From(in _steam6Particles));
		info.AddProperty(PropertyName._steamLeakParticles1, Variant.From(in _steamLeakParticles1));
		info.AddProperty(PropertyName._steamLeakParticles2, Variant.From(in _steamLeakParticles2));
		info.AddProperty(PropertyName._steamLeakParticles3, Variant.From(in _steamLeakParticles3));
		info.AddProperty(PropertyName._mistParticles, Variant.From(in _mistParticles));
		info.AddProperty(PropertyName._mouthParticles, Variant.From(in _mouthParticles));
		info.AddProperty(PropertyName._dropletParticles, Variant.From(in _dropletParticles));
		info.AddProperty(PropertyName._isDead, Variant.From(in _isDead));
		info.AddProperty(PropertyName._parent, Variant.From(in _parent));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._steam1Particles, out var value))
		{
			_steam1Particles = value.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._steam2Particles, out var value2))
		{
			_steam2Particles = value2.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._steam3Particles, out var value3))
		{
			_steam3Particles = value3.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._steam4Particles, out var value4))
		{
			_steam4Particles = value4.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._steam5Particles, out var value5))
		{
			_steam5Particles = value5.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._steam6Particles, out var value6))
		{
			_steam6Particles = value6.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._steamLeakParticles1, out var value7))
		{
			_steamLeakParticles1 = value7.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._steamLeakParticles2, out var value8))
		{
			_steamLeakParticles2 = value8.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._steamLeakParticles3, out var value9))
		{
			_steamLeakParticles3 = value9.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._mistParticles, out var value10))
		{
			_mistParticles = value10.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._mouthParticles, out var value11))
		{
			_mouthParticles = value11.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._dropletParticles, out var value12))
		{
			_dropletParticles = value12.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._isDead, out var value13))
		{
			_isDead = value13.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._parent, out var value14))
		{
			_parent = value14.As<Node2D>();
		}
	}
}
