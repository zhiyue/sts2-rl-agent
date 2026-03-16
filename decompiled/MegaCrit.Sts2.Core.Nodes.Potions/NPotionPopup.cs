using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Nodes.Potions;

[ScriptPath("res://src/Core/Nodes/Potions/NPotionPopup.cs")]
public class NPotionPopup : Control
{
	public new class MethodName : Control.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnUseButtonPressed = "OnUseButtonPressed";

		public static readonly StringName OnDiscardButtonPressed = "OnDiscardButtonPressed";

		public new static readonly StringName _Input = "_Input";

		public static readonly StringName Remove = "Remove";

		public static readonly StringName RefreshUseButton = "RefreshUseButton";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName IsUsable = "IsUsable";

		public static readonly StringName InACardSelectScreen = "InACardSelectScreen";

		public static readonly StringName _holder = "_holder";

		public static readonly StringName _popupContainer = "_popupContainer";

		public static readonly StringName _useButton = "_useButton";

		public static readonly StringName _discardButton = "_discardButton";

		public static readonly StringName _hoverTipBounds = "_hoverTipBounds";

		public static readonly StringName _tween = "_tween";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private NPotionHolder _holder;

	private Control _popupContainer;

	private NPotionPopupButton _useButton;

	private NPotionPopupButton _discardButton;

	private Control _hoverTipBounds;

	private Tween? _tween;

	private PotionModel? Potion => _holder.Potion?.Model;

	public bool IsUsable => _useButton.IsEnabled;

	private static string ScenePath => SceneHelper.GetScenePath("/potions/potion_popup");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	private bool InACardSelectScreen
	{
		get
		{
			if (!NPlayerHand.Instance.IsInCardSelection)
			{
				return NOverlayStack.Instance.Peek() is ICardSelector;
			}
			return true;
		}
	}

	public static NPotionPopup Create(NPotionHolder holder)
	{
		NPotionPopup nPotionPopup = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NPotionPopup>(PackedScene.GenEditState.Disabled);
		nPotionPopup._holder = holder;
		return nPotionPopup;
	}

