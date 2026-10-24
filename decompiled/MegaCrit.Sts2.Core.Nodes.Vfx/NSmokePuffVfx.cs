using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

/// <summary>
/// A nice puff of smoke used by cards and potions.
/// </summary>
[ScriptPath("res://src/Core/Nodes/Vfx/NSmokePuffVfx.cs")]
public class NSmokePuffVfx : Node2D
{
	/// <summary>
	/// Special vfx-only enum as passing in any color can be problematic.
	/// Color treatments require bespoke tuning (i.e. the color of the associated embers)
	/// </summary>
	public enum SmokePuffColor
	{
		Green,
		Purple
	}

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
		/// Cached name for the '_clouds' field.
		/// </summary>
		public static readonly StringName _clouds = "_clouds";

		/// <summary>
		/// Cached name for the '_ember' field.
		/// </summary>
		public static readonly StringName _ember = "_ember";

		/// <summary>
		/// Cached name for the '_color' field.
		/// </summary>
		public static readonly StringName _color = "_color";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Node2D.SignalName
	{
	}

	private GpuParticles2D _clouds;

	private GpuParticles2D _ember;

	private SmokePuffColor _color;

	private CancellationTokenSource? _cts;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	private static string ScenePath => SceneHelper.GetScenePath("vfx/vfx_smoke_puff");

	public override void _ExitTree()
	{
		_cts?.Cancel();
	}

	public static NSmokePuffVfx? Create(Creature target, SmokePuffColor puffColor)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NCreature creatureNode = NCombatRoom.Instance.GetCreatureNode(target);
		if (creatureNode == null)
		{
			Log.Warn($"Tried to spawn {"NSmokePuffVfx"} on creature {target} without node!");
			return null;
		}
		NSmokePuffVfx nSmokePuffVfx = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NSmokePuffVfx>(PackedScene.GenEditState.Disabled);
		nSmokePuffVfx._color = puffColor;
		nSmokePuffVfx.GlobalPosition = creatureNode.VfxSpawnPosition;
		return nSmokePuffVfx;
	}

	public override void _Ready()
	{
		_ember = GetNode<GpuParticles2D>("Ember");
		_clouds = GetNode<GpuParticles2D>("Clouds");
		_ember.Emitting = true;
		_clouds.Emitting = true;
		if (_color == SmokePuffColor.Purple)
		{
			ParticleProcessMaterial particleProcessMaterial = (ParticleProcessMaterial)_clouds.ProcessMaterial;
			particleProcessMaterial.HueVariationMin = -0.02f;
			particleProcessMaterial.HueVariationMax = 0.02f;
			particleProcessMaterial.Color = new Color("F6B1FF");
		}
		TaskHelper.RunSafely(DeleteAfterComplete());
	}

	private async Task DeleteAfterComplete()
	{
		_cts = new CancellationTokenSource();
		await Task.Delay(2500, _cts.Token);
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
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName._ExitTree)
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
		if (name == PropertyName._clouds)
		{
			_clouds = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._ember)
		{
			_ember = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._color)
		{
			_color = VariantUtils.ConvertTo<SmokePuffColor>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._clouds)
		{
			value = VariantUtils.CreateFrom(in _clouds);
			return true;
		}
		if (name == PropertyName._ember)
		{
			value = VariantUtils.CreateFrom(in _ember);
			return true;
		}
		if (name == PropertyName._color)
		{
			value = VariantUtils.CreateFrom(in _color);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._clouds, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._ember, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._color, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._clouds, Variant.From(in _clouds));
		info.AddProperty(PropertyName._ember, Variant.From(in _ember));
		info.AddProperty(PropertyName._color, Variant.From(in _color));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._clouds, out var value))
		{
			_clouds = value.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._ember, out var value2))
		{
			_ember = value2.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._color, out var value3))
		{
			_color = value3.As<SmokePuffColor>();
		}
	}
}
