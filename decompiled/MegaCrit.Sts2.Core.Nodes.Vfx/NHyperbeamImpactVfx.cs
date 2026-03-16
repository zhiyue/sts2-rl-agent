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
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NHyperbeamImpactVfx.cs")]
public class NHyperbeamImpactVfx : Node2D
{
	public new class MethodName : Node2D.MethodName
	{
		public static readonly StringName Create = "Create";

		public static readonly StringName ApplyRotation = "ApplyRotation";

		public new static readonly StringName _Ready = "_Ready";
	}

	public new class PropertyName : Node2D.PropertyName
	{
		public static readonly StringName _impactStartParticles = "_impactStartParticles";

		public static readonly StringName _impactEndParticles = "_impactEndParticles";
	}

	public new class SignalName : Node2D.SignalName
	{
	}

	public static readonly string scenePath = SceneHelper.GetScenePath("vfx/vfx_hyperbeam_impact");

	[Export(PropertyHint.None, "")]
	private Array<GpuParticles2D> _impactStartParticles = new Array<GpuParticles2D>();

	[Export(PropertyHint.None, "")]
	private Array<GpuParticles2D> _impactEndParticles = new Array<GpuParticles2D>();

	public static NHyperbeamImpactVfx? Create(Creature owner, Creature target)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(owner);
		NCreature nCreature2 = NCombatRoom.Instance?.GetCreatureNode(target);
		if (nCreature2 != null && nCreature != null)
		{
			Vector2 vfxSpawnPosition = nCreature.VfxSpawnPosition;
			Player player = owner.Player;
			if (player != null && player.Character is Defect defect)
			{
				vfxSpawnPosition += defect.EyelineOffset;
			}
			return Create(vfxSpawnPosition, nCreature2.VfxSpawnPosition);
		}
		return null;
	}

	public static NHyperbeamImpactVfx? Create(Vector2 hyperbeamSourcePosition, Vector2 targetCenterPosition)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NHyperbeamImpactVfx nHyperbeamImpactVfx = PreloadManager.Cache.GetScene(scenePath).Instantiate<NHyperbeamImpactVfx>(PackedScene.GenEditState.Disabled);
		nHyperbeamImpactVfx.GlobalPosition = targetCenterPosition;
		nHyperbeamImpactVfx.ApplyRotation(hyperbeamSourcePosition, targetCenterPosition);
		return nHyperbeamImpactVfx;
	}

	public void ApplyRotation(Vector2 sourcePosition, Vector2 targetPosition)
	{
		Vector2 vector = targetPosition - sourcePosition;
		float rotationDegrees = Mathf.RadToDeg(Mathf.Atan2(vector.Y, vector.X));
		base.RotationDegrees = rotationDegrees;
	}

	public override void _Ready()
	{
		TaskHelper.RunSafely(PlaySequence());
	}

	private async Task PlaySequence()
	{
		for (int i = 0; i < _impactStartParticles.Count; i++)
		{
			_impactStartParticles[i].Visible = true;
			_impactStartParticles[i].Restart();
		}
		await Cmd.Wait(NHyperbeamVfx.hyperbeamLaserDuration);
		for (int j = 0; j < _impactStartParticles.Count; j++)
		{
			_impactStartParticles[j].Visible = false;
		}
		for (int k = 0; k < _impactEndParticles.Count; k++)
		{
			_impactEndParticles[k].Restart();
		}
		await Cmd.Wait(2f);
		this.QueueFreeSafely();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "hyperbeamSourcePosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Vector2, "targetCenterPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ApplyRotation, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "sourcePosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Vector2, "targetPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<NHyperbeamImpactVfx>(Create(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<Vector2>(in args[1])));
			return true;
		}
		if (method == MethodName.ApplyRotation && args.Count == 2)
		{
			ApplyRotation(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<Vector2>(in args[1]));
			ret = default(godot_variant);
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
			ret = VariantUtils.CreateFrom<NHyperbeamImpactVfx>(Create(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<Vector2>(in args[1])));
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
		if (method == MethodName.ApplyRotation)
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
		if (name == PropertyName._impactStartParticles)
		{
			_impactStartParticles = VariantUtils.ConvertToArray<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._impactEndParticles)
		{
			_impactEndParticles = VariantUtils.ConvertToArray<GpuParticles2D>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._impactStartParticles)
		{
			value = VariantUtils.CreateFromArray(_impactStartParticles);
			return true;
		}
		if (name == PropertyName._impactEndParticles)
		{
			value = VariantUtils.CreateFromArray(_impactEndParticles);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._impactStartParticles, PropertyHint.TypeString, "24/34:GPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._impactEndParticles, PropertyHint.TypeString, "24/34:GPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._impactStartParticles, Variant.CreateFrom(_impactStartParticles));
		info.AddProperty(PropertyName._impactEndParticles, Variant.CreateFrom(_impactEndParticles));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._impactStartParticles, out var value))
		{
			_impactStartParticles = value.AsGodotArray<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._impactEndParticles, out var value2))
		{
			_impactEndParticles = value2.AsGodotArray<GpuParticles2D>();
		}
	}
}
