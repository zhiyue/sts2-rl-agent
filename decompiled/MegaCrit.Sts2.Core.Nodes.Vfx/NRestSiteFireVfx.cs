using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NRestSiteFireVfx.cs")]
public class NRestSiteFireVfx : Node2D
{
	public new class MethodName : Node2D.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName Flicker = "Flicker";

		public static readonly StringName Sway = "Sway";

		public static readonly StringName Extinguish = "Extinguish";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : Node2D.PropertyName
	{
		public static readonly StringName _minFlickerScale = "_minFlickerScale";

		public static readonly StringName _maxFlickerScale = "_maxFlickerScale";

		public static readonly StringName _minFlickerTime = "_minFlickerTime";

		public static readonly StringName _maxFlickerTime = "_maxFlickerTime";

		public static readonly StringName _minSkew = "_minSkew";

		public static readonly StringName _maxSkew = "_maxSkew";

		public static readonly StringName _minSkewTime = "_minSkewTime";

		public static readonly StringName _maxSkewTime = "_maxSkewTime";

		public static readonly StringName _extinguishTime = "_extinguishTime";

		public static readonly StringName _enabled = "_enabled";

		public static readonly StringName _cpuGlowParticles = "_cpuGlowParticles";

		public static readonly StringName _gpuSparkParticles = "_gpuSparkParticles";

		public static readonly StringName _baseScale = "_baseScale";

		public static readonly StringName _baseSkew = "_baseSkew";

		public static readonly StringName _scaleTweenRef = "_scaleTweenRef";
	}

	public new class SignalName : Node2D.SignalName
	{
	}

	[Export(PropertyHint.None, "")]
	private float _minFlickerScale = 0.85f;

	[Export(PropertyHint.None, "")]
	private float _maxFlickerScale = 1.05f;

	[Export(PropertyHint.None, "")]
	private float _minFlickerTime = 0.3f;

	[Export(PropertyHint.None, "")]
	private float _maxFlickerTime = 0.5f;

	[Export(PropertyHint.None, "")]
	private float _minSkew = -0.1f;

	[Export(PropertyHint.None, "")]
	private float _maxSkew = 0.1f;

	[Export(PropertyHint.None, "")]
	private float _minSkewTime = 0.8f;

	[Export(PropertyHint.None, "")]
	private float _maxSkewTime = 1.5f;

	[Export(PropertyHint.None, "")]
	private float _extinguishTime = 0.2f;

	[Export(PropertyHint.None, "")]
	private bool _enabled = true;

	[Export(PropertyHint.None, "")]
	private Array<CpuParticles2D> _cpuGlowParticles = new Array<CpuParticles2D>();

	[Export(PropertyHint.None, "")]
	private Array<GpuParticles2D> _gpuSparkParticles = new Array<GpuParticles2D>();

	private Vector2 _baseScale;

	private float _baseSkew;

	private Tween? _scaleTweenRef;

	public override void _Ready()
	{
		if (_enabled)
		{
			_baseScale = base.Scale;
			_baseSkew = base.Skew;
			Flicker();
			Sway();
		}
	}

