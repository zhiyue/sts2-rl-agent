using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[GlobalClass]
[ScriptPath("res://src/Core/Nodes/Vfx/NAmalgamVfx.cs")]
public class NAmalgamVfx : Node
{
	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnAnimationEvent = "OnAnimationEvent";

		public static readonly StringName PoofToDeath = "PoofToDeath";

		public static readonly StringName RestartTorches = "RestartTorches";

		public static readonly StringName KillTorches = "KillTorches";

		public static readonly StringName PlayHit1 = "PlayHit1";

		public static readonly StringName PlayHit2 = "PlayHit2";

		public static readonly StringName PlayHit3 = "PlayHit3";

		public static readonly StringName PlayLaserBase = "PlayLaserBase";

		public static readonly StringName PlayLaserHit = "PlayLaserHit";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName _hitFxParticles = "_hitFxParticles";

		public static readonly StringName _hitBoneNode = "_hitBoneNode";

		public static readonly StringName _deathBodyParticles = "_deathBodyParticles";

		public static readonly StringName _laserBaseParticles = "_laserBaseParticles";

		public static readonly StringName _hitParticles1 = "_hitParticles1";

		public static readonly StringName _hitParticles2 = "_hitParticles2";

		public static readonly StringName _hitParticles3 = "_hitParticles3";

		public static readonly StringName _constantSparks1 = "_constantSparks1";

		public static readonly StringName _constantSparks2 = "_constantSparks2";

		public static readonly StringName _constantSparks3 = "_constantSparks3";

		public static readonly StringName _torch1Node = "_torch1Node";

		public static readonly StringName _torch2Node = "_torch2Node";

		public static readonly StringName _torch3Node = "_torch3Node";

