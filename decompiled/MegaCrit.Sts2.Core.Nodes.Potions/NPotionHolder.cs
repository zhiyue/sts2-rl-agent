using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Nodes.Potions;

[ScriptPath("res://src/Core/Nodes/Potions/NPotionHolder.cs")]
public class NPotionHolder : NClickableControl
{
	public new class MethodName : NClickableControl.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";

		public new static readonly StringName OnPress = "OnPress";

		public new static readonly StringName OnRelease = "OnRelease";

		public static readonly StringName OpenPotionPopup = "OpenPotionPopup";

		public static readonly StringName AddPotion = "AddPotion";

		public static readonly StringName DisableUntilPotionRemoved = "DisableUntilPotionRemoved";

		public static readonly StringName CancelPotionUse = "CancelPotionUse";

		public static readonly StringName RemoveUsedPotion = "RemoveUsedPotion";

		public static readonly StringName DiscardPotion = "DiscardPotion";

		public static readonly StringName ShouldCancelTargeting = "ShouldCancelTargeting";
	}

	public new class PropertyName : NClickableControl.PropertyName
	{
		public static readonly StringName Potion = "Potion";

		public static readonly StringName HasPotion = "HasPotion";

		public static readonly StringName IsPotionUsable = "IsPotionUsable";

		public static readonly StringName _potionScale = "_potionScale";

		public static readonly StringName _emptyIcon = "_emptyIcon";

		public static readonly StringName _popup = "_popup";

		public static readonly StringName _potionTargeting = "_potionTargeting";

		public static readonly StringName _isUsable = "_isUsable";

		public static readonly StringName _emptyPotionTween = "_emptyPotionTween";

		public static readonly StringName _hoverTween = "_hoverTween";

		public static readonly StringName _disabledUntilPotionRemoved = "_disabledUntilPotionRemoved";

		public static readonly StringName _isFocused = "_isFocused";
	}

	public new class SignalName : NClickableControl.SignalName
	{
	}

	private Vector2 _potionScale = 0.9f * Vector2.One;

	private static readonly HoverTip _emptyHoverTip = new HoverTip(new LocString("static_hover_tips", "POTION_SLOT.title"), new LocString("static_hover_tips", "POTION_SLOT.description"));

	private TextureRect _emptyIcon;

	private NPotionPopup? _popup;

	private bool _potionTargeting;

	private bool _isUsable;

	private Tween? _emptyPotionTween;

	private Tween? _hoverTween;

	private bool _disabledUntilPotionRemoved;

	private bool _isFocused;

	private CancellationTokenSource? _cancelGrayOutPotionSource;

	public NPotion? Potion { get; private set; }

	public bool HasPotion => Potion != null;

	public bool IsPotionUsable => _popup.IsUsable;

	private static string ScenePath => SceneHelper.GetScenePath("/potions/potion_holder");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	public static NPotionHolder Create(bool isUsable)
	{
		NPotionHolder nPotionHolder = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NPotionHolder>(PackedScene.GenEditState.Disabled);
		nPotionHolder._isUsable = isUsable;
		return nPotionHolder;
	}

	public override void _Ready()
	{
		_emptyIcon = GetNode<TextureRect>("%EmptyIcon");
		ConnectSignals();
	}

	protected override void OnFocus()
	{
		if (_isFocused)
		{
			return;
		}
		_isFocused = true;
		_hoverTween?.Kill();
		_hoverTween = CreateTween().SetParallel();
		if (Potion != null)
		{
			Potion.DoBounce();
			_hoverTween.TweenProperty(Potion, "scale", _potionScale * 1.15f, 0.05);
			NDebugAudioManager.Instance?.Play(Rng.Chaotic.NextItem(TmpSfx.PotionSlosh), 0.5f, PitchVariance.Large);
			if (!GodotObject.IsInstanceValid(_popup))
			{
				NHoverTipSet nHoverTipSet = NHoverTipSet.CreateAndShow(this, Potion.Model.HoverTips, HoverTipAlignment.Center);
				nHoverTipSet.GlobalPosition = base.GlobalPosition + Vector2.Down * base.Size.Y * Mathf.Max(1.5f, base.Scale.Y);
			}
		}
		else
		{
			_hoverTween.TweenProperty(_emptyIcon, "scale", _potionScale * 1.15f, 0.05);
			NHoverTipSet nHoverTipSet2 = NHoverTipSet.CreateAndShow(this, _emptyHoverTip);
			nHoverTipSet2.GlobalPosition = base.GlobalPosition + Vector2.Down * base.Size.Y * Mathf.Max(1.5f, base.Scale.Y);
			nHoverTipSet2.SetAlignment(this, HoverTipAlignment.Center);
		}
	}

	protected override void OnUnfocus()
	{
		_isFocused = false;
		NHoverTipSet.Remove(this);
		_hoverTween?.Kill();
		_hoverTween = CreateTween().SetParallel();
		if (Potion != null)
		{
			if (!_disabledUntilPotionRemoved)
			{
				_hoverTween.TweenProperty(Potion, "scale", _potionScale, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
			}
		}
		else
		{
			_hoverTween.TweenProperty(_emptyIcon, "scale", _potionScale, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		}
	}

	protected override void OnPress()
	{
		if (Potion != null)
		{
			GetViewport().SetInputAsHandled();
		}
	}

	protected override void OnRelease()
	{
		if (_isUsable)
		{
			OpenPotionPopup();
		}
		if (Potion != null)
		{
			GetViewport().SetInputAsHandled();
		}
	}

	private void OpenPotionPopup()
	{
		if (HasPotion && !Potion.Model.Owner.RunState.IsGameOver && !_disabledUntilPotionRemoved)
		{
			NHoverTipSet.Remove(this);
			_popup = NPotionPopup.Create(this);
			this.AddChildSafely(_popup);
		}
	}

	public void AddPotion(NPotion potion)
	{
		if (Potion != null)
		{
			throw new InvalidOperationException("Slot already contains a potion");
		}
		Potion = potion;
		_emptyPotionTween?.Kill();
		_emptyIcon.Modulate = Colors.Transparent;
		this.AddChildSafely(Potion);
		Potion.Scale = _potionScale;
		Potion.PivotOffset = Potion.Size * 0.5f;
	}

	public void DisableUntilPotionRemoved()
	{
		if (_popup != null && GodotObject.IsInstanceValid(_popup))
		{
			_popup.Remove();
		}
		_disabledUntilPotionRemoved = true;
		TaskHelper.RunSafely(GrayPotionHolderUntilPlayedAfterDelay());
		this.TryGrabFocus();
	}

	private async Task GrayPotionHolderUntilPlayedAfterDelay()
	{
		_cancelGrayOutPotionSource = new CancellationTokenSource();
		await Task.Delay(100, _cancelGrayOutPotionSource.Token);
		if (!_cancelGrayOutPotionSource.IsCancellationRequested)
		{
			base.Modulate = StsColors.gray;
		}
	}

	public void CancelPotionUse()
	{
		_cancelGrayOutPotionSource?.Cancel();
		_disabledUntilPotionRemoved = false;
		base.Modulate = Colors.White;
	}

	public void RemoveUsedPotion()
	{
		if (Potion == null)
		{
			throw new InvalidOperationException("This slot doesn't contain a potion");
		}
		if (_popup != null && GodotObject.IsInstanceValid(_popup))
		{
			_popup.Remove();
		}
		NHoverTipSet.Remove(this);
		_disabledUntilPotionRemoved = false;
		_cancelGrayOutPotionSource?.Cancel();
		base.Modulate = Colors.White;
		NPotion potionToRemove = Potion;
		Potion = null;
		Tween tween = CreateTween();
		tween.TweenProperty(potionToRemove, "scale", Vector2.Zero, 0.20000000298023224).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Back)
			.FromCurrent();
		tween.TweenCallback(Callable.From(delegate
		{
			this.RemoveChildSafely(potionToRemove);
			potionToRemove.QueueFreeSafely();
		}));
		if (base.IsFocused)
		{
			NHoverTipSet nHoverTipSet = NHoverTipSet.CreateAndShow(this, _emptyHoverTip);
			nHoverTipSet.GlobalPosition = base.GlobalPosition + Vector2.Down * base.Size.Y * 1.5f;
			nHoverTipSet.SetAlignment(this, HoverTipAlignment.Center);
		}
		_emptyPotionTween?.Kill();
		_emptyPotionTween = CreateTween();
		_emptyPotionTween.TweenProperty(_emptyIcon, "modulate", Colors.White, 0.20000000298023224).SetDelay(0.20000000298023224);
	}

	public void DiscardPotion()
	{
		if (Potion == null)
		{
			throw new InvalidOperationException("This slot doesn't contain a potion");
		}
		if (_popup != null && GodotObject.IsInstanceValid(_popup))
		{
			_popup.Remove();
		}
		_disabledUntilPotionRemoved = false;
		_cancelGrayOutPotionSource?.Cancel();
		base.Modulate = Colors.White;
		NPotion potionToRemove = Potion;
		Potion = null;
		Tween tween = CreateTween();
		tween.TweenProperty(potionToRemove, "position:y", -100f, 0.4000000059604645).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Back);
		tween.TweenCallback(Callable.From(delegate
		{
			this.RemoveChildSafely(potionToRemove);
			potionToRemove.QueueFreeSafely();
		}));
		_emptyPotionTween?.Kill();
		_emptyPotionTween = CreateTween();
		_emptyPotionTween.TweenProperty(_emptyIcon, "modulate", Colors.White, 0.20000000298023224).FromCurrent().SetDelay(0.20000000298023224);
	}

	public async Task UsePotion()
	{
		if (Potion == null)
		{
			Log.Warn("Tried to use potion in holder, but potion node is null!");
			return;
		}
		TargetType targetType = Potion.Model.TargetType;
		bool flag = ((targetType == TargetType.AnyEnemy || targetType == TargetType.TargetedNoCreature) ? true : false);
		if (flag || Potion.Model.CanThrowAtAlly())
		{
			RunManager.Instance.HoveredModelTracker.OnLocalPotionSelected(Potion.Model);
			await TargetNode(Potion.Model.TargetType);
			RunManager.Instance.HoveredModelTracker.OnLocalPotionDeselected();
		}
		else
		{
			Potion.Model.EnqueueManualUse(Potion.Model.Owner.Creature);
			this.TryGrabFocus();
		}
	}

	private async Task TargetNode(TargetType targetType)
	{
		Vector2 startPosition = base.GlobalPosition + Vector2.Right * base.Size.X * 0.5f + Vector2.Down * 50f;
		NTargetManager instance = NTargetManager.Instance;
		bool isUsingController = NControllerManager.Instance.IsUsingController;
		instance.StartTargeting(targetType, startPosition, isUsingController ? TargetMode.Controller : TargetMode.ClickMouseToTarget, ShouldCancelTargeting, null);
		Creature creature = Potion.Model.Owner.Creature;
		if (isUsingController && CombatManager.Instance.IsInProgress)
		{
			CombatState combatState = creature.CombatState;
			List<Creature> source = (targetType switch
			{
				TargetType.AnyEnemy => combatState.GetOpponentsOf(creature), 
				TargetType.AnyPlayer => combatState.GetTeammatesOf(creature), 
				_ => throw new ArgumentOutOfRangeException("targetType", targetType, null), 
			}).Where((Creature c) => c.IsAlive).ToList();
			NCombatRoom.Instance.RestrictControllerNavigation(source.Select((Creature c) => NCombatRoom.Instance.GetCreatureNode(c).Hitbox));
			NCombatRoom.Instance.GetCreatureNode(source.First()).Hitbox.TryGrabFocus();
		}
		else if (isUsingController && targetType == TargetType.AnyPlayer)
		{
			NMultiplayerPlayerStateContainer multiplayerPlayerContainer = NRun.Instance.GlobalUi.MultiplayerPlayerContainer;
			multiplayerPlayerContainer.FirstPlayerState?.Hitbox.TryGrabFocus();
			multiplayerPlayerContainer.LockNavigation();
		}
		bool flag = Potion.Model is FoulPotion;
		bool flag2 = creature.Player.RunState.CurrentRoom.RoomType == RoomType.Shop;
		bool isFoulPotionInShop = isUsingController && flag && flag2;
		if (isFoulPotionInShop)
		{
			NMerchantButton merchantButton = NMerchantRoom.Instance.MerchantButton;
			merchantButton.SetFocusMode(FocusModeEnum.All);
			merchantButton.TryGrabFocus();
		}
		try
		{
			Node node = await instance.SelectionFinished();
			NCombatRoom.Instance?.EnableControllerNavigation();
			NRun.Instance.GlobalUi.MultiplayerPlayerContainer.UnlockNavigation();
			if (node != null)
			{
				Creature creature2;
				if (!(node is NCreature nCreature))
				{
					if (!(node is NMultiplayerPlayerState nMultiplayerPlayerState))
					{
						if (!(node is NMerchantButton))
						{
							throw new ArgumentOutOfRangeException("targetNode", node, null);
						}
						creature2 = null;
					}
					else
					{
						creature2 = nMultiplayerPlayerState.Player.Creature;
					}
				}
				else
				{
					creature2 = nCreature.Entity;
				}
				Creature target = creature2;
				Potion.Model.EnqueueManualUse(target);
			}
		}
		finally
		{
			if (isFoulPotionInShop)
			{
				NMerchantButton merchantButton2 = NMerchantRoom.Instance.MerchantButton;
				merchantButton2.SetFocusMode(FocusModeEnum.None);
			}
		}
		this.TryGrabFocus();
	}

	private bool ShouldCancelTargeting()
	{
		if (Potion != null)
		{
			if (CombatManager.Instance.IsInProgress)
			{
				if (NOverlayStack.Instance.ScreenCount <= 0)
				{
					return NCapstoneContainer.Instance.InUse;
				}
				return true;
			}
			return false;
		}
		return true;
	}

	public async Task ShineOnStartOfCombat()
	{
		if (HasPotion)
		{
			Potion.DoBounce();
			await Cmd.Wait(0.25f);
			NDebugAudioManager.Instance?.Play(Rng.Chaotic.NextItem(TmpSfx.PotionSlosh), 0.3f, PitchVariance.Large);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(13);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "isUsable", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPress, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnRelease, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OpenPotionPopup, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AddPotion, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "potion", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.DisableUntilPotionRemoved, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CancelPotionUse, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RemoveUsedPotion, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DiscardPotion, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ShouldCancelTargeting, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NPotionHolder>(Create(VariantUtils.ConvertTo<bool>(in args[0])));
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
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
		if (method == MethodName.OnPress && args.Count == 0)
		{
			OnPress();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnRelease && args.Count == 0)
		{
			OnRelease();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenPotionPopup && args.Count == 0)
		{
			OpenPotionPopup();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AddPotion && args.Count == 1)
		{
			AddPotion(VariantUtils.ConvertTo<NPotion>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DisableUntilPotionRemoved && args.Count == 0)
		{
			DisableUntilPotionRemoved();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CancelPotionUse && args.Count == 0)
		{
			CancelPotionUse();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RemoveUsedPotion && args.Count == 0)
		{
			RemoveUsedPotion();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DiscardPotion && args.Count == 0)
		{
			DiscardPotion();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ShouldCancelTargeting && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(ShouldCancelTargeting());
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NPotionHolder>(Create(VariantUtils.ConvertTo<bool>(in args[0])));
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
		if (method == MethodName.OnFocus)
		{
			return true;
		}
		if (method == MethodName.OnUnfocus)
		{
			return true;
		}
		if (method == MethodName.OnPress)
		{
			return true;
		}
		if (method == MethodName.OnRelease)
		{
			return true;
		}
		if (method == MethodName.OpenPotionPopup)
		{
			return true;
		}
		if (method == MethodName.AddPotion)
		{
			return true;
		}
		if (method == MethodName.DisableUntilPotionRemoved)
		{
			return true;
		}
		if (method == MethodName.CancelPotionUse)
		{
			return true;
		}
		if (method == MethodName.RemoveUsedPotion)
		{
			return true;
		}
		if (method == MethodName.DiscardPotion)
		{
			return true;
		}
		if (method == MethodName.ShouldCancelTargeting)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.Potion)
		{
			Potion = VariantUtils.ConvertTo<NPotion>(in value);
			return true;
		}
		if (name == PropertyName._potionScale)
		{
			_potionScale = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._emptyIcon)
		{
			_emptyIcon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._popup)
		{
			_popup = VariantUtils.ConvertTo<NPotionPopup>(in value);
			return true;
		}
		if (name == PropertyName._potionTargeting)
		{
			_potionTargeting = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._isUsable)
		{
			_isUsable = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._emptyPotionTween)
		{
			_emptyPotionTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._hoverTween)
		{
			_hoverTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._disabledUntilPotionRemoved)
		{
			_disabledUntilPotionRemoved = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._isFocused)
		{
			_isFocused = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.Potion)
		{
			value = VariantUtils.CreateFrom<NPotion>(Potion);
			return true;
		}
		bool from;
		if (name == PropertyName.HasPotion)
		{
			from = HasPotion;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.IsPotionUsable)
		{
			from = IsPotionUsable;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName._potionScale)
		{
			value = VariantUtils.CreateFrom(in _potionScale);
			return true;
		}
		if (name == PropertyName._emptyIcon)
		{
			value = VariantUtils.CreateFrom(in _emptyIcon);
			return true;
		}
		if (name == PropertyName._popup)
		{
			value = VariantUtils.CreateFrom(in _popup);
			return true;
		}
		if (name == PropertyName._potionTargeting)
		{
			value = VariantUtils.CreateFrom(in _potionTargeting);
			return true;
		}
		if (name == PropertyName._isUsable)
		{
			value = VariantUtils.CreateFrom(in _isUsable);
			return true;
		}
		if (name == PropertyName._emptyPotionTween)
		{
			value = VariantUtils.CreateFrom(in _emptyPotionTween);
			return true;
		}
		if (name == PropertyName._hoverTween)
		{
			value = VariantUtils.CreateFrom(in _hoverTween);
			return true;
		}
		if (name == PropertyName._disabledUntilPotionRemoved)
		{
			value = VariantUtils.CreateFrom(in _disabledUntilPotionRemoved);
			return true;
		}
		if (name == PropertyName._isFocused)
		{
			value = VariantUtils.CreateFrom(in _isFocused);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._potionScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Potion, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._emptyIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.HasPotion, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._popup, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._potionTargeting, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isUsable, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._emptyPotionTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hoverTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._disabledUntilPotionRemoved, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsPotionUsable, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isFocused, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.Potion, Variant.From<NPotion>(Potion));
		info.AddProperty(PropertyName._potionScale, Variant.From(in _potionScale));
		info.AddProperty(PropertyName._emptyIcon, Variant.From(in _emptyIcon));
		info.AddProperty(PropertyName._popup, Variant.From(in _popup));
		info.AddProperty(PropertyName._potionTargeting, Variant.From(in _potionTargeting));
		info.AddProperty(PropertyName._isUsable, Variant.From(in _isUsable));
		info.AddProperty(PropertyName._emptyPotionTween, Variant.From(in _emptyPotionTween));
		info.AddProperty(PropertyName._hoverTween, Variant.From(in _hoverTween));
		info.AddProperty(PropertyName._disabledUntilPotionRemoved, Variant.From(in _disabledUntilPotionRemoved));
		info.AddProperty(PropertyName._isFocused, Variant.From(in _isFocused));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.Potion, out var value))
		{
			Potion = value.As<NPotion>();
		}
		if (info.TryGetProperty(PropertyName._potionScale, out var value2))
		{
			_potionScale = value2.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._emptyIcon, out var value3))
		{
			_emptyIcon = value3.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._popup, out var value4))
		{
			_popup = value4.As<NPotionPopup>();
		}
		if (info.TryGetProperty(PropertyName._potionTargeting, out var value5))
		{
			_potionTargeting = value5.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._isUsable, out var value6))
		{
			_isUsable = value6.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._emptyPotionTween, out var value7))
		{
			_emptyPotionTween = value7.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._hoverTween, out var value8))
		{
			_hoverTween = value8.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._disabledUntilPotionRemoved, out var value9))
		{
			_disabledUntilPotionRemoved = value9.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._isFocused, out var value10))
		{
			_isFocused = value10.As<bool>();
		}
	}
}
