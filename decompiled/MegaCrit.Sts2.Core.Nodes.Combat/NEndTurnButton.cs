using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Ftue;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Combat;

/// <summary>
/// End Turn button. Differs from other buttons as there are several flags which interact with each other and a pulsing vfx:
///    State.Shown: Indicates that this button is press-able. Additionally, flies on screen from the bottom with different behavior from isHidden.
///    State.Disabled: Grayed out and cannot be pressed.
///    State.Hidden: Grayed out and cannot be pressed. Additionally, flies off screen downward with different behavior from isHidden.
///    isShiny: Used when you have no valid cards to play so the button is visually more noticeable.
///    !isShiny: The opposite behavior of above.
/// </summary>
[ScriptPath("res://src/Core/Nodes/Combat/NEndTurnButton.cs")]
public class NEndTurnButton : NButton
{
	private enum State
	{
		Enabled,
		Disabled,
		Hidden
	}

	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : NButton.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the '_EnterTree' method.
		/// </summary>
		public new static readonly StringName _EnterTree = "_EnterTree";

		/// <summary>
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";

		/// <summary>
		/// Cached name for the 'StartOrStopPulseVfx' method.
		/// </summary>
		public static readonly StringName StartOrStopPulseVfx = "StartOrStopPulseVfx";

		/// <summary>
		/// Cached name for the 'GlowPulse' method.
		/// </summary>
		public static readonly StringName GlowPulse = "GlowPulse";

		/// <summary>
		/// Cached name for the 'OnRelease' method.
		/// </summary>
		public new static readonly StringName OnRelease = "OnRelease";

		/// <summary>
		/// Cached name for the 'CallReleaseLogic' method.
		/// </summary>
		public static readonly StringName CallReleaseLogic = "CallReleaseLogic";

		/// <summary>
		/// Cached name for the 'SecretEndTurnLogicViaFtue' method.
		/// </summary>
		public static readonly StringName SecretEndTurnLogicViaFtue = "SecretEndTurnLogicViaFtue";

		/// <summary>
		/// Cached name for the 'ShouldShowPlayableCardsFtue' method.
		/// </summary>
		public static readonly StringName ShouldShowPlayableCardsFtue = "ShouldShowPlayableCardsFtue";

		/// <summary>
		/// Cached name for the 'OnEnable' method.
		/// </summary>
		public new static readonly StringName OnEnable = "OnEnable";

		/// <summary>
		/// Cached name for the 'OnDisable' method.
		/// </summary>
		public new static readonly StringName OnDisable = "OnDisable";

		/// <summary>
		/// Cached name for the 'AnimOut' method.
		/// </summary>
		public static readonly StringName AnimOut = "AnimOut";

		/// <summary>
		/// Cached name for the 'AnimIn' method.
		/// </summary>
		public static readonly StringName AnimIn = "AnimIn";

		/// <summary>
		/// Cached name for the 'OnCombatEnded' method.
		/// </summary>
		public static readonly StringName OnCombatEnded = "OnCombatEnded";

		/// <summary>
		/// Cached name for the 'OnFocus' method.
		/// </summary>
		public new static readonly StringName OnFocus = "OnFocus";

		/// <summary>
		/// Cached name for the 'HasPlayableCard' method.
		/// </summary>
		public static readonly StringName HasPlayableCard = "HasPlayableCard";

		/// <summary>
		/// Cached name for the 'OnUnfocus' method.
		/// </summary>
		public new static readonly StringName OnUnfocus = "OnUnfocus";

		/// <summary>
		/// Cached name for the 'OnPress' method.
		/// </summary>
		public new static readonly StringName OnPress = "OnPress";

		/// <summary>
		/// Cached name for the 'UpdateShaderV' method.
		/// </summary>
		public static readonly StringName UpdateShaderV = "UpdateShaderV";

		/// <summary>
		/// Cached name for the 'SetState' method.
		/// </summary>
		public static readonly StringName SetState = "SetState";

