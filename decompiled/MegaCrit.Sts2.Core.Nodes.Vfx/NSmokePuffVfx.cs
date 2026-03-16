using System.Collections.Generic;
using System.ComponentModel;
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

[ScriptPath("res://src/Core/Nodes/Vfx/NSmokePuffVfx.cs")]
public class NSmokePuffVfx : Node2D
{
	public enum SmokePuffColor
	{
		Green,
		Purple
	}

	public new class MethodName : Node2D.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";
	}

	public new class PropertyName : Node2D.PropertyName
	{
		public static readonly StringName _clouds = "_clouds";

		public static readonly StringName _ember = "_ember";

		public static readonly StringName _color = "_color";
	}

	public new class SignalName : Node2D.SignalName
	{
	}

	private GpuParticles2D _clouds;

	private GpuParticles2D _ember;

	private SmokePuffColor _color;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	private static string ScenePath => SceneHelper.GetScenePath("vfx/vfx_smoke_puff");

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
		await Task.Delay(2500);
		this.QueueFreeSafely();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

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

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._clouds, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._ember, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._color, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._clouds, Variant.From(in _clouds));
		info.AddProperty(PropertyName._ember, Variant.From(in _ember));
		info.AddProperty(PropertyName._color, Variant.From(in _color));
	}

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
