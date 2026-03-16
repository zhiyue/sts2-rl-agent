using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NDamageNumVfx.cs")]
public class NDamageNumVfx : Node2D
{
	public new class MethodName : Node2D.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _Process = "_Process";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : Node2D.PropertyName
	{
		public static readonly StringName _globalSpawnPosition = "_globalSpawnPosition";

		public static readonly StringName _text = "_text";

		public static readonly StringName _tween = "_tween";

		public static readonly StringName _velocity = "_velocity";
	}

	public new class SignalName : Node2D.SignalName
	{
	}

	private Vector2 _globalSpawnPosition;

	private string _text;

	private Tween? _tween;

	private Vector2 _velocity;

	private static readonly Vector2 _gravity = new Vector2(0f, 2000f);

	private static readonly Vector2 _positionOffset = new Vector2(0f, -100f);

	private static readonly string _scenePath = SceneHelper.GetScenePath("vfx/vfx_damage_num");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(_scenePath);

	public static NDamageNumVfx? Create(Creature target, DamageResult result)
	{
		int num = result.UnblockedDamage;
		if (!(target.Monster is Osty))
		{
			num += result.OverkillDamage;
		}
		return Create(target, num);
	}

	public static NDamageNumVfx? Create(Creature target, int damage, bool requireInteractable = true)
	{
		NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(target);
		Vector2 globalPosition = Vector2.Zero;
		if (requireInteractable && (nCreature == null || !nCreature.IsInteractable))
		{
			if (!LocalContext.IsMe(target))
			{
				return null;
			}
			Vector2 size = ((SceneTree)Engine.GetMainLoop()).Root.GetViewport().GetVisibleRect().Size;
			globalPosition = size * new Vector2(0.25f, 0.5f);
		}
		else if (nCreature != null)
		{
			globalPosition = nCreature.VfxSpawnPosition + _positionOffset + new Vector2(Rng.Chaotic.NextFloat(-10f, 10f), Rng.Chaotic.NextFloat(-5f, 5f));
		}
		return Create(globalPosition, damage);
	}

	public static NDamageNumVfx? Create(Vector2 globalPosition, int damage)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NDamageNumVfx nDamageNumVfx = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NDamageNumVfx>(PackedScene.GenEditState.Disabled);
		nDamageNumVfx._globalSpawnPosition = globalPosition;
		nDamageNumVfx._text = damage.ToString();
		return nDamageNumVfx;
	}

	public override void _Ready()
	{
		MegaLabel node = GetNode<MegaLabel>("Label");
		node.SetTextAutoSize(_text);
		base.GlobalPosition = _globalSpawnPosition;
		_velocity = new Vector2(Rng.Chaotic.NextFloat(-100f, 100f), Rng.Chaotic.NextFloat(-800f, -700f));
		node.Scale = Vector2.One * Rng.Chaotic.NextFloat(1.2f, 1.3f);
		base.RotationDegrees = Rng.Chaotic.NextFloat(-5f, 5f);
		TaskHelper.RunSafely(AnimVfx());
	}

	private async Task AnimVfx()
	{
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(this, "modulate", StsColors.cream, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		_tween.TweenProperty(this, "modulate:a", 0f, 2.0).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quad);
		_tween.TweenProperty(this, "scale", Vector2.One, 1.2000000476837158).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad)
			.From(Vector2.One * 2.5f);
		await _tween.ToSignal(_tween, Tween.SignalName.Finished);
		this.QueueFreeSafely();
	}

	public override void _Process(double delta)
	{
		float num = (float)delta;
		base.Position += _velocity * num;
		_velocity += _gravity * num;
	}

	public override void _ExitTree()
	{
		_tween?.Kill();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "globalPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "damage", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<NDamageNumVfx>(Create(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<int>(in args[1])));
			return true;
		}
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
		if (method == MethodName.Create && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<NDamageNumVfx>(Create(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<int>(in args[1])));
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
		if (method == MethodName._Process)
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
		if (name == PropertyName._globalSpawnPosition)
		{
			_globalSpawnPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._text)
		{
			_text = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._velocity)
		{
			_velocity = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._globalSpawnPosition)
		{
			value = VariantUtils.CreateFrom(in _globalSpawnPosition);
			return true;
		}
		if (name == PropertyName._text)
		{
			value = VariantUtils.CreateFrom(in _text);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._velocity)
		{
			value = VariantUtils.CreateFrom(in _velocity);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._globalSpawnPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName._text, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._velocity, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._globalSpawnPosition, Variant.From(in _globalSpawnPosition));
		info.AddProperty(PropertyName._text, Variant.From(in _text));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._velocity, Variant.From(in _velocity));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._globalSpawnPosition, out var value))
		{
			_globalSpawnPosition = value.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._text, out var value2))
		{
			_text = value2.As<string>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value3))
		{
			_tween = value3.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._velocity, out var value4))
		{
			_velocity = value4.As<Vector2>();
		}
	}
}