		public static readonly StringName _parent = "_parent";
	}

	public new class SignalName : Node.SignalName
	{
	}

	[Export(PropertyHint.None, "")]
	private GpuParticles2D _hitFxParticles;

	[Export(PropertyHint.None, "")]
	private Node2D _hitBoneNode;

	private CpuParticles2D _deathBodyParticles;

	private GpuParticles2D _laserBaseParticles;

	private GpuParticles2D _hitParticles1;

	private GpuParticles2D _hitParticles2;

	private GpuParticles2D _hitParticles3;

	private GpuParticles2D _constantSparks1;

	private GpuParticles2D _constantSparks2;

	private GpuParticles2D _constantSparks3;

	private Node2D _torch1Node;

	private Node2D _torch2Node;

	private Node2D _torch3Node;

	private Node _parent;

	private MegaSprite _animController;

	public override void _Ready()
	{
		_parent = GetParent();
		_animController = new MegaSprite(_parent);
		_animController.ConnectAnimationEvent(Callable.From<GodotObject, GodotObject, GodotObject, GodotObject>(OnAnimationEvent));
		_deathBodyParticles = _parent.GetNode<CpuParticles2D>("CPUDeathParticles");
		_deathBodyParticles.Emitting = false;
		_deathBodyParticles.OneShot = true;
		_laserBaseParticles = _parent.GetNode<GpuParticles2D>("laserBaseBone/laserBaseParticles");
		_laserBaseParticles.Emitting = false;
		_torch1Node = _parent.GetNode<Node2D>("torch1Slot/fire1_small_green");
		_torch1Node.Visible = true;
		_torch2Node = _parent.GetNode<Node2D>("torch2Slot/fire2_small_green");
		_torch2Node.Visible = true;
		_torch3Node = _parent.GetNode<Node2D>("torch3Slot/fire3_small_green");
		_torch3Node.Visible = true;
		_hitParticles1 = _parent.GetNode<GpuParticles2D>("torch1UnscaledBone/hitParticles");
		_hitParticles1.Emitting = false;
		_hitParticles1.OneShot = true;
		_hitParticles2 = _parent.GetNode<GpuParticles2D>("torch2UnscaledBone/hitParticles");
		_hitParticles2.Emitting = false;
		_hitParticles2.OneShot = true;
		_hitParticles3 = _parent.GetNode<GpuParticles2D>("torch3UnscaledBone/hitParticles");
		_hitParticles3.Emitting = false;
		_hitParticles3.OneShot = true;
		_constantSparks1 = _parent.GetNode<GpuParticles2D>("torch1Slot/constantParticles");
		_constantSparks2 = _parent.GetNode<GpuParticles2D>("torch2Slot/constantParticles");
		_constantSparks3 = _parent.GetNode<GpuParticles2D>("torch3Slot/constantParticles");
		_hitFxParticles.Visible = false;
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
		case 4:
			switch (eventName[3])
			{
			case '1':
				if (eventName == "hit1")
				{
					PlayHit1();
				}
				break;
			case '2':
				if (eventName == "hit2")
				{
					PlayHit2();
				}
				break;
			case '3':
				if (eventName == "hit3")
				{
					PlayHit3();
				}
				break;
			}
			break;
		case 14:
			switch (eventName[6])
			{
			case 'b':
				if (eventName == "laser_base_off")
				{
					PlayLaserBase(starting: false);
				}
				break;
			case 'h':
				if (eventName == "laser_hit_fire")
				{
					PlayLaserHit(starting: true);
				}
				break;
			}
			break;
		case 7:
			if (eventName == "go_poof")
			{
				PoofToDeath();
			}
			break;
		case 11:
			if (eventName == "torches_out")
			{
				KillTorches();
			}
			break;
		case 10:
			if (eventName == "torches_on")
			{
				RestartTorches();
			}
			break;
		case 15:
			if (eventName == "laser_base_fire")
			{
				PlayLaserBase(starting: true);
			}
			break;
		case 13:
			if (eventName == "laser_hit_off")
			{
				PlayLaserHit(starting: false);
			}
			break;
		case 5:
		case 6:
		case 8:
		case 9:
		case 12:
			break;
		}
	}

	private void PoofToDeath()
	{
		_deathBodyParticles.Restart();
	}

	private void RestartTorches()
	{
		_torch1Node.Visible = true;
		_torch2Node.Visible = true;
		_torch3Node.Visible = true;
		_constantSparks1.Emitting = true;
		_constantSparks2.Emitting = true;
		_constantSparks3.Emitting = true;
	}

	private void KillTorches()
	{
		_torch1Node.Visible = false;
		_torch2Node.Visible = false;
		_torch3Node.Visible = false;
		_constantSparks1.Emitting = false;
		_constantSparks2.Emitting = false;
		_constantSparks3.Emitting = false;
	}

	private void PlayHit1()
	{
		_hitParticles1.Restart();
	}

	private void PlayHit2()
	{
		_hitParticles2.Restart();
	}

	private void PlayHit3()
	{
		_hitParticles3.Restart();
	}

	private void PlayLaserBase(bool starting)
	{
		if (starting)
		{
			_laserBaseParticles.Visible = true;
			_laserBaseParticles.Restart();
		}
		else
		{
			_laserBaseParticles.Emitting = false;
			_laserBaseParticles.Visible = false;
		}
	}

	private void PlayLaserHit(bool starting)
	{
		if (starting)
		{
			_hitFxParticles.GlobalPosition = _hitBoneNode.GlobalPosition;
			_hitFxParticles.Visible = true;
			_hitFxParticles.Restart();
		}
		else
		{
			_hitFxParticles.Emitting = false;
			_hitFxParticles.Visible = false;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(10);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnAnimationEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "__", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "___", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "spineEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.PoofToDeath, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RestartTorches, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.KillTorches, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.PlayHit1, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.PlayHit2, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.PlayHit3, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.PlayLaserBase, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "starting", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.PlayLaserHit, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "starting", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.PoofToDeath && args.Count == 0)
		{
			PoofToDeath();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RestartTorches && args.Count == 0)
		{
			RestartTorches();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.KillTorches && args.Count == 0)
		{
			KillTorches();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PlayHit1 && args.Count == 0)
		{
			PlayHit1();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PlayHit2 && args.Count == 0)
		{
			PlayHit2();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PlayHit3 && args.Count == 0)
		{
			PlayHit3();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PlayLaserBase && args.Count == 1)
		{
			PlayLaserBase(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PlayLaserHit && args.Count == 1)
		{
			PlayLaserHit(VariantUtils.ConvertTo<bool>(in args[0]));
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
		if (method == MethodName.PoofToDeath)
		{
			return true;
		}
		if (method == MethodName.RestartTorches)
		{
			return true;
		}
		if (method == MethodName.KillTorches)
		{
			return true;
		}
		if (method == MethodName.PlayHit1)
		{
			return true;
		}
		if (method == MethodName.PlayHit2)
		{
			return true;
		}
		if (method == MethodName.PlayHit3)
		{
			return true;
		}
		if (method == MethodName.PlayLaserBase)
		{
			return true;
		}
		if (method == MethodName.PlayLaserHit)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._hitFxParticles)
		{
			_hitFxParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._hitBoneNode)
		{
			_hitBoneNode = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._deathBodyParticles)
		{
			_deathBodyParticles = VariantUtils.ConvertTo<CpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._laserBaseParticles)
		{
			_laserBaseParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._hitParticles1)
		{
			_hitParticles1 = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._hitParticles2)
		{
			_hitParticles2 = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._hitParticles3)
		{
			_hitParticles3 = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._constantSparks1)
		{
			_constantSparks1 = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._constantSparks2)
		{
			_constantSparks2 = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._constantSparks3)
		{
			_constantSparks3 = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._torch1Node)
		{
			_torch1Node = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._torch2Node)
		{
			_torch2Node = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._torch3Node)
		{
			_torch3Node = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._parent)
		{
			_parent = VariantUtils.ConvertTo<Node>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._hitFxParticles)
		{
			value = VariantUtils.CreateFrom(in _hitFxParticles);
			return true;
		}
		if (name == PropertyName._hitBoneNode)
		{
			value = VariantUtils.CreateFrom(in _hitBoneNode);
			return true;
		}
		if (name == PropertyName._deathBodyParticles)
		{
			value = VariantUtils.CreateFrom(in _deathBodyParticles);
			return true;
		}
		if (name == PropertyName._laserBaseParticles)
		{
			value = VariantUtils.CreateFrom(in _laserBaseParticles);
			return true;
		}
		if (name == PropertyName._hitParticles1)
		{
			value = VariantUtils.CreateFrom(in _hitParticles1);
			return true;
		}
		if (name == PropertyName._hitParticles2)
		{
			value = VariantUtils.CreateFrom(in _hitParticles2);
			return true;
		}
		if (name == PropertyName._hitParticles3)
		{
			value = VariantUtils.CreateFrom(in _hitParticles3);
			return true;
		}
		if (name == PropertyName._constantSparks1)
		{
			value = VariantUtils.CreateFrom(in _constantSparks1);
			return true;
		}
		if (name == PropertyName._constantSparks2)
		{
			value = VariantUtils.CreateFrom(in _constantSparks2);
			return true;
		}
		if (name == PropertyName._constantSparks3)
		{
			value = VariantUtils.CreateFrom(in _constantSparks3);
			return true;
		}
		if (name == PropertyName._torch1Node)
		{
			value = VariantUtils.CreateFrom(in _torch1Node);
			return true;
		}
		if (name == PropertyName._torch2Node)
		{
			value = VariantUtils.CreateFrom(in _torch2Node);
			return true;
		}
		if (name == PropertyName._torch3Node)
		{
			value = VariantUtils.CreateFrom(in _torch3Node);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hitFxParticles, PropertyHint.NodeType, "GPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hitBoneNode, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._deathBodyParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._laserBaseParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hitParticles1, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hitParticles2, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hitParticles3, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._constantSparks1, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._constantSparks2, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._constantSparks3, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._torch1Node, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._torch2Node, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._torch3Node, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._parent, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._hitFxParticles, Variant.From(in _hitFxParticles));
		info.AddProperty(PropertyName._hitBoneNode, Variant.From(in _hitBoneNode));
		info.AddProperty(PropertyName._deathBodyParticles, Variant.From(in _deathBodyParticles));
		info.AddProperty(PropertyName._laserBaseParticles, Variant.From(in _laserBaseParticles));
		info.AddProperty(PropertyName._hitParticles1, Variant.From(in _hitParticles1));
		info.AddProperty(PropertyName._hitParticles2, Variant.From(in _hitParticles2));
		info.AddProperty(PropertyName._hitParticles3, Variant.From(in _hitParticles3));
		info.AddProperty(PropertyName._constantSparks1, Variant.From(in _constantSparks1));
		info.AddProperty(PropertyName._constantSparks2, Variant.From(in _constantSparks2));
		info.AddProperty(PropertyName._constantSparks3, Variant.From(in _constantSparks3));
		info.AddProperty(PropertyName._torch1Node, Variant.From(in _torch1Node));
		info.AddProperty(PropertyName._torch2Node, Variant.From(in _torch2Node));
		info.AddProperty(PropertyName._torch3Node, Variant.From(in _torch3Node));
		info.AddProperty(PropertyName._parent, Variant.From(in _parent));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._hitFxParticles, out var value))
		{
			_hitFxParticles = value.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._hitBoneNode, out var value2))
		{
			_hitBoneNode = value2.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._deathBodyParticles, out var value3))
		{
			_deathBodyParticles = value3.As<CpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._laserBaseParticles, out var value4))
		{
			_laserBaseParticles = value4.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._hitParticles1, out var value5))
		{
			_hitParticles1 = value5.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._hitParticles2, out var value6))
		{
			_hitParticles2 = value6.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._hitParticles3, out var value7))
		{
			_hitParticles3 = value7.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._constantSparks1, out var value8))
		{
			_constantSparks1 = value8.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._constantSparks2, out var value9))
		{
			_constantSparks2 = value9.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._constantSparks3, out var value10))
		{
			_constantSparks3 = value10.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._torch1Node, out var value11))
		{
			_torch1Node = value11.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._torch2Node, out var value12))
		{
			_torch2Node = value12.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._torch3Node, out var value13))
		{
			_torch3Node = value13.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._parent, out var value14))
		{
			_parent = value14.As<Node>();
		}
	}
}
