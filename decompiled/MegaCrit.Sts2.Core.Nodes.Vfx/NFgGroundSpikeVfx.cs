using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NFgGroundSpikeVfx.cs")]
public class NFgGroundSpikeVfx : NBgGroundSpikeVfx
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : NBgGroundSpikeVfx.MethodName
	{
		/// <summary>
		/// Cached name for the 'Create' method.
		/// </summary>
		public new static readonly StringName Create = "Create";

		/// <summary>
		/// Cached name for the 'AdjustStartPosition' method.
		/// </summary>
		public new static readonly StringName AdjustStartPosition = "AdjustStartPosition";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : NBgGroundSpikeVfx.PropertyName
	{
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NBgGroundSpikeVfx.SignalName
	{
	}

	private const string _scenePath = "res://scenes/vfx/fg_ground_spike_vfx.tscn";

	public new static NFgGroundSpikeVfx? Create(Vector2 position, bool movingRight = true, VfxColor vfxColor = VfxColor.Red)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NFgGroundSpikeVfx nFgGroundSpikeVfx = PreloadManager.Cache.GetScene("res://scenes/vfx/fg_ground_spike_vfx.tscn").Instantiate<NFgGroundSpikeVfx>(PackedScene.GenEditState.Disabled);
		nFgGroundSpikeVfx._startPosition = position;
		nFgGroundSpikeVfx._movingRight = movingRight;
		nFgGroundSpikeVfx._vfxColor = vfxColor;
		return nFgGroundSpikeVfx;
	}

	protected override void AdjustStartPosition()
	{
		_startPosition += new Vector2(_movingRight ? Rng.Chaotic.NextFloat(40f, 160f) : Rng.Chaotic.NextFloat(-160f, -40f), Rng.Chaotic.NextFloat(10f, 32f));
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Sprite2D"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "position", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Bool, "movingRight", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "vfxColor", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AdjustStartPosition, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 3)
		{
			ret = VariantUtils.CreateFrom<NFgGroundSpikeVfx>(Create(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1]), VariantUtils.ConvertTo<VfxColor>(in args[2])));
			return true;
		}
		if (method == MethodName.AdjustStartPosition && args.Count == 0)
		{
			AdjustStartPosition();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 3)
		{
			ret = VariantUtils.CreateFrom<NFgGroundSpikeVfx>(Create(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1]), VariantUtils.ConvertTo<VfxColor>(in args[2])));
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.Create)
		{
			return true;
		}
		if (method == MethodName.AdjustStartPosition)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
	}
}
