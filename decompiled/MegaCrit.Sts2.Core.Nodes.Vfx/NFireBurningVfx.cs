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

[ScriptPath("res://src/Core/Nodes/Vfx/NFireBurningVfx.cs")]
public class NFireBurningVfx : Node2D
{
	public new class MethodName : Node2D.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : Node2D.PropertyName
	{
		public static readonly StringName _startParticles = "_startParticles";

		public static readonly StringName _endParticles = "_endParticles";
	}

	public new class SignalName : Node2D.SignalName
	{
	}

	public static readonly string scenePath = SceneHelper.GetScenePath("vfx/vfx_fire_burning");

	[Export(PropertyHint.None, "")]
	private Array<GpuParticles2D> _startParticles = new Array<GpuParticles2D>();

	[Export(PropertyHint.None, "")]
	private Array<GpuParticles2D> _endParticles = new Array<GpuParticles2D>();

	private CancellationTokenSource? _cts;

	private static Color DefaultColor => Color.FromHtml("#ff8b57");

	public static NFireBurningVfx? Create(Creature creature, float scaleFactor, bool goingRight)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(creature);
		if (nCreature != null)
		{
			return Create(nCreature.GetBottomOfHitbox(), scaleFactor, goingRight);
		}
		return null;
	}

	public static NFireBurningVfx? Create(Vector2 targetFloorPosition, float scaleFactor, bool goingRight)
	{
		return Create(targetFloorPosition, scaleFactor, goingRight, DefaultColor);
	}

	public static NFireBurningVfx? Create(Vector2 targetFloorPosition, float scaleFactor, bool goingRight, Color tint)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NFireBurningVfx nFireBurningVfx = PreloadManager.Cache.GetScene(scenePath).Instantiate<NFireBurningVfx>(PackedScene.GenEditState.Disabled);
		nFireBurningVfx.GlobalPosition = targetFloorPosition;
		nFireBurningVfx.Modulate = tint;
		Vector2 scale = Vector2.One * scaleFactor;
		scale.X *= (goingRight ? 1f : (-1f));
		nFireBurningVfx.Scale = scale;
		return nFireBurningVfx;
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
		for (int i = 0; i < _startParticles.Count; i++)
		{
			_startParticles[i].Restart();
		}
		await Cmd.Wait(0.3f, _cts.Token);
		for (int j = 0; j < _endParticles.Count; j++)
		{
			_endParticles[j].Restart();
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
			new PropertyInfo(Variant.Type.Vector2, "targetFloorPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Float, "scaleFactor", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Bool, "goingRight", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "targetFloorPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Float, "scaleFactor", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Bool, "goingRight", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Color, "tint", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
			ret = VariantUtils.CreateFrom<NFireBurningVfx>(Create(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<float>(in args[1]), VariantUtils.ConvertTo<bool>(in args[2])));
			return true;
		}
		if (method == MethodName.Create && args.Count == 4)
		{
			ret = VariantUtils.CreateFrom<NFireBurningVfx>(Create(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<float>(in args[1]), VariantUtils.ConvertTo<bool>(in args[2]), VariantUtils.ConvertTo<Color>(in args[3])));
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
			ret = VariantUtils.CreateFrom<NFireBurningVfx>(Create(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<float>(in args[1]), VariantUtils.ConvertTo<bool>(in args[2])));
			return true;
		}
		if (method == MethodName.Create && args.Count == 4)
		{
			ret = VariantUtils.CreateFrom<NFireBurningVfx>(Create(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<float>(in args[1]), VariantUtils.ConvertTo<bool>(in args[2]), VariantUtils.ConvertTo<Color>(in args[3])));
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
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
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
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
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
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._startParticles, PropertyHint.TypeString, "24/34:GPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._endParticles, PropertyHint.TypeString, "24/34:GPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._startParticles, Variant.CreateFrom(_startParticles));
		info.AddProperty(PropertyName._endParticles, Variant.CreateFrom(_endParticles));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._startParticles, out var value))
		{
			_startParticles = value.AsGodotArray<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._endParticles, out var value2))
		{
			_endParticles = value2.AsGodotArray<GpuParticles2D>();
		}
	}
}