		/// <summary>
		/// Cached name for the 'RefreshEnabled' method.
		/// </summary>
		public static readonly StringName RefreshEnabled = "RefreshEnabled";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : NButton.PropertyName
	{
		/// <summary>
		/// Cached name for the 'CanTurnBeEnded' property.
		/// </summary>
		public static readonly StringName CanTurnBeEnded = "CanTurnBeEnded";

		/// <summary>
		/// Cached name for the 'ShowPos' property.
		/// </summary>
		public static readonly StringName ShowPos = "ShowPos";

		/// <summary>
		/// Cached name for the 'HidePos' property.
		/// </summary>
		public static readonly StringName HidePos = "HidePos";

		/// <summary>
		/// Cached name for the 'Hotkeys' property.
		/// </summary>
		public new static readonly StringName Hotkeys = "Hotkeys";

		/// <summary>
		/// Cached name for the '_state' field.
		/// </summary>
		public static readonly StringName _state = "_state";

		/// <summary>
		/// Cached name for the '_isShiny' field.
		/// </summary>
		public static readonly StringName _isShiny = "_isShiny";

		/// <summary>
		/// Cached name for the '_visuals' field.
		/// </summary>
		public static readonly StringName _visuals = "_visuals";

		/// <summary>
		/// Cached name for the '_glowTexture' field.
		/// </summary>
		public static readonly StringName _glowTexture = "_glowTexture";

		/// <summary>
		/// Cached name for the '_normalTexture' field.
		/// </summary>
		public static readonly StringName _normalTexture = "_normalTexture";

		/// <summary>
		/// Cached name for the '_image' field.
		/// </summary>
		public static readonly StringName _image = "_image";

		/// <summary>
		/// Cached name for the '_hsv' field.
		/// </summary>
		public static readonly StringName _hsv = "_hsv";

		/// <summary>
		/// Cached name for the '_glow' field.
		/// </summary>
		public static readonly StringName _glow = "_glow";

		/// <summary>
		/// Cached name for the '_glowVfx' field.
		/// </summary>
		public static readonly StringName _glowVfx = "_glowVfx";

		/// <summary>
		/// Cached name for the '_label' field.
		/// </summary>
		public static readonly StringName _label = "_label";

		/// <summary>
		/// Cached name for the '_combatUi' field.
		/// </summary>
		public static readonly StringName _combatUi = "_combatUi";

		/// <summary>
		/// Cached name for the '_viewport' field.
		/// </summary>
		public static readonly StringName _viewport = "_viewport";

		/// <summary>
		/// Cached name for the '_playerIconContainer' field.
		/// </summary>
		public static readonly StringName _playerIconContainer = "_playerIconContainer";

		/// <summary>
		/// Cached name for the '_longPressBar' field.
		/// </summary>
		public static readonly StringName _longPressBar = "_longPressBar";

		/// <summary>
		/// Cached name for the '_pulseTimer' field.
		/// </summary>
		public static readonly StringName _pulseTimer = "_pulseTimer";

		/// <summary>
		/// Cached name for the '_positionTween' field.
		/// </summary>
		public static readonly StringName _positionTween = "_positionTween";

		/// <summary>
		/// Cached name for the '_hoverTween' field.
		/// </summary>
		public static readonly StringName _hoverTween = "_hoverTween";

		/// <summary>
		/// Cached name for the '_glowVfxTween' field.
		/// </summary>
		public static readonly StringName _glowVfxTween = "_glowVfxTween";

		/// <summary>
		/// Cached name for the '_glowEnableTween' field.
		/// </summary>
		public static readonly StringName _glowEnableTween = "_glowEnableTween";

		/// <summary>
		/// Cached name for the '_endTurnWithNoPlayableCardsCount' field.
		/// </summary>
		public static readonly StringName _endTurnWithNoPlayableCardsCount = "_endTurnWithNoPlayableCardsCount";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NButton.SignalName
	{
	}

	private static readonly StringName _v = new StringName("v");

	private const float _flyInOutDuration = 0.5f;

	private static readonly LocString _endTurnLoc = new LocString("gameplay_ui", "END_TURN_BUTTON");

	private CombatState? _combatState;

	private CardPile? _playerHand;

	private State _state = State.Hidden;

	private bool _isShiny;

	private Control _visuals;

	private Texture2D _glowTexture;

	private Texture2D _normalTexture;

	private TextureRect _image;

	private ShaderMaterial _hsv;

	private Control _glow;

	private Control _glowVfx;

	private MegaLabel _label;

	private NCombatUi _combatUi;

	private Viewport _viewport;

	private NMultiplayerVoteContainer _playerIconContainer;

	private NEndTurnLongPressBar _longPressBar;

	private float _pulseTimer = 1f;

	private static readonly Vector2 _hoverTipOffset = new Vector2(-76f, -302f);

	private static readonly Vector2 _showPosRatio = new Vector2(1604f, 846f) / NGame.devResolution;

	private static readonly Vector2 _hidePosRatio = _showPosRatio + new Vector2(0f, 250f) / NGame.devResolution;

	private Tween? _positionTween;

	private Tween? _hoverTween;

	private Tween? _glowVfxTween;

	private Tween? _glowEnableTween;

	private const int _ftueDisableEndTurnCount = 3;

	private int _endTurnWithNoPlayableCardsCount;

	private static string EndTurnButtonPath => "res://images/packed/combat_ui/end_turn_button.png";

	private static string EndTurnButtonGlowPath => "res://images/packed/combat_ui/end_turn_button_glow.png";

	public static IEnumerable<string> AssetPaths
	{
		get
		{
			List<string> list = new List<string>();
			list.Add(EndTurnButtonPath);
			list.Add(EndTurnButtonGlowPath);
			list.AddRange(NMultiplayerVoteContainer.AssetPaths);
			return new _003C_003Ez__ReadOnlyList<string>(list);
		}
	}

	private bool CanTurnBeEnded
	{
		get
		{
			if (!NCombatRoom.Instance.Ui.Hand.InCardPlay)
			{
				return NCombatRoom.Instance.Ui.Hand.CurrentMode == NPlayerHand.Mode.Play;
			}
			return false;
		}
	}

	private Vector2 ShowPos => _showPosRatio * _viewport.GetVisibleRect().Size;

	private Vector2 HidePos => _hidePosRatio * _viewport.GetVisibleRect().Size;

	protected override string[] Hotkeys => new string[1] { MegaInput.accept };

	public override void _Ready()
	{
		ConnectSignals();
		_visuals = GetNode<Control>("Visuals");
		_image = GetNode<TextureRect>("Visuals/Image");
		_hsv = (ShaderMaterial)_image.Material;
		_glow = GetNode<TextureRect>("Visuals/Glow");
		_glowVfx = GetNode<TextureRect>("Visuals/GlowVfx");
		_label = GetNode<MegaLabel>("Visuals/Label");
		_playerIconContainer = GetNode<NMultiplayerVoteContainer>("PlayerIconContainer");
		_longPressBar = GetNode<NEndTurnLongPressBar>("%Bar");
		_longPressBar.Init(this);
		Disable();
		_combatUi = GetParent<NCombatUi>();
		_viewport = GetViewport();
		_glowTexture = PreloadManager.Cache.GetCompressedTexture2D(EndTurnButtonGlowPath);
		_normalTexture = PreloadManager.Cache.GetCompressedTexture2D(EndTurnButtonPath);
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		CombatManager.Instance.TurnStarted += OnTurnStarted;
		CombatManager.Instance.AboutToSwitchToEnemyTurn += OnAboutToSwitchToEnemyTurn;
		CombatManager.Instance.PlayerEndedTurn += AfterPlayerEndedTurn;
		CombatManager.Instance.PlayerUnendedTurn += AfterPlayerUnendedTurn;
		CombatManager.Instance.StateTracker.CombatStateChanged += OnCombatStateChanged;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		_positionTween?.Kill();
		_hoverTween?.Kill();
		CombatManager.Instance.TurnStarted -= OnTurnStarted;
		CombatManager.Instance.AboutToSwitchToEnemyTurn -= OnAboutToSwitchToEnemyTurn;
		CombatManager.Instance.PlayerEndedTurn -= AfterPlayerEndedTurn;
		CombatManager.Instance.PlayerUnendedTurn -= AfterPlayerUnendedTurn;
		CombatManager.Instance.StateTracker.CombatStateChanged -= OnCombatStateChanged;
	}

	public void Initialize(CombatState state)
	{
		_combatState = state;
		_playerHand = PileType.Hand.GetPile(LocalContext.GetMe(_combatState));
		_playerIconContainer.Initialize(ShouldDisplayPlayerIcon, _combatState.Players);
	}

	/// <summary>
	/// Returns true when the player's icon should be displayed above the end turn button, indicating readiness.
	/// The icons are only refreshed when _playerIconContainer.RefreshPlayerVotes is called.
	/// </summary>
	private bool ShouldDisplayPlayerIcon(Player player)
	{
		return CombatManager.Instance.IsPlayerReadyToEndTurn(player);
	}

	private bool PlayerCanTakeAction(Player player)
	{
		if (player.Creature.IsAlive)
		{
			if (CombatManager.Instance.PlayersTakingExtraTurn.Count != 0)
			{
				return CombatManager.Instance.PlayersTakingExtraTurn.Contains(player);
			}
			return true;
		}
		return false;
	}

	private void AfterPlayerEndedTurn(Player player, bool canBackOut)
	{
		_playerIconContainer.RefreshPlayerVotes();
		if (LocalContext.IsMe(player))
		{
			StartOrStopPulseVfx();
			Player me = LocalContext.GetMe(player.Creature.CombatState);
			if (!CombatManager.Instance.AllPlayersReadyToEndTurn() && PlayerCanTakeAction(me) && canBackOut)
			{
				SetState(State.Enabled);
				_label.SetTextAutoSize(new LocString("gameplay_ui", "UNDO_END_TURN_BUTTON").GetFormattedText());
			}
			else
			{
				SetState(State.Disabled);
			}
		}
		if (CombatManager.Instance.AllPlayersReadyToEndTurn())
		{
			SetState(State.Disabled);
		}
	}

	private void AfterPlayerUnendedTurn(Player player)
	{
		_playerIconContainer.RefreshPlayerVotes();
		if (LocalContext.IsMe(player) && PlayerCanTakeAction(player))
		{
			SetState(State.Enabled);
			_endTurnLoc.Add("turnNumber", player.PlayerCombatState.TurnNumber);
			_label.SetTextAutoSize(_endTurnLoc.GetFormattedText());
			StartOrStopPulseVfx();
		}
	}

	private void OnAboutToSwitchToEnemyTurn(CombatState _)
	{
		SetState(State.Hidden);
	}

	private void OnTurnStarted(CombatState state)
	{
		if (state.CurrentSide == CombatSide.Player && CombatManager.Instance.IsInProgress)
		{
			_playerIconContainer.RefreshPlayerVotes(animate: false);
			Player me = LocalContext.GetMe(state);
			_endTurnLoc.Add("turnNumber", me.PlayerCombatState.TurnNumber);
			_label.SetTextAutoSize(_endTurnLoc.GetFormattedText());
			if (PlayerCanTakeAction(me))
			{
				SetState(State.Enabled);
				return;
			}
			AnimIn();
			SetState(State.Disabled);
		}
	}

	private void OnCombatStateChanged(CombatState combatState)
	{
		StartOrStopPulseVfx();
	}

	private void StartOrStopPulseVfx()
	{
		bool flag = !HasPlayableCard() && !CombatManager.Instance.IsPlayerReadyToEndTurn(LocalContext.GetMe(_combatState)) && _state == State.Enabled;
		if (_isShiny)
		{
			if (!flag)
			{
				_isShiny = false;
				_glowEnableTween?.Kill();
				_glowEnableTween = CreateTween().SetParallel();
				_glowEnableTween.TweenProperty(_glow, "modulate:a", 0f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
				_glowVfxTween?.Kill();
				_glowVfxTween = CreateTween();
				_glowVfxTween.TweenProperty(_glowVfx, "modulate:a", 0f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
			}
		}
		else if (flag)
		{
			_isShiny = true;
			_glowVfxTween?.Kill();
			GlowPulse();
			_glowEnableTween?.Kill();
			_glowEnableTween = CreateTween().SetParallel();
			_glowEnableTween.TweenProperty(_glow, "modulate:a", 0.75f, 0.8).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
			_glowEnableTween.TweenProperty(_glow, "scale", Vector2.One * 0.5f, 0.8).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back)
				.From(Vector2.One * 0.45f);
		}
	}

	private void GlowPulse()
	{
		_glowVfxTween = CreateTween().SetParallel().SetLoops();
		_glowVfxTween.TweenProperty(_glowVfx, "scale", Vector2.One * 0.7f, 1.5).From(Vector2.One * 0.5f).SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Quart);
		_glowVfxTween.TweenProperty(_glowVfx, "modulate:a", 0f, 1.5).From(0.4f);
	}

	/// <summary>
	/// Things which occur when the EndTurn button is pressed
	/// </summary>
	protected override void OnRelease()
	{
		if (!ShouldShowPlayableCardsFtue())
		{
			if (SaveManager.Instance.PrefsSave.IsLongPressEnabled)
			{
				_longPressBar.CancelPress();
			}
			else
			{
				CallReleaseLogic();
			}
		}
	}

	/// <summary>
	/// Helper because we can call the End Turn button in numerous ways (controller, mouse, long press mouse, etc)
	/// </summary>
	public void CallReleaseLogic()
	{
		if (CanTurnBeEnded)
		{
			_glowEnableTween?.Kill();
			_glowEnableTween = CreateTween().SetParallel();
			_glowEnableTween.TweenProperty(_glow, "modulate:a", 0f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
			Player me = LocalContext.GetMe(_combatState);
			int turnNumber = me.PlayerCombatState.TurnNumber;
			if (!CombatManager.Instance.IsPlayerReadyToEndTurn(me))
			{
				SetState(State.Disabled);
				RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(new EndPlayerTurnAction(me, turnNumber));
			}
			else
			{
				SetState(State.Disabled);
				RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(new UndoEndPlayerTurnAction(me, turnNumber));
			}
		}
	}

	/// <summary>
	/// Don't call me &gt;:(
	/// </summary>
	public void SecretEndTurnLogicViaFtue()
	{
		_glowEnableTween?.Kill();
		_glowEnableTween = CreateTween().SetParallel();
		_glowEnableTween.TweenProperty(_glow, "modulate:a", 0f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		Player me = LocalContext.GetMe(_combatState);
		RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(new EndPlayerTurnAction(me, me.PlayerCombatState.TurnNumber));
	}

	private bool ShouldShowPlayableCardsFtue()
	{
		if (SaveManager.Instance.SeenFtue("can_play_cards_ftue"))
		{
			return false;
		}
		bool flag = LocalContext.GetMe(_combatState).PlayerCombatState.HasCardsToPlay();
		if (flag)
		{
			NModalContainer.Instance.Add(NCanPlayCardsFtue.Create());
			SaveManager.Instance.MarkFtueAsComplete("can_play_cards_ftue");
		}
		else
		{
			_endTurnWithNoPlayableCardsCount++;
			if (_endTurnWithNoPlayableCardsCount == 3)
			{
				Log.Info($"Ended {3} turns without cards left to play. Good job! Disabling can_play_cards ftue.");
				SaveManager.Instance.MarkFtueAsComplete("can_play_cards_ftue");
			}
		}
		return flag;
	}

	/// <summary>
	/// Called at the start of player turn, and when the button re-enables itself if not everyone in multiplayer is ready.
	/// </summary>
	protected override void OnEnable()
	{
		base.OnEnable();
		_hoverTween?.FastForwardToCompletion();
		_image.Texture = _normalTexture;
		_image.Modulate = Colors.White;
		_label.Modulate = StsColors.cream;
	}

	/// <summary>
	/// Called when the player presses End Turn or combat ends.
	/// </summary>
	protected override void OnDisable()
	{
		base.OnDisable();
		NHoverTipSet.Remove(this);
		_hoverTween?.FastForwardToCompletion();
		_image.Modulate = StsColors.gray;
		_label.Modulate = StsColors.gray;
		StartOrStopPulseVfx();
	}

	/// <summary>
	/// Animates the button out off the screen and disables the button if it is currently enabled.
	/// </summary>
	private void AnimOut()
	{
		_hoverTween?.Kill();
		_positionTween?.Kill();
		_positionTween = CreateTween();
		_positionTween.TweenProperty(this, "position", HidePos, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	/// <summary>
	/// Animates the button in from off the screen.
	/// </summary>
	private void AnimIn()
	{
		_positionTween?.Kill();
		_positionTween = CreateTween();
		_positionTween.TweenProperty(this, "position", ShowPos, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
	}

	public void OnCombatEnded()
	{
		SetState(State.Hidden);
	}

	protected override void OnFocus()
	{
		base.OnFocus();
		_hoverTween?.Kill();
		_hsv.SetShaderParameter(_v, 1.5);
		_visuals.Position = new Vector2(0f, -2f);
		Player me = LocalContext.GetMe(_combatState);
		if (!CombatManager.Instance.IsPlayerReadyToEndTurn(me))
		{
			_label.Modulate = (me.PlayerCombatState.HasCardsToPlay() ? StsColors.red : Colors.Cyan);
			_combatUi.Hand.FlashPlayableHolders();
			LocString locString = new LocString("static_hover_tips", "END_TURN.title");
			locString.Add("Hotkey", NInputManager.Instance.GetShortcutKey(MegaInput.accept).ToString());
			HoverTip hoverTip = new HoverTip(locString, new LocString("static_hover_tips", "END_TURN.description"));
			NHoverTipSet.CreateAndShow(this, hoverTip)?.SetGlobalPosition(base.GlobalPosition + _hoverTipOffset);
		}
		else
		{
			_label.Modulate = StsColors.cream;
		}
	}

	private bool HasPlayableCard()
	{
		if (_playerHand == null)
		{
			return false;
		}
		foreach (CardModel card in _playerHand.Cards)
		{
			if (card.CanPlay())
			{
				return true;
			}
		}
		return false;
	}

	protected override void OnUnfocus()
	{
		if (SaveManager.Instance.PrefsSave.IsLongPressEnabled)
		{
			_longPressBar.CancelPress();
		}
		NHoverTipSet.Remove(this);
		_hoverTween?.Kill();
		_hoverTween = CreateTween().SetParallel();
		_hoverTween.TweenMethod(Callable.From<float>(UpdateShaderV), _hsv.GetShaderParameter(_v), 1f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_hoverTween.TweenProperty(_visuals, "position", Vector2.Zero, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_hoverTween.TweenProperty(_label, "modulate", base.IsEnabled ? StsColors.cream : StsColors.gray, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	protected override void OnPress()
	{
		if (CanTurnBeEnded)
		{
			if (SaveManager.Instance.PrefsSave.IsLongPressEnabled)
			{
				_longPressBar.StartPress();
			}
			_hoverTween?.Kill();
			_hoverTween = CreateTween().SetParallel();
			_hoverTween.TweenMethod(Callable.From<float>(UpdateShaderV), _hsv.GetShaderParameter(_v), 1f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
			_hoverTween.TweenProperty(_visuals, "position", new Vector2(0f, 8f), 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
			_hoverTween.TweenProperty(_label, "modulate", Colors.DarkGray, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		}
	}

	private void UpdateShaderV(float value)
	{
		_hsv.SetShaderParameter(_v, value);
	}

	/// <summary>
	/// Set the state of the button.
	/// Use this instead of calling Enable or Disable directly, as the enabled/disabled state of the button can also be
	/// controlled by the state of the UI above the button (see RefreshEnabled).
	/// </summary>
	private void SetState(State newState)
	{
		if (_state != newState)
		{
			if (newState == State.Hidden)
			{
				AnimOut();
			}
			if (newState == State.Enabled && _state == State.Hidden)
			{
				AnimIn();
			}
			_state = newState;
			RefreshEnabled();
		}
	}

	/// <summary>
	/// Refreshes whether the button is enabled or not. Should be called when a screen appears/disappears.
	/// Use this instead of calling Enable or Disable directly! The enabled/disabled state of the button should be
	/// controlled by the button itself.
	/// </summary>
	public void RefreshEnabled()
	{
		bool flag = NCombatRoom.Instance == null || NCombatRoom.Instance.Mode != CombatRoomMode.ActiveCombat || !ActiveScreenContext.Instance.IsCurrent(NCombatRoom.Instance) || NCombatRoom.Instance.Ui.Hand.IsInCardSelection;
		if (_state == State.Enabled && !flag)
		{
			Enable();
		}
		else
		{
			Disable();
		}
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(21);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.StartOrStopPulseVfx, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GlowPulse, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnRelease, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CallReleaseLogic, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SecretEndTurnLogicViaFtue, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ShouldShowPlayableCardsFtue, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnEnable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnDisable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimOut, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimIn, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnCombatEnded, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.HasPlayableCard, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPress, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateShaderV, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "value", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "newState", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RefreshEnabled, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._EnterTree && args.Count == 0)
		{
			_EnterTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartOrStopPulseVfx && args.Count == 0)
		{
			StartOrStopPulseVfx();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GlowPulse && args.Count == 0)
		{
			GlowPulse();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnRelease && args.Count == 0)
		{
			OnRelease();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CallReleaseLogic && args.Count == 0)
		{
			CallReleaseLogic();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SecretEndTurnLogicViaFtue && args.Count == 0)
		{
			SecretEndTurnLogicViaFtue();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ShouldShowPlayableCardsFtue && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(ShouldShowPlayableCardsFtue());
			return true;
		}
		if (method == MethodName.OnEnable && args.Count == 0)
		{
			OnEnable();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnDisable && args.Count == 0)
		{
			OnDisable();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimOut && args.Count == 0)
		{
			AnimOut();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimIn && args.Count == 0)
		{
			AnimIn();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnCombatEnded && args.Count == 0)
		{
			OnCombatEnded();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnFocus && args.Count == 0)
		{
			OnFocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HasPlayableCard && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(HasPlayableCard());
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
		if (method == MethodName.UpdateShaderV && args.Count == 1)
		{
			UpdateShaderV(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetState && args.Count == 1)
		{
			SetState(VariantUtils.ConvertTo<State>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshEnabled && args.Count == 0)
		{
			RefreshEnabled();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.StartOrStopPulseVfx)
		{
			return true;
		}
		if (method == MethodName.GlowPulse)
		{
			return true;
		}
		if (method == MethodName.OnRelease)
		{
			return true;
		}
		if (method == MethodName.CallReleaseLogic)
		{
			return true;
		}
		if (method == MethodName.SecretEndTurnLogicViaFtue)
		{
			return true;
		}
		if (method == MethodName.ShouldShowPlayableCardsFtue)
		{
			return true;
		}
		if (method == MethodName.OnEnable)
		{
			return true;
		}
		if (method == MethodName.OnDisable)
		{
			return true;
		}
		if (method == MethodName.AnimOut)
		{
			return true;
		}
		if (method == MethodName.AnimIn)
		{
			return true;
		}
		if (method == MethodName.OnCombatEnded)
		{
			return true;
		}
		if (method == MethodName.OnFocus)
		{
			return true;
		}
		if (method == MethodName.HasPlayableCard)
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
		if (method == MethodName.UpdateShaderV)
		{
			return true;
		}
		if (method == MethodName.SetState)
		{
			return true;
		}
		if (method == MethodName.RefreshEnabled)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._state)
		{
			_state = VariantUtils.ConvertTo<State>(in value);
			return true;
		}
		if (name == PropertyName._isShiny)
		{
			_isShiny = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._visuals)
		{
			_visuals = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._glowTexture)
		{
			_glowTexture = VariantUtils.ConvertTo<Texture2D>(in value);
			return true;
		}
		if (name == PropertyName._normalTexture)
		{
			_normalTexture = VariantUtils.ConvertTo<Texture2D>(in value);
			return true;
		}
		if (name == PropertyName._image)
		{
			_image = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._hsv)
		{
			_hsv = VariantUtils.ConvertTo<ShaderMaterial>(in value);
			return true;
		}
		if (name == PropertyName._glow)
		{
			_glow = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._glowVfx)
		{
			_glowVfx = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._label)
		{
			_label = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._combatUi)
		{
			_combatUi = VariantUtils.ConvertTo<NCombatUi>(in value);
			return true;
		}
		if (name == PropertyName._viewport)
		{
			_viewport = VariantUtils.ConvertTo<Viewport>(in value);
			return true;
		}
		if (name == PropertyName._playerIconContainer)
		{
			_playerIconContainer = VariantUtils.ConvertTo<NMultiplayerVoteContainer>(in value);
			return true;
		}
		if (name == PropertyName._longPressBar)
		{
			_longPressBar = VariantUtils.ConvertTo<NEndTurnLongPressBar>(in value);
			return true;
		}
		if (name == PropertyName._pulseTimer)
		{
			_pulseTimer = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._positionTween)
		{
			_positionTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._hoverTween)
		{
			_hoverTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._glowVfxTween)
		{
			_glowVfxTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._glowEnableTween)
		{
			_glowEnableTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._endTurnWithNoPlayableCardsCount)
		{
			_endTurnWithNoPlayableCardsCount = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.CanTurnBeEnded)
		{
			value = VariantUtils.CreateFrom<bool>(CanTurnBeEnded);
			return true;
		}
		Vector2 from;
		if (name == PropertyName.ShowPos)
		{
			from = ShowPos;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.HidePos)
		{
			from = HidePos;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.Hotkeys)
		{
			value = VariantUtils.CreateFrom<string[]>(Hotkeys);
			return true;
		}
		if (name == PropertyName._state)
		{
			value = VariantUtils.CreateFrom(in _state);
			return true;
		}
		if (name == PropertyName._isShiny)
		{
			value = VariantUtils.CreateFrom(in _isShiny);
			return true;
		}
		if (name == PropertyName._visuals)
		{
			value = VariantUtils.CreateFrom(in _visuals);
			return true;
		}
		if (name == PropertyName._glowTexture)
		{
			value = VariantUtils.CreateFrom(in _glowTexture);
			return true;
		}
		if (name == PropertyName._normalTexture)
		{
			value = VariantUtils.CreateFrom(in _normalTexture);
			return true;
		}
		if (name == PropertyName._image)
		{
			value = VariantUtils.CreateFrom(in _image);
			return true;
		}
		if (name == PropertyName._hsv)
		{
			value = VariantUtils.CreateFrom(in _hsv);
			return true;
		}
		if (name == PropertyName._glow)
		{
			value = VariantUtils.CreateFrom(in _glow);
			return true;
		}
		if (name == PropertyName._glowVfx)
		{
			value = VariantUtils.CreateFrom(in _glowVfx);
			return true;
		}
		if (name == PropertyName._label)
		{
			value = VariantUtils.CreateFrom(in _label);
			return true;
		}
		if (name == PropertyName._combatUi)
		{
			value = VariantUtils.CreateFrom(in _combatUi);
			return true;
		}
		if (name == PropertyName._viewport)
		{
			value = VariantUtils.CreateFrom(in _viewport);
			return true;
		}
		if (name == PropertyName._playerIconContainer)
		{
			value = VariantUtils.CreateFrom(in _playerIconContainer);
			return true;
		}
		if (name == PropertyName._longPressBar)
		{
			value = VariantUtils.CreateFrom(in _longPressBar);
			return true;
		}
		if (name == PropertyName._pulseTimer)
		{
			value = VariantUtils.CreateFrom(in _pulseTimer);
			return true;
		}
		if (name == PropertyName._positionTween)
		{
			value = VariantUtils.CreateFrom(in _positionTween);
			return true;
		}
		if (name == PropertyName._hoverTween)
		{
			value = VariantUtils.CreateFrom(in _hoverTween);
			return true;
		}
		if (name == PropertyName._glowVfxTween)
		{
			value = VariantUtils.CreateFrom(in _glowVfxTween);
			return true;
		}
		if (name == PropertyName._glowEnableTween)
		{
			value = VariantUtils.CreateFrom(in _glowEnableTween);
			return true;
		}
		if (name == PropertyName._endTurnWithNoPlayableCardsCount)
		{
			value = VariantUtils.CreateFrom(in _endTurnWithNoPlayableCardsCount);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	/// <summary>
	/// Get the property information for all the properties declared in this class.
	/// This method is used by Godot to register the available properties in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.CanTurnBeEnded, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._state, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isShiny, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._visuals, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._glowTexture, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._normalTexture, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._image, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hsv, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._glow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._glowVfx, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._label, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._combatUi, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._viewport, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._playerIconContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._longPressBar, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._pulseTimer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName.ShowPos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName.HidePos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._positionTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hoverTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._glowVfxTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._glowEnableTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._endTurnWithNoPlayableCardsCount, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.PackedStringArray, PropertyName.Hotkeys, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._state, Variant.From(in _state));
		info.AddProperty(PropertyName._isShiny, Variant.From(in _isShiny));
		info.AddProperty(PropertyName._visuals, Variant.From(in _visuals));
		info.AddProperty(PropertyName._glowTexture, Variant.From(in _glowTexture));
		info.AddProperty(PropertyName._normalTexture, Variant.From(in _normalTexture));
		info.AddProperty(PropertyName._image, Variant.From(in _image));
		info.AddProperty(PropertyName._hsv, Variant.From(in _hsv));
		info.AddProperty(PropertyName._glow, Variant.From(in _glow));
		info.AddProperty(PropertyName._glowVfx, Variant.From(in _glowVfx));
		info.AddProperty(PropertyName._label, Variant.From(in _label));
		info.AddProperty(PropertyName._combatUi, Variant.From(in _combatUi));
		info.AddProperty(PropertyName._viewport, Variant.From(in _viewport));
		info.AddProperty(PropertyName._playerIconContainer, Variant.From(in _playerIconContainer));
		info.AddProperty(PropertyName._longPressBar, Variant.From(in _longPressBar));
		info.AddProperty(PropertyName._pulseTimer, Variant.From(in _pulseTimer));
		info.AddProperty(PropertyName._positionTween, Variant.From(in _positionTween));
		info.AddProperty(PropertyName._hoverTween, Variant.From(in _hoverTween));
		info.AddProperty(PropertyName._glowVfxTween, Variant.From(in _glowVfxTween));
		info.AddProperty(PropertyName._glowEnableTween, Variant.From(in _glowEnableTween));
		info.AddProperty(PropertyName._endTurnWithNoPlayableCardsCount, Variant.From(in _endTurnWithNoPlayableCardsCount));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._state, out var value))
		{
			_state = value.As<State>();
		}
		if (info.TryGetProperty(PropertyName._isShiny, out var value2))
		{
			_isShiny = value2.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._visuals, out var value3))
		{
			_visuals = value3.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._glowTexture, out var value4))
		{
			_glowTexture = value4.As<Texture2D>();
		}
		if (info.TryGetProperty(PropertyName._normalTexture, out var value5))
		{
			_normalTexture = value5.As<Texture2D>();
		}
		if (info.TryGetProperty(PropertyName._image, out var value6))
		{
			_image = value6.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._hsv, out var value7))
		{
			_hsv = value7.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._glow, out var value8))
		{
			_glow = value8.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._glowVfx, out var value9))
		{
			_glowVfx = value9.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._label, out var value10))
		{
			_label = value10.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._combatUi, out var value11))
		{
			_combatUi = value11.As<NCombatUi>();
		}
		if (info.TryGetProperty(PropertyName._viewport, out var value12))
		{
			_viewport = value12.As<Viewport>();
		}
		if (info.TryGetProperty(PropertyName._playerIconContainer, out var value13))
		{
			_playerIconContainer = value13.As<NMultiplayerVoteContainer>();
		}
		if (info.TryGetProperty(PropertyName._longPressBar, out var value14))
		{
			_longPressBar = value14.As<NEndTurnLongPressBar>();
		}
		if (info.TryGetProperty(PropertyName._pulseTimer, out var value15))
		{
			_pulseTimer = value15.As<float>();
		}
		if (info.TryGetProperty(PropertyName._positionTween, out var value16))
		{
			_positionTween = value16.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._hoverTween, out var value17))
		{
			_hoverTween = value17.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._glowVfxTween, out var value18))
		{
			_glowVfxTween = value18.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._glowEnableTween, out var value19))
		{
			_glowEnableTween = value19.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._endTurnWithNoPlayableCardsCount, out var value20))
		{
			_endTurnWithNoPlayableCardsCount = value20.As<int>();
		}
	}
}
