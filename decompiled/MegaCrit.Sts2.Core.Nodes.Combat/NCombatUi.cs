using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Nodes.Combat;

[ScriptPath("res://src/Core/Nodes/Combat/NCombatUi.cs")]
public class NCombatUi : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName Deactivate = "Deactivate";

		public static readonly StringName DisconnectSignals = "DisconnectSignals";

		public static readonly StringName AddToPlayContainer = "AddToPlayContainer";

		public static readonly StringName AnimIn = "AnimIn";

		public static readonly StringName OnHandSelectModeEntered = "OnHandSelectModeEntered";

		public static readonly StringName OnHandSelectModeExited = "OnHandSelectModeExited";

		public static readonly StringName OnPeekButtonReady = "OnPeekButtonReady";

		public static readonly StringName OnPeekButtonToggled = "OnPeekButtonToggled";

		public static readonly StringName Enable = "Enable";

		public static readonly StringName Disable = "Disable";

		public new static readonly StringName _Input = "_Input";

		public static readonly StringName DebugHideCombatUi = "DebugHideCombatUi";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName EnergyCounterContainer = "EnergyCounterContainer";

		public static readonly StringName EndTurnButton = "EndTurnButton";

		public static readonly StringName PingButton = "PingButton";

		public static readonly StringName DrawPile = "DrawPile";

		public static readonly StringName DiscardPile = "DiscardPile";

		public static readonly StringName ExhaustPile = "ExhaustPile";

		public static readonly StringName Hand = "Hand";

		public static readonly StringName PlayContainer = "PlayContainer";

		public static readonly StringName PlayQueue = "PlayQueue";

		public static readonly StringName CardPreviewContainer = "CardPreviewContainer";

		public static readonly StringName MessyCardPreviewContainer = "MessyCardPreviewContainer";

		public static readonly StringName _starCounter = "_starCounter";

		public static readonly StringName _energyCounter = "_energyCounter";

		public static readonly StringName _combatPilesContainer = "_combatPilesContainer";

		public static readonly StringName _playContainerPeekModeTween = "_playContainerPeekModeTween";

		public static readonly StringName _originalHandChildIndex = "_originalHandChildIndex";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private NStarCounter _starCounter;

	private NEnergyCounter _energyCounter;

	private NCombatPilesContainer _combatPilesContainer;

	private readonly Dictionary<NCard, Vector2> _originalPlayContainerCardPositions = new Dictionary<NCard, Vector2>();

	private readonly Dictionary<NCard, Vector2> _originalPlayContainerCardScales = new Dictionary<NCard, Vector2>();

	private Tween? _playContainerPeekModeTween;

	private int _originalHandChildIndex;

	private CombatState _state;

	private static bool _isDebugSlowRewards;

	private static bool _isDebugHidden;

	private static bool _isDebugHidingHand;

	public Control EnergyCounterContainer { get; private set; }

	public NEndTurnButton EndTurnButton { get; private set; }

	private NPingButton PingButton { get; set; }

	public NDrawPileButton DrawPile => _combatPilesContainer.DrawPile;

	public NDiscardPileButton DiscardPile => _combatPilesContainer.DiscardPile;

	public NExhaustPileButton ExhaustPile => _combatPilesContainer.ExhaustPile;

	public NPlayerHand Hand { get; private set; }

	public Control PlayContainer { get; private set; }

	public NCardPlayQueue PlayQueue { get; private set; }

	public Control CardPreviewContainer { get; private set; }

	public NMessyCardPreviewContainer MessyCardPreviewContainer { get; private set; }

	private IEnumerable<NCard> PlayContainerCards => PlayContainer.GetChildren().OfType<NCard>();

	public static bool IsDebugHidingIntent { get; private set; }

	public static bool IsDebugHidingPlayContainer { get; private set; }

	public static bool IsDebugHidingHpBar { get; private set; }

	public static bool IsDebugHideTextVfx { get; private set; }

	public static bool IsDebugHideTargetingUi { get; private set; }

	public static bool IsDebugHideMpTargetingUi { get; private set; }

	public static bool IsDebugHideMpIntents { get; private set; }

	public event Action? DebugToggleIntent;

	public event Action? DebugToggleHpBar;

	public override void _Ready()
	{
		EnergyCounterContainer = GetNode<Control>("%EnergyCounterContainer");
		_starCounter = GetNode<NStarCounter>("%StarCounter");
		EndTurnButton = GetNode<NEndTurnButton>("%EndTurnButton");
		PingButton = GetNode<NPingButton>("%PingButton");
		_combatPilesContainer = GetNode<NCombatPilesContainer>("%CombatPileContainer");
		Hand = GetNode<NPlayerHand>("%Hand");
		PlayContainer = GetNode<Control>("%PlayContainer");
		PlayQueue = GetNode<NCardPlayQueue>("%PlayQueue");
		CardPreviewContainer = GetNode<Control>("%CardPreviewContainer");
		MessyCardPreviewContainer = GetNode<NMessyCardPreviewContainer>("%MessyCardPreviewContainer");
		if (!_isDebugHidden)
		{
			return;
		}
		foreach (Control item in GetChildren().OfType<Control>())
		{
			if (item != Hand)
			{
				item.Modulate = (_isDebugHidden ? Colors.Transparent : Colors.White);
			}
		}
	}

	public override void _ExitTree()
	{
		DisconnectSignals();
	}

	public void Activate(CombatState state)
	{
		CombatManager.Instance.CombatEnded += AnimOut;
		CombatManager.Instance.CombatEnded += PostCombatCleanUp;
		CombatManager.Instance.CombatWon += OnCombatWon;
		_state = state;
		Player me = LocalContext.GetMe(_state);
		_combatPilesContainer.Initialize(me);
		_starCounter.Initialize(me);
		EndTurnButton.Initialize(state);
		if (me.Character.ShouldAlwaysShowStarCounter)
		{
			EnergyCounterContainer.SetPosition(new Vector2(100f, 806f), keepOffsets: true);
		}
		_energyCounter = NEnergyCounter.Create(me);
		EnergyCounterContainer.AddChildSafely(_energyCounter);
		_starCounter.Reparent(_energyCounter);
		base.Visible = true;
		AnimIn();
	}

	public void Deactivate()
	{
		DisconnectSignals();
		base.Visible = false;
	}

	private void DisconnectSignals()
	{
		CombatManager.Instance.CombatEnded -= AnimOut;
		CombatManager.Instance.CombatEnded -= PostCombatCleanUp;
		CombatManager.Instance.CombatWon -= OnCombatWon;
	}

	public void AddToPlayContainer(NCard card)
	{
		card.GetParent()?.RemoveChildSafely(card);
		PlayContainer.AddChildSafely(card);
	}

	public NCard? GetCardFromPlayContainer(CardModel model)
	{
		return PlayContainerCards.FirstOrDefault((NCard n) => n.Model == model);
	}

	private void OnCombatWon(CombatRoom room)
	{
		if (room.Encounter.ShouldGiveRewards)
		{
			TaskHelper.RunSafely(ShowRewards(room));
		}
		else
		{
			TaskHelper.RunSafely(ProceedWithoutRewards());
		}
	}

	private async Task ProceedWithoutRewards()
	{
		await Cmd.Wait(1f);
		await RunManager.Instance.ProceedFromTerminalRewardsScreen();
	}

	private async Task ShowRewards(CombatRoom room)
	{
		float num = 0f;
		foreach (NCreature removingCreatureNode in NCombatRoom.Instance.RemovingCreatureNodes)
		{
			if (removingCreatureNode != null && removingCreatureNode.HasSpineAnimation && removingCreatureNode.IsPlayingDeathAnimation)
			{
				num = Math.Max(num, removingCreatureNode.GetCurrentAnimationTimeRemaining());
				continue;
			}
			MonsterModel monster = removingCreatureNode.Entity.Monster;
			if (monster != null && monster.HasDeathAnimLengthOverride)
			{
				num = Math.Max(num, removingCreatureNode.Entity.Monster.DeathAnimLengthOverride);
			}
		}
		if (_isDebugSlowRewards)
		{
			await Cmd.Wait(num + 3f);
		}
		else if (room.RoomType == RoomType.Boss)
		{
			await Cmd.CustomScaledWait(num * 0.5f, num + 1f);
		}
		else
		{
			await Cmd.CustomScaledWait(0.5f, num + 1f);
		}
		Player me = LocalContext.GetMe(_state);
		await RewardsCmd.OfferForRoomEnd(me, room);
	}

	private void AnimIn()
	{
		Hand.AnimIn();
		_energyCounter.AnimIn();
		_combatPilesContainer.AnimIn();
	}

	public void AnimOut(CombatRoom _)
	{
		Hand.AnimOut();
		PlayQueue.AnimOut();
		EndTurnButton.OnCombatEnded();
		PingButton.OnCombatEnded();
		_energyCounter.AnimOut();
		_combatPilesContainer.AnimOut();
	}

	private void PostCombatCleanUp(CombatRoom _)
	{
		Tween tween = CreateTween();
		tween.TweenProperty(PlayContainer, "modulate", Colors.Transparent, 0.25);
	}

	public void OnHandSelectModeEntered()
	{
		_originalHandChildIndex = Hand.GetIndex();
		Hand.MoveToFront();
		ActiveScreenContext.Instance.Update();
	}

	public void OnHandSelectModeExited()
	{
		MoveChild(Hand, _originalHandChildIndex);
		ActiveScreenContext.Instance.Update();
	}

	public void OnPeekButtonReady(NPeekButton peekButton)
	{
		peekButton.Connect(NPeekButton.SignalName.Toggled, Callable.From<NPeekButton>(OnPeekButtonToggled));
	}

	private void OnPeekButtonToggled(NPeekButton peekButton)
	{
		if (_playContainerPeekModeTween != null)
		{
			_playContainerPeekModeTween.Pause();
			_playContainerPeekModeTween.CustomStep(0.25);
			_playContainerPeekModeTween.Kill();
			_playContainerPeekModeTween = null;
		}
		Vector2 size = GetViewportRect().Size;
		if (peekButton.IsPeeking)
		{
			PlayQueue.Hide();
			foreach (NCard playContainerCard in PlayContainerCards)
			{
				_originalPlayContainerCardPositions[playContainerCard] = playContainerCard.Position;
				_originalPlayContainerCardScales[playContainerCard] = playContainerCard.Scale;
				Vector2 globalPosition = peekButton.CurrentCardMarker.GlobalPosition;
				Vector2 vector = playContainerCard.Scale * 0.5f;
				if (_playContainerPeekModeTween == null)
				{
					_playContainerPeekModeTween = CreateTween();
				}
				_playContainerPeekModeTween.TweenProperty(playContainerCard, "global_position", globalPosition, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
				_playContainerPeekModeTween.Parallel().TweenProperty(playContainerCard, "scale", vector, 0.25).SetEase(Tween.EaseType.Out)
					.SetTrans(Tween.TransitionType.Cubic);
			}
		}
		else
		{
			PlayQueue.Show();
			foreach (NCard playContainerCard2 in PlayContainerCards)
			{
				Vector2 value;
				Vector2 vector2 = (_originalPlayContainerCardPositions.TryGetValue(playContainerCard2, out value) ? value : (size * 0.5f));
				Vector2 value2;
				Vector2 vector3 = (_originalPlayContainerCardScales.TryGetValue(playContainerCard2, out value2) ? value2 : Vector2.One);
				if (_playContainerPeekModeTween == null)
				{
					_playContainerPeekModeTween = CreateTween();
				}
				_playContainerPeekModeTween.TweenProperty(playContainerCard2, "position", vector2, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
				_playContainerPeekModeTween.Parallel().TweenProperty(playContainerCard2, "scale", vector3, 0.25).SetEase(Tween.EaseType.Out)
					.SetTrans(Tween.TransitionType.Cubic);
			}
			_originalPlayContainerCardPositions.Clear();
			_originalPlayContainerCardScales.Clear();
		}
		ActiveScreenContext.Instance.Update();
	}

	public void Enable()
	{
		NPlayerHand hand = Hand;
		if (hand != null && hand.IsInCardSelection)
		{
			NPeekButton peekButton = hand.PeekButton;
			if (peekButton != null && !peekButton.IsPeeking)
			{
				_combatPilesContainer.Disable();
				goto IL_003c;
			}
		}
		_combatPilesContainer.Enable();
		goto IL_003c;
		IL_003c:
		if (_state.CurrentSide == CombatSide.Player)
		{
			EndTurnButton.RefreshEnabled();
		}
		if (_state.PlayerCreatures.Count > 1)
		{
			PingButton.RefreshEnabled();
		}
		NPlayerHand.Mode currentMode = Hand.CurrentMode;
		if ((uint)(currentMode - 2) <= 1u)
		{
			Hand.PeekButton.Enable();
		}
	}

	public void Disable()
	{
		_combatPilesContainer.Disable();
		EndTurnButton.RefreshEnabled();
		PingButton.RefreshEnabled();
		Hand.PeekButton.Disable();
		Hand.CancelAllCardPlay();
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (inputEvent.IsActionReleased(DebugHotkey.hideIntents))
		{
			IsDebugHidingIntent = !IsDebugHidingIntent;
			NGame.Instance.AddChildSafely(NFullscreenTextVfx.Create(IsDebugHidingIntent ? "Hide Intents" : "Show Intents"));
			this.DebugToggleIntent?.Invoke();
		}
		if (inputEvent.IsActionReleased(DebugHotkey.hideCombatUi))
		{
			_isDebugHidden = !_isDebugHidden;
			NGame.Instance.AddChildSafely(NFullscreenTextVfx.Create(_isDebugHidden ? "Hide Combat UI" : "Show Combat UI"));
			DebugHideCombatUi();
		}
		else if (inputEvent.IsActionReleased(DebugHotkey.hidePlayContainer))
		{
			IsDebugHidingPlayContainer = !IsDebugHidingPlayContainer;
			NGame.Instance.AddChildSafely(NFullscreenTextVfx.Create(IsDebugHidingPlayContainer ? "Hide Played Card" : "Show Played Card"));
			DebugHideCombatUi();
		}
		else if (inputEvent.IsActionReleased(DebugHotkey.hideHand))
		{
			_isDebugHidingHand = !_isDebugHidingHand;
			NGame.Instance.AddChildSafely(NFullscreenTextVfx.Create(_isDebugHidingHand ? "Hide Hand Cards" : "Show Hand Cards"));
			DebugHideCombatUi();
		}
		else if (inputEvent.IsActionReleased(DebugHotkey.hideHpBars))
		{
			IsDebugHidingHpBar = !IsDebugHidingHpBar;
			NGame.Instance.AddChildSafely(NFullscreenTextVfx.Create(IsDebugHidingHpBar ? "Hide HP Bars" : "Show HP Bars"));
			this.DebugToggleHpBar?.Invoke();
		}
		else if (inputEvent.IsActionReleased(DebugHotkey.hideTextVfx))
		{
			IsDebugHideTextVfx = !IsDebugHideTextVfx;
			NGame.Instance.AddChildSafely(NFullscreenTextVfx.Create(IsDebugHideTextVfx ? "Hide Text Vfx" : "Show Text Vfx"));
		}
		else if (inputEvent.IsActionReleased(DebugHotkey.hideTargetingUi))
		{
			IsDebugHideTargetingUi = !IsDebugHideTargetingUi;
			NGame.Instance.AddChildSafely(NFullscreenTextVfx.Create(IsDebugHideTargetingUi ? "Hide Targeting UI" : "Show Targeting UI"));
		}
		else if (inputEvent.IsActionReleased(DebugHotkey.slowRewards))
		{
			_isDebugSlowRewards = !_isDebugSlowRewards;
			NGame.Instance.AddChildSafely(NFullscreenTextVfx.Create(_isDebugSlowRewards ? "Slow Rewards Screens" : "Normal Rewards Screen"));
		}
		else if (inputEvent.IsActionReleased(DebugHotkey.hideMpTargeting))
		{
			IsDebugHideMpTargetingUi = !IsDebugHideMpTargetingUi;
			NGame.Instance.AddChildSafely(NFullscreenTextVfx.Create(IsDebugHideMpTargetingUi ? "Hide MP Targeting" : "Show MP Targeting"));
		}
		else if (inputEvent.IsActionReleased(DebugHotkey.hideMpIntents))
		{
			IsDebugHideMpIntents = !IsDebugHideMpIntents;
			NGame.Instance.AddChildSafely(NFullscreenTextVfx.Create(IsDebugHideMpIntents ? "Hide MP Intents" : "Show MP Intents"));
		}
	}

	private void DebugHideCombatUi()
	{
		foreach (Control item in GetChildren().OfType<Control>())
		{
			if (item == Hand)
			{
				item.Modulate = (_isDebugHidingHand ? Colors.Transparent : Colors.White);
			}
			else if (item.Name == PropertyName.PlayContainer)
			{
				item.Modulate = (IsDebugHidingPlayContainer ? Colors.Transparent : Colors.White);
			}
			else
			{
				item.Modulate = (_isDebugHidden ? Colors.Transparent : Colors.White);
			}
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(14);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Deactivate, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DisconnectSignals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AddToPlayContainer, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "card", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AnimIn, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnHandSelectModeEntered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnHandSelectModeExited, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPeekButtonReady, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "peekButton", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnPeekButtonToggled, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "peekButton", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.Enable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Disable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.DebugHideCombatUi, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.Deactivate && args.Count == 0)
		{
			Deactivate();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DisconnectSignals && args.Count == 0)
		{
			DisconnectSignals();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AddToPlayContainer && args.Count == 1)
		{
			AddToPlayContainer(VariantUtils.ConvertTo<NCard>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimIn && args.Count == 0)
		{
			AnimIn();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnHandSelectModeEntered && args.Count == 0)
		{
			OnHandSelectModeEntered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnHandSelectModeExited && args.Count == 0)
		{
			OnHandSelectModeExited();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPeekButtonReady && args.Count == 1)
		{
			OnPeekButtonReady(VariantUtils.ConvertTo<NPeekButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPeekButtonToggled && args.Count == 1)
		{
			OnPeekButtonToggled(VariantUtils.ConvertTo<NPeekButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Enable && args.Count == 0)
		{
			Enable();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Disable && args.Count == 0)
		{
			Disable();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Input && args.Count == 1)
		{
			_Input(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DebugHideCombatUi && args.Count == 0)
		{
			DebugHideCombatUi();
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
		if (method == MethodName.Deactivate)
		{
			return true;
		}
		if (method == MethodName.DisconnectSignals)
		{
			return true;
		}
		if (method == MethodName.AddToPlayContainer)
		{
			return true;
		}
		if (method == MethodName.AnimIn)
		{
			return true;
		}
		if (method == MethodName.OnHandSelectModeEntered)
		{
			return true;
		}
		if (method == MethodName.OnHandSelectModeExited)
		{
			return true;
		}
		if (method == MethodName.OnPeekButtonReady)
		{
			return true;
		}
		if (method == MethodName.OnPeekButtonToggled)
		{
			return true;
		}
		if (method == MethodName.Enable)
		{
			return true;
		}
		if (method == MethodName.Disable)
		{
			return true;
		}
		if (method == MethodName._Input)
		{
			return true;
		}
		if (method == MethodName.DebugHideCombatUi)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.EnergyCounterContainer)
		{
			EnergyCounterContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName.EndTurnButton)
		{
			EndTurnButton = VariantUtils.ConvertTo<NEndTurnButton>(in value);
			return true;
		}
		if (name == PropertyName.PingButton)
		{
			PingButton = VariantUtils.ConvertTo<NPingButton>(in value);
			return true;
		}
		if (name == PropertyName.Hand)
		{
			Hand = VariantUtils.ConvertTo<NPlayerHand>(in value);
			return true;
		}
		if (name == PropertyName.PlayContainer)
		{
			PlayContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName.PlayQueue)
		{
			PlayQueue = VariantUtils.ConvertTo<NCardPlayQueue>(in value);
			return true;
		}
		if (name == PropertyName.CardPreviewContainer)
		{
			CardPreviewContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName.MessyCardPreviewContainer)
		{
			MessyCardPreviewContainer = VariantUtils.ConvertTo<NMessyCardPreviewContainer>(in value);
			return true;
		}
		if (name == PropertyName._starCounter)
		{
			_starCounter = VariantUtils.ConvertTo<NStarCounter>(in value);
			return true;
		}
		if (name == PropertyName._energyCounter)
		{
			_energyCounter = VariantUtils.ConvertTo<NEnergyCounter>(in value);
			return true;
		}
		if (name == PropertyName._combatPilesContainer)
		{
			_combatPilesContainer = VariantUtils.ConvertTo<NCombatPilesContainer>(in value);
			return true;
		}
		if (name == PropertyName._playContainerPeekModeTween)
		{
			_playContainerPeekModeTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._originalHandChildIndex)
		{
			_originalHandChildIndex = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		Control from;
		if (name == PropertyName.EnergyCounterContainer)
		{
			from = EnergyCounterContainer;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.EndTurnButton)
		{
			value = VariantUtils.CreateFrom<NEndTurnButton>(EndTurnButton);
			return true;
		}
		if (name == PropertyName.PingButton)
		{
			value = VariantUtils.CreateFrom<NPingButton>(PingButton);
			return true;
		}
		if (name == PropertyName.DrawPile)
		{
			value = VariantUtils.CreateFrom<NDrawPileButton>(DrawPile);
			return true;
		}
		if (name == PropertyName.DiscardPile)
		{
			value = VariantUtils.CreateFrom<NDiscardPileButton>(DiscardPile);
			return true;
		}
		if (name == PropertyName.ExhaustPile)
		{
			value = VariantUtils.CreateFrom<NExhaustPileButton>(ExhaustPile);
			return true;
		}
		if (name == PropertyName.Hand)
		{
			value = VariantUtils.CreateFrom<NPlayerHand>(Hand);
			return true;
		}
		if (name == PropertyName.PlayContainer)
		{
			from = PlayContainer;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.PlayQueue)
		{
			value = VariantUtils.CreateFrom<NCardPlayQueue>(PlayQueue);
			return true;
		}
		if (name == PropertyName.CardPreviewContainer)
		{
			from = CardPreviewContainer;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.MessyCardPreviewContainer)
		{
			value = VariantUtils.CreateFrom<NMessyCardPreviewContainer>(MessyCardPreviewContainer);
			return true;
		}
		if (name == PropertyName._starCounter)
		{
			value = VariantUtils.CreateFrom(in _starCounter);
			return true;
		}
		if (name == PropertyName._energyCounter)
		{
			value = VariantUtils.CreateFrom(in _energyCounter);
			return true;
		}
		if (name == PropertyName._combatPilesContainer)
		{
			value = VariantUtils.CreateFrom(in _combatPilesContainer);
			return true;
		}
		if (name == PropertyName._playContainerPeekModeTween)
		{
			value = VariantUtils.CreateFrom(in _playContainerPeekModeTween);
			return true;
		}
		if (name == PropertyName._originalHandChildIndex)
		{
			value = VariantUtils.CreateFrom(in _originalHandChildIndex);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.EnergyCounterContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.EndTurnButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.PingButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._starCounter, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._energyCounter, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._combatPilesContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DrawPile, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DiscardPile, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.ExhaustPile, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Hand, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.PlayContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.PlayQueue, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.CardPreviewContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.MessyCardPreviewContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._playContainerPeekModeTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._originalHandChildIndex, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.EnergyCounterContainer, Variant.From<Control>(EnergyCounterContainer));
		info.AddProperty(PropertyName.EndTurnButton, Variant.From<NEndTurnButton>(EndTurnButton));
		info.AddProperty(PropertyName.PingButton, Variant.From<NPingButton>(PingButton));
		info.AddProperty(PropertyName.Hand, Variant.From<NPlayerHand>(Hand));
		info.AddProperty(PropertyName.PlayContainer, Variant.From<Control>(PlayContainer));
		info.AddProperty(PropertyName.PlayQueue, Variant.From<NCardPlayQueue>(PlayQueue));
		info.AddProperty(PropertyName.CardPreviewContainer, Variant.From<Control>(CardPreviewContainer));
		info.AddProperty(PropertyName.MessyCardPreviewContainer, Variant.From<NMessyCardPreviewContainer>(MessyCardPreviewContainer));
		info.AddProperty(PropertyName._starCounter, Variant.From(in _starCounter));
		info.AddProperty(PropertyName._energyCounter, Variant.From(in _energyCounter));
		info.AddProperty(PropertyName._combatPilesContainer, Variant.From(in _combatPilesContainer));
		info.AddProperty(PropertyName._playContainerPeekModeTween, Variant.From(in _playContainerPeekModeTween));
		info.AddProperty(PropertyName._originalHandChildIndex, Variant.From(in _originalHandChildIndex));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.EnergyCounterContainer, out var value))
		{
			EnergyCounterContainer = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName.EndTurnButton, out var value2))
		{
			EndTurnButton = value2.As<NEndTurnButton>();
		}
		if (info.TryGetProperty(PropertyName.PingButton, out var value3))
		{
			PingButton = value3.As<NPingButton>();
		}
		if (info.TryGetProperty(PropertyName.Hand, out var value4))
		{
			Hand = value4.As<NPlayerHand>();
		}
		if (info.TryGetProperty(PropertyName.PlayContainer, out var value5))
		{
			PlayContainer = value5.As<Control>();
		}
		if (info.TryGetProperty(PropertyName.PlayQueue, out var value6))
		{
			PlayQueue = value6.As<NCardPlayQueue>();
		}
		if (info.TryGetProperty(PropertyName.CardPreviewContainer, out var value7))
		{
			CardPreviewContainer = value7.As<Control>();
		}
		if (info.TryGetProperty(PropertyName.MessyCardPreviewContainer, out var value8))
		{
			MessyCardPreviewContainer = value8.As<NMessyCardPreviewContainer>();
		}
		if (info.TryGetProperty(PropertyName._starCounter, out var value9))
		{
			_starCounter = value9.As<NStarCounter>();
		}
		if (info.TryGetProperty(PropertyName._energyCounter, out var value10))
		{
			_energyCounter = value10.As<NEnergyCounter>();
		}
		if (info.TryGetProperty(PropertyName._combatPilesContainer, out var value11))
		{
			_combatPilesContainer = value11.As<NCombatPilesContainer>();
		}
		if (info.TryGetProperty(PropertyName._playContainerPeekModeTween, out var value12))
		{
			_playContainerPeekModeTween = value12.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._originalHandChildIndex, out var value13))
		{
			_originalHandChildIndex = value13.As<int>();
		}
	}
}
