using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NWormyImpactVfx.cs")]
public class NWormyImpactVfx : Node2D
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Node2D.MethodName
	{
		/// <summary>
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";

		/// <summary>
		/// Cached name for the 'Create' method.
		/// </summary>
		public static readonly StringName Create = "Create";

		/// <summary>
		/// Cached name for the 'Initialize' method.
		/// </summary>
		public static readonly StringName Initialize = "Initialize";

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
		/// Cached name for the '_centerPivot' field.
		/// </summary>
		public static readonly StringName _centerPivot = "_centerPivot";

		/// <summary>
		/// Cached name for the '_groundPivot' field.
		/// </summary>
		public static readonly StringName _groundPivot = "_groundPivot";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Node2D.SignalName
	{
	}

	public static readonly string scenePath = SceneHelper.GetScenePath("vfx/vfx_wormy_impact");

	[Export(PropertyHint.None, "")]
	private Array<GpuParticles2D> _particles = new Array<GpuParticles2D>();

	[Export(PropertyHint.None, "")]
	private Node2D? _centerPivot;

	[Export(PropertyHint.None, "")]
	private Node2D? _groundPivot;

	private CancellationTokenSource? _cts;

	public override void _ExitTree()
	{
		_cts?.Cancel();
	}

	public static NWormyImpactVfx? Create(Creature creature)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(creature);
		if (nCreature != null)
		{
			return Create(nCreature.GetBottomOfHitbox(), nCreature.VfxSpawnPosition);
		}
		return null;
	}

	public static NWormyImpactVfx? Create(Vector2 targetGroundPosition, Vector2 targetCenterPosition)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NWormyImpactVfx nWormyImpactVfx = PreloadManager.Cache.GetScene(scenePath).Instantiate<NWormyImpactVfx>(PackedScene.GenEditState.Disabled);
		nWormyImpactVfx.GlobalPosition = targetGroundPosition;
		nWormyImpactVfx.Initialize(targetGroundPosition, targetCenterPosition);
		return nWormyImpactVfx;
	}

	private void Initialize(Vector2 targetGroundPosition, Vector2 targetCenterPosition)
	{
		_groundPivot.GlobalPosition = targetGroundPosition;
		_centerPivot.GlobalPosition = targetCenterPosition;
	}

	public override void _Ready()
	{
		TaskHelper.RunSafely(PlaySequence());
	}

	private async Task PlaySequence()
	{
		_cts = new CancellationTokenSource();
		for (int i = 0; i < _particles.Count; i++)
		{
			_particles[i].Restart();
		}
		await Cmd.Wait(2f, _cts.Token);
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
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "targetGroundPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Vector2, "targetCenterPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.Initialize, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "targetGroundPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Vector2, "targetCenterPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Create && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<NWormyImpactVfx>(Create(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<Vector2>(in args[1])));
			return true;
		}
		if (method == MethodName.Initialize && args.Count == 2)
		{
			Initialize(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<Vector2>(in args[1]));
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
			ret = VariantUtils.CreateFrom<NWormyImpactVfx>(Create(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<Vector2>(in args[1])));
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.Create)
		{
			return true;
		}
		if (method == MethodName.Initialize)
		{
			return true;
		}
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
		if (name == PropertyName._centerPivot)
		{
			_centerPivot = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._groundPivot)
		{
			_groundPivot = VariantUtils.ConvertTo<Node2D>(in value);
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
		if (name == PropertyName._centerPivot)
		{
			value = VariantUtils.CreateFrom(in _centerPivot);
			return true;
		}
		if (name == PropertyName._groundPivot)
		{
			value = VariantUtils.CreateFrom(in _groundPivot);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._centerPivot, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._groundPivot, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._particles, Variant.CreateFrom(_particles));
		info.AddProperty(PropertyName._centerPivot, Variant.From(in _centerPivot));
		info.AddProperty(PropertyName._groundPivot, Variant.From(in _groundPivot));
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
		if (info.TryGetProperty(PropertyName._centerPivot, out var value2))
		{
			_centerPivot = value2.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._groundPivot, out var value3))
		{
			_groundPivot = value3.As<Node2D>();
		}
	}
}
