using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[GlobalClass]
[ScriptPath("res://src/Core/Nodes/Vfx/NDecimillipedeSegmentVfx.cs")]
public class NDecimillipedeSegmentVfx : Node
{
	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName Regenerate = "Regenerate";

		public static readonly StringName EndRegenerate = "EndRegenerate";

		public static readonly StringName Wither = "Wither";

		public static readonly StringName OnAnimationEvent = "OnAnimationEvent";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName _damageParticleNodes = "_damageParticleNodes";

		public static readonly StringName _particleGravity = "_particleGravity";

		public static readonly StringName _particleSpeedScale = "_particleSpeedScale";

		public static readonly StringName _particleVelocityMinMax = "_particleVelocityMinMax";

		public static readonly StringName _sprayNodes = "_sprayNodes";

		public static readonly StringName _parent = "_parent";
	}

	public new class SignalName : Node.SignalName
	{
	}

	private static readonly StringName _opacity = new StringName("opacity");

	private static readonly StringName _direction = new StringName("direction");

	[Export(PropertyHint.None, "")]
	private CpuParticles2D[] _damageParticleNodes;

	private readonly Vector2 _particleGravity = new Vector2(0f, 300f);

	private float _particleSpeedScale = 2f;

	private Vector2 _particleVelocityMinMax = new Vector2(400f, 600f);

	[Export(PropertyHint.None, "")]
	private Node2D[] _sprayNodes;

	private Node2D _parent;

	private MegaSprite _animController;

	private readonly List<Vector2> _sprayNodeScales = new List<Vector2>();

	public override void _Ready()
	{
		_parent = GetParent<Node2D>();
		_animController = new MegaSprite(_parent);
		_animController.ConnectAnimationEvent(Callable.From<GodotObject, GodotObject, GodotObject, GodotObject>(OnAnimationEvent));
		Node2D[] sprayNodes = _sprayNodes;
		foreach (Node2D node2D in sprayNodes)
		{
			node2D.Visible = false;
			_sprayNodeScales.Add(node2D.Scale);
		}
	}

	public void Regenerate()
	{
		for (int i = 0; i < _sprayNodes.Length; i++)
		{
			Node2D node2D = _sprayNodes[i];
			node2D.Visible = true;
			ShaderMaterial shaderMaterial = (ShaderMaterial)node2D.Material;
			shaderMaterial.SetShaderParameter(_direction, -1);
			Tween tween = CreateTween().SetParallel();
			float num = Rng.Chaotic.NextFloat(0.5f);
			shaderMaterial.SetShaderParameter(_opacity, 0.5f);
			tween.TweenProperty(node2D, "scale", _sprayNodeScales[i], num + 0.5f).SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.Out);
			tween.TweenProperty(node2D.Material, "shader_parameter/opacity", 0.9f, num).SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.Out);
		}
	}

	private void EndRegenerate()
	{
		Node2D[] sprayNodes = _sprayNodes;
		foreach (Node2D node2D in sprayNodes)
		{
			Vector2 scale = node2D.Scale;
			Tween tween = CreateTween();
			float num = Rng.Chaotic.NextFloat(0.9f, 1.25f);
			tween.TweenProperty(node2D, "scale", Vector2.Zero, num).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
			tween.TweenProperty(node2D, "visible", false, 0.0);
			tween.TweenProperty(node2D, "scale", scale, 0.0);
		}
	}

	private void Wither()
	{
		CpuParticles2D[] damageParticleNodes = _damageParticleNodes;
		foreach (CpuParticles2D cpuParticles2D in damageParticleNodes)
		{
			cpuParticles2D.Gravity = _particleGravity;
			cpuParticles2D.SpeedScale = _particleSpeedScale;
			cpuParticles2D.InitialVelocityMin = _particleVelocityMinMax.X;
			cpuParticles2D.InitialVelocityMax = _particleVelocityMinMax.Y;
			cpuParticles2D.Restart();
		}
	}

	private void OnAnimationEvent(GodotObject _, GodotObject __, GodotObject ___, GodotObject animEvent)
	{
		string eventName = new MegaEvent(animEvent).GetData().GetEventName();
		if (!(eventName == "suck_complete"))
		{
			if (eventName == "explode")
			{
				Wither();
			}
		}
		else
		{
			EndRegenerate();
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Regenerate, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.EndRegenerate, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Wither, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnAnimationEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "__", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "___", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "animEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false)
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
		if (method == MethodName.Regenerate && args.Count == 0)
		{
			Regenerate();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EndRegenerate && args.Count == 0)
		{
			EndRegenerate();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Wither && args.Count == 0)
		{
			Wither();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnAnimationEvent && args.Count == 4)
		{
			OnAnimationEvent(VariantUtils.ConvertTo<GodotObject>(in args[0]), VariantUtils.ConvertTo<GodotObject>(in args[1]), VariantUtils.ConvertTo<GodotObject>(in args[2]), VariantUtils.ConvertTo<GodotObject>(in args[3]));
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
		if (method == MethodName.Regenerate)
		{
			return true;
		}
		if (method == MethodName.EndRegenerate)
		{
			return true;
		}
		if (method == MethodName.Wither)
		{
			return true;
		}
		if (method == MethodName.OnAnimationEvent)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._damageParticleNodes)
		{
			_damageParticleNodes = VariantUtils.ConvertToSystemArrayOfGodotObject<CpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._particleSpeedScale)
		{
			_particleSpeedScale = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._particleVelocityMinMax)
		{
			_particleVelocityMinMax = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._sprayNodes)
		{
			_sprayNodes = VariantUtils.ConvertToSystemArrayOfGodotObject<Node2D>(in value);
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
		if (name == PropertyName._damageParticleNodes)
		{
			GodotObject[] damageParticleNodes = _damageParticleNodes;
			value = VariantUtils.CreateFromSystemArrayOfGodotObject(damageParticleNodes);
			return true;
		}
		if (name == PropertyName._particleGravity)
		{
			value = VariantUtils.CreateFrom(in _particleGravity);
			return true;
		}
		if (name == PropertyName._particleSpeedScale)
		{
			value = VariantUtils.CreateFrom(in _particleSpeedScale);
			return true;
		}
		if (name == PropertyName._particleVelocityMinMax)
		{
			value = VariantUtils.CreateFrom(in _particleVelocityMinMax);
			return true;
		}
		if (name == PropertyName._sprayNodes)
		{
			GodotObject[] damageParticleNodes = _sprayNodes;
			value = VariantUtils.CreateFromSystemArrayOfGodotObject(damageParticleNodes);
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
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._damageParticleNodes, PropertyHint.TypeString, "24/34:CPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._particleGravity, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._particleSpeedScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._particleVelocityMinMax, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._sprayNodes, PropertyHint.TypeString, "24/34:Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._parent, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		StringName damageParticleNodes = PropertyName._damageParticleNodes;
		GodotObject[] damageParticleNodes2 = _damageParticleNodes;
		info.AddProperty(damageParticleNodes, Variant.CreateFrom(damageParticleNodes2));
		info.AddProperty(PropertyName._particleSpeedScale, Variant.From(in _particleSpeedScale));
		info.AddProperty(PropertyName._particleVelocityMinMax, Variant.From(in _particleVelocityMinMax));
		StringName sprayNodes = PropertyName._sprayNodes;
		damageParticleNodes2 = _sprayNodes;
		info.AddProperty(sprayNodes, Variant.CreateFrom(damageParticleNodes2));
		info.AddProperty(PropertyName._parent, Variant.From(in _parent));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._damageParticleNodes, out var value))
		{
			_damageParticleNodes = value.AsGodotObjectArray<CpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._particleSpeedScale, out var value2))
		{
			_particleSpeedScale = value2.As<float>();
		}
		if (info.TryGetProperty(PropertyName._particleVelocityMinMax, out var value3))
		{
			_particleVelocityMinMax = value3.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._sprayNodes, out var value4))
		{
			_sprayNodes = value4.AsGodotObjectArray<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._parent, out var value5))
		{
			_parent = value5.As<Node2D>();
		}
	}
}
