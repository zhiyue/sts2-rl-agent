using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NStabVfx.cs")]
public class NStabVfx : Node2D
{
	public new class MethodName : Node2D.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName SetColor = "SetColor";

		public static readonly StringName GenerateSpawnPosition = "GenerateSpawnPosition";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : Node2D.PropertyName
	{
		public static readonly StringName _primaryVfx = "_primaryVfx";

		public static readonly StringName _secondaryVfx = "_secondaryVfx";

		public static readonly StringName _creatureCenter = "_creatureCenter";

		public static readonly StringName _vfxColor = "_vfxColor";

		public static readonly StringName _facingEnemies = "_facingEnemies";

		public static readonly StringName _tween = "_tween";
	}

	public new class SignalName : Node2D.SignalName
	{
	}

	private const string _scenePath = "res://scenes/vfx/stab_vfx.tscn";

	private Node2D _primaryVfx;

	private Node2D _secondaryVfx;

	private Vector2 _creatureCenter;

	private VfxColor _vfxColor;

	private bool _facingEnemies;

	private Tween? _tween;

	public static NStabVfx? Create(Creature? target, bool facingEnemies = false, VfxColor vfxColor = VfxColor.Red)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NCreature creatureNode = NCombatRoom.Instance.GetCreatureNode(target);
		if (creatureNode == null)
		{
			return null;
		}
		Vector2 vfxSpawnPosition = creatureNode.VfxSpawnPosition;
		NStabVfx nStabVfx = PreloadManager.Cache.GetScene("res://scenes/vfx/stab_vfx.tscn").Instantiate<NStabVfx>(PackedScene.GenEditState.Disabled);
		nStabVfx._vfxColor = vfxColor;
		nStabVfx._facingEnemies = facingEnemies;
		Vector2 vector = new Vector2(facingEnemies ? Rng.Chaotic.NextFloat(0f, 48f) : Rng.Chaotic.NextFloat(-48f, 0f), Rng.Chaotic.NextFloat(-50f, 50f));
		nStabVfx._creatureCenter = vfxSpawnPosition + vector;
		return nStabVfx;
	}

	public override void _Ready()
	{
		_primaryVfx = GetNode<Node2D>("Primary");
		_secondaryVfx = GetNode<Node2D>("%Secondary");
		_primaryVfx.GlobalPosition = GenerateSpawnPosition();
		_primaryVfx.Rotation = MathHelper.GetAngle(_primaryVfx.GlobalPosition - _creatureCenter) + (float)Math.PI / 2f;
		SetColor();
		TaskHelper.RunSafely(Animate());
	}

	private void SetColor()
	{
		switch (_vfxColor)
		{
		case VfxColor.Green:
			_primaryVfx.SelfModulate = new Color("00A52F");
			_secondaryVfx.SelfModulate = new Color("FFCB2D");
			break;
		case VfxColor.Blue:
			_primaryVfx.SelfModulate = new Color("007BDD");
			_secondaryVfx.SelfModulate = new Color("00EFF6");
			break;
		case VfxColor.Purple:
			_primaryVfx.SelfModulate = new Color("A803FF");
			_secondaryVfx.SelfModulate = new Color("00EFF3");
			break;
		case VfxColor.White:
			_primaryVfx.SelfModulate = new Color("808080");
			_secondaryVfx.SelfModulate = new Color("FFFFFF");
			break;
		case VfxColor.Cyan:
			_primaryVfx.SelfModulate = new Color("009599");
			_secondaryVfx.SelfModulate = new Color("5CDCFF");
			break;
		case VfxColor.Gold:
			_primaryVfx.SelfModulate = new Color("EBA800");
			_secondaryVfx.SelfModulate = new Color("FFE39C");
			break;
		default:
			_primaryVfx.SelfModulate = new Color("FF0000");
			_secondaryVfx.SelfModulate = new Color("FFCB2D");
			break;
		case VfxColor.Black:
			break;
		}
	}

	private Vector2 GenerateSpawnPosition()
	{
		Vector2 vector = new Vector2(Rng.Chaotic.NextFloat(-12f, 12f), Rng.Chaotic.NextFloat(-64f, 64f));
		Vector2 vector2 = new Vector2(_facingEnemies ? (-200f) : 200f, 0f);
		return _creatureCenter + vector + vector2;
	}

	public override void _ExitTree()
	{
		_tween?.Kill();
	}

	private async Task Animate()
	{
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(this, "modulate:a", 1f, 0.25);
		_tween.TweenProperty(_primaryVfx, "position", _creatureCenter, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Elastic);
		_tween.TweenProperty(this, "modulate:a", 0f, 0.25).SetDelay(0.25);
		await ToSignal(_tween, Tween.SignalName.Finished);
		this.QueueFreeSafely();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetColor, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GenerateSpawnPosition, new PropertyInfo(Variant.Type.Vector2, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.SetColor && args.Count == 0)
		{
			SetColor();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GenerateSpawnPosition && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Vector2>(GenerateSpawnPosition());
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
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.SetColor)
		{
			return true;
		}
		if (method == MethodName.GenerateSpawnPosition)
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
		if (name == PropertyName._primaryVfx)
		{
			_primaryVfx = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._secondaryVfx)
		{
			_secondaryVfx = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._creatureCenter)
		{
			_creatureCenter = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._vfxColor)
		{
			_vfxColor = VariantUtils.ConvertTo<VfxColor>(in value);
			return true;
		}
		if (name == PropertyName._facingEnemies)
		{
			_facingEnemies = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._primaryVfx)
		{
			value = VariantUtils.CreateFrom(in _primaryVfx);
			return true;
		}
		if (name == PropertyName._secondaryVfx)
		{
			value = VariantUtils.CreateFrom(in _secondaryVfx);
			return true;
		}
		if (name == PropertyName._creatureCenter)
		{
			value = VariantUtils.CreateFrom(in _creatureCenter);
			return true;
		}
		if (name == PropertyName._vfxColor)
		{
			value = VariantUtils.CreateFrom(in _vfxColor);
			return true;
		}
		if (name == PropertyName._facingEnemies)
		{
			value = VariantUtils.CreateFrom(in _facingEnemies);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._primaryVfx, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._secondaryVfx, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._creatureCenter, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._vfxColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._facingEnemies, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._primaryVfx, Variant.From(in _primaryVfx));
		info.AddProperty(PropertyName._secondaryVfx, Variant.From(in _secondaryVfx));
		info.AddProperty(PropertyName._creatureCenter, Variant.From(in _creatureCenter));
		info.AddProperty(PropertyName._vfxColor, Variant.From(in _vfxColor));
		info.AddProperty(PropertyName._facingEnemies, Variant.From(in _facingEnemies));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._primaryVfx, out var value))
		{
			_primaryVfx = value.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._secondaryVfx, out var value2))
		{
			_secondaryVfx = value2.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._creatureCenter, out var value3))
		{
			_creatureCenter = value3.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._vfxColor, out var value4))
		{
			_vfxColor = value4.As<VfxColor>();
		}
		if (info.TryGetProperty(PropertyName._facingEnemies, out var value5))
		{
			_facingEnemies = value5.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value6))
		{
			_tween = value6.As<Tween>();
		}
	}
}
