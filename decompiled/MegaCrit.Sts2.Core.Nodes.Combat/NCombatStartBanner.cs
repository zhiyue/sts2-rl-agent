using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Combat;

[ScriptPath("res://src/Core/Nodes/Combat/NCombatStartBanner.cs")]
public class NCombatStartBanner : Control
{
	public new class MethodName : Control.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _colorRect = "_colorRect";

		public static readonly StringName _label = "_label";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private ColorRect _colorRect;

	private MegaLabel _label;

	private static readonly string _scenePath = SceneHelper.GetScenePath("combat/combat_start_banner");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(_scenePath);

	public static NCombatStartBanner? Create()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		if (NCombatUi.IsDebugHideTextVfx)
		{
			return null;
		}
		return PreloadManager.Cache.GetScene(_scenePath).Instantiate<NCombatStartBanner>(PackedScene.GenEditState.Disabled);
	}

	public override void _Ready()
	{
		_colorRect = GetNode<ColorRect>("ColorRect");
		_colorRect.Modulate = Colors.Transparent;
		_label = GetNode<MegaLabel>("Label");
		_label.SetTextAutoSize(new LocString("gameplay_ui", "BATTLE_START").GetFormattedText());
		_label.Modulate = Colors.Transparent;
		TaskHelper.RunSafely(AnimateVfx());
	}

	private async Task AnimateVfx()
	{
		NDebugAudioManager.Instance?.Play(Rng.Chaotic.NextItem(TmpSfx.BattleStart));
		Tween tween = CreateTween().SetParallel();
		tween.TweenInterval(0.3);
		tween.Chain();
		tween.TweenProperty(_colorRect, "modulate:a", 0.5f, 0.75).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		tween.TweenProperty(_colorRect, "scale", Vector2.One, 0.75).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
			.From(Vector2.One * 1.2f);
		tween.TweenProperty(_label, "modulate:a", 1f, 1.2999999523162842).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		tween.TweenProperty(_label, "scale", Vector2.One, 0.75).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
			.From(Vector2.One * 2f);
		tween.TweenProperty(_label, "modulate:a", 0f, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic)
			.SetDelay(1.2999999523162842);
		await ToSignal(tween, Tween.SignalName.Finished);
		GetParent().AddChildSafely(NPlayerTurnBanner.Create(1));
		tween = CreateTween();
		tween.TweenProperty(_colorRect, "modulate:a", 0f, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic)
			.SetDelay(1.5);
		await ToSignal(tween, Tween.SignalName.Finished);
		this.QueueFreeSafely();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NCombatStartBanner>(Create());
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

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NCombatStartBanner>(Create());
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
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._colorRect)
		{
			_colorRect = VariantUtils.ConvertTo<ColorRect>(in value);
			return true;
		}
		if (name == PropertyName._label)
		{
			_label = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._colorRect)
		{
			value = VariantUtils.CreateFrom(in _colorRect);
			return true;
		}
		if (name == PropertyName._label)
		{
			value = VariantUtils.CreateFrom(in _label);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._colorRect, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._label, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._colorRect, Variant.From(in _colorRect));
		info.AddProperty(PropertyName._label, Variant.From(in _label));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._colorRect, out var value))
		{
			_colorRect = value.As<ColorRect>();
		}
		if (info.TryGetProperty(PropertyName._label, out var value2))
		{
			_label = value2.As<MegaLabel>();
		}
	}
}
