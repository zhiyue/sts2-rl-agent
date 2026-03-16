using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[GlobalClass]
[ScriptPath("res://src/Core/Nodes/Vfx/NKnowledgeDemonVfx.cs")]
public class NKnowledgeDemonVfx : Node
{
	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnAnimationEvent = "OnAnimationEvent";

		public static readonly StringName OnExplode = "OnExplode";

		public static readonly StringName OnTakeDamage = "OnTakeDamage";

		public static readonly StringName OnBurningStart = "OnBurningStart";

		public static readonly StringName OnEmbersStart = "OnEmbersStart";

		public static readonly StringName OnThinEmbersStart = "OnThinEmbersStart";

		public static readonly StringName OnBurningEnd = "OnBurningEnd";

		public static readonly StringName OnEmbersEnd = "OnEmbersEnd";

		public static readonly StringName OnThinEmbersEnd = "OnThinEmbersEnd";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName _fireNode1 = "_fireNode1";

		public static readonly StringName _fireNode2 = "_fireNode2";

		public static readonly StringName _fireNode3 = "_fireNode3";

		public static readonly StringName _fireNode4 = "_fireNode4";

		public static readonly StringName _explosionParticles = "_explosionParticles";

		public static readonly StringName _damageParticles = "_damageParticles";

		public static readonly StringName _emberParticles = "_emberParticles";

		public static readonly StringName _thinEmberParticles = "_thinEmberParticles";

