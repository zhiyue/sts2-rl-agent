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
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NBlockSparkVfx.cs")]
public class NBlockSparkVfx : Node2D
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Node2D.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Node2D.PropertyName
	{
		/// <summary>
		/// Cached name for the '_particles' field.
		/// </summary>
		public static readonly StringName _particles = "_particles";

		/// <summary>
		/// Cached name for the '_specks' field.
		/// </summary>
		public static readonly StringName _specks = "_specks";

		/// <summary>
		/// Cached name for the '_creatureNode' field.
		/// </summary>
		public static readonly StringName _creatureNode = "_creatureNode";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Node2D.SignalName
	{
	}

	[Export(PropertyHint.None, "")]
	private Array<GpuParticles2D> _particles = new Array<GpuParticles2D>();

	[Export(PropertyHint.None, "")]
	private GpuParticles2D _specks;

	private NCreature _creatureNode;

	private static string ScenePath => SceneHelper.GetScenePath("vfx/block_spark_vfx");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	public static NBlockSparkVfx? Create(Creature target)
	{
		NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(target);
		if (nCreature == null || !nCreature.IsInteractable)
		{
			return null;
		}
		NBlockSparkVfx nBlockSparkVfx = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NBlockSparkVfx>(PackedScene.GenEditState.Disabled);
		nBlockSparkVfx._creatureNode = nCreature;
		return nBlockSparkVfx;
	}

	public override void _Ready()
	{
		base.GlobalPosition = _creatureNode.VfxSpawnPosition;
		for (int i = 0; i < _particles.Count; i++)
		{
			_particles[i].Restart();
		}
		TaskHelper.RunSafely(FlashAndFree());
	}

	private async Task FlashAndFree()
	{
		await _specks.AwaitSignal(GpuParticles2D.SignalName.Finished, this);
		this.QueueFreeSafely();
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
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

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
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

	/// <inheritdoc />
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

	/// <summary>
	/// Get the property information for all the properties declared in this class.
	/// This method is used by Godot to register the available properties in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._particles, PropertyHint.TypeString, "24/34:GPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._specks, PropertyHint.NodeType, "GPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._creatureNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._particles, Variant.CreateFrom(_particles));
		info.AddProperty(PropertyName._specks, Variant.From(in _specks));
		info.AddProperty(PropertyName._creatureNode, Variant.From(in _creatureNode));
	}

	/// <inheritdoc />
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
