using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Nodes.Vfx.Cards;

[ScriptPath("res://src/Core/Nodes/Vfx/Cards/NSpookyHandVfx.cs")]
public class NSpookyHandVfx : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName AnimateIn = "AnimateIn";

		public new static readonly StringName _Process = "_Process";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _elapsedPauseTime = "_elapsedPauseTime";

		public static readonly StringName _pauseCounter = "_pauseCounter";

		public static readonly StringName _isPaused = "_isPaused";

		public static readonly StringName _timer = "_timer";

		public static readonly StringName _totalPauses = "_totalPauses";

		public static readonly StringName _canPauseTimer = "_canPauseTimer";

		public static readonly StringName _intensity = "_intensity";

		public static readonly StringName _speed = "_speed";

		public static readonly StringName _duration = "_duration";

		public static readonly StringName _originalRotation = "_originalRotation";

		public static readonly StringName _targetScale = "_targetScale";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private const float _pauseDuration = 0.05f;

	private float _elapsedPauseTime;

	private int _pauseCounter;

	private bool _isPaused;

	private float _timer;

	private int _totalPauses;

	private float _canPauseTimer;

	private float _intensity;

	private float _speed;

	private float _duration;

	private float _originalRotation;

	private Vector2 _targetScale;

	public override void _Ready()
	{
		_totalPauses = Rng.Chaotic.NextInt(2, 7);
		_canPauseTimer = Rng.Chaotic.NextFloat(0.5f, 1.2f);
		_speed = Rng.Chaotic.NextFloat(3f, 5f);
		_intensity = Rng.Chaotic.NextFloat(0.1f, 0.3f);
		_originalRotation = base.Rotation;
		_targetScale = base.Scale;
		AnimateIn();
	}

	private void AnimateIn()
	{
		base.Scale = Vector2.Zero;
		Tween tween = CreateTween().SetParallel();
		base.Modulate = new Color(base.Modulate.R + Rng.Chaotic.NextFloat(-0.2f, 0.2f), base.Modulate.G, base.Modulate.B);
		tween.TweenInterval(Rng.Chaotic.NextDouble(0.0, 0.4));
		tween.Chain();
		tween.TweenProperty(this, "scale", _targetScale, Rng.Chaotic.NextDouble(0.4, 0.5)).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Spring);
		tween.Chain();
		tween.TweenInterval(Rng.Chaotic.NextDouble(0.3, 0.6));
		tween.Chain();
		double duration = Rng.Chaotic.NextDouble(0.4, 0.6);
		tween.TweenProperty(this, "scale", _targetScale * 0.5f, duration).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Back);
		tween.TweenProperty(this, "modulate:a", 0f, duration).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
	}

	public override void _Process(double delta)
	{
		float num = (float)delta;
		_duration += num * _speed;
		_canPauseTimer -= num;
		if (_isPaused)
		{
			_elapsedPauseTime += num;
			if (_elapsedPauseTime >= 0.05f)
			{
				_isPaused = false;
				_elapsedPauseTime = 0f;
				_pauseCounter++;
			}
			base.Rotation = _originalRotation + Mathf.Sin(_duration) * _intensity * 0.5f;
		}
		else
		{
			base.Rotation = _originalRotation + Mathf.Sin(_duration) * _intensity;
		}
		_timer += num;
		if (_canPauseTimer < 0f && _pauseCounter < _totalPauses)
		{
			_isPaused = true;
			_timer = 0f;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimateIn, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.AnimateIn && args.Count == 0)
		{
			AnimateIn();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
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
		if (method == MethodName.AnimateIn)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._elapsedPauseTime)
		{
			_elapsedPauseTime = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._pauseCounter)
		{
			_pauseCounter = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._isPaused)
		{
			_isPaused = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._timer)
		{
			_timer = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._totalPauses)
		{
			_totalPauses = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._canPauseTimer)
		{
			_canPauseTimer = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._intensity)
		{
			_intensity = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._speed)
		{
			_speed = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._duration)
		{
			_duration = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._originalRotation)
		{
			_originalRotation = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._targetScale)
		{
			_targetScale = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._elapsedPauseTime)
		{
			value = VariantUtils.CreateFrom(in _elapsedPauseTime);
			return true;
		}
		if (name == PropertyName._pauseCounter)
		{
			value = VariantUtils.CreateFrom(in _pauseCounter);
			return true;
		}
		if (name == PropertyName._isPaused)
		{
			value = VariantUtils.CreateFrom(in _isPaused);
			return true;
		}
		if (name == PropertyName._timer)
		{
			value = VariantUtils.CreateFrom(in _timer);
			return true;
		}
		if (name == PropertyName._totalPauses)
		{
			value = VariantUtils.CreateFrom(in _totalPauses);
			return true;
		}
		if (name == PropertyName._canPauseTimer)
		{
			value = VariantUtils.CreateFrom(in _canPauseTimer);
			return true;
		}
		if (name == PropertyName._intensity)
		{
			value = VariantUtils.CreateFrom(in _intensity);
			return true;
		}
		if (name == PropertyName._speed)
		{
			value = VariantUtils.CreateFrom(in _speed);
			return true;
		}
		if (name == PropertyName._duration)
		{
			value = VariantUtils.CreateFrom(in _duration);
			return true;
		}
		if (name == PropertyName._originalRotation)
		{
			value = VariantUtils.CreateFrom(in _originalRotation);
			return true;
		}
		if (name == PropertyName._targetScale)
		{
			value = VariantUtils.CreateFrom(in _targetScale);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._elapsedPauseTime, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._pauseCounter, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isPaused, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._timer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._totalPauses, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._canPauseTimer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._intensity, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._speed, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._duration, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._originalRotation, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._targetScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._elapsedPauseTime, Variant.From(in _elapsedPauseTime));
		info.AddProperty(PropertyName._pauseCounter, Variant.From(in _pauseCounter));
		info.AddProperty(PropertyName._isPaused, Variant.From(in _isPaused));
		info.AddProperty(PropertyName._timer, Variant.From(in _timer));
		info.AddProperty(PropertyName._totalPauses, Variant.From(in _totalPauses));
		info.AddProperty(PropertyName._canPauseTimer, Variant.From(in _canPauseTimer));
		info.AddProperty(PropertyName._intensity, Variant.From(in _intensity));
		info.AddProperty(PropertyName._speed, Variant.From(in _speed));
		info.AddProperty(PropertyName._duration, Variant.From(in _duration));
		info.AddProperty(PropertyName._originalRotation, Variant.From(in _originalRotation));
		info.AddProperty(PropertyName._targetScale, Variant.From(in _targetScale));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._elapsedPauseTime, out var value))
		{
			_elapsedPauseTime = value.As<float>();
		}
		if (info.TryGetProperty(PropertyName._pauseCounter, out var value2))
		{
			_pauseCounter = value2.As<int>();
		}
		if (info.TryGetProperty(PropertyName._isPaused, out var value3))
		{
			_isPaused = value3.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._timer, out var value4))
		{
			_timer = value4.As<float>();
		}
		if (info.TryGetProperty(PropertyName._totalPauses, out var value5))
		{
			_totalPauses = value5.As<int>();
		}
		if (info.TryGetProperty(PropertyName._canPauseTimer, out var value6))
		{
			_canPauseTimer = value6.As<float>();
		}
		if (info.TryGetProperty(PropertyName._intensity, out var value7))
		{
			_intensity = value7.As<float>();
		}
		if (info.TryGetProperty(PropertyName._speed, out var value8))
		{
			_speed = value8.As<float>();
		}
		if (info.TryGetProperty(PropertyName._duration, out var value9))
		{
			_duration = value9.As<float>();
		}
		if (info.TryGetProperty(PropertyName._originalRotation, out var value10))
		{
			_originalRotation = value10.As<float>();
		}
		if (info.TryGetProperty(PropertyName._targetScale, out var value11))
		{
			_targetScale = value11.As<Vector2>();
		}
	}
}
