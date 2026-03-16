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
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NShivThrowVfx.cs")]
public class NShivThrowVfx : Node2D
{
	public new class MethodName : Node2D.MethodName
	{
		public static readonly StringName Create = "Create";

		public static readonly StringName ApplyTint = "ApplyTint";

		public static readonly StringName ApplyRotation = "ApplyRotation";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : Node2D.PropertyName
	{
		public static readonly StringName _throwParticles = "_throwParticles";

		public static readonly StringName _impactParticles = "_impactParticles";

		public static readonly StringName _modulateParticles = "_modulateParticles";
	}

	public new class SignalName : Node2D.SignalName
	{
	}

	private static readonly StringName _color = new StringName("color");

	public static readonly string scenePath = SceneHelper.GetScenePath("vfx/vfx_shiv_throw");

	[Export(PropertyHint.None, "")]
	private Array<GpuParticles2D> _throwParticles = new Array<GpuParticles2D>();

	[Export(PropertyHint.None, "")]
	private Array<GpuParticles2D> _impactParticles = new Array<GpuParticles2D>();

	[Export(PropertyHint.None, "")]
	private Array<GpuParticles2D> _modulateParticles = new Array<GpuParticles2D>();

	private CancellationTokenSource? _cts;

	public static NShivThrowVfx? Create(Creature owner, Creature? target, Color tint)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(owner);
		if (nCreature == null)
		{
			return null;
		}
		NCreature nCreature2 = NCombatRoom.Instance?.GetCreatureNode(target);
		if (nCreature2 == null)
		{
			return null;
		}
		return Create(nCreature.VfxSpawnPosition, nCreature2.VfxSpawnPosition, tint);
	}

	public static NShivThrowVfx? Create(Vector2 throwerCenterPosition, Vector2 targetCenterPosition, Color tint)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NShivThrowVfx nShivThrowVfx = PreloadManager.Cache.GetScene(scenePath).Instantiate<NShivThrowVfx>(PackedScene.GenEditState.Disabled);
		nShivThrowVfx.GlobalPosition = targetCenterPosition;
		nShivThrowVfx.ApplyRotation(throwerCenterPosition, targetCenterPosition);
		nShivThrowVfx.ApplyTint(tint);
		return nShivThrowVfx;
	}

	public void ApplyTint(Color tint)
	{
		for (int i = 0; i < _modulateParticles.Count; i++)
		{
			_modulateParticles[i].ProcessMaterial = (ParticleProcessMaterial)_modulateParticles[i].ProcessMaterial.Duplicate();
			_modulateParticles[i].ProcessMaterial.Set(_color, tint);
		}
	}

	public void ApplyRotation(Vector2 throwerPosition, Vector2 targetPosition)
	{
		Vector2 vector = targetPosition - throwerPosition;
		float rotationDegrees = Mathf.RadToDeg(Mathf.Atan2(vector.Y, vector.X));
		base.RotationDegrees = rotationDegrees;
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

	private async Task PlaySequence()
	{
		_cts = new CancellationTokenSource();
		for (int i = 0; i < _throwParticles.Count; i++)
		{
			_throwParticles[i].Restart();
		}
		await Cmd.Wait(0.15f, _cts.Token);
		for (int j = 0; j < _impactParticles.Count; j++)
		{
			_impactParticles[j].Restart();
		}
		NGame.Instance?.ScreenShake(ShakeStrength.Weak, ShakeDuration.Short);
		await Cmd.Wait(2f, _cts.Token);
		this.QueueFreeSafely();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "throwerCenterPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Vector2, "targetCenterPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Color, "tint", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ApplyTint, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Color, "tint", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ApplyRotation, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "throwerPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Vector2, "targetPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 3)
		{
			ret = VariantUtils.CreateFrom<NShivThrowVfx>(Create(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<Vector2>(in args[1]), VariantUtils.ConvertTo<Color>(in args[2])));
			return true;
		}
		if (method == MethodName.ApplyTint && args.Count == 1)
		{
			ApplyTint(VariantUtils.ConvertTo<Color>(in args[0]));
			ret = default(godot_variant);
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
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
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
			ret = VariantUtils.CreateFrom<NShivThrowVfx>(Create(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<Vector2>(in args[1]), VariantUtils.ConvertTo<Color>(in args[2])));
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
		if (method == MethodName.ApplyTint)
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
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._throwParticles)
		{
			_throwParticles = VariantUtils.ConvertToArray<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._impactParticles)
		{
			_impactParticles = VariantUtils.ConvertToArray<GpuParticles2D>(in value);
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
		if (name == PropertyName._throwParticles)
		{
			value = VariantUtils.CreateFromArray(_throwParticles);
			return true;
		}
		if (name == PropertyName._impactParticles)
		{
			value = VariantUtils.CreateFromArray(_impactParticles);
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
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._throwParticles, PropertyHint.TypeString, "24/34:GPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._impactParticles, PropertyHint.TypeString, "24/34:GPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._modulateParticles, PropertyHint.TypeString, "24/34:GPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._throwParticles, Variant.CreateFrom(_throwParticles));
		info.AddProperty(PropertyName._impactParticles, Variant.CreateFrom(_impactParticles));
		info.AddProperty(PropertyName._modulateParticles, Variant.CreateFrom(_modulateParticles));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._throwParticles, out var value))
		{
			_throwParticles = value.AsGodotArray<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._impactParticles, out var value2))
		{
			_impactParticles = value2.AsGodotArray<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._modulateParticles, out var value3))
		{
			_modulateParticles = value3.AsGodotArray<GpuParticles2D>();
		}
	}
}
