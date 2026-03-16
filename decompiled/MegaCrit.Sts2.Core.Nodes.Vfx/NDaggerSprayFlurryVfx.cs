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

[ScriptPath("res://src/Core/Nodes/Vfx/NDaggerSprayFlurryVfx.cs")]
public class NDaggerSprayFlurryVfx : Node2D
{
	public new class MethodName : Node2D.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName ApplyTint = "ApplyTint";
	}

	public new class PropertyName : Node2D.PropertyName
	{
		public static readonly StringName _particles = "_particles";

		public static readonly StringName _modulateParticles = "_modulateParticles";
	}

	public new class SignalName : Node2D.SignalName
	{
	}

	private static readonly StringName _color = new StringName("color");

	public static readonly string scenePath = SceneHelper.GetScenePath("vfx/vfx_dagger_spray_flurry");

	[Export(PropertyHint.None, "")]
	private Array<GpuParticles2D> _particles = new Array<GpuParticles2D>();

	[Export(PropertyHint.None, "")]
	private Array<GpuParticles2D> _modulateParticles = new Array<GpuParticles2D>();

	private CancellationTokenSource? _cts;

	public static NDaggerSprayFlurryVfx? Create(Creature creature, Color tint, bool goingRight)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(creature);
		if (nCreature != null)
		{
			return Create(nCreature.VfxSpawnPosition, tint, goingRight);
		}
		return null;
	}

	public static NDaggerSprayFlurryVfx? Create(Vector2 targetCenter, Color tint, bool goingRight)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NDaggerSprayFlurryVfx nDaggerSprayFlurryVfx = PreloadManager.Cache.GetScene(scenePath).Instantiate<NDaggerSprayFlurryVfx>(PackedScene.GenEditState.Disabled);
		nDaggerSprayFlurryVfx.GlobalPosition = targetCenter;
		nDaggerSprayFlurryVfx.ApplyTint(tint);
		nDaggerSprayFlurryVfx.Scale = new Vector2(goingRight ? 1 : (-1), 1f);
		return nDaggerSprayFlurryVfx;
	}

	public override void _Ready()
	{
		TaskHelper.RunSafely(PlaySequence());
	}

	public override void _ExitTree()
	{
		_cts?.Cancel();
		_cts?.Dispose();
	}

	public void ApplyTint(Color tint)
	{
		for (int i = 0; i < _modulateParticles.Count; i++)
		{
			_modulateParticles[i].ProcessMaterial = (ParticleProcessMaterial)_modulateParticles[i].ProcessMaterial.Duplicate();
			_modulateParticles[i].ProcessMaterial.Set(_color, tint);
		}
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

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "targetCenter", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Color, "tint", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Bool, "goingRight", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ApplyTint, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Color, "tint", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 3)
		{
			ret = VariantUtils.CreateFrom<NDaggerSprayFlurryVfx>(Create(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<Color>(in args[1]), VariantUtils.ConvertTo<bool>(in args[2])));
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ApplyTint && args.Count == 1)
		{
			ApplyTint(VariantUtils.ConvertTo<Color>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 3)
		{
			ret = VariantUtils.CreateFrom<NDaggerSprayFlurryVfx>(Create(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<Color>(in args[1]), VariantUtils.ConvertTo<bool>(in args[2])));
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
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.ApplyTint)
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
		if (name == PropertyName._modulateParticles)
		{
			_modulateParticles = VariantUtils.ConvertToArray<GpuParticles2D>(in value);
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
		if (name == PropertyName._modulateParticles)
		{
			value = VariantUtils.CreateFromArray(_modulateParticles);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._particles, PropertyHint.TypeString, "24/34:GPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._modulateParticles, PropertyHint.TypeString, "24/34:GPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._particles, Variant.CreateFrom(_particles));
		info.AddProperty(PropertyName._modulateParticles, Variant.CreateFrom(_modulateParticles));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._particles, out var value))
		{
			_particles = value.AsGodotArray<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._modulateParticles, out var value2))
		{
			_modulateParticles = value2.AsGodotArray<GpuParticles2D>();
		}
	}
}
