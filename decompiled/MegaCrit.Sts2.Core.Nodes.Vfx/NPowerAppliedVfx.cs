using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NPowerAppliedVfx.cs")]
public class NPowerAppliedVfx : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _icon = "_icon";

		public static readonly StringName _iconEcho = "_iconEcho";

		public static readonly StringName _powerField = "_powerField";

		public static readonly StringName _amount = "_amount";

		public static readonly StringName _textTween = "_textTween";

		public static readonly StringName _spriteTween = "_spriteTween";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private const string _scenePath = "res://scenes/vfx/power_applied_vfx.tscn";

	private TextureRect _icon;

	private TextureRect _iconEcho;

	private MegaLabel _powerField;

	private PowerModel _power;

	private int _amount;

	private Tween? _textTween;

	private Tween? _spriteTween;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>("res://scenes/vfx/power_applied_vfx.tscn");

	public override void _Ready()
	{
		_icon = GetNode<TextureRect>("Icon");
		_iconEcho = GetNode<TextureRect>("Icon/IconEcho");
		_powerField = GetNode<MegaLabel>("Label");
		NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(_power.Owner);
		if (nCreature == null)
		{
			this.QueueFreeSafely();
			return;
		}
		base.GlobalPosition = nCreature.VfxSpawnPosition;
		TaskHelper.RunSafely(StartVfx());
	}

	public static NPowerAppliedVfx? Create(PowerModel power, int amount)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		if (NCombatUi.IsDebugHideTextVfx)
		{
			return null;
		}
		if (!power.ShouldPlayVfx)
		{
			return null;
		}
		NPowerAppliedVfx nPowerAppliedVfx = PreloadManager.Cache.GetScene("res://scenes/vfx/power_applied_vfx.tscn").Instantiate<NPowerAppliedVfx>(PackedScene.GenEditState.Disabled);
		nPowerAppliedVfx._power = power;
		nPowerAppliedVfx._amount = amount;
		return nPowerAppliedVfx;
	}

	public override void _ExitTree()
	{
		_spriteTween?.Kill();
		_textTween?.Kill();
	}

	private async Task StartVfx()
	{
		_powerField.SetTextAutoSize(_power.Title.GetFormattedText());
		_icon.Texture = _power.BigIcon;
		_iconEcho.Texture = _power.BigIcon;
		_powerField.Modulate = ((_power.GetTypeForAmount(_amount) == PowerType.Buff) ? StsColors.green : StsColors.red);
		_powerField.Position = new Vector2(_powerField.Position.X, _powerField.Position.Y - 200f);
		_spriteTween = CreateTween().SetParallel();
		_spriteTween.TweenProperty(_icon, "scale", Vector2.One * 0.8f, 1.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
			.From(Vector2.One * 0.4f);
		_spriteTween.TweenProperty(_icon, "modulate:a", 0.5f, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
		_spriteTween.TweenProperty(_icon, "modulate:a", 0f, 1.0).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Sine)
			.SetDelay(0.25);
		_textTween = CreateTween().SetParallel();
		CreateTween().TweenProperty(_powerField, "position:y", _powerField.Position.Y + 50f, 1.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		_textTween.TweenProperty(_powerField, "modulate:a", 1f, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_textTween.TweenProperty(_powerField, "modulate:a", 0f, 0.75).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Expo)
			.From(1f)
			.SetDelay(0.25);
		await ToSignal(_spriteTween, Tween.SignalName.Finished);
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
		if (name == PropertyName._icon)
		{
			_icon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._iconEcho)
		{
			_iconEcho = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._powerField)
		{
			_powerField = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._amount)
		{
			_amount = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._textTween)
		{
			_textTween = VariantUtils.ConvertTo<Tween>(in value);
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
		if (name == PropertyName._icon)
		{
			value = VariantUtils.CreateFrom(in _icon);
			return true;
		}
		if (name == PropertyName._iconEcho)
		{
			value = VariantUtils.CreateFrom(in _iconEcho);
			return true;
		}
		if (name == PropertyName._powerField)
		{
			value = VariantUtils.CreateFrom(in _powerField);
			return true;
		}
		if (name == PropertyName._amount)
		{
			value = VariantUtils.CreateFrom(in _amount);
			return true;
		}
		if (name == PropertyName._textTween)
		{
			value = VariantUtils.CreateFrom(in _textTween);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._icon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._iconEcho, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._powerField, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._amount, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._textTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._spriteTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._icon, Variant.From(in _icon));
		info.AddProperty(PropertyName._iconEcho, Variant.From(in _iconEcho));
		info.AddProperty(PropertyName._powerField, Variant.From(in _powerField));
		info.AddProperty(PropertyName._amount, Variant.From(in _amount));
		info.AddProperty(PropertyName._textTween, Variant.From(in _textTween));
		info.AddProperty(PropertyName._spriteTween, Variant.From(in _spriteTween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._icon, out var value))
		{
			_icon = value.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._iconEcho, out var value2))
		{
			_iconEcho = value2.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._powerField, out var value3))
		{
			_powerField = value3.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._amount, out var value4))
		{
			_amount = value4.As<int>();
		}
		if (info.TryGetProperty(PropertyName._textTween, out var value5))
		{
			_textTween = value5.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._spriteTween, out var value6))
		{
			_spriteTween = value6.As<Tween>();
		}
	}
}
