using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NBounceSparkVfx.cs")]
public class NBounceSparkVfx : Node2D
{
	public new class MethodName : Node2D.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _Process = "_Process";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : Node2D.PropertyName
	{
		public static readonly StringName _particle = "_particle";

		public static readonly StringName _velocity = "_velocity";

		public static readonly StringName _startPosition = "_startPosition";

		public static readonly StringName _floorY = "_floorY";

		public static readonly StringName _tween = "_tween";
	}

	public new class SignalName : Node2D.SignalName
	{
	}

	private const string _scenePath = "res://scenes/vfx/bounce_spark_vfx.tscn";

	[Export(PropertyHint.None, "")]
	private Node2D _particle;

	private Vector2 _velocity;

	private Vector2 _startPosition;

	private float _floorY;

	private const float _targetAlpha = 0.8f;

	private static readonly Vector2 _gravity = new Vector2(0f, 1500f);

	private Tween? _tween;

	public static NBounceSparkVfx? Create(Creature target, VfxColor vfxColor = VfxColor.Gold)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NBounceSparkVfx nBounceSparkVfx = PreloadManager.Cache.GetScene("res://scenes/vfx/bounce_spark_vfx.tscn").Instantiate<NBounceSparkVfx>(PackedScene.GenEditState.Disabled);
		nBounceSparkVfx._startPosition = NCombatRoom.Instance.GetCreatureNode(target).GetBottomOfHitbox();
		return nBounceSparkVfx;
	}

	public override void _Ready()
	{
		_startPosition += new Vector2(Rng.Chaotic.NextFloat(-120f, 120f), Rng.Chaotic.NextFloat(0f, 20f));
		_floorY = _startPosition.Y + Rng.Chaotic.NextFloat(0f, 64f);
		base.GlobalPosition = _startPosition;
		_velocity = new Vector2(Rng.Chaotic.NextFloat(-400f, 400f), Rng.Chaotic.NextFloat(-800f, -300f));
		float num = Rng.Chaotic.NextFloat(0.8f, 1.2f);
		base.Scale = new Vector2(num, 2f - num) * Rng.Chaotic.NextFloat(0.1f, 0.8f);
		base.Modulate = new Color(1f, Rng.Chaotic.NextFloat(0.2f, 0.8f), Rng.Chaotic.NextFloat(0f, 0.2f), 0f);
		TaskHelper.RunSafely(Animate());
	}

	public override void _Process(double delta)
	{
		float num = (float)delta;
		base.Position += _velocity * num;
		_velocity += _gravity * num;
		base.Rotation = MathHelper.GetAngle(_velocity);
		if (base.Position.Y > _floorY)
		{
			base.Position = new Vector2(base.Position.X, _floorY);
			_velocity = new Vector2(_velocity.X, (0f - _velocity.Y) * 0.5f);
		}
	}

	public override void _ExitTree()
	{
		_tween?.Kill();
	}

	private async Task Animate()
	{
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(this, "modulate:a", 0.8f, 0.10000000149011612);
		_tween.Chain();
		float num = Rng.Chaotic.NextFloat(0.4f, 1.5f);
		_tween.TweenProperty(this, "modulate:a", 0f, num);
		_tween.TweenProperty(this, "scale", base.Scale * 0.5f, num);
		await ToSignal(_tween, Tween.SignalName.Finished);
		this.QueueFreeSafely();
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
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._particle)
		{
			_particle = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._velocity)
		{
			_velocity = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._startPosition)
		{
			_startPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._floorY)
		{
			_floorY = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._particle)
		{
			value = VariantUtils.CreateFrom(in _particle);
			return true;
		}
		if (name == PropertyName._velocity)
		{
			value = VariantUtils.CreateFrom(in _velocity);
			return true;
		}
		if (name == PropertyName._startPosition)
		{
			value = VariantUtils.CreateFrom(in _startPosition);
			return true;
		}
		if (name == PropertyName._floorY)
		{
			value = VariantUtils.CreateFrom(in _floorY);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._particle, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._velocity, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._startPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._floorY, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._particle, Variant.From(in _particle));
		info.AddProperty(PropertyName._velocity, Variant.From(in _velocity));
		info.AddProperty(PropertyName._startPosition, Variant.From(in _startPosition));
		info.AddProperty(PropertyName._floorY, Variant.From(in _floorY));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._particle, out var value))
		{
			_particle = value.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._velocity, out var value2))
		{
			_velocity = value2.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._startPosition, out var value3))
		{
			_startPosition = value3.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._floorY, out var value4))
		{
			_floorY = value4.As<float>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value5))
		{
			_tween = value5.As<Tween>();
		}
	}
}
