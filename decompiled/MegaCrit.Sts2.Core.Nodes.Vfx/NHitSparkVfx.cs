using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NHitSparkVfx.cs")]
public class NHitSparkVfx : Node2D
{
	public new class MethodName : Node2D.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";
	}

	public new class PropertyName : Node2D.PropertyName
	{
		public static readonly StringName _particles = "_particles";

		public static readonly StringName _specks = "_specks";

		public static readonly StringName _creatureNode = "_creatureNode";
	}

	public new class SignalName : Node2D.SignalName
	{
	}

	[Export(PropertyHint.None, "")]
	private Array<GpuParticles2D> _particles = new Array<GpuParticles2D>();

	[Export(PropertyHint.None, "")]
	private GpuParticles2D _specks;

	private NCreature _creatureNode;

	private static string ScenePath => SceneHelper.GetScenePath("vfx/hit_spark_vfx");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	public static NHitSparkVfx? Create(Creature target, bool requireInteractable = true)
	{
		NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(target);
		if (nCreature == null)
		{
			return null;
		}
		if (requireInteractable && !nCreature.IsInteractable)
		{
			return null;
		}
		NHitSparkVfx nHitSparkVfx = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NHitSparkVfx>(PackedScene.GenEditState.Disabled);
		nHitSparkVfx._creatureNode = nCreature;
		return nHitSparkVfx;
	}

	public override void _Ready()
	{
		base.GlobalPosition = _creatureNode.VfxSpawnPosition;
		foreach (GpuParticles2D particle in _particles)
		{
			particle.Restart();
		}
		TaskHelper.RunSafely(FlashAndFree());
	}

	private async Task FlashAndFree()
	{
		await ToSignal(_specks, GpuParticles2D.SignalName.Finished);
		this.QueueFreeSafely();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._particles)
		{
			_particles = VariantUtils.ConvertToArray<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._specks)
		{
			_specks = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._creatureNode)
		{
			_creatureNode = VariantUtils.ConvertTo<NCreature>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._particles)
		{
			value = VariantUtils.CreateFromArray(_particles);
			return true;
		}
		if (name == PropertyName._specks)
		{
			value = VariantUtils.CreateFrom(in _specks);
			return true;
		}
		if (name == PropertyName._creatureNode)
		{
			value = VariantUtils.CreateFrom(in _creatureNode);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._particles, PropertyHint.TypeString, "24/34:GPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._specks, PropertyHint.NodeType, "GPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._creatureNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._particles, Variant.CreateFrom(_particles));
		info.AddProperty(PropertyName._specks, Variant.From(in _specks));
		info.AddProperty(PropertyName._creatureNode, Variant.From(in _creatureNode));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._particles, out var value))
		{
			_particles = value.AsGodotArray<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._specks, out var value2))
		{
			_specks = value2.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._creatureNode, out var value3))
		{
			_creatureNode = value3.As<NCreature>();
		}
	}
}
