using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Combat;

[ScriptPath("res://src/Core/Nodes/Combat/NPlayerTurnBanner.cs")]
public class NPlayerTurnBanner : Control
{
	public new class MethodName : Control.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _label = "_label";

		public static readonly StringName _turnLabel = "_turnLabel";

		public static readonly StringName _roundNumber = "_roundNumber";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private MegaLabel _label;

	private MegaLabel _turnLabel;

	private int _roundNumber;

	private static readonly string _scenePath = SceneHelper.GetScenePath("combat/player_turn_banner");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(_scenePath);

	public static NPlayerTurnBanner? Create(int roundNumber)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		if (NCombatUi.IsDebugHideTextVfx)
		{
			return null;
		}
		NPlayerTurnBanner nPlayerTurnBanner = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NPlayerTurnBanner>(PackedScene.GenEditState.Disabled);
		nPlayerTurnBanner._roundNumber = roundNumber;
		return nPlayerTurnBanner;
	}

	public override void _Ready()
	{
		_label = GetNode<MegaLabel>("Label");
		if (CombatManager.Instance.PlayersTakingExtraTurn.Count > 0)
		{
			_label.SetTextAutoSize(new LocString("gameplay_ui", "PLAYER_TURN_EXTRA").GetFormattedText());
		}
		else
		{
			_label.SetTextAutoSize(new LocString("gameplay_ui", "PLAYER_TURN").GetFormattedText());
		}
		_turnLabel = GetNode<MegaLabel>("TurnNumber");
		LocString locString = new LocString("gameplay_ui", "TURN_COUNT");
		locString.Add("turnNumber", _roundNumber);
		_turnLabel.SetTextAutoSize(locString.GetFormattedText());
		base.Modulate = Colors.Transparent;
		TaskHelper.RunSafely(Display());
	}

	private async Task Display()
	{
		NDebugAudioManager.Instance?.Play("player_turn.mp3");
		Tween tween = CreateTween();
		tween.SetParallel();
		tween.TweenProperty(this, "modulate:a", 1f, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		tween.TweenProperty(_label, "position", _label.Position + new Vector2(0f, -50f), 1.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		tween.TweenProperty(_turnLabel, "position", _turnLabel.Position + new Vector2(0f, 50f), 1.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		await ToSignal(tween, Tween.SignalName.Finished);
		tween = CreateTween();
		tween.TweenInterval(0.4);
		tween.TweenProperty(this, "modulate:a", 0f, 0.30000001192092896).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
		await ToSignal(tween, Tween.SignalName.Finished);
		this.QueueFreeSafely();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "roundNumber", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NPlayerTurnBanner>(Create(VariantUtils.ConvertTo<int>(in args[0])));
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
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NPlayerTurnBanner>(Create(VariantUtils.ConvertTo<int>(in args[0])));
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
		if (name == PropertyName._label)
		{
			_label = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._turnLabel)
		{
			_turnLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._roundNumber)
		{
			_roundNumber = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._label)
		{
			value = VariantUtils.CreateFrom(in _label);
			return true;
		}
		if (name == PropertyName._turnLabel)
		{
			value = VariantUtils.CreateFrom(in _turnLabel);
			return true;
		}
		if (name == PropertyName._roundNumber)
		{
			value = VariantUtils.CreateFrom(in _roundNumber);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._label, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._turnLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._roundNumber, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._label, Variant.From(in _label));
		info.AddProperty(PropertyName._turnLabel, Variant.From(in _turnLabel));
		info.AddProperty(PropertyName._roundNumber, Variant.From(in _roundNumber));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._label, out var value))
		{
			_label = value.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._turnLabel, out var value2))
		{
			_turnLabel = value2.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._roundNumber, out var value3))
		{
			_roundNumber = value3.As<int>();
		}
	}
}