	public override void _Ready()
	{
		base.GlobalPosition = _holder.GlobalPosition + Vector2.Down * _holder.Size.Y * 1.5f + Vector2.Right * _holder.Size * 0.5f + Vector2.Left * base.Size * 0.5f;
		_hoverTipBounds = GetNode<Control>("%HoverTipBounds");
		NHoverTipSet.CreateAndShow(_hoverTipBounds, _holder.Potion.Model.HoverTips, HoverTipAlignment.Right);
		NHoverTipSet.shouldBlockHoverTips = true;
		_popupContainer = GetNode<Control>("%Container");
		_useButton = GetNode<NPotionPopupButton>("%UseButton");
		_useButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnUseButtonPressed));
		_discardButton = GetNode<NPotionPopupButton>("%DiscardButton");
		_discardButton.SetLocKey("POTION_POPUP.discard");
		_discardButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnDiscardButtonPressed));
		_useButton.FocusNeighborLeft = _useButton.GetPath();
		_useButton.FocusNeighborRight = _useButton.GetPath();
		_useButton.FocusNeighborTop = _useButton.GetPath();
		_useButton.FocusNeighborBottom = _discardButton.GetPath();
		_discardButton.FocusNeighborLeft = _discardButton.GetPath();
		_discardButton.FocusNeighborRight = _discardButton.GetPath();
		_discardButton.FocusNeighborTop = _useButton.GetPath();
		_discardButton.FocusNeighborBottom = _discardButton.GetPath();
		if (Potion == null || Potion.IsQueued || Potion.Owner.Creature.IsDead)
		{
			_useButton.Disable();
			_discardButton.Disable();
		}
		else
		{
			switch (Potion.Usage)
			{
			case PotionUsage.None:
				throw new InvalidOperationException("No potions should have 'None' usage.");
			case PotionUsage.CombatOnly:
				CombatManager.Instance.StateTracker.CombatStateChanged += OnCombatStateChanged;
				CombatManager.Instance.TurnStarted += OnTurnStarted;
				CombatManager.Instance.PlayerEndedTurn += OnPlayerEndTurnStatusChanged;
				CombatManager.Instance.PlayerUnendedTurn += OnPlayerEndTurnStatusChanged;
				NOverlayStack.Instance.Changed += Remove;
				NCapstoneContainer.Instance.Changed += Remove;
				RefreshUseButton();
				break;
			case PotionUsage.AnyTime:
				_useButton.Enable();
				break;
			case PotionUsage.Automatic:
				_useButton.Disable();
				break;
			default:
				throw new ArgumentOutOfRangeException("Usage");
			}
			if (!Potion.Owner.CanRemovePotions)
			{
				_useButton.Disable();
				_discardButton.Disable();
			}
			if (!Potion.PassesCustomUsabilityCheck)
			{
				_useButton.Disable();
			}
			if (_useButton.IsEnabled)
			{
				_useButton.TryGrabFocus();
			}
			else if (_discardButton.IsEnabled)
			{
				_discardButton.TryGrabFocus();
			}
			else
			{
				this.TryGrabFocus();
			}
		}
		if (Potion == null)
		{
			_useButton.SetLocKey("POTION_POPUP.drink");
		}
		else
		{
			TargetType targetType = Potion.TargetType;
			bool flag = ((targetType == TargetType.AnyEnemy || targetType == TargetType.TargetedNoCreature) ? true : false);
			if (flag || Potion.CanThrowAtAlly())
			{
				_useButton.SetLocKey("POTION_POPUP.throw");
			}
			else
			{
				_useButton.SetLocKey("POTION_POPUP.drink");
			}
		}
		_tween?.Kill();
		base.Modulate = Colors.Transparent;
		_popupContainer.Position += Vector2.Up * 25f;
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(this, "modulate", Colors.White, 0.10000000149011612).SetTrans(Tween.TransitionType.Sine);
		_tween.TweenProperty(_popupContainer, "position:y", _popupContainer.Position.Y + 25f, 0.15000000596046448).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Sine);
	}

	private void OnUseButtonPressed(NButton _)
	{
		TaskHelper.RunSafely(UsePotion());
		Remove();
	}

	private async Task UsePotion()
	{
		if (Potion == null)
		{
			return;
		}
		PotionModel potion = Potion;
		potion.BeforeUse += DisableHolder;
		try
		{
			await _holder.UsePotion();
		}
		finally
		{
			potion.BeforeUse -= DisableHolder;
		}
		void DisableHolder()
		{
			_holder.DisableUntilPotionRemoved();
		}
	}

	private void OnDiscardButtonPressed(NButton _)
	{
		if (Potion != null)
		{
			Player owner = Potion.Owner;
			int num = owner.PotionSlots.IndexOf<PotionModel>(_holder.Potion.Model);
			if (num < 0)
			{
				throw new InvalidOperationException($"Tried to discard potion {_holder.Potion.Model} but it's not in the player's belt!");
			}
			_holder.DisableUntilPotionRemoved();
			DiscardPotionGameAction action = new DiscardPotionGameAction(owner, (uint)num);
			RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(action);
		}
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (inputEvent is InputEventMouseButton { ButtonIndex: var buttonIndex } inputEventMouseButton)
		{
			bool flag = (((ulong)(buttonIndex - 1) <= 1uL) ? true : false);
			if (flag && inputEventMouseButton.IsReleased())
			{
				Remove();
			}
		}
		else if (inputEvent.IsActionPressed(MegaInput.cancel))
		{
			Remove();
			_holder.TryGrabFocus();
		}
	}

	public void Remove()
	{
		NHoverTipSet.shouldBlockHoverTips = false;
		NHoverTipSet.Remove(_hoverTipBounds);
		CombatManager.Instance.StateTracker.CombatStateChanged -= OnCombatStateChanged;
		CombatManager.Instance.TurnStarted -= OnTurnStarted;
		CombatManager.Instance.PlayerEndedTurn -= OnPlayerEndTurnStatusChanged;
		CombatManager.Instance.PlayerUnendedTurn -= OnPlayerEndTurnStatusChanged;
		NOverlayStack.Instance.Changed -= Remove;
		NCapstoneContainer.Instance.Changed -= Remove;
		Callable.From(delegate
		{
			_useButton.Disable();
			_discardButton.Disable();
		}).CallDeferred();
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(this, "modulate", Colors.Transparent, 0.10000000149011612).SetTrans(Tween.TransitionType.Sine);
		_tween.TweenProperty(_popupContainer, "position:y", -25f, 0.20000000298023224).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
		_tween.Chain().TweenCallback(Callable.From(this.QueueFreeSafely));
	}

	private void OnTurnStarted(CombatState _)
	{
		RefreshUseButton();
	}

	private void OnPlayerEndTurnStatusChanged(Player _, bool __)
	{
		RefreshUseButton();
	}

	private void OnPlayerEndTurnStatusChanged(Player _)
	{
		RefreshUseButton();
	}

	private void OnCombatStateChanged(CombatState _)
	{
		RefreshUseButton();
	}

	private void RefreshUseButton()
	{
		Creature creature = Potion?.Owner.Creature;
		if (creature != null && CombatManager.Instance.IsInProgress && creature.CombatState.CurrentSide == creature.Side && !InACardSelectScreen && !CombatManager.Instance.PlayerActionsDisabled)
		{
			_useButton.Enable();
		}
		else
		{
			_useButton.Disable();
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(7);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "holder", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUseButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnDiscardButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.Remove, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshUseButton, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NPotionPopup>(Create(VariantUtils.ConvertTo<NPotionHolder>(in args[0])));
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnUseButtonPressed && args.Count == 1)
		{
			OnUseButtonPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnDiscardButtonPressed && args.Count == 1)
		{
			OnDiscardButtonPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Input && args.Count == 1)
		{
			_Input(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Remove && args.Count == 0)
		{
			Remove();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshUseButton && args.Count == 0)
		{
			RefreshUseButton();
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
			ret = VariantUtils.CreateFrom<NPotionPopup>(Create(VariantUtils.ConvertTo<NPotionHolder>(in args[0])));
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
		if (method == MethodName.OnUseButtonPressed)
		{
			return true;
		}
		if (method == MethodName.OnDiscardButtonPressed)
		{
			return true;
		}
		if (method == MethodName._Input)
		{
			return true;
		}
		if (method == MethodName.Remove)
		{
			return true;
		}
		if (method == MethodName.RefreshUseButton)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._holder)
		{
			_holder = VariantUtils.ConvertTo<NPotionHolder>(in value);
			return true;
		}
		if (name == PropertyName._popupContainer)
		{
			_popupContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._useButton)
		{
			_useButton = VariantUtils.ConvertTo<NPotionPopupButton>(in value);
			return true;
		}
		if (name == PropertyName._discardButton)
		{
			_discardButton = VariantUtils.ConvertTo<NPotionPopupButton>(in value);
			return true;
		}
		if (name == PropertyName._hoverTipBounds)
		{
			_hoverTipBounds = VariantUtils.ConvertTo<Control>(in value);
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
		bool from;
		if (name == PropertyName.IsUsable)
		{
			from = IsUsable;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.InACardSelectScreen)
		{
			from = InACardSelectScreen;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName._holder)
		{
			value = VariantUtils.CreateFrom(in _holder);
			return true;
		}
		if (name == PropertyName._popupContainer)
		{
			value = VariantUtils.CreateFrom(in _popupContainer);
			return true;
		}
		if (name == PropertyName._useButton)
		{
			value = VariantUtils.CreateFrom(in _useButton);
			return true;
		}
		if (name == PropertyName._discardButton)
		{
			value = VariantUtils.CreateFrom(in _discardButton);
			return true;
		}
		if (name == PropertyName._hoverTipBounds)
		{
			value = VariantUtils.CreateFrom(in _hoverTipBounds);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._holder, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._popupContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._useButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._discardButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hoverTipBounds, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsUsable, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.InACardSelectScreen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._holder, Variant.From(in _holder));
		info.AddProperty(PropertyName._popupContainer, Variant.From(in _popupContainer));
		info.AddProperty(PropertyName._useButton, Variant.From(in _useButton));
		info.AddProperty(PropertyName._discardButton, Variant.From(in _discardButton));
		info.AddProperty(PropertyName._hoverTipBounds, Variant.From(in _hoverTipBounds));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._holder, out var value))
		{
			_holder = value.As<NPotionHolder>();
		}
		if (info.TryGetProperty(PropertyName._popupContainer, out var value2))
		{
			_popupContainer = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._useButton, out var value3))
		{
			_useButton = value3.As<NPotionPopupButton>();
		}
		if (info.TryGetProperty(PropertyName._discardButton, out var value4))
		{
			_discardButton = value4.As<NPotionPopupButton>();
		}
		if (info.TryGetProperty(PropertyName._hoverTipBounds, out var value5))
		{
			_hoverTipBounds = value5.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value6))
		{
			_tween = value6.As<Tween>();
		}
	}
}
