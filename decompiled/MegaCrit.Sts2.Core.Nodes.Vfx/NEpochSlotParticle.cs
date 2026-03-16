using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NEpochSlotParticle.cs")]
public class NEpochSlotParticle : Sprite2D
{
	public new class MethodName : Sprite2D.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _Process = "_Process";
	}

	public new class PropertyName : Sprite2D.PropertyName
	{
		public static readonly StringName _timer = "_timer";

		public static readonly StringName _target = "_target";

		public static readonly StringName _speed = "_speed";
	}

	public new class SignalName : Sprite2D.SignalName
	{
	}

	public static readonly string scenePath = SceneHelper.GetScenePath("vfx/epoch_slot_particle_vfx");

	private const double _checkRate = 0.25;

	private double _timer;

	private Control _target;

	private float _speed;

	private const float _deleteDistance = 1500f;

	public static NEpochSlotParticle Create(Control target)
	{
		NEpochSlotParticle nEpochSlotParticle = PreloadManager.Cache.GetScene(scenePath).Instantiate<NEpochSlotParticle>(PackedScene.GenEditState.Disabled);
		nEpochSlotParticle._target = target;
		return nEpochSlotParticle;
	}

	public override void _Ready()
	{
		base.GlobalPosition = _target.GlobalPosition + new Vector2(Rng.Chaotic.NextFloat(40f, 350f) * (Rng.Chaotic.NextBool() ? (-1f) : 1f), Rng.Chaotic.NextFloat(40f, 300f) * (Rng.Chaotic.NextBool() ? (-1f) : 1f));
		float num = Rng.Chaotic.NextFloat(0.5f, 1.5f);
		base.Scale = new Vector2(1f, 1f) * num;
		base.Modulate = new Color(Rng.Chaotic.NextFloat(0.75f, 0.9f), Rng.Chaotic.NextFloat(0.7f, 0.8f), Rng.Chaotic.NextFloat(0f, 0.4f), 0f);
		_speed = Rng.Chaotic.NextFloat(3f, 4f) / num;
		Vector2 vector = _target.GlobalPosition - base.GlobalPosition;
		base.Rotation = Mathf.Atan2(vector.Y, vector.X) + (float)Math.PI / 2f;
		Tween tween = CreateTween();
		tween.TweenProperty(this, "modulate:a", Rng.Chaotic.NextFloat(0.75f, 1f), 0.25);
	}

	public override void _Process(double delta)
	{
		_timer -= delta;
		if (_timer < 0.0)
		{
			_timer += 0.25;
			if (base.GlobalPosition.DistanceSquaredTo(_target.GlobalPosition) < 1500f)
			{
				this.QueueFreeSafely();
			}
		}
		base.GlobalPosition = base.GlobalPosition.Lerp(_target.GlobalPosition, (float)((double)_speed * delta));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Sprite2D"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "target", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NEpochSlotParticle>(Create(VariantUtils.ConvertTo<Control>(in args[0])));
			return true;
		}
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
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NEpochSlotParticle>(Create(VariantUtils.ConvertTo<Control>(in args[0])));
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.Create)
		{
			return true;
		}
		if (method == MethodName._Ready)
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
		if (name == PropertyName._timer)
		{
			_timer = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName._target)
		{
			_target = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._speed)
		{
			_speed = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._timer)
		{
			value = VariantUtils.CreateFrom(in _timer);
			return true;
		}
		if (name == PropertyName._target)
		{
			value = VariantUtils.CreateFrom(in _target);
			return true;
		}
		if (name == PropertyName._speed)
		{
			value = VariantUtils.CreateFrom(in _speed);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._timer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._target, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._speed, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._timer, Variant.From(in _timer));
		info.AddProperty(PropertyName._target, Variant.From(in _target));
		info.AddProperty(PropertyName._speed, Variant.From(in _speed));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._timer, out var value))
		{
			_timer = value.As<double>();
		}
		if (info.TryGetProperty(PropertyName._target, out var value2))
		{
			_target = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._speed, out var value3))
		{
			_speed = value3.As<float>();
		}
	}
}
