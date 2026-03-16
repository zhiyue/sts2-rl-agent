using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NSweepingBeamVfx.cs")]
public class NSweepingBeamVfx : Node2D
{
	public new class MethodName : Node2D.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";
	}

	public new class PropertyName : Node2D.PropertyName
	{
		public static readonly StringName _emittingParticles = "_emittingParticles";

		public static readonly StringName _startParticles = "_startParticles";

		public static readonly StringName _endParticles = "_endParticles";

		public static readonly StringName _sweepingParticles = "_sweepingParticles";

		public static readonly StringName _sweepingIndexCurve = "_sweepingIndexCurve";

		public static readonly StringName _sweepDuration = "_sweepDuration";

		public static readonly StringName _targetCenterPositions = "_targetCenterPositions";
	}

	public new class SignalName : Node2D.SignalName
	{
	}

	public static readonly string scenePath = SceneHelper.GetScenePath("vfx/vfx_sweeping_beam");

	[Export(PropertyHint.None, "")]
	private Array<GpuParticles2D> _emittingParticles = new Array<GpuParticles2D>();

	[Export(PropertyHint.None, "")]
	private Array<GpuParticles2D> _startParticles = new Array<GpuParticles2D>();

	[Export(PropertyHint.None, "")]
	private Array<GpuParticles2D> _endParticles = new Array<GpuParticles2D>();

	[Export(PropertyHint.None, "")]
	private Array<GpuParticles2D> _sweepingParticles = new Array<GpuParticles2D>();

	[Export(PropertyHint.None, "")]
	private Curve? _sweepingIndexCurve;

	[Export(PropertyHint.None, "")]
	private float _sweepDuration = 0.65f;

	private Array<Vector2> _targetCenterPositions = new Array<Vector2>();

	public static NSweepingBeamVfx? Create(Creature owner, List<Creature> targets)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(owner);
		if (nCreature != null)
		{
			Vector2 vfxSpawnPosition = nCreature.VfxSpawnPosition;
			Player player = owner.Player;
			if (player != null && player.Character is Defect defect)
			{
				vfxSpawnPosition += defect.EyelineOffset;
			}
			Array<Vector2> array = new Array<Vector2>();
			foreach (Creature target in targets)
			{
				NCreature nCreature2 = NCombatRoom.Instance?.GetCreatureNode(target);
				if (nCreature2 != null)
				{
					array.Add(nCreature2.VfxSpawnPosition);
				}
			}
			return Create(vfxSpawnPosition, array);
		}
		return null;
	}

	public static NSweepingBeamVfx? Create(Vector2 defectEyeCenter, Array<Vector2> targetCenterPositions)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NSweepingBeamVfx nSweepingBeamVfx = PreloadManager.Cache.GetScene(scenePath).Instantiate<NSweepingBeamVfx>(PackedScene.GenEditState.Disabled);
		nSweepingBeamVfx.GlobalPosition = defectEyeCenter;
		nSweepingBeamVfx._targetCenterPositions = targetCenterPositions;
		return nSweepingBeamVfx;
	}

	public override void _Ready()
	{
		for (int i = 0; i < _emittingParticles.Count; i++)
		{
			_emittingParticles[i].Emitting = false;
		}
		TaskHelper.RunSafely(PlaySequence());
	}

	private async Task PlaySequence()
	{
		double timer = 0.0;
		bool playedImpactParticles = false;
		for (int i = 0; i < _startParticles.Count; i++)
		{
			_startParticles[i].Restart();
		}
		for (int j = 0; j < _emittingParticles.Count; j++)
		{
			_emittingParticles[j].Restart();
			_emittingParticles[j].Emitting = true;
		}
		int previousSweepIndex = -1;
		while (timer < (double)_sweepDuration)
		{
			double processDeltaTime = GetProcessDeltaTime();
			float num = (float)(timer / (double)_sweepDuration);
			int num2 = Mathf.FloorToInt(_sweepingIndexCurve.Sample(num));
			if (previousSweepIndex != num2 && num2 >= 0 && num2 < _sweepingParticles.Count)
			{
				_sweepingParticles[num2].Restart();
				previousSweepIndex = num2;
			}
			if (num >= 0.5f && !playedImpactParticles)
			{
				playedImpactParticles = true;
				NGame.Instance?.ScreenShake(ShakeStrength.Medium, ShakeDuration.Normal);
				for (int k = 0; k < _targetCenterPositions.Count; k++)
				{
					NSweepingBeamImpactVfx child = NSweepingBeamImpactVfx.Create(_targetCenterPositions[k]);
					GetTree().Root.AddChildSafely(child);
				}
			}
			timer += processDeltaTime;
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}
		for (int l = 0; l < _endParticles.Count; l++)
		{
			_endParticles[l].Restart();
		}
		for (int m = 0; m < _emittingParticles.Count; m++)
		{
			_emittingParticles[m].Emitting = false;
		}
		await Cmd.Wait(2f);
		this.QueueFreeSafely();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "defectEyeCenter", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Array, "targetCenterPositions", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<NSweepingBeamVfx>(Create(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertToArray<Vector2>(in args[1])));
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<NSweepingBeamVfx>(Create(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertToArray<Vector2>(in args[1])));
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
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._emittingParticles)
		{
			_emittingParticles = VariantUtils.ConvertToArray<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._startParticles)
		{
			_startParticles = VariantUtils.ConvertToArray<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._endParticles)
		{
			_endParticles = VariantUtils.ConvertToArray<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._sweepingParticles)
		{
			_sweepingParticles = VariantUtils.ConvertToArray<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._sweepingIndexCurve)
		{
			_sweepingIndexCurve = VariantUtils.ConvertTo<Curve>(in value);
			return true;
		}
		if (name == PropertyName._sweepDuration)
		{
			_sweepDuration = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._targetCenterPositions)
		{
			_targetCenterPositions = VariantUtils.ConvertToArray<Vector2>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._emittingParticles)
		{
			value = VariantUtils.CreateFromArray(_emittingParticles);
			return true;
		}
		if (name == PropertyName._startParticles)
		{
			value = VariantUtils.CreateFromArray(_startParticles);
			return true;
		}
		if (name == PropertyName._endParticles)
		{
			value = VariantUtils.CreateFromArray(_endParticles);
			return true;
		}
		if (name == PropertyName._sweepingParticles)
		{
			value = VariantUtils.CreateFromArray(_sweepingParticles);
			return true;
		}
		if (name == PropertyName._sweepingIndexCurve)
		{
			value = VariantUtils.CreateFrom(in _sweepingIndexCurve);
			return true;
		}
		if (name == PropertyName._sweepDuration)
		{
			value = VariantUtils.CreateFrom(in _sweepDuration);
			return true;
		}
		if (name == PropertyName._targetCenterPositions)
		{
			value = VariantUtils.CreateFromArray(_targetCenterPositions);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._emittingParticles, PropertyHint.TypeString, "24/34:GPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._startParticles, PropertyHint.TypeString, "24/34:GPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._endParticles, PropertyHint.TypeString, "24/34:GPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._sweepingParticles, PropertyHint.TypeString, "24/34:GPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._sweepingIndexCurve, PropertyHint.ResourceType, "Curve", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._sweepDuration, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._targetCenterPositions, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._emittingParticles, Variant.CreateFrom(_emittingParticles));
		info.AddProperty(PropertyName._startParticles, Variant.CreateFrom(_startParticles));
		info.AddProperty(PropertyName._endParticles, Variant.CreateFrom(_endParticles));
		info.AddProperty(PropertyName._sweepingParticles, Variant.CreateFrom(_sweepingParticles));
		info.AddProperty(PropertyName._sweepingIndexCurve, Variant.From(in _sweepingIndexCurve));
		info.AddProperty(PropertyName._sweepDuration, Variant.From(in _sweepDuration));
		info.AddProperty(PropertyName._targetCenterPositions, Variant.CreateFrom(_targetCenterPositions));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._emittingParticles, out var value))
		{
			_emittingParticles = value.AsGodotArray<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._startParticles, out var value2))
		{
			_startParticles = value2.AsGodotArray<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._endParticles, out var value3))
		{
			_endParticles = value3.AsGodotArray<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._sweepingParticles, out var value4))
		{
			_sweepingParticles = value4.AsGodotArray<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._sweepingIndexCurve, out var value5))
		{
			_sweepingIndexCurve = value5.As<Curve>();
		}
		if (info.TryGetProperty(PropertyName._sweepDuration, out var value6))
		{
			_sweepDuration = value6.As<float>();
		}
		if (info.TryGetProperty(PropertyName._targetCenterPositions, out var value7))
		{
			_targetCenterPositions = value7.AsGodotArray<Vector2>();
		}
	}
}