		public static readonly StringName _parent = "_parent";
	}

	public new class SignalName : Node.SignalName
	{
	}

	private Node2D _fireNode1;

	private Node2D _fireNode2;

	private Node2D _fireNode3;

	private Node2D _fireNode4;

	private GpuParticles2D _explosionParticles;

	private GpuParticles2D _damageParticles;

	private GpuParticles2D _emberParticles;

	private GpuParticles2D _thinEmberParticles;

	private Node2D _parent;

	private MegaSprite _animController;

	public override void _Ready()
	{
		_parent = GetParent<Node2D>();
		_animController = new MegaSprite(_parent);
		_animController.ConnectAnimationEvent(Callable.From<GodotObject, GodotObject, GodotObject, GodotObject>(OnAnimationEvent));
		_fireNode1 = _parent.GetNode<Node2D>("FireSlot1/FireHolder1");
		_fireNode2 = _parent.GetNode<Node2D>("FireSlot2/FireHolder2");
		_fireNode3 = _parent.GetNode<Node2D>("FireSlot3/FireHolder3");
		_fireNode4 = _parent.GetNode<Node2D>("FireSlot4/FireHolder4");
		_explosionParticles = _parent.GetNode<GpuParticles2D>("ExplosionParticles");
		_damageParticles = _parent.GetNode<GpuParticles2D>("DamageParticles");
		_emberParticles = _parent.GetNode<GpuParticles2D>("EmberParticles");
		_thinEmberParticles = _parent.GetNode<GpuParticles2D>("ThinEmberParticles");
		_fireNode1.Visible = false;
		_fireNode2.Visible = false;
		_fireNode3.Visible = false;
		_fireNode4.Visible = false;
		_explosionParticles.Emitting = false;
		_explosionParticles.OneShot = true;
		_damageParticles.Emitting = false;
		_damageParticles.OneShot = true;
		_emberParticles.Emitting = false;
		_thinEmberParticles.Emitting = false;
		OnBurningEnd();
		_animController.GetAnimationState().SetAnimation("idle_loop");
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
		case 11:
			switch (eventName[0])
			{
			case 't':
				if (eventName == "take_damage")
				{
					OnTakeDamage();
				}
				break;
			case 'b':
				if (eventName == "burning_end")
				{
					OnBurningEnd();
				}
				break;
			}
			break;
		case 7:
			if (eventName == "explode")
			{
				OnExplode();
			}
			break;
		case 13:
			if (eventName == "burning_start")
			{
				OnBurningStart();
			}
			break;
		case 12:
			if (eventName == "embers_start")
			{
				OnEmbersStart();
			}
			break;
		case 17:
			if (eventName == "thin_embers_start")
			{
				OnThinEmbersStart();
			}
			break;
		case 10:
			if (eventName == "embers_end")
			{
				OnEmbersEnd();
			}
			break;
		case 15:
			if (eventName == "thin_embers_end")
			{
				OnThinEmbersEnd();
			}
			break;
		case 8:
		case 9:
		case 14:
		case 16:
			break;
		}
	}

	private void OnExplode()
	{
		_explosionParticles.Restart();
	}

	private void OnTakeDamage()
	{
		_damageParticles.Restart();
	}

	private void OnBurningStart()
	{
		_fireNode1.Visible = true;
		_fireNode2.Visible = true;
		_fireNode3.Visible = true;
		_fireNode4.Visible = true;
	}

	private void OnEmbersStart()
	{
		_emberParticles.Restart();
	}

	private void OnThinEmbersStart()
	{
		_thinEmberParticles.Restart();
	}

	private void OnBurningEnd()
	{
		_fireNode1.Visible = false;
		_fireNode2.Visible = false;
		_fireNode3.Visible = false;
		_fireNode4.Visible = false;
	}

	private void OnEmbersEnd()
	{
		_emberParticles.Emitting = false;
	}

	private void OnThinEmbersEnd()
	{
		_thinEmberParticles.Emitting = false;
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
		list.Add(new MethodInfo(MethodName.OnExplode, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnTakeDamage, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnBurningStart, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnEmbersStart, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnThinEmbersStart, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnBurningEnd, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnEmbersEnd, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnThinEmbersEnd, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.OnExplode && args.Count == 0)
		{
			OnExplode();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnTakeDamage && args.Count == 0)
		{
			OnTakeDamage();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnBurningStart && args.Count == 0)
		{
			OnBurningStart();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnEmbersStart && args.Count == 0)
		{
			OnEmbersStart();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnThinEmbersStart && args.Count == 0)
		{
			OnThinEmbersStart();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnBurningEnd && args.Count == 0)
		{
			OnBurningEnd();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnEmbersEnd && args.Count == 0)
		{
			OnEmbersEnd();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnThinEmbersEnd && args.Count == 0)
		{
			OnThinEmbersEnd();
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
		if (method == MethodName.OnExplode)
		{
			return true;
		}
		if (method == MethodName.OnTakeDamage)
		{
			return true;
		}
		if (method == MethodName.OnBurningStart)
		{
			return true;
		}
		if (method == MethodName.OnEmbersStart)
		{
			return true;
		}
		if (method == MethodName.OnThinEmbersStart)
		{
			return true;
		}
		if (method == MethodName.OnBurningEnd)
		{
			return true;
		}
		if (method == MethodName.OnEmbersEnd)
		{
			return true;
		}
		if (method == MethodName.OnThinEmbersEnd)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._fireNode1)
		{
			_fireNode1 = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._fireNode2)
		{
			_fireNode2 = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._fireNode3)
		{
			_fireNode3 = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._fireNode4)
		{
			_fireNode4 = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._explosionParticles)
		{
			_explosionParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._damageParticles)
		{
			_damageParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._emberParticles)
		{
			_emberParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._thinEmberParticles)
		{
			_thinEmberParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
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
		if (name == PropertyName._fireNode1)
		{
			value = VariantUtils.CreateFrom(in _fireNode1);
			return true;
		}
		if (name == PropertyName._fireNode2)
		{
			value = VariantUtils.CreateFrom(in _fireNode2);
			return true;
		}
		if (name == PropertyName._fireNode3)
		{
			value = VariantUtils.CreateFrom(in _fireNode3);
			return true;
		}
		if (name == PropertyName._fireNode4)
		{
			value = VariantUtils.CreateFrom(in _fireNode4);
			return true;
		}
		if (name == PropertyName._explosionParticles)
		{
			value = VariantUtils.CreateFrom(in _explosionParticles);
			return true;
		}
		if (name == PropertyName._damageParticles)
		{
			value = VariantUtils.CreateFrom(in _damageParticles);
			return true;
		}
		if (name == PropertyName._emberParticles)
		{
			value = VariantUtils.CreateFrom(in _emberParticles);
			return true;
		}
		if (name == PropertyName._thinEmberParticles)
		{
			value = VariantUtils.CreateFrom(in _thinEmberParticles);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._fireNode1, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._fireNode2, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._fireNode3, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._fireNode4, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._explosionParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._damageParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._emberParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._thinEmberParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._parent, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._fireNode1, Variant.From(in _fireNode1));
		info.AddProperty(PropertyName._fireNode2, Variant.From(in _fireNode2));
		info.AddProperty(PropertyName._fireNode3, Variant.From(in _fireNode3));
		info.AddProperty(PropertyName._fireNode4, Variant.From(in _fireNode4));
		info.AddProperty(PropertyName._explosionParticles, Variant.From(in _explosionParticles));
		info.AddProperty(PropertyName._damageParticles, Variant.From(in _damageParticles));
		info.AddProperty(PropertyName._emberParticles, Variant.From(in _emberParticles));
		info.AddProperty(PropertyName._thinEmberParticles, Variant.From(in _thinEmberParticles));
		info.AddProperty(PropertyName._parent, Variant.From(in _parent));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._fireNode1, out var value))
		{
			_fireNode1 = value.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._fireNode2, out var value2))
		{
			_fireNode2 = value2.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._fireNode3, out var value3))
		{
			_fireNode3 = value3.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._fireNode4, out var value4))
		{
			_fireNode4 = value4.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._explosionParticles, out var value5))
		{
			_explosionParticles = value5.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._damageParticles, out var value6))
		{
			_damageParticles = value6.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._emberParticles, out var value7))
		{
			_emberParticles = value7.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._thinEmberParticles, out var value8))
		{
			_thinEmberParticles = value8.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._parent, out var value9))
		{
			_parent = value9.As<Node2D>();
		}
	}
}