	private void Flicker()
	{
		Vector2 vector = new Vector2(_baseScale.X, Rng.Chaotic.NextFloat(_baseScale.Y * _minFlickerScale, _baseScale.Y));
		Vector2 vector2 = new Vector2(_baseScale.X, Rng.Chaotic.NextFloat(_baseScale.Y, _baseScale.Y * _maxFlickerScale));
		Tween tween = CreateTween();
		tween.TweenProperty(this, "scale", vector, Rng.Chaotic.NextFloat(_minFlickerTime, _maxFlickerTime)).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.InOut);
		tween.TweenProperty(this, "scale", vector2, Rng.Chaotic.NextFloat(_minFlickerTime, _maxFlickerTime)).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.InOut);
		tween.TweenCallback(Callable.From(Flicker));
		_scaleTweenRef = tween;
	}

	private void Sway()
	{
		float num = Rng.Chaotic.NextFloat(_baseSkew + _minSkew, _baseSkew);
		float num2 = Rng.Chaotic.NextFloat(_baseSkew, _baseSkew + _maxSkew);
		Tween tween = CreateTween();
		tween.TweenProperty(this, "skew", num, Rng.Chaotic.NextFloat(_minSkewTime, _maxSkewTime)).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
		tween.TweenProperty(this, "skew", num2, Rng.Chaotic.NextFloat(_minSkewTime, _maxSkewTime)).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
		tween.TweenCallback(Callable.From(Sway));
	}

	public void Extinguish()
	{
		Tween tween = CreateTween().SetParallel();
		foreach (CpuParticles2D cpuGlowParticle in _cpuGlowParticles)
		{
			cpuGlowParticle.Emitting = false;
			tween.TweenProperty(cpuGlowParticle, "scale", Vector2.Zero, _extinguishTime).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.In);
		}
		foreach (GpuParticles2D gpuSparkParticle in _gpuSparkParticles)
		{
			gpuSparkParticle.Emitting = false;
		}
		_scaleTweenRef?.Kill();
		Tween tween2 = CreateTween();
		tween2.TweenProperty(this, "scale", Vector2.Zero, _extinguishTime).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.In);
	}

	public override void _ExitTree()
	{
		_scaleTweenRef?.Kill();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Flicker, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Sway, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Extinguish, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.Flicker && args.Count == 0)
		{
			Flicker();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Sway && args.Count == 0)
		{
			Sway();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Extinguish && args.Count == 0)
		{
			Extinguish();
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
		if (method == MethodName.Flicker)
		{
			return true;
		}
		if (method == MethodName.Sway)
		{
			return true;
		}
		if (method == MethodName.Extinguish)
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
		if (name == PropertyName._minFlickerScale)
		{
			_minFlickerScale = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._maxFlickerScale)
		{
			_maxFlickerScale = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._minFlickerTime)
		{
			_minFlickerTime = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._maxFlickerTime)
		{
			_maxFlickerTime = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._minSkew)
		{
			_minSkew = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._maxSkew)
		{
			_maxSkew = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._minSkewTime)
		{
			_minSkewTime = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._maxSkewTime)
		{
			_maxSkewTime = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._extinguishTime)
		{
			_extinguishTime = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._enabled)
		{
			_enabled = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._cpuGlowParticles)
		{
			_cpuGlowParticles = VariantUtils.ConvertToArray<CpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._gpuSparkParticles)
		{
			_gpuSparkParticles = VariantUtils.ConvertToArray<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._baseScale)
		{
			_baseScale = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._baseSkew)
		{
			_baseSkew = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._scaleTweenRef)
		{
			_scaleTweenRef = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._minFlickerScale)
		{
			value = VariantUtils.CreateFrom(in _minFlickerScale);
			return true;
		}
		if (name == PropertyName._maxFlickerScale)
		{
			value = VariantUtils.CreateFrom(in _maxFlickerScale);
			return true;
		}
		if (name == PropertyName._minFlickerTime)
		{
			value = VariantUtils.CreateFrom(in _minFlickerTime);
			return true;
		}
		if (name == PropertyName._maxFlickerTime)
		{
			value = VariantUtils.CreateFrom(in _maxFlickerTime);
			return true;
		}
		if (name == PropertyName._minSkew)
		{
			value = VariantUtils.CreateFrom(in _minSkew);
			return true;
		}
		if (name == PropertyName._maxSkew)
		{
			value = VariantUtils.CreateFrom(in _maxSkew);
			return true;
		}
		if (name == PropertyName._minSkewTime)
		{
			value = VariantUtils.CreateFrom(in _minSkewTime);
			return true;
		}
		if (name == PropertyName._maxSkewTime)
		{
			value = VariantUtils.CreateFrom(in _maxSkewTime);
			return true;
		}
		if (name == PropertyName._extinguishTime)
		{
			value = VariantUtils.CreateFrom(in _extinguishTime);
			return true;
		}
		if (name == PropertyName._enabled)
		{
			value = VariantUtils.CreateFrom(in _enabled);
			return true;
		}
		if (name == PropertyName._cpuGlowParticles)
		{
			value = VariantUtils.CreateFromArray(_cpuGlowParticles);
			return true;
		}
		if (name == PropertyName._gpuSparkParticles)
		{
			value = VariantUtils.CreateFromArray(_gpuSparkParticles);
			return true;
		}
		if (name == PropertyName._baseScale)
		{
			value = VariantUtils.CreateFrom(in _baseScale);
			return true;
		}
		if (name == PropertyName._baseSkew)
		{
			value = VariantUtils.CreateFrom(in _baseSkew);
			return true;
		}
		if (name == PropertyName._scaleTweenRef)
		{
			value = VariantUtils.CreateFrom(in _scaleTweenRef);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._minFlickerScale, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._maxFlickerScale, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._minFlickerTime, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._maxFlickerTime, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._minSkew, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._maxSkew, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._minSkewTime, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._maxSkewTime, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._extinguishTime, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._enabled, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._cpuGlowParticles, PropertyHint.TypeString, "24/34:CPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._gpuSparkParticles, PropertyHint.TypeString, "24/34:GPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._baseScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._baseSkew, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._scaleTweenRef, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._minFlickerScale, Variant.From(in _minFlickerScale));
		info.AddProperty(PropertyName._maxFlickerScale, Variant.From(in _maxFlickerScale));
		info.AddProperty(PropertyName._minFlickerTime, Variant.From(in _minFlickerTime));
		info.AddProperty(PropertyName._maxFlickerTime, Variant.From(in _maxFlickerTime));
		info.AddProperty(PropertyName._minSkew, Variant.From(in _minSkew));
		info.AddProperty(PropertyName._maxSkew, Variant.From(in _maxSkew));
		info.AddProperty(PropertyName._minSkewTime, Variant.From(in _minSkewTime));
		info.AddProperty(PropertyName._maxSkewTime, Variant.From(in _maxSkewTime));
		info.AddProperty(PropertyName._extinguishTime, Variant.From(in _extinguishTime));
		info.AddProperty(PropertyName._enabled, Variant.From(in _enabled));
		info.AddProperty(PropertyName._cpuGlowParticles, Variant.CreateFrom(_cpuGlowParticles));
		info.AddProperty(PropertyName._gpuSparkParticles, Variant.CreateFrom(_gpuSparkParticles));
		info.AddProperty(PropertyName._baseScale, Variant.From(in _baseScale));
		info.AddProperty(PropertyName._baseSkew, Variant.From(in _baseSkew));
		info.AddProperty(PropertyName._scaleTweenRef, Variant.From(in _scaleTweenRef));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._minFlickerScale, out var value))
		{
			_minFlickerScale = value.As<float>();
		}
		if (info.TryGetProperty(PropertyName._maxFlickerScale, out var value2))
		{
			_maxFlickerScale = value2.As<float>();
		}
		if (info.TryGetProperty(PropertyName._minFlickerTime, out var value3))
		{
			_minFlickerTime = value3.As<float>();
		}
		if (info.TryGetProperty(PropertyName._maxFlickerTime, out var value4))
		{
			_maxFlickerTime = value4.As<float>();
		}
		if (info.TryGetProperty(PropertyName._minSkew, out var value5))
		{
			_minSkew = value5.As<float>();
		}
		if (info.TryGetProperty(PropertyName._maxSkew, out var value6))
		{
			_maxSkew = value6.As<float>();
		}
		if (info.TryGetProperty(PropertyName._minSkewTime, out var value7))
		{
			_minSkewTime = value7.As<float>();
		}
		if (info.TryGetProperty(PropertyName._maxSkewTime, out var value8))
		{
			_maxSkewTime = value8.As<float>();
		}
		if (info.TryGetProperty(PropertyName._extinguishTime, out var value9))
		{
			_extinguishTime = value9.As<float>();
		}
		if (info.TryGetProperty(PropertyName._enabled, out var value10))
		{
			_enabled = value10.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._cpuGlowParticles, out var value11))
		{
			_cpuGlowParticles = value11.AsGodotArray<CpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._gpuSparkParticles, out var value12))
		{
			_gpuSparkParticles = value12.AsGodotArray<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._baseScale, out var value13))
		{
			_baseScale = value13.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._baseSkew, out var value14))
		{
			_baseSkew = value14.As<float>();
		}
		if (info.TryGetProperty(PropertyName._scaleTweenRef, out var value15))
		{
			_scaleTweenRef = value15.As<Tween>();
		}
	}
}
