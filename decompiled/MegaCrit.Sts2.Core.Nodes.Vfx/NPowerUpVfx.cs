using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NPowerUpVfx.cs")]
public class NPowerUpVfx : Node2D
{
	public new class MethodName : Node2D.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _Process = "_Process";
	}

	public new class PropertyName : Node2D.PropertyName
	{
		public static readonly StringName _timer = "_timer";

		public static readonly StringName _creatureVisuals = "_creatureVisuals";

		public static readonly StringName _backVfx = "_backVfx";
	}

	public new class SignalName : Node2D.SignalName
	{
	}

	private float _timer;

	private const float _vfxDuration = 1f;

	private Control _creatureVisuals;

	private Sprite2D _backVfx;

	private static string NormalScenePath => SceneHelper.GetScenePath("/vfx/vfx_power_up/vfx_power_up");

	private static string GhostlyScenePath => SceneHelper.GetScenePath("/vfx/vfx_ghostly_power_up/vfx_ghostly_power_up");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { NormalScenePath, GhostlyScenePath });

	public static NPowerUpVfx? CreateNormal(Creature target)
	{
		return CreatePowerUpVfx(target, NormalScenePath);
	}

	public static NPowerUpVfx? CreateGhostly(Creature target)
	{
		return CreatePowerUpVfx(target, GhostlyScenePath);
	}

	private static NPowerUpVfx? CreatePowerUpVfx(Creature target, string scenePath)
	{
		NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(target);
		if (nCreature == null || !nCreature.IsInteractable)
		{
			return null;
		}
		NPowerUpVfx nPowerUpVfx = PreloadManager.Cache.GetScene(scenePath).Instantiate<NPowerUpVfx>(PackedScene.GenEditState.Disabled);
		nPowerUpVfx.GlobalPosition = nCreature.VfxSpawnPosition;
		NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(nPowerUpVfx);
		return nPowerUpVfx;
	}

	public override void _Ready()
	{
		_timer = 1f;
		_backVfx = GetNode<Sprite2D>("%BackVfx");
		Vector2 globalPosition = _backVfx.GlobalPosition;
		_backVfx.Reparent(NCombatRoom.Instance.BackCombatVfxContainer);
		_backVfx.GlobalPosition = globalPosition;
	}

	public override void _Process(double delta)
	{
		_timer -= (float)delta;
		float a = 1f;
		if (Mathf.Abs(_timer / 1f - 0.5f) > 0.4f)
		{
			a = Mathf.Max(0f, 1f - (Mathf.Abs(_timer / 1f - 0.5f) - 0.4f) / 0.1f);
		}
		base.Modulate = new Color(1f, 1f, 1f, a);
		_backVfx.Modulate = new Color(1f, 1f, 1f, a);
		if (_timer < 0f)
		{
			this.QueueFreeSafely();
			_backVfx.QueueFreeSafely();
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
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
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
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
		if (method == MethodName._Process)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._timer)
		{
			_timer = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._creatureVisuals)
		{
			_creatureVisuals = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._backVfx)
		{
			_backVfx = VariantUtils.ConvertTo<Sprite2D>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._timer)
		{
			value = VariantUtils.CreateFrom(in _timer);
			return true;
		}
		if (name == PropertyName._creatureVisuals)
		{
			value = VariantUtils.CreateFrom(in _creatureVisuals);
			return true;
		}
		if (name == PropertyName._backVfx)
		{
			value = VariantUtils.CreateFrom(in _backVfx);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._timer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._creatureVisuals, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._backVfx, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._timer, Variant.From(in _timer));
		info.AddProperty(PropertyName._creatureVisuals, Variant.From(in _creatureVisuals));
		info.AddProperty(PropertyName._backVfx, Variant.From(in _backVfx));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._timer, out var value))
		{
			_timer = value.As<float>();
		}
		if (info.TryGetProperty(PropertyName._creatureVisuals, out var value2))
		{
			_creatureVisuals = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._backVfx, out var value3))
		{
			_backVfx = value3.As<Sprite2D>();
		}
	}
}
