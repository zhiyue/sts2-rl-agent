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

[ScriptPath("res://src/Core/Nodes/Vfx/NBigSlashImpactVfx.cs")]
public class NBigSlashImpactVfx : Node2D
{
	public new class MethodName : Node2D.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName RotateCore = "RotateCore";

		public static readonly StringName ModulateParticles = "ModulateParticles";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : Node2D.PropertyName
	{
		public static readonly StringName _anticipationParticles = "_anticipationParticles";

		public static readonly StringName _impactParticles = "_impactParticles";

		public static readonly StringName _modulateParticles = "_modulateParticles";

		public static readonly StringName _corePivot = "_corePivot";
	}

	public new class SignalName : Node2D.SignalName
	{
	}

	public static readonly string scenePath = SceneHelper.GetScenePath("vfx/vfx_big_slash_impact");

	[Export(PropertyHint.None, "")]
	private Array<GpuParticles2D> _anticipationParticles = new Array<GpuParticles2D>();

	[Export(PropertyHint.None, "")]
	private Array<GpuParticles2D> _impactParticles = new Array<GpuParticles2D>();

	[Export(PropertyHint.None, "")]
	private Array<GpuParticles2D> _modulateParticles = new Array<GpuParticles2D>();

	[Export(PropertyHint.None, "")]
	private Node2D? _corePivot;

	private CancellationTokenSource? _cts;

	public static NBigSlashImpactVfx? Create(Creature creature)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(creature);
		if (nCreature != null)
		{
			return Create(nCreature.VfxSpawnPosition);
		}
		return null;
	}

	public static NBigSlashImpactVfx? Create(Vector2 targetCenterPosition)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		return Create(targetCenterPosition, 60f, Color.FromHtml("#80dbff"));
	}

	public static NBigSlashImpactVfx? Create(Vector2 targetCenterPosition, float rotationDegrees, Color tint)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NBigSlashImpactVfx nBigSlashImpactVfx = PreloadManager.Cache.GetScene(scenePath).Instantiate<NBigSlashImpactVfx>(PackedScene.GenEditState.Disabled);
		nBigSlashImpactVfx.GlobalPosition = targetCenterPosition;
		nBigSlashImpactVfx.RotateCore(rotationDegrees);
		nBigSlashImpactVfx.ModulateParticles(tint);
		return nBigSlashImpactVfx;
	}

	public override void _Ready()
	{
		TaskHelper.RunSafely(PlaySequence());
	}

	private void RotateCore(float rotationDegrees)
	{
		if (_corePivot != null)
		{
			_corePivot.RotationDegrees = rotationDegrees;
		}
	}

	private void ModulateParticles(Color tint)
	{
		for (int i = 0; i < _modulateParticles.Count; i++)
		{
			_modulateParticles[i].SelfModulate = tint;
		}
	}

	public override void _ExitTree()
	{
		_cts?.Cancel();
		_cts?.Dispose();
	}

	private async Task PlaySequence()
	{
		_cts = new CancellationTokenSource();
		for (int i = 0; i < _anticipationParticles.Count; i++)
		{
			_anticipationParticles[i].Restart();
		}
		await Cmd.Wait(0.1f, _cts.Token);
		NGame.Instance?.ScreenShake(ShakeStrength.Strong, ShakeDuration.Short);
		for (int j = 0; j < _impactParticles.Count; j++)
		{
			_impactParticles[j].Restart();
		}
		await Cmd.Wait(2f, _cts.Token);
		this.QueueFreeSafely();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(6);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "targetCenterPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "targetCenterPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Float, "rotationDegrees", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Color, "tint", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RotateCore, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "rotationDegrees", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ModulateParticles, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Color, "tint", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NBigSlashImpactVfx>(Create(VariantUtils.ConvertTo<Vector2>(in args[0])));
			return true;
		}
		if (method == MethodName.Create && args.Count == 3)
		{
			ret = VariantUtils.CreateFrom<NBigSlashImpactVfx>(Create(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<float>(in args[1]), VariantUtils.ConvertTo<Color>(in args[2])));
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RotateCore && args.Count == 1)
		{
			RotateCore(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ModulateParticles && args.Count == 1)
		{
			ModulateParticles(VariantUtils.ConvertTo<Color>(in args[0]));
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
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NBigSlashImpactVfx>(Create(VariantUtils.ConvertTo<Vector2>(in args[0])));
			return true;
		}
		if (method == MethodName.Create && args.Count == 3)
		{
			ret = VariantUtils.CreateFrom<NBigSlashImpactVfx>(Create(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<float>(in args[1]), VariantUtils.ConvertTo<Color>(in args[2])));
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
		if (method == MethodName.RotateCore)
		{
			return true;
		}
		if (method == MethodName.ModulateParticles)
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
		if (name == PropertyName._anticipationParticles)
		{
			_anticipationParticles = VariantUtils.ConvertToArray<GpuParticles2D>(in value);
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
		if (name == PropertyName._corePivot)
		{
			_corePivot = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._anticipationParticles)
		{
			value = VariantUtils.CreateFromArray(_anticipationParticles);
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
		if (name == PropertyName._corePivot)
		{
			value = VariantUtils.CreateFrom(in _corePivot);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._anticipationParticles, PropertyHint.TypeString, "24/34:GPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._impactParticles, PropertyHint.TypeString, "24/34:GPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._modulateParticles, PropertyHint.TypeString, "24/34:GPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._corePivot, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._anticipationParticles, Variant.CreateFrom(_anticipationParticles));
		info.AddProperty(PropertyName._impactParticles, Variant.CreateFrom(_impactParticles));
		info.AddProperty(PropertyName._modulateParticles, Variant.CreateFrom(_modulateParticles));
		info.AddProperty(PropertyName._corePivot, Variant.From(in _corePivot));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._anticipationParticles, out var value))
		{
			_anticipationParticles = value.AsGodotArray<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._impactParticles, out var value2))
		{
			_impactParticles = value2.AsGodotArray<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._modulateParticles, out var value3))
		{
			_modulateParticles = value3.AsGodotArray<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._corePivot, out var value4))
		{
			_corePivot = value4.As<Node2D>();
		}
	}
}
