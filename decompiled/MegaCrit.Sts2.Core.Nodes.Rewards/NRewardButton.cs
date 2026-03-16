using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Rewards;

[ScriptPath("res://src/Core/Nodes/Rewards/NRewardButton.cs")]
public class NRewardButton : NButton
{
	[Signal]
	public delegate void RewardClaimedEventHandler(NRewardButton button);

	[Signal]
	public delegate void RewardSkippedEventHandler(NRewardButton button);

	public new class MethodName : NButton.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName Reload = "Reload";

		public new static readonly StringName OnRelease = "OnRelease";

		public new static readonly StringName OnPress = "OnPress";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";

		public static readonly StringName UpdateShaderParam = "UpdateShaderParam";
	}

	public new class PropertyName : NButton.PropertyName
	{
		public static readonly StringName _background = "_background";

		public static readonly StringName _iconContainer = "_iconContainer";

		public static readonly StringName _label = "_label";

		public static readonly StringName _reticle = "_reticle";

		public static readonly StringName _hsv = "_hsv";

		public static readonly StringName _currentTween = "_currentTween";

		public static readonly StringName _hsvDefault = "_hsvDefault";

		public static readonly StringName _hsvHover = "_hsvHover";

		public static readonly StringName _hsvDown = "_hsvDown";
	}

	public new class SignalName : NButton.SignalName
	{
		public static readonly StringName RewardClaimed = "RewardClaimed";

		public static readonly StringName RewardSkipped = "RewardSkipped";
	}

	private static readonly StringName _v = new StringName("v");

	private static readonly StringName _fontOutlineColor = new StringName("theme_override_colors/font_outline_color");

	private static readonly StringName _defaultColor = new StringName("theme_override_colors/default_color");

	private TextureRect _background;

	private Control _iconContainer;

	private MegaRichTextLabel _label;

	private NSelectionReticle _reticle;

	private ShaderMaterial _hsv;

	private Tween? _currentTween;

	private Variant _hsvDefault = 0.9;

	private Variant _hsvHover = 1.1;

	private Variant _hsvDown = 0.7;

	private RewardClaimedEventHandler backing_RewardClaimed;

	private RewardSkippedEventHandler backing_RewardSkipped;

	public Reward? Reward { get; private set; }

	private static string ScenePath => "res://scenes/rewards/reward_button.tscn";

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	public event RewardClaimedEventHandler RewardClaimed
	{
		add
		{
			backing_RewardClaimed = (RewardClaimedEventHandler)Delegate.Combine(backing_RewardClaimed, value);
		}
		remove
		{
			backing_RewardClaimed = (RewardClaimedEventHandler)Delegate.Remove(backing_RewardClaimed, value);
		}
	}

	public event RewardSkippedEventHandler RewardSkipped
	{
		add
		{
			backing_RewardSkipped = (RewardSkippedEventHandler)Delegate.Combine(backing_RewardSkipped, value);
		}
		remove
		{
			backing_RewardSkipped = (RewardSkippedEventHandler)Delegate.Remove(backing_RewardSkipped, value);
		}
	}

	public static NRewardButton Create(Reward reward, NRewardsScreen screen)
	{
		NRewardButton nRewardButton = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NRewardButton>(PackedScene.GenEditState.Disabled);
		nRewardButton.SetReward(reward);
		return nRewardButton;
	}

	public override void _Ready()
	{
		ConnectSignals();
		_background = GetNode<TextureRect>("%Background");
		_iconContainer = GetNode<Control>("%Icon");
		_label = GetNode<MegaRichTextLabel>("%Label");
		_reticle = GetNode<NSelectionReticle>("%SelectionReticle");
		_hsv = (ShaderMaterial)_background.Material;
		Reload();
	}

	private void SetReward(Reward reward)
	{
		if (reward is LinkedRewardSet)
		{
			throw new ArgumentException("You aren't allowed to apply a RewardChainSet to a NRewardButton");
		}
		Reward = reward;
		if (IsNodeReady())
		{
			Reload();
		}
	}

	private void Reload()
	{
		if (IsNodeReady() && Reward != null)
		{
			Control control = Reward.CreateIcon();
			_iconContainer.AddChildSafely(control);
			control.Position = Reward.IconPosition;
			if (Reward is PotionReward)
			{
				control.Scale = 0.8f * Vector2.One;
			}
			_label.Text = Reward.Description.GetFormattedText();
		}
	}

	private async Task GetReward()
	{
		Disable();
		if (await Reward.OnSelectWrapper())
		{
			if (TestMode.IsOff)
			{
				NGlobalUi globalUi = NRun.Instance.GlobalUi;
				Reward reward = Reward;
				if (reward is RelicReward relicReward)
				{
					RelicModel claimedRelic = relicReward.ClaimedRelic;
					if (claimedRelic != null)
					{
						globalUi.RelicInventory.AnimateRelic(relicReward.ClaimedRelic, _iconContainer.GlobalPosition);
					}
				}
				else if (reward is PotionReward potionReward)
				{
					globalUi.TopBar.PotionContainer.AnimatePotion(potionReward.ClaimedPotion, _iconContainer.GlobalPosition);
				}
			}
			_isEnabled = false;
			EmitSignal(SignalName.RewardClaimed, this);
		}
		else
		{
			Enable();
			this.TryGrabFocus();
			EmitSignal(SignalName.RewardSkipped, this);
		}
	}

	protected override void OnRelease()
	{
		base.OnRelease();
		OnUnfocus();
		TaskHelper.RunSafely(GetReward());
	}

	protected override void OnPress()
	{
		base.OnPress();
		_currentTween?.Kill();
		_currentTween = CreateTween().SetParallel();
		_currentTween.TweenMethod(Callable.From<float>(UpdateShaderParam), _hsvHover, _hsvDown, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_currentTween.TweenProperty(_label, "modulate:a", 1f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		NHoverTipSet.Remove(this);
	}

	protected override void OnFocus()
	{
		base.OnFocus();
		_label.Set(_defaultColor, StsColors.gold);
		_label.Set(_fontOutlineColor, StsColors.rewardLabelGoldOutline);
		_currentTween?.Kill();
		_hsv.SetShaderParameter(_v, _hsvHover);
		NHoverTipSet nHoverTipSet = NHoverTipSet.CreateAndShow(this, Reward.HoverTips);
		nHoverTipSet.GlobalPosition = base.GlobalPosition + Vector2.Left * 45f;
		nHoverTipSet.SetAlignment(this, HoverTipAlignment.Left);
		_reticle.OnSelect();
	}

	protected override void OnUnfocus()
	{
		base.OnUnfocus();
		_label.Set(_defaultColor, StsColors.cream);
		_label.Set(_fontOutlineColor, StsColors.rewardLabelOutline);
		_currentTween?.Kill();
		_currentTween = CreateTween();
		_currentTween.TweenMethod(Callable.From<float>(UpdateShaderParam), _hsv.GetShaderParameter(_v), _hsvDefault, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		NHoverTipSet.Remove(this);
		_reticle.OnDeselect();
	}

	private void UpdateShaderParam(float value)
	{
		_hsv.SetShaderParameter(_v, value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(7);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Reload, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnRelease, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPress, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateShaderParam, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "value", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.Reload && args.Count == 0)
		{
			Reload();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnRelease && args.Count == 0)
		{
			OnRelease();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPress && args.Count == 0)
		{
			OnPress();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnFocus && args.Count == 0)
		{
			OnFocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnUnfocus && args.Count == 0)
		{
			OnUnfocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateShaderParam && args.Count == 1)
		{
			UpdateShaderParam(VariantUtils.ConvertTo<float>(in args[0]));
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
		if (method == MethodName.Reload)
		{
			return true;
		}
		if (method == MethodName.OnRelease)
		{
			return true;
		}
		if (method == MethodName.OnPress)
		{
			return true;
		}
		if (method == MethodName.OnFocus)
		{
			return true;
		}
		if (method == MethodName.OnUnfocus)
		{
			return true;
		}
		if (method == MethodName.UpdateShaderParam)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._background)
		{
			_background = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._iconContainer)
		{
			_iconContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._label)
		{
			_label = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._reticle)
		{
			_reticle = VariantUtils.ConvertTo<NSelectionReticle>(in value);
			return true;
		}
		if (name == PropertyName._hsv)
		{
			_hsv = VariantUtils.ConvertTo<ShaderMaterial>(in value);
			return true;
		}
		if (name == PropertyName._currentTween)
		{
			_currentTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._hsvDefault)
		{
			_hsvDefault = VariantUtils.ConvertTo<Variant>(in value);
			return true;
		}
		if (name == PropertyName._hsvHover)
		{
			_hsvHover = VariantUtils.ConvertTo<Variant>(in value);
			return true;
		}
		if (name == PropertyName._hsvDown)
		{
			_hsvDown = VariantUtils.ConvertTo<Variant>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._background)
		{
			value = VariantUtils.CreateFrom(in _background);
			return true;
		}
		if (name == PropertyName._iconContainer)
		{
			value = VariantUtils.CreateFrom(in _iconContainer);
			return true;
		}
		if (name == PropertyName._label)
		{
			value = VariantUtils.CreateFrom(in _label);
			return true;
		}
		if (name == PropertyName._reticle)
		{
			value = VariantUtils.CreateFrom(in _reticle);
			return true;
		}
		if (name == PropertyName._hsv)
		{
			value = VariantUtils.CreateFrom(in _hsv);
			return true;
		}
		if (name == PropertyName._currentTween)
		{
			value = VariantUtils.CreateFrom(in _currentTween);
			return true;
		}
		if (name == PropertyName._hsvDefault)
		{
			value = VariantUtils.CreateFrom(in _hsvDefault);
			return true;
		}
		if (name == PropertyName._hsvHover)
		{
			value = VariantUtils.CreateFrom(in _hsvHover);
			return true;
		}
		if (name == PropertyName._hsvDown)
		{
			value = VariantUtils.CreateFrom(in _hsvDown);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._background, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._iconContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._label, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._reticle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hsv, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._currentTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Nil, PropertyName._hsvDefault, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Nil, PropertyName._hsvHover, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Nil, PropertyName._hsvDown, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._background, Variant.From(in _background));
		info.AddProperty(PropertyName._iconContainer, Variant.From(in _iconContainer));
		info.AddProperty(PropertyName._label, Variant.From(in _label));
		info.AddProperty(PropertyName._reticle, Variant.From(in _reticle));
		info.AddProperty(PropertyName._hsv, Variant.From(in _hsv));
		info.AddProperty(PropertyName._currentTween, Variant.From(in _currentTween));
		info.AddProperty(PropertyName._hsvDefault, Variant.From(in _hsvDefault));
		info.AddProperty(PropertyName._hsvHover, Variant.From(in _hsvHover));
		info.AddProperty(PropertyName._hsvDown, Variant.From(in _hsvDown));
		info.AddSignalEventDelegate(SignalName.RewardClaimed, backing_RewardClaimed);
		info.AddSignalEventDelegate(SignalName.RewardSkipped, backing_RewardSkipped);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._background, out var value))
		{
			_background = value.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._iconContainer, out var value2))
		{
			_iconContainer = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._label, out var value3))
		{
			_label = value3.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._reticle, out var value4))
		{
			_reticle = value4.As<NSelectionReticle>();
		}
		if (info.TryGetProperty(PropertyName._hsv, out var value5))
		{
			_hsv = value5.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._currentTween, out var value6))
		{
			_currentTween = value6.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._hsvDefault, out var value7))
		{
			_hsvDefault = value7.As<Variant>();
		}
		if (info.TryGetProperty(PropertyName._hsvHover, out var value8))
		{
			_hsvHover = value8.As<Variant>();
		}
		if (info.TryGetProperty(PropertyName._hsvDown, out var value9))
		{
			_hsvDown = value9.As<Variant>();
		}
		if (info.TryGetSignalEventDelegate<RewardClaimedEventHandler>(SignalName.RewardClaimed, out var value10))
		{
			backing_RewardClaimed = value10;
		}
		if (info.TryGetSignalEventDelegate<RewardSkippedEventHandler>(SignalName.RewardSkipped, out var value11))
		{
			backing_RewardSkipped = value11;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotSignalList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(SignalName.RewardClaimed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "button", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(SignalName.RewardSkipped, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "button", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		return list;
	}

	protected void EmitSignalRewardClaimed(NRewardButton button)
	{
		EmitSignal(SignalName.RewardClaimed, button);
	}

	protected void EmitSignalRewardSkipped(NRewardButton button)
	{
		EmitSignal(SignalName.RewardSkipped, button);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.RewardClaimed && args.Count == 1)
		{
			backing_RewardClaimed?.Invoke(VariantUtils.ConvertTo<NRewardButton>(in args[0]));
		}
		else if (signal == SignalName.RewardSkipped && args.Count == 1)
		{
			backing_RewardSkipped?.Invoke(VariantUtils.ConvertTo<NRewardButton>(in args[0]));
		}
		else
		{
			base.RaiseGodotClassSignalCallbacks(in signal, args);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassSignal(in godot_string_name signal)
	{
		if (signal == SignalName.RewardClaimed)
		{
			return true;
		}
		if (signal == SignalName.RewardSkipped)
		{
			return true;
		}
		return base.HasGodotClassSignal(in signal);
	}
}
