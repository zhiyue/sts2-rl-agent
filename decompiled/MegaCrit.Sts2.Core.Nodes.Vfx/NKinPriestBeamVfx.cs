using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NKinPriestBeamVfx.cs")]
public class NKinPriestBeamVfx : Node2D
{
	public new class MethodName : Node2D.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _Process = "_Process";

		public static readonly StringName Fire = "Fire";

		public static readonly StringName OnTweenComplete = "OnTweenComplete";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : Node2D.PropertyName
	{
		public static readonly StringName _beam = "_beam";

		public static readonly StringName _beamHolder = "_beamHolder";

		public static readonly StringName _staticParticles = "_staticParticles";

		public static readonly StringName _baseBeamScale = "_baseBeamScale";

		public static readonly StringName _lengthTween = "_lengthTween";

		public static readonly StringName _rotationTween = "_rotationTween";
	}

	public new class SignalName : Node2D.SignalName
	{
	}

	private const float _beamMaxLengthScale = 4f;

	private const float _startRotation = 1f;

	private const float _endRotation = -1f;

	private Sprite2D _beam;

	private Node2D _beamHolder;

	private GpuParticles2D _staticParticles;

	private Vector2 _baseBeamScale;

	private Tween? _lengthTween;

	private Tween? _rotationTween;

	public override void _Ready()
	{
		_beam = GetNode<Sprite2D>("BeamHolder/Beam");
		_staticParticles = GetNode<GpuParticles2D>("BeamHolder/StaticParticles");
		_beamHolder = GetNode<Node2D>("BeamHolder");
		_baseBeamScale = _beam.Scale;
		_staticParticles.Emitting = false;
		_staticParticles.Visible = false;
		_beamHolder.Visible = false;
	}

	public override void _Process(double delta)
	{
		Vector2 vector = new Vector2(Rng.Chaotic.NextFloat(-0.05f, 0.05f), Rng.Chaotic.NextFloat(-0.7f, 0.7f));
		_beam.Scale = _baseBeamScale + vector;
		Color modulate = base.Modulate;
		modulate.A = Rng.Chaotic.NextFloat(0.8f, 1f);
		base.Modulate = modulate;
	}

	public void Fire()
	{
		_staticParticles.Restart();
		_staticParticles.Visible = true;
		_beamHolder.Visible = true;
		_rotationTween = CreateTween();
		base.RotationDegrees = 1f;
		_rotationTween.TweenProperty(this, "rotation_degrees", -1f, 0.800000011920929).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		_beamHolder.Scale = Vector2.One;
		_lengthTween = CreateTween();
		_lengthTween.TweenProperty(_beamHolder, "scale:x", 4f, 0.3799999952316284).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_lengthTween.Chain().TweenProperty(_beamHolder, "scale:x", 0.5, 0.6000000238418579).SetEase(Tween.EaseType.In)
			.SetTrans(Tween.TransitionType.Expo);
		_lengthTween.TweenCallback(Callable.From(OnTweenComplete));
	}

	private void OnTweenComplete()
	{
		_rotationTween.Kill();
		_lengthTween.Kill();
		_staticParticles.Emitting = false;
		_staticParticles.Visible = false;
		_beamHolder.Visible = false;
	}

	public override void _ExitTree()
	{
		_lengthTween?.Kill();
		_rotationTween?.Kill();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.Fire, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnTweenComplete, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Fire && args.Count == 0)
		{
			Fire();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnTweenComplete && args.Count == 0)
		{
			OnTweenComplete();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
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
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName.Fire)
		{
			return true;
		}
		if (method == MethodName.OnTweenComplete)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._beam)
		{
			_beam = VariantUtils.ConvertTo<Sprite2D>(in value);
			return true;
		}
		if (name == PropertyName._beamHolder)
		{
			_beamHolder = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._staticParticles)
		{
			_staticParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._baseBeamScale)
		{
			_baseBeamScale = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._lengthTween)
		{
			_lengthTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._rotationTween)
		{
			_rotationTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._beam)
		{
			value = VariantUtils.CreateFrom(in _beam);
			return true;
		}
		if (name == PropertyName._beamHolder)
		{
			value = VariantUtils.CreateFrom(in _beamHolder);
			return true;
		}
		if (name == PropertyName._staticParticles)
		{
			value = VariantUtils.CreateFrom(in _staticParticles);
			return true;
		}
		if (name == PropertyName._baseBeamScale)
		{
			value = VariantUtils.CreateFrom(in _baseBeamScale);
			return true;
		}
		if (name == PropertyName._lengthTween)
		{
			value = VariantUtils.CreateFrom(in _lengthTween);
			return true;
		}
		if (name == PropertyName._rotationTween)
		{
			value = VariantUtils.CreateFrom(in _rotationTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._beam, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._beamHolder, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._staticParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._baseBeamScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._lengthTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._rotationTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._beam, Variant.From(in _beam));
		info.AddProperty(PropertyName._beamHolder, Variant.From(in _beamHolder));
		info.AddProperty(PropertyName._staticParticles, Variant.From(in _staticParticles));
		info.AddProperty(PropertyName._baseBeamScale, Variant.From(in _baseBeamScale));
		info.AddProperty(PropertyName._lengthTween, Variant.From(in _lengthTween));
		info.AddProperty(PropertyName._rotationTween, Variant.From(in _rotationTween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._beam, out var value))
		{
			_beam = value.As<Sprite2D>();
		}
		if (info.TryGetProperty(PropertyName._beamHolder, out var value2))
		{
			_beamHolder = value2.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._staticParticles, out var value3))
		{
			_staticParticles = value3.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._baseBeamScale, out var value4))
		{
			_baseBeamScale = value4.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._lengthTween, out var value5))
		{
			_lengthTween = value5.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._rotationTween, out var value6))
		{
			_rotationTween = value6.As<Tween>();
		}
	}
}
