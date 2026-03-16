using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NBlockBrokenVfx.cs")]
public class NBlockBrokenVfx : Sprite2D
{
	public new class MethodName : Sprite2D.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnAnimationFinished = "OnAnimationFinished";

		public static readonly StringName Create = "Create";
	}

	public new class PropertyName : Sprite2D.PropertyName
	{
	}

	public new class SignalName : Sprite2D.SignalName
	{
	}

	private const string _scenePath = "res://scenes/vfx/vfx_block_broken.tscn";

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>("res://scenes/vfx/vfx_block_broken.tscn");

	public override void _Ready()
	{
		AnimationPlayer node = GetNode<AnimationPlayer>("AnimationPlayer");
		node.Connect(AnimationMixer.SignalName.AnimationFinished, Callable.From<StringName>(OnAnimationFinished));
	}

	private void OnAnimationFinished(StringName _)
	{
		this.QueueFreeSafely();
	}

	public static NBlockBrokenVfx? Create()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		return PreloadManager.Cache.GetScene("res://scenes/vfx/vfx_block_broken.tscn").Instantiate<NBlockBrokenVfx>(PackedScene.GenEditState.Disabled);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnAnimationFinished, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.StringName, "_", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Sprite2D"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
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
		if (method == MethodName.OnAnimationFinished && args.Count == 1)
		{
			OnAnimationFinished(VariantUtils.ConvertTo<StringName>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NBlockBrokenVfx>(Create());
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NBlockBrokenVfx>(Create());
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.OnAnimationFinished)
		{
			return true;
		}
		if (method == MethodName.Create)
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
