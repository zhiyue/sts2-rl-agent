using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NRollingBoulderVfx.cs")]
public class NRollingBoulderVfx : Node2D
{
	[Signal]
	public delegate void HitCreatureEventHandler(NCreature creature);

	public new class MethodName : Node2D.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName CleanUpBeforeEarlyExit = "CleanUpBeforeEarlyExit";
	}

	public new class PropertyName : Node2D.PropertyName
	{
		public static readonly StringName _boulder = "_boulder";

		public static readonly StringName _shadow = "_shadow";

		public static readonly StringName _slamBehind = "_slamBehind";

		public static readonly StringName _slamFront = "_slamFront";
	}

	public new class SignalName : Node2D.SignalName
	{
		public static readonly StringName HitCreature = "HitCreature";
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("vfx/vfx_rolling_boulder");

	private const float _maxScale = 1.5f;

	private const decimal _halfScaleDamage = 40m;

	private const float _maxTimeToImpact = 0.5f;

	private const float _minTimeToImpact = 0.25f;

	private const float _minRotationSpeed = 600f;

	private const float _maxRotationSpeed = 1000f;

	private const float _minXOffset = 600f;

	private const float _maxXOffset = 1000f;

	private Sprite2D _boulder;

	private Sprite2D _shadow;

	private GpuParticles2D _slamBehind;

	private GpuParticles2D _slamFront;

	private List<Creature> _creatures;

	private Vector2? _debugFinalPosition;

	private decimal _damage;

	private HitCreatureEventHandler backing_HitCreature;

	public static string[] AssetPaths => new string[1] { _scenePath };

	public event HitCreatureEventHandler HitCreature
	{
		add
		{
			backing_HitCreature = (HitCreatureEventHandler)Delegate.Combine(backing_HitCreature, value);
		}
		remove
		{
			backing_HitCreature = (HitCreatureEventHandler)Delegate.Remove(backing_HitCreature, value);
		}
	}

	public static NRollingBoulderVfx? Create(IEnumerable<Creature> creatures, decimal damage, Vector2? debugFinalPosition = null)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NRollingBoulderVfx nRollingBoulderVfx = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NRollingBoulderVfx>(PackedScene.GenEditState.Disabled);
		nRollingBoulderVfx._creatures = creatures.ToList();
		nRollingBoulderVfx._damage = damage;
		nRollingBoulderVfx._debugFinalPosition = debugFinalPosition;
		return nRollingBoulderVfx;
	}

	public override void _Ready()
	{
		_boulder = GetNode<Sprite2D>("Boulder");
		_shadow = GetNode<Sprite2D>("Shadow");
		_slamBehind = GetNode<GpuParticles2D>("SlamBehind");
		_slamFront = GetNode<GpuParticles2D>("SlamFront");
		float num = (float)(_damage / (_damage + 40m));
		float num2 = num * 1.5f;
		float timeToImpact = Mathf.Lerp(0.5f, 0.25f, num);
		float rotationSpeed = Mathf.Lerp(600f, 1000f, num);
		float xOffset = Mathf.Lerp(600f, 1000f, num);
		base.Scale = Vector2.One * num2;
		foreach (GpuParticles2D item in GetChildren().OfType<GpuParticles2D>())
		{
			if (!item.LocalCoords && item.ProcessMaterial is ParticleProcessMaterial particleProcessMaterial)
			{
				particleProcessMaterial.Scale = Vector2.One * num2;
			}
		}
		TaskHelper.RunSafely(PlayAnim(timeToImpact, xOffset, rotationSpeed));
	}

	private async Task PlayAnim(float timeToImpact, float xOffset, float rotationSpeed)
	{
		if (NCombatRoom.Instance == null || _creatures.Count == 0)
		{
			CleanUpBeforeEarlyExit();
			return;
		}
		Vector2 initialBoulderPosition = base.GlobalPosition;
		Vector2 initialShadowOffset = _shadow.Position;
		Vector2 initialShadowScale = _shadow.Scale;
		Vector2 vector = Vector2.Zero;
		List<Creature> creaturesHit = new List<Creature>();
		int num = 0;
		foreach (Creature creature in _creatures)
		{
			NCreature creatureNode = NCombatRoom.Instance.GetCreatureNode(creature);
			if (creatureNode != null)
			{
				vector += creatureNode.Visuals.GlobalPosition;
				num++;
			}
		}
		vector /= (float)num;
		_shadow.Reparent(NCombatRoom.Instance.BackCombatVfxContainer);
		if (vector == Vector2.Zero)
		{
			if (!_debugFinalPosition.HasValue)
			{
				CleanUpBeforeEarlyExit();
				return;
			}
			vector = _debugFinalPosition.Value;
		}
		Vector2 impactPoint = new Vector2(GetViewportRect().Size.X * 0.5f, vector.Y);
		Vector2 globalPosition = new Vector2(impactPoint.X - xOffset, -150f);
		float x = (impactPoint.X - globalPosition.X) / timeToImpact;
		float yAccel = 2f * (impactPoint.Y - globalPosition.Y) / (timeToImpact * timeToImpact);
		_shadow.Scale = Vector2.Zero;
		base.GlobalPosition = globalPosition;
		Vector2 velocity = new Vector2(x, 0f);
		float timer = 0f;
		bool firstImpact = false;
		while (timer <= 1f)
		{
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			float num2 = (float)GetProcessDeltaTime();
			velocity.Y += yAccel * num2;
			base.GlobalPosition += velocity * num2;
			_boulder.Rotation += Mathf.DegToRad(rotationSpeed) * num2;
			_shadow.Scale = initialShadowScale * base.Scale * Mathf.InverseLerp(initialBoulderPosition.Y, impactPoint.Y, base.GlobalPosition.Y);
			_shadow.GlobalPosition = new Vector2(base.GlobalPosition.X, impactPoint.Y) + initialShadowOffset * base.Scale;
			foreach (Creature creature2 in _creatures)
			{
				if (creaturesHit.Contains(creature2))
				{
					continue;
				}
				NCreature creatureNode2 = NCombatRoom.Instance.GetCreatureNode(creature2);
				if (creatureNode2 == null)
				{
					creaturesHit.Add(creature2);
					continue;
				}
				float num3 = creatureNode2.Visuals.GlobalPosition.X - creatureNode2.Visuals.Bounds.Size.X * 0.5f;
				if (base.GlobalPosition.X >= num3)
				{
					EmitSignalHitCreature(creatureNode2);
					creaturesHit.Add(creature2);
				}
			}
			if (_creatures.Count == creaturesHit.Count)
			{
				timer += num2;
			}
			if (base.GlobalPosition.Y >= impactPoint.Y)
			{
				if (!firstImpact)
				{
					_slamBehind.GlobalPosition = impactPoint;
					_slamFront.GlobalPosition = impactPoint;
					_slamBehind.Emitting = true;
					_slamFront.Emitting = true;
					firstImpact = true;
				}
				velocity.Y = (0f - velocity.Y) * 0.33f;
				if (Mathf.Abs(velocity.Y) < 1f)
				{
					velocity.Y = 0f;
					yAccel = 0f;
				}
				NRollingBoulderVfx nRollingBoulderVfx = this;
				Vector2 globalPosition2 = base.GlobalPosition;
				globalPosition2.Y = impactPoint.Y;
				nRollingBoulderVfx.GlobalPosition = globalPosition2;
			}
		}
		_shadow.QueueFreeSafely();
		this.QueueFreeSafely();
	}

	private void CleanUpBeforeEarlyExit()
	{
		Log.Warn("Rolling boulder VFX spawned with no targets, disabling");
		_shadow.QueueFreeSafely();
		this.QueueFreeSafely();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CleanUpBeforeEarlyExit, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.CleanUpBeforeEarlyExit && args.Count == 0)
		{
			CleanUpBeforeEarlyExit();
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
		if (method == MethodName.CleanUpBeforeEarlyExit)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._boulder)
		{
			_boulder = VariantUtils.ConvertTo<Sprite2D>(in value);
			return true;
		}
		if (name == PropertyName._shadow)
		{
			_shadow = VariantUtils.ConvertTo<Sprite2D>(in value);
			return true;
		}
		if (name == PropertyName._slamBehind)
		{
			_slamBehind = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._slamFront)
		{
			_slamFront = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._boulder)
		{
			value = VariantUtils.CreateFrom(in _boulder);
			return true;
		}
		if (name == PropertyName._shadow)
		{
			value = VariantUtils.CreateFrom(in _shadow);
			return true;
		}
		if (name == PropertyName._slamBehind)
		{
			value = VariantUtils.CreateFrom(in _slamBehind);
			return true;
		}
		if (name == PropertyName._slamFront)
		{
			value = VariantUtils.CreateFrom(in _slamFront);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._boulder, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._shadow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._slamBehind, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._slamFront, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._boulder, Variant.From(in _boulder));
		info.AddProperty(PropertyName._shadow, Variant.From(in _shadow));
		info.AddProperty(PropertyName._slamBehind, Variant.From(in _slamBehind));
		info.AddProperty(PropertyName._slamFront, Variant.From(in _slamFront));
		info.AddSignalEventDelegate(SignalName.HitCreature, backing_HitCreature);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._boulder, out var value))
		{
			_boulder = value.As<Sprite2D>();
		}
		if (info.TryGetProperty(PropertyName._shadow, out var value2))
		{
			_shadow = value2.As<Sprite2D>();
		}
		if (info.TryGetProperty(PropertyName._slamBehind, out var value3))
		{
			_slamBehind = value3.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._slamFront, out var value4))
		{
			_slamFront = value4.As<GpuParticles2D>();
		}
		if (info.TryGetSignalEventDelegate<HitCreatureEventHandler>(SignalName.HitCreature, out var value5))
		{
			backing_HitCreature = value5;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotSignalList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(SignalName.HitCreature, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "creature", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		return list;
	}

	protected void EmitSignalHitCreature(NCreature creature)
	{
		EmitSignal(SignalName.HitCreature, creature);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.HitCreature && args.Count == 1)
		{
			backing_HitCreature?.Invoke(VariantUtils.ConvertTo<NCreature>(in args[0]));
		}
		else
		{
			base.RaiseGodotClassSignalCallbacks(in signal, args);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassSignal(in godot_string_name signal)
	{
		if (signal == SignalName.HitCreature)
		{
			return true;
		}
		return base.HasGodotClassSignal(in signal);
	}
}
