using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NLineBurstVfx.cs")]
public class NLineBurstVfx : GpuParticles2D
{
	public new class MethodName : GpuParticles2D.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";
	}

	public new class PropertyName : GpuParticles2D.PropertyName
	{
	}

	public new class SignalName : GpuParticles2D.SignalName
	{
	}

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	private static string ScenePath => SceneHelper.GetScenePath("vfx/vfx_line_burst");

	public static NLineBurstVfx? Create(Creature target)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NLineBurstVfx nLineBurstVfx = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NLineBurstVfx>(PackedScene.GenEditState.Disabled);
		nLineBurstVfx.GlobalPosition = NCombatRoom.Instance.GetCreatureNode(target).VfxSpawnPosition;
		return nLineBurstVfx;
	}

	public static NLineBurstVfx? Create(Vector2 position)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NLineBurstVfx nLineBurstVfx = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NLineBurstVfx>(PackedScene.GenEditState.Disabled);
		nLineBurstVfx.GlobalPosition = position;
		return nLineBurstVfx;
	}

	public override void _Ready()
	{
		base.Emitting = true;
		TaskHelper.RunSafely(DeleteAfterComplete());
	}

	private async Task DeleteAfterComplete()
	{
		await ToSignal(this, GpuParticles2D.SignalName.Finished);
		this.QueueFreeSafely();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("GPUParticles2D"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "position", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NLineBurstVfx>(Create(VariantUtils.ConvertTo<Vector2>(in args[0])));
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
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NLineBurstVfx>(Create(VariantUtils.ConvertTo<Vector2>(in args[0])));
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
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
	}
}
