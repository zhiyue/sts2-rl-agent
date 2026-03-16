using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Timeline;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Timeline.UnlockScreens;

[ScriptPath("res://src/Core/Nodes/Screens/Timeline/UnlockScreens/NUnlockCharacterScreen.cs")]
public class NUnlockCharacterScreen : NUnlockScreen
{
	public new class MethodName : NUnlockScreen.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName Open = "Open";

		public new static readonly StringName OnScreenPreClose = "OnScreenPreClose";

		public new static readonly StringName OnScreenClose = "OnScreenClose";
	}

	public new class PropertyName : NUnlockScreen.PropertyName
	{
		public static readonly StringName _topLabel = "_topLabel";

		public static readonly StringName _bottomLabel = "_bottomLabel";

		public static readonly StringName _spineAnchor = "_spineAnchor";

		public static readonly StringName _creatureVisuals = "_creatureVisuals";

		public static readonly StringName _rareGlow = "_rareGlow";

		public new static readonly StringName _tween = "_tween";
	}

	public new class SignalName : NUnlockScreen.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("timeline_screen/unlock_character_screen");

	private MegaRichTextLabel _topLabel;

	private MegaRichTextLabel _bottomLabel;

	private Control _spineAnchor;

	private NCreatureVisuals _creatureVisuals;

	private GpuParticles2D _rareGlow;

	private EpochModel _epoch;

	private CharacterModel _character;

	private Tween? _tween;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(_scenePath);

	public static NUnlockCharacterScreen Create(EpochModel epoch, CharacterModel character)
	{
		NUnlockCharacterScreen nUnlockCharacterScreen = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NUnlockCharacterScreen>(PackedScene.GenEditState.Disabled);
		nUnlockCharacterScreen._character = character;
		nUnlockCharacterScreen._epoch = epoch;
		return nUnlockCharacterScreen;
	}

	public override void _Ready()
	{
		ConnectSignals();
		_topLabel = GetNode<MegaRichTextLabel>("%TopLabel");
		_bottomLabel = GetNode<MegaRichTextLabel>("%BottomLabel");
		_spineAnchor = GetNode<Control>("%SpineAnchor");
		_rareGlow = GetNode<GpuParticles2D>("%RareGlow");
		_topLabel.Text = new LocString("epochs", _epoch.Id + ".unlock").GetFormattedText();
		_bottomLabel.Text = new LocString("epochs", _epoch.Id + ".unlockText").GetFormattedText();
		_topLabel.Modulate = StsColors.transparentBlack;
		_bottomLabel.Modulate = StsColors.transparentBlack;
		_spineAnchor.Modulate = StsColors.transparentBlack;
		_creatureVisuals = _character.CreateVisuals();
		_spineAnchor.AddChildSafely(_creatureVisuals);
	}

	public override void Open()
	{
		base.Open();
		SfxCmd.Play("event:/sfx/ui/timeline/ui_timeline_unlock");
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_spineAnchor, "modulate", Colors.White, 0.25).SetDelay(0.25);
		_tween.TweenProperty(_topLabel, "modulate", Colors.White, 1.0).SetDelay(1.0);
		_tween.TweenProperty(_bottomLabel, "modulate", Colors.White, 1.0).SetDelay(1.5);
		_tween.TweenProperty(_rareGlow, "modulate:a", 1f, 0.5).SetDelay(1.0);
		_creatureVisuals.SpineBody.GetAnimationState().AddAnimation("idle_loop");
		_creatureVisuals.SpineBody.GetAnimationState().AddAnimation("attack", 0.5f, loop: false);
		_creatureVisuals.SpineBody.GetAnimationState().AddAnimation("idle_loop");
	}

	protected override void OnScreenPreClose()
	{
		_tween?.Kill();
		_tween = CreateTween();
		_tween.TweenProperty(_rareGlow, "modulate:a", 0f, 0.5);
	}

	protected override void OnScreenClose()
	{
		NTimelineScreen.Instance.EnableInput();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Open, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnScreenPreClose, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnScreenClose, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.Open && args.Count == 0)
		{
			Open();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnScreenPreClose && args.Count == 0)
		{
			OnScreenPreClose();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnScreenClose && args.Count == 0)
		{
			OnScreenClose();
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
		if (method == MethodName.Open)
		{
			return true;
		}
		if (method == MethodName.OnScreenPreClose)
		{
			return true;
		}
		if (method == MethodName.OnScreenClose)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._topLabel)
		{
			_topLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._bottomLabel)
		{
			_bottomLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._spineAnchor)
		{
			_spineAnchor = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._creatureVisuals)
		{
			_creatureVisuals = VariantUtils.ConvertTo<NCreatureVisuals>(in value);
			return true;
		}
		if (name == PropertyName._rareGlow)
		{
			_rareGlow = VariantUtils.ConvertTo<GpuParticles2D>(in value);
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
		if (name == PropertyName._topLabel)
		{
			value = VariantUtils.CreateFrom(in _topLabel);
			return true;
		}
		if (name == PropertyName._bottomLabel)
		{
			value = VariantUtils.CreateFrom(in _bottomLabel);
			return true;
		}
		if (name == PropertyName._spineAnchor)
		{
			value = VariantUtils.CreateFrom(in _spineAnchor);
			return true;
		}
		if (name == PropertyName._creatureVisuals)
		{
			value = VariantUtils.CreateFrom(in _creatureVisuals);
			return true;
		}
		if (name == PropertyName._rareGlow)
		{
			value = VariantUtils.CreateFrom(in _rareGlow);
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
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._topLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bottomLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._spineAnchor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._creatureVisuals, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._rareGlow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._topLabel, Variant.From(in _topLabel));
		info.AddProperty(PropertyName._bottomLabel, Variant.From(in _bottomLabel));
		info.AddProperty(PropertyName._spineAnchor, Variant.From(in _spineAnchor));
		info.AddProperty(PropertyName._creatureVisuals, Variant.From(in _creatureVisuals));
		info.AddProperty(PropertyName._rareGlow, Variant.From(in _rareGlow));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._topLabel, out var value))
		{
			_topLabel = value.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._bottomLabel, out var value2))
		{
			_bottomLabel = value2.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._spineAnchor, out var value3))
		{
			_spineAnchor = value3.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._creatureVisuals, out var value4))
		{
			_creatureVisuals = value4.As<NCreatureVisuals>();
		}
		if (info.TryGetProperty(PropertyName._rareGlow, out var value5))
		{
			_rareGlow = value5.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value6))
		{
			_tween = value6.As<Tween>();
		}
	}
}
