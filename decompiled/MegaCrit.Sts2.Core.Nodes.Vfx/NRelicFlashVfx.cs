using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NRelicFlashVfx.cs")]
public class NRelicFlashVfx : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _sprite = "_sprite";

		public static readonly StringName _sprite2 = "_sprite2";

		public static readonly StringName _sprite3 = "_sprite3";

		public static readonly StringName _tween = "_tween";
	}

	public new class SignalName : Control.SignalName
	{
	}

	public const float activationDuration = 1f;

	private const string _scenePath = "res://scenes/vfx/relic_flash_vfx.tscn";

	private TextureRect _sprite;

	private TextureRect _sprite2;

	private TextureRect _sprite3;

	private static readonly Vector2 _targetScale = Vector2.One * 1.25f;

	private RelicModel? _relic;

	private Creature? _target;

	private Tween? _tween;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>("res://scenes/vfx/relic_flash_vfx.tscn");

	public static NRelicFlashVfx? Create(RelicModel relic)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NRelicFlashVfx nRelicFlashVfx = PreloadManager.Cache.GetScene("res://scenes/vfx/relic_flash_vfx.tscn").Instantiate<NRelicFlashVfx>(PackedScene.GenEditState.Disabled);
		nRelicFlashVfx._relic = relic;
		return nRelicFlashVfx;
	}

	public static NRelicFlashVfx? Create(RelicModel relic, Creature target)
	{
		NRelicFlashVfx nRelicFlashVfx = Create(relic);
		if (nRelicFlashVfx == null)
		{
			return null;
		}
		nRelicFlashVfx._target = target;
		return nRelicFlashVfx;
	}

	public override void _Ready()
	{
		_sprite = GetNode<TextureRect>("Image1");
		_sprite2 = GetNode<TextureRect>("Image2");
		_sprite3 = GetNode<TextureRect>("Image3");
		if (_target != null)
		{
			base.GlobalPosition = NCombatRoom.Instance.GetCreatureNode(_target).GetTopOfHitbox();
		}
		TaskHelper.RunSafely(StartVfx());
	}

	public override void _ExitTree()
	{
		_tween?.Kill();
	}

	private async Task StartVfx()
	{
		_sprite.Texture = _relic.Icon;
		_sprite2.Texture = _relic.Icon;
		_sprite3.Texture = _relic.Icon;
		_tween = CreateTween().SetParallel();
		if (_target != null)
		{
			base.Position += new Vector2(0f, 64f);
			_tween.TweenProperty(this, "position:y", base.Position.Y - 64f, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		}
		_tween.TweenProperty(_sprite, "modulate:a", 1f, 0.01);
		_tween.TweenProperty(_sprite, "scale", _targetScale, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		_tween.TweenProperty(_sprite, "modulate:a", 0f, 1.5).SetDelay(0.01);
		_tween.TweenProperty(_sprite2, "modulate:a", 1f, 0.01).SetDelay(0.2);
		_tween.TweenProperty(_sprite2, "scale", _targetScale, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic)
			.SetDelay(0.2);
		_tween.TweenProperty(_sprite2, "modulate:a", 0f, 1.5).SetDelay(0.21);
		_tween.TweenProperty(_sprite3, "modulate:a", 1f, 0.01).SetDelay(0.4);
		_tween.TweenProperty(_sprite3, "scale", _targetScale, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic)
			.SetDelay(0.4);
		_tween.TweenProperty(_sprite3, "modulate:a", 0f, 1.5).SetDelay(0.41);
		await ToSignal(_tween, Tween.SignalName.Finished);
		this.QueueFreeSafely();
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
			_sprite = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._sprite2)
		{
			_sprite2 = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._sprite3)
		{
			_sprite3 = VariantUtils.ConvertTo<TextureRect>(in value);
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
		if (name == PropertyName._sprite)
		{
			value = VariantUtils.CreateFrom(in _sprite);
			return true;
		}
		if (name == PropertyName._sprite2)
		{
			value = VariantUtils.CreateFrom(in _sprite2);
			return true;
		}
		if (name == PropertyName._sprite3)
		{
			value = VariantUtils.CreateFrom(in _sprite3);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._sprite, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._sprite2, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._sprite3, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._sprite, Variant.From(in _sprite));
		info.AddProperty(PropertyName._sprite2, Variant.From(in _sprite2));
		info.AddProperty(PropertyName._sprite3, Variant.From(in _sprite3));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._sprite, out var value))
		{
			_sprite = value.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._sprite2, out var value2))
		{
			_sprite2 = value2.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._sprite3, out var value3))
		{
			_sprite3 = value3.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value4))
		{
			_tween = value4.As<Tween>();
		}
	}
}
