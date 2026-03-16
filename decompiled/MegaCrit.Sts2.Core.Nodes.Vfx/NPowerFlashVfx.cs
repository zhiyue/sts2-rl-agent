using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NPowerFlashVfx.cs")]
public class NPowerFlashVfx : Node2D
{
	public new class MethodName : Node2D.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : Node2D.PropertyName
	{
		public static readonly StringName _sprite = "_sprite";

		public static readonly StringName _spriteTween = "_spriteTween";
	}

	public new class SignalName : Node2D.SignalName
	{
	}

	private const string _scenePath = "res://scenes/vfx/power_flash_vfx.tscn";

	private Sprite2D _sprite;

	private PowerModel _power;

	private Tween? _spriteTween;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>("res://scenes/vfx/power_flash_vfx.tscn");

	public override void _Ready()
	{
		NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(_power.Owner);
		if (nCreature == null)
		{
			this.QueueFreeSafely();
			return;
		}
		_sprite = GetNode<Sprite2D>("Sprite2D");
		base.GlobalPosition = nCreature.VfxSpawnPosition;
		TaskHelper.RunSafely(StartVfx());
	}

	public override void _ExitTree()
	{
		_spriteTween?.Kill();
	}

	private async Task StartVfx()
	{
		_sprite.Texture = _power.BigIcon;
		_sprite.Modulate = Colors.White;
		_spriteTween = CreateTween();
		_spriteTween.SetParallel();
		_spriteTween.TweenProperty(_sprite, "scale", Vector2.One * 0.4f, 0.4);
		_spriteTween.TweenProperty(_sprite, "modulate:a", 1, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
		_spriteTween.SetParallel(parallel: false);
		_spriteTween.TweenProperty(_sprite, "scale", Vector2.One * 0.45f, 0.4);
		_spriteTween.TweenProperty(_sprite, "modulate:a", 0, 0.25).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Sine);
		await ToSignal(_spriteTween, Tween.SignalName.Finished);
		this.QueueFreeSafely();
	}

	public static NPowerFlashVfx? Create(PowerModel power)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		if (!power.ShouldPlayVfx)
		{
			return null;
		}
		NPowerFlashVfx nPowerFlashVfx = (NPowerFlashVfx)PreloadManager.Cache.GetScene("res://scenes/vfx/power_flash_vfx.tscn").Instantiate(PackedScene.GenEditState.Disabled);
		nPowerFlashVfx._power = power;
		return nPowerFlashVfx;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._sprite)
		{
			_sprite = VariantUtils.ConvertTo<Sprite2D>(in value);
			return true;
		}
		if (name == PropertyName._spriteTween)
		{
			_spriteTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._sprite)
		{
			value = VariantUtils.CreateFrom(in _sprite);
			return true;
		}
		if (name == PropertyName._spriteTween)
		{
			value = VariantUtils.CreateFrom(in _spriteTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._sprite, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._spriteTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._sprite, Variant.From(in _sprite));
		info.AddProperty(PropertyName._spriteTween, Variant.From(in _spriteTween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._sprite, out var value))
		{
			_sprite = value.As<Sprite2D>();
		}
		if (info.TryGetProperty(PropertyName._spriteTween, out var value2))
		{
			_spriteTween = value2.As<Tween>();
		}
	}
}
