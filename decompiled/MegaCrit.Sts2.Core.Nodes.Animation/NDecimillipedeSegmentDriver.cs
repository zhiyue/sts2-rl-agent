using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

namespace MegaCrit.Sts2.Core.Nodes.Animation;

[ScriptPath("res://src/Core/Nodes/Animation/NDecimillipedeSegmentDriver.cs")]
public class NDecimillipedeSegmentDriver : Node2D
{
	public new class MethodName : Node2D.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _Process = "_Process";

		public static readonly StringName AttackShake = "AttackShake";
	}

	public new class PropertyName : Node2D.PropertyName
	{
		public static readonly StringName _leftSegment = "_leftSegment";

		public static readonly StringName _speed = "_speed";

		public static readonly StringName _magnitude = "_magnitude";

		public static readonly StringName _originPos = "_originPos";

		public static readonly StringName _noise = "_noise";

		public static readonly StringName _time = "_time";

		public static readonly StringName _decimillipedeStrikeOffset = "_decimillipedeStrikeOffset";

		public static readonly StringName _attackTween = "_attackTween";
	}

	public new class SignalName : Node2D.SignalName
	{
	}

	[Export(PropertyHint.None, "")]
	private bool _leftSegment;

	private float _speed;

	private float _magnitude;

	private Vector2 _originPos;

	private FastNoiseLite _noise = new FastNoiseLite();

	private float _time;

	private Vector2 _decimillipedeStrikeOffset = Vector2.Zero;

	private Tween? _attackTween;

	public override void _Ready()
	{
		_originPos = base.Position;
		_speed = (_leftSegment ? 0.1f : 0.05f);
		_magnitude = (_leftSegment ? 250f : 300f);
		_noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
		_noise.Frequency = 1f;
	}

	public override void _Process(double delta)
	{
		_time += (float)delta;
		float num = _time * _speed + (_leftSegment ? 0.25f : 0f);
		Vector2 vector = new Vector2(_noise.GetNoise1D(num), _noise.GetNoise1D(num + 0.25f)) * _magnitude;
		base.Position = _originPos + vector + _decimillipedeStrikeOffset;
	}

	public void AttackShake()
	{
		_attackTween?.Kill();
		_attackTween = CreateTween();
		_attackTween.TweenProperty(this, "_decimillipedeStrikeOffset", Vector2.Left * 100f, 0.4).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
		_attackTween.TweenProperty(this, "_decimillipedeStrikeOffset", Vector2.Right * 100f, 0.10000000149011612).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
		_attackTween.TweenProperty(this, "_decimillipedeStrikeOffset", Vector2.Zero, 0.75).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AttackShake, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.AttackShake && args.Count == 0)
		{
			AttackShake();
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
		if (method == MethodName.AttackShake)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._leftSegment)
		{
			_leftSegment = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._speed)
		{
			_speed = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._magnitude)
		{
			_magnitude = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._originPos)
		{
			_originPos = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._noise)
		{
			_noise = VariantUtils.ConvertTo<FastNoiseLite>(in value);
			return true;
		}
		if (name == PropertyName._time)
		{
			_time = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._decimillipedeStrikeOffset)
		{
			_decimillipedeStrikeOffset = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._attackTween)
		{
			_attackTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._leftSegment)
		{
			value = VariantUtils.CreateFrom(in _leftSegment);
			return true;
		}
		if (name == PropertyName._speed)
		{
			value = VariantUtils.CreateFrom(in _speed);
			return true;
		}
		if (name == PropertyName._magnitude)
		{
			value = VariantUtils.CreateFrom(in _magnitude);
			return true;
		}
		if (name == PropertyName._originPos)
		{
			value = VariantUtils.CreateFrom(in _originPos);
			return true;
		}
		if (name == PropertyName._noise)
		{
			value = VariantUtils.CreateFrom(in _noise);
			return true;
		}
		if (name == PropertyName._time)
		{
			value = VariantUtils.CreateFrom(in _time);
			return true;
		}
		if (name == PropertyName._decimillipedeStrikeOffset)
		{
			value = VariantUtils.CreateFrom(in _decimillipedeStrikeOffset);
			return true;
		}
		if (name == PropertyName._attackTween)
		{
			value = VariantUtils.CreateFrom(in _attackTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._leftSegment, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._speed, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._magnitude, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._originPos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._noise, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._time, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._decimillipedeStrikeOffset, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._attackTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._leftSegment, Variant.From(in _leftSegment));
		info.AddProperty(PropertyName._speed, Variant.From(in _speed));
		info.AddProperty(PropertyName._magnitude, Variant.From(in _magnitude));
		info.AddProperty(PropertyName._originPos, Variant.From(in _originPos));
		info.AddProperty(PropertyName._noise, Variant.From(in _noise));
		info.AddProperty(PropertyName._time, Variant.From(in _time));
		info.AddProperty(PropertyName._decimillipedeStrikeOffset, Variant.From(in _decimillipedeStrikeOffset));
		info.AddProperty(PropertyName._attackTween, Variant.From(in _attackTween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._leftSegment, out var value))
		{
			_leftSegment = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._speed, out var value2))
		{
			_speed = value2.As<float>();
		}
		if (info.TryGetProperty(PropertyName._magnitude, out var value3))
		{
			_magnitude = value3.As<float>();
		}
		if (info.TryGetProperty(PropertyName._originPos, out var value4))
		{
			_originPos = value4.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._noise, out var value5))
		{
			_noise = value5.As<FastNoiseLite>();
		}
		if (info.TryGetProperty(PropertyName._time, out var value6))
		{
			_time = value6.As<float>();
		}
		if (info.TryGetProperty(PropertyName._decimillipedeStrikeOffset, out var value7))
		{
			_decimillipedeStrikeOffset = value7.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._attackTween, out var value8))
		{
			_attackTween = value8.As<Tween>();
		}
	}
}
