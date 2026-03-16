using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NLivingGasVfx.cs")]
public class NLivingGasVfx : Node
{
	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnAnimationEvent = "OnAnimationEvent";

		public static readonly StringName OnAttackStart = "OnAttackStart";

		public static readonly StringName OnAttackEnd = "OnAttackEnd";

		public static readonly StringName OnHurtStart = "OnHurtStart";

		public static readonly StringName OnHurtEnd = "OnHurtEnd";

		public static readonly StringName OnDebuffStart = "OnDebuffStart";

		public static readonly StringName OnDebuffEnd = "OnDebuffEnd";

		public static readonly StringName OnDissipate = "OnDissipate";

		public static readonly StringName DissipateFunction = "DissipateFunction";

		public static readonly StringName OnDeathBreathStart = "OnDeathBreathStart";

		public static readonly StringName OnDeathBreathEnd = "OnDeathBreathEnd";

		public static readonly StringName OnReconstitute = "OnReconstitute";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName _attackPuffParticles = "_attackPuffParticles";

		public static readonly StringName _attackSparkParticles = "_attackSparkParticles";

		public static readonly StringName _debuffPuffParticles = "_debuffPuffParticles";

		public static readonly StringName _gasPuffParticles = "_gasPuffParticles";

		public static readonly StringName _parent = "_parent";
	}

	public new class SignalName : Node.SignalName
	{
	}

	private static readonly StringName _alphaStep = new StringName("AlphaStep");

	private GpuParticles2D _attackPuffParticles;

	private GpuParticles2D _attackSparkParticles;

	private GpuParticles2D _debuffPuffParticles;

	private GpuParticles2D _gasPuffParticles;

	private MegaSlotNode _smokeSlot1;

	private MegaSlotNode _smokeSlot2;

	private MegaSlotNode _smokeSlot3;

	private List<ShaderMaterial?> _smokeMaterials;

	private List<Vector2> _smokeSteps;

	private Node2D _parent;

	private MegaSprite _megaSprite;

	public override void _Ready()
	{
		_parent = GetParent<Node2D>();
		_attackPuffParticles = GetNode<GpuParticles2D>("../AttackFXNode/AttackPuffParticles");
		_attackSparkParticles = GetNode<GpuParticles2D>("../AttackFXNode/AttackSparkParticles");
		_debuffPuffParticles = GetNode<GpuParticles2D>("../AttackFXNode/DebuffPuffParticles");
		_gasPuffParticles = GetNode<GpuParticles2D>("../AttackFXNode/GasPuffParticles");
		_smokeSlot1 = new MegaSlotNode(GetNode<Node2D>("../SmokeSlot1"));
		_smokeSlot2 = new MegaSlotNode(GetNode<Node2D>("../SmokeSlot2"));
		_smokeSlot3 = new MegaSlotNode(GetNode<Node2D>("../SmokeSlot3"));
		_smokeMaterials = new List<ShaderMaterial>();
		_smokeMaterials.Add(_smokeSlot1.GetNormalMaterial() as ShaderMaterial);
		_smokeMaterials.Add(_smokeSlot2.GetNormalMaterial() as ShaderMaterial);
		_smokeMaterials.Add(_smokeSlot3.GetNormalMaterial() as ShaderMaterial);
		_smokeSteps = new List<Vector2>();
		_smokeSteps.Add((Vector2)_smokeMaterials[0].GetShaderParameter(_alphaStep));
		_smokeSteps.Add((Vector2)_smokeMaterials[1].GetShaderParameter(_alphaStep));
		_smokeSteps.Add((Vector2)_smokeMaterials[2].GetShaderParameter(_alphaStep));
		_attackPuffParticles.Emitting = false;
		_attackSparkParticles.Emitting = false;
		_debuffPuffParticles.Emitting = false;
		_gasPuffParticles.Emitting = false;
		_megaSprite = new MegaSprite(GetParent<Node2D>());
		_megaSprite.ConnectAnimationEvent(Callable.From<GodotObject, GodotObject, GodotObject, GodotObject>(OnAnimationEvent));
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
		case 12:
			switch (eventName[0])
			{
			case 'a':
				if (eventName == "attack_start")
				{
					OnAttackStart();
				}
				break;
			case 'd':
				if (eventName == "debuff_start")
				{
					OnDebuffStart();
				}
				break;
			case 'r':
				if (eventName == "reconstitute")
				{
					OnReconstitute();
				}
				break;
			}
			break;
		case 10:
			switch (eventName[0])
			{
			case 'a':
				if (eventName == "attack_end")
				{
					OnAttackEnd();
				}
				break;
			case 'h':
				if (eventName == "hurt_start")
				{
					OnHurtStart();
				}
				break;
			case 'd':
				if (eventName == "debuff_end")
				{
					OnDebuffEnd();
				}
				break;
			}
			break;
		case 8:
			if (eventName == "hurt_end")
			{
				OnHurtEnd();
			}
			break;
		case 9:
			if (eventName == "dissipate")
			{
				OnDissipate();
			}
			break;
		case 16:
			if (eventName == "gas_breath_start")
			{
				OnDeathBreathStart();
			}
			break;
		case 14:
			if (eventName == "gas_breath_end")
			{
				OnDeathBreathEnd();
			}
			break;
		case 11:
		case 13:
		case 15:
			break;
		}
	}

	private void OnAttackStart()
	{
		_attackPuffParticles.Emitting = true;
		_attackSparkParticles.Amount = 24;
		_attackSparkParticles.Restart();
	}

	private void OnAttackEnd()
	{
		_attackPuffParticles.Emitting = false;
		_attackSparkParticles.Emitting = false;
	}

	private void OnHurtStart()
	{
		_attackSparkParticles.Amount = 100;
		_attackSparkParticles.Emitting = true;
	}

	private void OnHurtEnd()
	{
		_attackSparkParticles.Emitting = false;
	}

	private void OnDebuffStart()
	{
		_debuffPuffParticles.Emitting = true;
	}

	private void OnDebuffEnd()
	{
		_debuffPuffParticles.Emitting = false;
	}

	private void OnDissipate()
	{
		Tween tween = CreateTween();
		tween.TweenMethod(Callable.From<float>(DissipateFunction), 0f, 1f, 1.399999976158142);
		tween.SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quart);
	}

	private void DissipateFunction(float t)
	{
		for (int i = 0; i < _smokeMaterials.Count; i++)
		{
			_smokeMaterials[i].SetShaderParameter(_alphaStep, _smokeSteps[i] + (Vector2.One - _smokeSteps[i]) * t);
		}
	}

	private void OnDeathBreathStart()
	{
		_gasPuffParticles.Emitting = true;
	}

	private void OnDeathBreathEnd()
	{
		_gasPuffParticles.Emitting = false;
	}

	private void OnReconstitute()
	{
		for (int i = 0; i < _smokeMaterials.Count; i++)
		{
			_smokeMaterials[i].SetShaderParameter(_alphaStep, _smokeSteps[i]);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(13);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnAnimationEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "__", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "___", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "spineEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnAttackStart, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnAttackEnd, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnHurtStart, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnHurtEnd, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnDebuffStart, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnDebuffEnd, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnDissipate, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DissipateFunction, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "t", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnDeathBreathStart, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnDeathBreathEnd, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnReconstitute, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.OnAttackStart && args.Count == 0)
		{
			OnAttackStart();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnAttackEnd && args.Count == 0)
		{
			OnAttackEnd();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnHurtStart && args.Count == 0)
		{
			OnHurtStart();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnHurtEnd && args.Count == 0)
		{
			OnHurtEnd();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnDebuffStart && args.Count == 0)
		{
			OnDebuffStart();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnDebuffEnd && args.Count == 0)
		{
			OnDebuffEnd();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnDissipate && args.Count == 0)
		{
			OnDissipate();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DissipateFunction && args.Count == 1)
		{
			DissipateFunction(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnDeathBreathStart && args.Count == 0)
		{
			OnDeathBreathStart();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnDeathBreathEnd && args.Count == 0)
		{
			OnDeathBreathEnd();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnReconstitute && args.Count == 0)
		{
			OnReconstitute();
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
		if (method == MethodName.OnAttackStart)
		{
			return true;
		}
		if (method == MethodName.OnAttackEnd)
		{
			return true;
		}
		if (method == MethodName.OnHurtStart)
		{
			return true;
		}
		if (method == MethodName.OnHurtEnd)
		{
			return true;
		}
		if (method == MethodName.OnDebuffStart)
		{
			return true;
		}
		if (method == MethodName.OnDebuffEnd)
		{
			return true;
		}
		if (method == MethodName.OnDissipate)
		{
			return true;
		}
		if (method == MethodName.DissipateFunction)
		{
			return true;
		}
		if (method == MethodName.OnDeathBreathStart)
		{
			return true;
		}
		if (method == MethodName.OnDeathBreathEnd)
		{
			return true;
		}
		if (method == MethodName.OnReconstitute)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._attackPuffParticles)
		{
			_attackPuffParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._attackSparkParticles)
		{
			_attackSparkParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._debuffPuffParticles)
		{
			_debuffPuffParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._gasPuffParticles)
		{
			_gasPuffParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
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
		if (name == PropertyName._attackPuffParticles)
		{
			value = VariantUtils.CreateFrom(in _attackPuffParticles);
			return true;
		}
		if (name == PropertyName._attackSparkParticles)
		{
			value = VariantUtils.CreateFrom(in _attackSparkParticles);
			return true;
		}
		if (name == PropertyName._debuffPuffParticles)
		{
			value = VariantUtils.CreateFrom(in _debuffPuffParticles);
			return true;
		}
		if (name == PropertyName._gasPuffParticles)
		{
			value = VariantUtils.CreateFrom(in _gasPuffParticles);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._attackPuffParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._attackSparkParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._debuffPuffParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._gasPuffParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._parent, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._attackPuffParticles, Variant.From(in _attackPuffParticles));
		info.AddProperty(PropertyName._attackSparkParticles, Variant.From(in _attackSparkParticles));
		info.AddProperty(PropertyName._debuffPuffParticles, Variant.From(in _debuffPuffParticles));
		info.AddProperty(PropertyName._gasPuffParticles, Variant.From(in _gasPuffParticles));
		info.AddProperty(PropertyName._parent, Variant.From(in _parent));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._attackPuffParticles, out var value))
		{
			_attackPuffParticles = value.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._attackSparkParticles, out var value2))
		{
			_attackSparkParticles = value2.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._debuffPuffParticles, out var value3))
		{
			_debuffPuffParticles = value3.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._gasPuffParticles, out var value4))
		{
			_gasPuffParticles = value4.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._parent, out var value5))
		{
			_parent = value5.As<Node2D>();
		}
	}
}
