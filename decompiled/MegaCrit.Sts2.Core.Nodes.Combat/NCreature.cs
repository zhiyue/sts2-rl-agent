using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Orbs;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Combat;

[ScriptPath("res://src/Core/Nodes/Combat/NCreature.cs")]
public class NCreature : Control
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Control.MethodName
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
		/// Cached name for the 'ConnectSpineAnimatorSignals' method.
		/// </summary>
		public static readonly StringName ConnectSpineAnimatorSignals = "ConnectSpineAnimatorSignals";

		/// <summary>
		/// Cached name for the 'UpdateBounds' method.
		/// </summary>
		public static readonly StringName UpdateBounds = "UpdateBounds";

		/// <summary>
		/// Cached name for the 'UpdatePhobiaMode' method.
		/// </summary>
		public static readonly StringName UpdatePhobiaMode = "UpdatePhobiaMode";

		/// <summary>
		/// Cached name for the 'UpdateNavigation' method.
		/// </summary>
		public static readonly StringName UpdateNavigation = "UpdateNavigation";

		/// <summary>
		/// Cached name for the 'OnFocus' method.
		/// </summary>
		public static readonly StringName OnFocus = "OnFocus";

		/// <summary>
		/// Cached name for the 'OnUnfocus' method.
		/// </summary>
		public static readonly StringName OnUnfocus = "OnUnfocus";

		/// <summary>
		/// Cached name for the 'OnTargetingStarted' method.
		/// </summary>
		public static readonly StringName OnTargetingStarted = "OnTargetingStarted";

		/// <summary>
		/// Cached name for the 'SetRemotePlayerFocused' method.
		/// </summary>
		public static readonly StringName SetRemotePlayerFocused = "SetRemotePlayerFocused";

		/// <summary>
		/// Cached name for the 'HideHoverTips' method.
		/// </summary>
		public static readonly StringName HideHoverTips = "HideHoverTips";

		/// <summary>
		/// Cached name for the 'SetAnimationTrigger' method.
		/// </summary>
		public static readonly StringName SetAnimationTrigger = "SetAnimationTrigger";

		/// <summary>
		/// Cached name for the 'GetCurrentAnimationLength' method.
		/// </summary>
		public static readonly StringName GetCurrentAnimationLength = "GetCurrentAnimationLength";

		/// <summary>
		/// Cached name for the 'GetCurrentAnimationTimeRemaining' method.
		/// </summary>
		public static readonly StringName GetCurrentAnimationTimeRemaining = "GetCurrentAnimationTimeRemaining";

		/// <summary>
		/// Cached name for the 'ToggleIsInteractable' method.
		/// </summary>
		public static readonly StringName ToggleIsInteractable = "ToggleIsInteractable";

		/// <summary>
		/// Cached name for the 'AnimDisableUi' method.
		/// </summary>
		public static readonly StringName AnimDisableUi = "AnimDisableUi";

		/// <summary>
		/// Cached name for the 'AnimEnableUi' method.
		/// </summary>
		public static readonly StringName AnimEnableUi = "AnimEnableUi";

		/// <summary>
		/// Cached name for the 'StartDeathAnim' method.
		/// </summary>
		public static readonly StringName StartDeathAnim = "StartDeathAnim";

		/// <summary>
		/// Cached name for the 'StartReviveAnim' method.
		/// </summary>
		public static readonly StringName StartReviveAnim = "StartReviveAnim";

		/// <summary>
		/// Cached name for the 'AnimTempRevive' method.
		/// </summary>
		public static readonly StringName AnimTempRevive = "AnimTempRevive";

		/// <summary>
		/// Cached name for the 'ImmediatelySetIdle' method.
		/// </summary>
		public static readonly StringName ImmediatelySetIdle = "ImmediatelySetIdle";

		/// <summary>
		/// Cached name for the 'AnimHideIntent' method.
		/// </summary>
		public static readonly StringName AnimHideIntent = "AnimHideIntent";

		/// <summary>
		/// Cached name for the 'SetScaleAndHue' method.
		/// </summary>
		public static readonly StringName SetScaleAndHue = "SetScaleAndHue";

		/// <summary>
		/// Cached name for the 'ScaleTo' method.
		/// </summary>
		public static readonly StringName ScaleTo = "ScaleTo";

		/// <summary>
		/// Cached name for the 'SetDefaultScaleTo' method.
		/// </summary>
		public static readonly StringName SetDefaultScaleTo = "SetDefaultScaleTo";

		/// <summary>
		/// Cached name for the 'OstyScaleToSize' method.
		/// </summary>
		public static readonly StringName OstyScaleToSize = "OstyScaleToSize";

		/// <summary>
		/// Cached name for the 'AnimShake' method.
		/// </summary>
		public static readonly StringName AnimShake = "AnimShake";

		/// <summary>
		/// Cached name for the 'DoScaleTween' method.
		/// </summary>
		public static readonly StringName DoScaleTween = "DoScaleTween";

		/// <summary>
		/// Cached name for the 'SetOrbManagerPosition' method.
		/// </summary>
		public static readonly StringName SetOrbManagerPosition = "SetOrbManagerPosition";

		/// <summary>
		/// Cached name for the 'GetTopOfHitbox' method.
		/// </summary>
		public static readonly StringName GetTopOfHitbox = "GetTopOfHitbox";

		/// <summary>
		/// Cached name for the 'GetBottomOfHitbox' method.
		/// </summary>
		public static readonly StringName GetBottomOfHitbox = "GetBottomOfHitbox";

		/// <summary>
		/// Cached name for the 'ShowMultiselectReticle' method.
		/// </summary>
		public static readonly StringName ShowMultiselectReticle = "ShowMultiselectReticle";

		/// <summary>
		/// Cached name for the 'HideMultiselectReticle' method.
		/// </summary>
		public static readonly StringName HideMultiselectReticle = "HideMultiselectReticle";

		/// <summary>
		/// Cached name for the 'ShowSingleSelectReticle' method.
		/// </summary>
		public static readonly StringName ShowSingleSelectReticle = "ShowSingleSelectReticle";

		/// <summary>
		/// Cached name for the 'HideSingleSelectReticle' method.
		/// </summary>
		public static readonly StringName HideSingleSelectReticle = "HideSingleSelectReticle";

		/// <summary>
		/// Cached name for the 'SetupForBestiary' method.
		/// </summary>
		public static readonly StringName SetupForBestiary = "SetupForBestiary";

		/// <summary>
		/// Cached name for the 'StartSfxLoop' method.
		/// </summary>
		public static readonly StringName StartSfxLoop = "StartSfxLoop";

		/// <summary>
		/// Cached name for the 'StopSfxLoop' method.
		/// </summary>
		public static readonly StringName StopSfxLoop = "StopSfxLoop";

		/// <summary>
		/// Cached name for the 'StopAllSfxLoops' method.
		/// </summary>
		public static readonly StringName StopAllSfxLoops = "StopAllSfxLoops";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
		/// <summary>
		/// Cached name for the 'Hitbox' property.
		/// </summary>
		public static readonly StringName Hitbox = "Hitbox";

		/// <summary>
		/// Cached name for the 'OrbManager' property.
		/// </summary>
		public static readonly StringName OrbManager = "OrbManager";

		/// <summary>
		/// Cached name for the 'IsInteractable' property.
		/// </summary>
		public static readonly StringName IsInteractable = "IsInteractable";

		/// <summary>
		/// Cached name for the 'VfxSpawnPosition' property.
		/// </summary>
		public static readonly StringName VfxSpawnPosition = "VfxSpawnPosition";

		/// <summary>
		/// Cached name for the 'PowerAppliedVfxSpawnPosition' property.
		/// </summary>
		public static readonly StringName PowerAppliedVfxSpawnPosition = "PowerAppliedVfxSpawnPosition";

		/// <summary>
		/// Cached name for the 'Visuals' property.
		/// </summary>
		public static readonly StringName Visuals = "Visuals";

		/// <summary>
		/// Cached name for the 'Body' property.
		/// </summary>
		public static readonly StringName Body = "Body";

		/// <summary>
		/// Cached name for the 'IntentContainer' property.
		/// </summary>
		public static readonly StringName IntentContainer = "IntentContainer";

		/// <summary>
		/// Cached name for the 'IsPlayingDeathAnimation' property.
		/// </summary>
		public static readonly StringName IsPlayingDeathAnimation = "IsPlayingDeathAnimation";

		/// <summary>
		/// Cached name for the 'HasSpineAnimation' property.
		/// </summary>
		public static readonly StringName HasSpineAnimation = "HasSpineAnimation";

		/// <summary>
		/// Cached name for the 'IsFocused' property.
		/// </summary>
		public static readonly StringName IsFocused = "IsFocused";

		/// <summary>
		/// Cached name for the 'PlayerIntentHandler' property.
		/// </summary>
		public static readonly StringName PlayerIntentHandler = "PlayerIntentHandler";

		/// <summary>
		/// Cached name for the '_stateDisplay' field.
		/// </summary>
		public static readonly StringName _stateDisplay = "_stateDisplay";

		/// <summary>
		/// Cached name for the '_intentFadeTween' field.
		/// </summary>
		public static readonly StringName _intentFadeTween = "_intentFadeTween";

		/// <summary>
		/// Cached name for the '_shakeTween' field.
		/// </summary>
		public static readonly StringName _shakeTween = "_shakeTween";

		/// <summary>
		/// Cached name for the '_isRemotePlayerOrPet' field.
		/// </summary>
		public static readonly StringName _isRemotePlayerOrPet = "_isRemotePlayerOrPet";

		/// <summary>
		/// Cached name for the '_isInBestiary' field.
		/// </summary>
		public static readonly StringName _isInBestiary = "_isInBestiary";

		/// <summary>
		/// Cached name for the '_tempScale' field.
		/// </summary>
		public static readonly StringName _tempScale = "_tempScale";

		/// <summary>
		/// Cached name for the '_scaleTween' field.
		/// </summary>
		public static readonly StringName _scaleTween = "_scaleTween";

		/// <summary>
		/// Cached name for the '_isInMultiselect' field.
		/// </summary>
		public static readonly StringName _isInMultiselect = "_isInMultiselect";

		/// <summary>
		/// Cached name for the '_selectionReticle' field.
		/// </summary>
		public static readonly StringName _selectionReticle = "_selectionReticle";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("combat/creature");

	private NCreatureStateDisplay _stateDisplay;

	private Tween? _intentFadeTween;

	private Tween? _shakeTween;

	private CreatureAnimator? _spineAnimator;

	private bool _isRemotePlayerOrPet;

	private bool _isInBestiary;

	private float _tempScale = 1f;

	private Tween? _scaleTween;

	private bool _isInMultiselect;

	private NSelectionReticle _selectionReticle;

	/// <summary>
	/// Keeps track of the looping sfx that are owned by this creature (ie thieving hopper's fluttering).
	/// We use this as a way to easily cancel all looping sfx when a creature instance is killed or despawned.
	/// - The Key is the string field of the sfx we are tracking (ie event://flying_sfx)
	///
	/// - The Value is a Tuple, where the first item is the loop param we will use to end the sfx and the second item
	///   is the value that we need to set that param to end the sfx. (ie ("loop", 1)).
	///   We need to do this because not all sfx loops use the same loop param or loop value to finish the FMOD event
	///   (ie waterfall giant uses the same loop param to ramp up how intense it is AND to end the loop).
	/// </summary>
	private readonly Dictionary<string, (string, float)> _sfxLoops = new Dictionary<string, (string, float)>();

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(_scenePath);

	public static Vector2 PowerAppliedVfxPositionOffset => new Vector2(0f, -200f);

	public Task? DeathAnimationTask { get; set; }

	public CancellationTokenSource DeathAnimCancelToken { get; } = new CancellationTokenSource();

	public Control Hitbox { get; private set; }

	public NOrbManager? OrbManager { get; private set; }

	public bool IsInteractable { get; private set; } = true;

	public Creature Entity { get; private set; }

	public Vector2 VfxSpawnPosition => Visuals.VfxSpawnPosition.GlobalPosition;

	public Vector2 PowerAppliedVfxSpawnPosition => VfxSpawnPosition + PowerAppliedVfxPositionOffset;

	public NCreatureVisuals Visuals { get; private set; }

	public Node2D Body => Visuals.GetCurrentBody();

	public Control IntentContainer { get; private set; }

	public bool IsPlayingDeathAnimation => DeathAnimationTask != null;

	public bool HasSpineAnimation => Visuals.HasSpineAnimation;

	public SpineAnimationAccess SpineAnimation => Visuals.SpineAnimation;

	public bool IsFocused { get; private set; }

	public NMultiplayerPlayerIntentHandler? PlayerIntentHandler { get; private set; }

	public T? GetSpecialNode<T>(string name) where T : Node
	{
		return Visuals.GetNodeOrNull<T>(name);
	}

	public static NCreature? Create(Creature entity)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NCreature nCreature = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NCreature>(PackedScene.GenEditState.Disabled);
		nCreature.Entity = entity;
		nCreature.Visuals = entity.CreateVisuals();
		return nCreature;
	}

	public override void _Ready()
	{
		_stateDisplay = GetNode<NCreatureStateDisplay>("%HealthBar");
		IntentContainer = GetNode<Control>("%Intents");
		Hitbox = GetNode<Control>("%Hitbox");
		_selectionReticle = GetNode<NSelectionReticle>("%SelectionReticle");
		Hitbox.Connect(Control.SignalName.FocusEntered, Callable.From(OnFocus));
		Hitbox.Connect(Control.SignalName.FocusExited, Callable.From(OnUnfocus));
		Hitbox.Connect(Control.SignalName.MouseEntered, Callable.From(OnFocus));
		Hitbox.Connect(Control.SignalName.MouseExited, Callable.From(OnUnfocus));
		if (Entity.IsPlayer)
		{
			OrbManager = NOrbManager.Create(this, LocalContext.IsMe(Entity));
			this.AddChildSafely(OrbManager);
			UpdateNavigation();
		}
		if (Entity.IsPlayer)
		{
			ICombatState? combatState = Entity.CombatState;
			if (combatState != null && combatState.RunState.Players.Count > 1)
			{
				PlayerIntentHandler = NMultiplayerPlayerIntentHandler.Create(Entity.Player);
				if (PlayerIntentHandler != null)
				{
					IntentContainer.AddChildSafely(PlayerIntentHandler);
					IntentContainer.Modulate = Colors.White;
				}
			}
		}
		this.AddChildSafely(Visuals);
		this.MoveChildSafely(Visuals, 0);
		Visuals.Position = Vector2.Zero;
		_stateDisplay.SetCreature(Entity);
		bool flag = Entity.PetOwner != null && !LocalContext.IsMe(Entity.PetOwner);
		bool flag2 = Entity.IsPlayer && !LocalContext.IsMe(Entity);
		_isRemotePlayerOrPet = flag2 || flag;
		if (_isRemotePlayerOrPet)
		{
			_stateDisplay.HideImmediately();
		}
		else
		{
			bool flag3 = NCombatRoom.Instance != null && Time.GetTicksMsec() - NCombatRoom.Instance.CreatedMsec < 1000;
			_stateDisplay.AnimateIn(flag3 ? HealthBarAnimMode.SpawnedAtCombatStart : HealthBarAnimMode.SpawnedDuringCombat);
		}
		if (HasSpineAnimation)
		{
			if (Entity.Player != null)
			{
				_spineAnimator = Entity.Player.Character.GenerateAnimator(Visuals.SpineBody);
			}
			else
			{
				_spineAnimator = Entity.Monster.GenerateAnimator(Visuals.SpineBody);
				Visuals.SetUpSkin(Entity.Monster);
			}
			ConnectSpineAnimatorSignals();
			if (Entity.IsDead)
			{
				SetAnimationTrigger("Dead");
				MegaTrackEntry currentTrack = SpineAnimation.GetCurrentTrack();
				currentTrack?.SetTrackTime(currentTrack.GetAnimationEnd());
			}
		}
		SetOrbManagerPosition();
		if (Entity.Monster != null)
		{
			ToggleIsInteractable(Entity.Monster.IsHealthBarVisible);
		}
		UpdateBounds(Visuals);
		UpdatePhobiaMode();
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		NGame.Instance?.Connect(NGame.SignalName.PhobiaModeToggled, Callable.From(UpdatePhobiaMode));
		CombatManager.Instance.CombatEnded += OnCombatEnded;
		Entity.PowerApplied += OnPowerApplied;
		Entity.PowerRemoved += OnPowerRemoved;
		Entity.PowerIncreased += OnPowerIncreased;
		foreach (PowerModel power in Entity.Powers)
		{
			SubscribeToPower(power);
		}
		ConnectSpineAnimatorSignals();
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		StopAllSfxLoops();
		DeathAnimCancelToken.Cancel();
		NGame.Instance?.Disconnect(NGame.SignalName.PhobiaModeToggled, Callable.From(UpdatePhobiaMode));
		CombatManager.Instance.CombatEnded -= OnCombatEnded;
		Entity.PowerApplied -= OnPowerApplied;
		Entity.PowerRemoved -= OnPowerRemoved;
		Entity.PowerIncreased -= OnPowerIncreased;
		foreach (PowerModel power in Entity.Powers)
		{
			UnsubscribeFromPower(power);
		}
		if (_spineAnimator != null)
		{
			_spineAnimator.BoundsUpdated -= UpdateBounds;
		}
		CombatManager.Instance.StateTracker.CombatStateChanged -= ShowCreatureHoverTips;
	}

	private void ConnectSpineAnimatorSignals()
	{
		if (_spineAnimator != null)
		{
			_spineAnimator.BoundsUpdated -= UpdateBounds;
			_spineAnimator.BoundsUpdated += UpdateBounds;
		}
	}

	private void UpdateBounds(string boundsNodeName)
	{
		UpdateBounds(Visuals.GetNode<Control>(boundsNodeName));
	}

	private void UpdatePhobiaMode()
	{
		Visuals.UpdatePhobiaMode(Entity.Monster);
	}

	/// <summary>
	/// Called when a creatures hitbox is updated. Called on initialization and things like Osty getting bigger, thieving hopper flying, etc.
	/// </summary>
	/// <param name="boundsContainer"></param> The parent node which has the children: Bounds, CenterPos, and IntentPos nodes.
	private void UpdateBounds(Node boundsContainer)
	{
		Control node = boundsContainer.GetNode<Control>("%Bounds");
		Vector2 size = node.Size * Visuals.Scale / _tempScale;
		Vector2 vector = (node.GlobalPosition - base.GlobalPosition) / _tempScale;
		Hitbox.Size = size;
		Hitbox.GlobalPosition = base.GlobalPosition + vector;
		_selectionReticle.Size = size;
		_selectionReticle.GlobalPosition = base.GlobalPosition + vector;
		_selectionReticle.PivotOffset = _selectionReticle.Size * 0.5f;
		IntentContainer.Position = boundsContainer.GetNode<Marker2D>("IntentPos").Position - IntentContainer.Size * 0.5f;
		IntentContainer.Position = new Vector2(IntentContainer.Position.X, IntentContainer.Position.Y * Visuals.Scale.X);
		_stateDisplay.SetCreatureBounds(Hitbox);
	}

	public void UpdateNavigation()
	{
		if (OrbManager != null)
		{
			Hitbox.FocusNeighborTop = OrbManager.DefaultFocusOwner.GetPath();
		}
	}

	public Task UpdateIntent(IEnumerable<Creature> targets)
	{
		if (Entity.Monster == null)
		{
			throw new InvalidOperationException("Only valid on monsters.");
		}
		IReadOnlyList<AbstractIntent> intents = Entity.Monster.NextMove.Intents;
		int i;
		for (i = 0; i < intents.Count && i < IntentContainer.GetChildCount(); i++)
		{
			NIntent child = IntentContainer.GetChild<NIntent>(i);
			child.SetFrozen(isFrozen: false);
			child.UpdateIntent(intents[i], targets, Entity);
		}
		float num = (float)GetHashCode() * 0.01f;
		for (; i < intents.Count; i++)
		{
			NIntent nIntent = NIntent.Create(num + (float)i * 0.3f);
			IntentContainer.AddChildSafely(nIntent);
			nIntent.UpdateIntent(intents[i], targets, Entity);
		}
		List<Node> list = IntentContainer.GetChildren().TakeLast(IntentContainer.GetChildCount() - i).ToList();
		foreach (Node item in list)
		{
			IntentContainer.RemoveChildSafely(item);
			item.QueueFreeSafely();
		}
		return Task.CompletedTask;
	}

	public async Task PerformIntent()
	{
		foreach (NIntent item in IntentContainer.GetChildren().OfType<NIntent>())
		{
			item.PlayPerform();
			item.SetFrozen(isFrozen: true);
		}
		if (SaveManager.Instance.PrefsSave.FastMode == FastModeType.Instant)
		{
			IntentContainer.Modulate = new Color(IntentContainer.Modulate.R, IntentContainer.Modulate.G, IntentContainer.Modulate.B, 0f);
			return;
		}
		AnimHideIntent(0.4);
		await Cmd.CustomScaledWait(0.25f, 0.4f);
	}

	public async Task RefreshIntents()
	{
		await UpdateIntent(Entity.CombatState.Players.Select((Player p) => p.Creature));
		await RevealIntents();
	}

	/// <summary>
	/// This makes the intents visible again!
	/// So let's fade them in.
	/// </summary>
	private Task RevealIntents()
	{
		IntentContainer.Modulate = Colors.Transparent;
		_intentFadeTween?.Kill();
		_intentFadeTween = CreateTween().SetParallel();
		_intentFadeTween.TweenProperty(IntentContainer, "modulate:a", 1f, 1.0).SetDelay(Rng.Chaotic.NextFloat(0f, 0.3f));
		return Task.CompletedTask;
	}

	private void OnFocus()
	{
		if (IsFocused || _isInBestiary)
		{
			return;
		}
		IsFocused = true;
		if (_isRemotePlayerOrPet)
		{
			_stateDisplay.AnimateIn(HealthBarAnimMode.FromHidden);
			_stateDisplay.ZIndex = 1;
			Player me = LocalContext.GetMe(Entity.CombatState);
			NCombatRoom.Instance?.GetCreatureNode(me?.Creature)?.SetRemotePlayerFocused(remotePlayerFocused: true);
		}
		else
		{
			_stateDisplay.ShowNameplate();
		}
		NRun.Instance.GlobalUi.MultiplayerPlayerContainer.HighlightPlayer(Entity.Player);
		if (NTargetManager.Instance.IsInSelection)
		{
			NTargetManager.Instance.OnNodeHovered(this);
			return;
		}
		if (NControllerManager.Instance.IsUsingController)
		{
			ShowSingleSelectReticle();
		}
		ShowHoverTips(Entity.HoverTips);
		CombatManager.Instance.StateTracker.CombatStateChanged += ShowCreatureHoverTips;
	}

	private void OnUnfocus()
	{
		if (!_isInBestiary)
		{
			IsFocused = false;
			HideSingleSelectReticle();
			if (_isRemotePlayerOrPet)
			{
				_stateDisplay.AnimateOut();
				Player me = LocalContext.GetMe(Entity.CombatState);
				NCombatRoom.Instance?.GetCreatureNode(me?.Creature)?.SetRemotePlayerFocused(remotePlayerFocused: false);
			}
			else
			{
				_stateDisplay.HideNameplate();
			}
			NRun.Instance.GlobalUi.MultiplayerPlayerContainer.UnhighlightPlayer(Entity.Player);
			NTargetManager.Instance.OnNodeUnhovered(this);
			CombatManager.Instance.StateTracker.CombatStateChanged -= ShowCreatureHoverTips;
			HideHoverTips();
		}
	}

	public void OnTargetingStarted()
	{
		if (IsFocused)
		{
			NTargetManager.Instance.OnNodeHovered(this);
			CombatManager.Instance.StateTracker.CombatStateChanged -= ShowCreatureHoverTips;
			HideHoverTips();
		}
	}

	private void ShowCreatureHoverTips(CombatState _)
	{
		if (Entity.CombatState != null)
		{
			ShowHoverTips(Entity.HoverTips);
		}
	}

	public void ShowHoverTips(IEnumerable<IHoverTip> hoverTips)
	{
		if (!NCombatRoom.Instance.Ui.Hand.InCardPlay)
		{
			HideHoverTips();
			NHoverTipSet.CreateAndShow(Hitbox, hoverTips, HoverTip.GetHoverTipAlignment(this, 0.5f));
		}
	}

	public void SetRemotePlayerFocused(bool remotePlayerFocused)
	{
		if (!LocalContext.IsMe(Entity))
		{
			throw new InvalidOperationException("This should only be called on the local player's creature node!");
		}
		if (remotePlayerFocused)
		{
			_stateDisplay.AnimateOut();
		}
		else if (Entity.IsAlive)
		{
			_stateDisplay.AnimateIn(HealthBarAnimMode.FromHidden);
		}
	}

	public void HideHoverTips()
	{
		NHoverTipSet.Remove(Hitbox);
	}

	private void SubscribeToPower(PowerModel power)
	{
		power.Flashed += OnPowerFlashed;
	}

	private void UnsubscribeFromPower(PowerModel power)
	{
		power.Flashed -= OnPowerFlashed;
	}

	private void OnPowerApplied(PowerModel power)
	{
		SubscribeToPower(power);
	}

	private void OnPowerIncreased(PowerModel power, int amount, bool silent)
	{
		if (silent || !CombatManager.Instance.IsInProgress)
		{
			return;
		}
		bool flag = power.GetTypeForAmount(power.Amount) == PowerType.Buff;
		NPowerAppliedVfx vfx = NPowerAppliedVfx.Create(power, amount, flag);
		if (vfx != null)
		{
			if (flag)
			{
				NPowerAppliedBuffVfx buffVfx = NPowerAppliedBuffVfx.Create(PowerAppliedVfxSpawnPosition);
				Callable.From(delegate
				{
					NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(buffVfx);
				}).CallDeferred();
			}
			else
			{
				NPowerAppliedDebuffVfx debuffVfx = NPowerAppliedDebuffVfx.Create(PowerAppliedVfxSpawnPosition);
				Callable.From(delegate
				{
					NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(debuffVfx);
				}).CallDeferred();
			}
			Callable.From(delegate
			{
				NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(vfx);
			}).CallDeferred();
		}
		if (power.ShouldPlayVfx)
		{
			SfxCmd.Play(flag ? "event:/sfx/buff" : "event:/sfx/debuff");
		}
		if (!flag)
		{
			AnimShake();
		}
	}

	private void OnPowerRemoved(PowerModel power)
	{
		NPowerRemovedVfx vfx = NPowerRemovedVfx.Create(power);
		if (vfx != null)
		{
			Callable.From(delegate
			{
				NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(vfx);
			}).CallDeferred();
		}
		UnsubscribeFromPower(power);
	}

	private void OnPowerFlashed(PowerModel power)
	{
		NPowerFlashVfx vfx = NPowerFlashVfx.Create(power);
		if (vfx != null)
		{
			Callable.From(delegate
			{
				NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(vfx);
			}).CallDeferred();
		}
	}

	private void OnCombatEnded(CombatRoom _)
	{
		AnimHideIntent();
		OrbManager?.ClearOrbs();
	}

	public void SetAnimationTrigger(string trigger)
	{
		_spineAnimator?.SetTrigger(trigger);
	}

	public float GetCurrentAnimationLength()
	{
		return SpineAnimation.GetCurrentAnimationDuration().GetValueOrDefault();
	}

	public float GetCurrentAnimationTimeRemaining()
	{
		MegaTrackEntry currentTrack = SpineAnimation.GetCurrentTrack();
		if (currentTrack == null)
		{
			return 0f;
		}
		return currentTrack.GetTrackComplete() - currentTrack.GetTrackTime();
	}

	public void ToggleIsInteractable(bool on)
	{
		IsInteractable = on;
		_stateDisplay.Visible = !NCombatUi.IsDebugHidingHpBar && on;
		Hitbox.MouseFilter = (MouseFilterEnum)(on ? 0 : 2);
	}

	public Tween AnimDisableUi()
	{
		Tween tween = CreateTween();
		if (!IsNodeReady())
		{
			tween.TweenInterval(0.0);
			return tween;
		}
		tween.TweenProperty(_stateDisplay, "modulate:a", 0f, 0.5).SetDelay(0.5).SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Expo);
		return tween;
	}

	public Tween AnimEnableUi()
	{
		Tween tween = CreateTween();
		tween.TweenProperty(_stateDisplay, "modulate:a", 1f, 0.5).SetDelay(0.5).SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Expo);
		return tween;
	}

	public float StartDeathAnim(bool shouldRemove)
	{
		if (Hitbox.HasFocus())
		{
			ActiveScreenContext.Instance.FocusOnDefaultControl();
		}
		Hitbox.FocusMode = FocusModeEnum.None;
		foreach (NIntent item in IntentContainer.GetChildren().OfType<NIntent>())
		{
			item.SetFrozen(isFrozen: true);
		}
		Task deathAnimationTask = DeathAnimationTask;
		if (deathAnimationTask != null && !deathAnimationTask.IsCompleted)
		{
			return 0f;
		}
		float a = 0f;
		if (_spineAnimator != null)
		{
			MonsterModel? monster = Entity.Monster;
			if (monster != null && monster.HasDeathSfx)
			{
				SfxCmd.PlayDeath(Entity.Monster);
			}
			if (Entity.Player != null)
			{
				SfxCmd.PlayDeath(Entity.Player);
			}
			SetAnimationTrigger("Dead");
			a = GetCurrentAnimationLength();
		}
		DeathAnimationTask = AnimDie(shouldRemove, DeathAnimCancelToken.Token);
		TaskHelper.RunSafely(DeathAnimationTask);
		MonsterModel monster2 = Entity.Monster;
		if (monster2 != null && monster2.HasDeathAnimLengthOverride)
		{
			return Entity.Monster.DeathAnimLengthOverride;
		}
		return Mathf.Min(a, 30f);
	}

	public void StartReviveAnim()
	{
		CreatureAnimator? spineAnimator = _spineAnimator;
		if (spineAnimator != null && spineAnimator.HasTrigger("Revive"))
		{
			SetAnimationTrigger("Revive");
		}
		else if (Entity.IsPlayer)
		{
			AnimTempRevive();
		}
		if (!_isRemotePlayerOrPet)
		{
			AnimEnableUi();
		}
		Hitbox.MouseFilter = MouseFilterEnum.Stop;
	}

	private void AnimTempRevive()
	{
		Tween tween = CreateTween();
		tween.TweenProperty(Visuals, "modulate:a", 0f, 0.2);
		tween.TweenCallback(Callable.From(ImmediatelySetIdle));
		tween.TweenProperty(Visuals, "modulate:a", 1f, 0.2);
	}

	private void ImmediatelySetIdle()
	{
		_spineAnimator?.SetTrigger("Idle");
		MegaTrackEntry currentTrack = SpineAnimation.GetCurrentTrack();
		if (currentTrack != null)
		{
			currentTrack.SetMixDuration(0f);
			currentTrack.SetTrackTime(currentTrack.GetAnimationEnd());
		}
	}

	private async Task AnimDie(bool shouldRemove, CancellationToken cancelToken)
	{
		Tween disableUiTween = AnimDisableUi();
		Hitbox.MouseFilter = MouseFilterEnum.Ignore;
		if (!RunManager.Instance.IsSingleplayerOrFakeMultiplayer)
		{
			OrbManager?.ClearOrbs();
		}
		if (shouldRemove)
		{
			AnimHideIntent();
		}
		if (_spineAnimator != null)
		{
			float seconds = Math.Min(GetCurrentAnimationTimeRemaining() + 0.5f, 20f);
			await Cmd.Wait(seconds, cancelToken, ignoreCombatEnd: true);
		}
		else
		{
			MonsterModel monster = Entity.Monster;
			if (monster != null && monster.HasDeathAnimLengthOverride)
			{
				await Cmd.Wait(Entity.Monster.DeathAnimLengthOverride, cancelToken, ignoreCombatEnd: true);
			}
		}
		if (cancelToken.IsCancellationRequested)
		{
			return;
		}
		if (shouldRemove)
		{
			Task fadeVfx = null;
			MonsterModel monster = Entity.Monster;
			if (monster != null && monster.ShouldFadeAfterDeath && Body.IsVisibleInTree())
			{
				NMonsterDeathVfx nMonsterDeathVfx = NMonsterDeathVfx.Create(this, cancelToken);
				Node parent = GetParent();
				parent.AddChildSafely(nMonsterDeathVfx);
				if (nMonsterDeathVfx != null)
				{
					parent.MoveChildSafely(nMonsterDeathVfx, GetIndex());
				}
				fadeVfx = nMonsterDeathVfx?.PlayVfx();
			}
			if (SaveManager.Instance.PrefsSave.FastMode != FastModeType.Instant)
			{
				if (disableUiTween.IsValid() && disableUiTween.IsRunning() && !(await disableUiTween.AwaitFinished(this)))
				{
					return;
				}
				foreach (IDeathDelayer item in this.GetChildrenRecursive<IDeathDelayer>())
				{
					await item.GetDelayTask();
				}
			}
			if (fadeVfx != null)
			{
				await fadeVfx;
			}
			this.QueueFreeSafely();
		}
		else if (Entity.Monster is Osty)
		{
			OstyScaleToSize(0f, 0.75);
		}
	}

	public void AnimHideIntent(double delay = 0.0)
	{
		_intentFadeTween?.Kill();
		_intentFadeTween = CreateTween().SetParallel();
		PropertyTweener propertyTweener = _intentFadeTween.TweenProperty(IntentContainer, "modulate:a", 0f, 0.5);
		if (delay > 0.0)
		{
			propertyTweener.SetDelay(delay);
		}
	}

	public void SetScaleAndHue(float scale, float hue)
	{
		Visuals.SetScaleAndHue(scale, hue);
		UpdateBounds(Visuals);
	}

	public void ScaleTo(float size, double duration)
	{
		if (!Entity.IsMonster || Entity.Monster.CanChangeScale)
		{
			_tempScale = size;
			_scaleTween?.Kill();
			_scaleTween = CreateTween();
			_scaleTween.TweenMethod(Callable.From<Vector2>(DoScaleTween), Visuals.Scale, Vector2.One * _tempScale * Visuals.DefaultScale, duration).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
		}
	}

	public void SetDefaultScaleTo(float size, float duration)
	{
		if (!Entity.IsMonster || Entity.Monster.CanChangeScale)
		{
			Visuals.DefaultScale = size;
			ScaleTo(_tempScale, duration);
		}
	}

	public void OstyScaleToSize(float ostyHealth, double duration)
	{
		float num = Mathf.Lerp(Osty.ScaleRange.X, Osty.ScaleRange.Y, Mathf.Clamp(ostyHealth / 150f, 0f, 1f));
		NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(Entity.PetOwner.Creature);
		_scaleTween = CreateTween();
		_scaleTween.TweenProperty(Visuals, "scale", Vector2.One * num * Visuals.DefaultScale, duration).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
		if (LocalContext.IsMe(Entity.PetOwner))
		{
			_scaleTween.Parallel().TweenProperty(this, "position", nCreature.Position + GetOstyOffsetFromPlayer(Entity), duration);
		}
		_scaleTween.TweenCallback(Callable.From(delegate
		{
			UpdateBounds(Visuals);
		}));
	}

	public static Vector2 GetOstyOffsetFromPlayer(Creature osty)
	{
		NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(osty.PetOwner.Creature);
		return Vector2.Right * nCreature.Hitbox.Size.X * 0.5f + Osty.MinOffset.Lerp(Osty.MaxOffset, Mathf.Clamp((float)osty.MaxHp / 150f, 0f, 1f));
	}

	public void AnimShake()
	{
		if (IsInsideTree() && (_shakeTween == null || !_shakeTween.IsRunning()) && !Visuals.IsPlayingHurtAnimation())
		{
			Visuals.Position = Vector2.Zero;
			_shakeTween = CreateTween();
			_shakeTween.TweenMethod(Callable.From(delegate(float t)
			{
				Visuals.Position = Vector2.Right * 10f * Mathf.Sin(t * 4f) * Mathf.Sin(t * 0.5f);
			}), 0f, (float)Math.PI * 2f, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		}
	}

	private void DoScaleTween(Vector2 scale)
	{
		Visuals.Scale = scale;
		SetOrbManagerPosition();
	}

	private void SetOrbManagerPosition()
	{
		if (OrbManager != null)
		{
			OrbManager.Scale = ((Visuals.Scale.X > 1f) ? Vector2.One : Visuals.Scale.Lerp(Vector2.One, 0.5f));
			OrbManager.Position = Visuals.OrbPosition.Position * Mathf.Min(Visuals.Scale.X, 1.25f);
			if (!OrbManager.IsLocal)
			{
				OrbManager.Position += Vector2.Up * 50f;
			}
		}
	}

	/// <summary>
	/// Helper function to get the top of this creature's hitbox position.
	/// Used for spawning vfx or aligning UI elements to a creature's hitbox.
	/// </summary>
	public Vector2 GetTopOfHitbox()
	{
		return Hitbox.GlobalPosition + new Vector2(Hitbox.Size.X * 0.5f, 0f);
	}

	/// <summary>
	/// Helper function to get the top of this creature's hitbox position.
	/// Used for spawning vfx or aligning UI elements to a creature's hitbox.
	/// </summary>
	/// <returns></returns>
	public Vector2 GetBottomOfHitbox()
	{
		return Hitbox.GlobalPosition + new Vector2(Hitbox.Size.X * 0.5f, Hitbox.Size.Y);
	}

	/// <summary>
	/// Track the block status of another creature.
	/// Used for pets who want to show extra UI when their owner has block.
	/// For example, Osty tracks Necrobinder's block.
	/// </summary>
	/// <param name="creature">Extra creature (different from _creature) whose block status we want to track in this UI.</param>
	public void TrackBlockStatus(Creature creature)
	{
		_stateDisplay.TrackBlockStatus(creature);
	}

	public void ShowMultiselectReticle()
	{
		_isInMultiselect = true;
		ShowSingleSelectReticle();
	}

	public void HideMultiselectReticle()
	{
		_isInMultiselect = false;
		HideSingleSelectReticle();
	}

	public void ShowSingleSelectReticle()
	{
		_selectionReticle.OnSelect();
	}

	public void HideSingleSelectReticle()
	{
		if (!_isInMultiselect)
		{
			_selectionReticle.OnDeselect();
		}
	}

	public void SetupForBestiary()
	{
		_stateDisplay.Visible = false;
		IntentContainer.Visible = false;
		_isInBestiary = true;
	}

	public void StartSfxLoop(string sfxName)
	{
		StartSfxLoop(sfxName, "loop", 1f);
	}

	public void StartSfxLoop(string sfxName, string loopParam, float loopStopValue)
	{
		if (!_sfxLoops.ContainsKey(sfxName))
		{
			_sfxLoops.Add(sfxName, (loopParam, loopStopValue));
			SfxCmd.PlayLoop(sfxName, usesLoopParam: false);
		}
	}

	public void StopSfxLoop(string sfxName)
	{
		if (_sfxLoops.TryGetValue(sfxName, out (string, float) value))
		{
			SfxCmd.SetParam(sfxName, value.Item1, value.Item2);
			SfxCmd.StopLoop(sfxName);
			_sfxLoops.Remove(sfxName);
		}
	}

	public void StopAllSfxLoops()
	{
		List<string> list = _sfxLoops.Keys.ToList();
		foreach (string item in list)
		{
			StopSfxLoop(item);
		}
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(41);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ConnectSpineAnimatorSignals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateBounds, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "boundsNodeName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdatePhobiaMode, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateNavigation, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnTargetingStarted, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetRemotePlayerFocused, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "remotePlayerFocused", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.HideHoverTips, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetAnimationTrigger, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "trigger", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetCurrentAnimationLength, new PropertyInfo(Variant.Type.Float, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GetCurrentAnimationTimeRemaining, new PropertyInfo(Variant.Type.Float, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ToggleIsInteractable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "on", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AnimDisableUi, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Tween"), exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimEnableUi, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Tween"), exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.StartDeathAnim, new PropertyInfo(Variant.Type.Float, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "shouldRemove", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.StartReviveAnim, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimTempRevive, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ImmediatelySetIdle, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimHideIntent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delay", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetScaleAndHue, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "scale", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Float, "hue", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ScaleTo, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "size", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Float, "duration", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetDefaultScaleTo, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "size", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Float, "duration", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OstyScaleToSize, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "ostyHealth", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Float, "duration", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AnimShake, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DoScaleTween, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "scale", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetOrbManagerPosition, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GetTopOfHitbox, new PropertyInfo(Variant.Type.Vector2, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GetBottomOfHitbox, new PropertyInfo(Variant.Type.Vector2, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ShowMultiselectReticle, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.HideMultiselectReticle, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ShowSingleSelectReticle, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.HideSingleSelectReticle, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetupForBestiary, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.StartSfxLoop, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "sfxName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.StartSfxLoop, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "sfxName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.String, "loopParam", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Float, "loopStopValue", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.StopSfxLoop, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "sfxName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.StopAllSfxLoops, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.ConnectSpineAnimatorSignals && args.Count == 0)
		{
			ConnectSpineAnimatorSignals();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateBounds && args.Count == 1)
		{
			UpdateBounds(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdatePhobiaMode && args.Count == 0)
		{
			UpdatePhobiaMode();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateNavigation && args.Count == 0)
		{
			UpdateNavigation();
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
		if (method == MethodName.OnTargetingStarted && args.Count == 0)
		{
			OnTargetingStarted();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetRemotePlayerFocused && args.Count == 1)
		{
			SetRemotePlayerFocused(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HideHoverTips && args.Count == 0)
		{
			HideHoverTips();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetAnimationTrigger && args.Count == 1)
		{
			SetAnimationTrigger(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetCurrentAnimationLength && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<float>(GetCurrentAnimationLength());
			return true;
		}
		if (method == MethodName.GetCurrentAnimationTimeRemaining && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<float>(GetCurrentAnimationTimeRemaining());
			return true;
		}
		if (method == MethodName.ToggleIsInteractable && args.Count == 1)
		{
			ToggleIsInteractable(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimDisableUi && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Tween>(AnimDisableUi());
			return true;
		}
		if (method == MethodName.AnimEnableUi && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Tween>(AnimEnableUi());
			return true;
		}
		if (method == MethodName.StartDeathAnim && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<float>(StartDeathAnim(VariantUtils.ConvertTo<bool>(in args[0])));
			return true;
		}
		if (method == MethodName.StartReviveAnim && args.Count == 0)
		{
			StartReviveAnim();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimTempRevive && args.Count == 0)
		{
			AnimTempRevive();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ImmediatelySetIdle && args.Count == 0)
		{
			ImmediatelySetIdle();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimHideIntent && args.Count == 1)
		{
			AnimHideIntent(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetScaleAndHue && args.Count == 2)
		{
			SetScaleAndHue(VariantUtils.ConvertTo<float>(in args[0]), VariantUtils.ConvertTo<float>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ScaleTo && args.Count == 2)
		{
			ScaleTo(VariantUtils.ConvertTo<float>(in args[0]), VariantUtils.ConvertTo<double>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetDefaultScaleTo && args.Count == 2)
		{
			SetDefaultScaleTo(VariantUtils.ConvertTo<float>(in args[0]), VariantUtils.ConvertTo<float>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OstyScaleToSize && args.Count == 2)
		{
			OstyScaleToSize(VariantUtils.ConvertTo<float>(in args[0]), VariantUtils.ConvertTo<double>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimShake && args.Count == 0)
		{
			AnimShake();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DoScaleTween && args.Count == 1)
		{
			DoScaleTween(VariantUtils.ConvertTo<Vector2>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetOrbManagerPosition && args.Count == 0)
		{
			SetOrbManagerPosition();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetTopOfHitbox && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Vector2>(GetTopOfHitbox());
			return true;
		}
		if (method == MethodName.GetBottomOfHitbox && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Vector2>(GetBottomOfHitbox());
			return true;
		}
		if (method == MethodName.ShowMultiselectReticle && args.Count == 0)
		{
			ShowMultiselectReticle();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HideMultiselectReticle && args.Count == 0)
		{
			HideMultiselectReticle();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ShowSingleSelectReticle && args.Count == 0)
		{
			ShowSingleSelectReticle();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HideSingleSelectReticle && args.Count == 0)
		{
			HideSingleSelectReticle();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetupForBestiary && args.Count == 0)
		{
			SetupForBestiary();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartSfxLoop && args.Count == 1)
		{
			StartSfxLoop(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartSfxLoop && args.Count == 3)
		{
			StartSfxLoop(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<string>(in args[1]), VariantUtils.ConvertTo<float>(in args[2]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StopSfxLoop && args.Count == 1)
		{
			StopSfxLoop(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StopAllSfxLoops && args.Count == 0)
		{
			StopAllSfxLoops();
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
		if (method == MethodName.ConnectSpineAnimatorSignals)
		{
			return true;
		}
		if (method == MethodName.UpdateBounds)
		{
			return true;
		}
		if (method == MethodName.UpdatePhobiaMode)
		{
			return true;
		}
		if (method == MethodName.UpdateNavigation)
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
		if (method == MethodName.OnTargetingStarted)
		{
			return true;
		}
		if (method == MethodName.SetRemotePlayerFocused)
		{
			return true;
		}
		if (method == MethodName.HideHoverTips)
		{
			return true;
		}
		if (method == MethodName.SetAnimationTrigger)
		{
			return true;
		}
		if (method == MethodName.GetCurrentAnimationLength)
		{
			return true;
		}
		if (method == MethodName.GetCurrentAnimationTimeRemaining)
		{
			return true;
		}
		if (method == MethodName.ToggleIsInteractable)
		{
			return true;
		}
		if (method == MethodName.AnimDisableUi)
		{
			return true;
		}
		if (method == MethodName.AnimEnableUi)
		{
			return true;
		}
		if (method == MethodName.StartDeathAnim)
		{
			return true;
		}
		if (method == MethodName.StartReviveAnim)
		{
			return true;
		}
		if (method == MethodName.AnimTempRevive)
		{
			return true;
		}
		if (method == MethodName.ImmediatelySetIdle)
		{
			return true;
		}
		if (method == MethodName.AnimHideIntent)
		{
			return true;
		}
		if (method == MethodName.SetScaleAndHue)
		{
			return true;
		}
		if (method == MethodName.ScaleTo)
		{
			return true;
		}
		if (method == MethodName.SetDefaultScaleTo)
		{
			return true;
		}
		if (method == MethodName.OstyScaleToSize)
		{
			return true;
		}
		if (method == MethodName.AnimShake)
		{
			return true;
		}
		if (method == MethodName.DoScaleTween)
		{
			return true;
		}
		if (method == MethodName.SetOrbManagerPosition)
		{
			return true;
		}
		if (method == MethodName.GetTopOfHitbox)
		{
			return true;
		}
		if (method == MethodName.GetBottomOfHitbox)
		{
			return true;
		}
		if (method == MethodName.ShowMultiselectReticle)
		{
			return true;
		}
		if (method == MethodName.HideMultiselectReticle)
		{
			return true;
		}
		if (method == MethodName.ShowSingleSelectReticle)
		{
			return true;
		}
		if (method == MethodName.HideSingleSelectReticle)
		{
			return true;
		}
		if (method == MethodName.SetupForBestiary)
		{
			return true;
		}
		if (method == MethodName.StartSfxLoop)
		{
			return true;
		}
		if (method == MethodName.StopSfxLoop)
		{
			return true;
		}
		if (method == MethodName.StopAllSfxLoops)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.Hitbox)
		{
			Hitbox = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName.OrbManager)
		{
			OrbManager = VariantUtils.ConvertTo<NOrbManager>(in value);
			return true;
		}
		if (name == PropertyName.IsInteractable)
		{
			IsInteractable = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.Visuals)
		{
			Visuals = VariantUtils.ConvertTo<NCreatureVisuals>(in value);
			return true;
		}
		if (name == PropertyName.IntentContainer)
		{
			IntentContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName.IsFocused)
		{
			IsFocused = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.PlayerIntentHandler)
		{
			PlayerIntentHandler = VariantUtils.ConvertTo<NMultiplayerPlayerIntentHandler>(in value);
			return true;
		}
		if (name == PropertyName._stateDisplay)
		{
			_stateDisplay = VariantUtils.ConvertTo<NCreatureStateDisplay>(in value);
			return true;
		}
		if (name == PropertyName._intentFadeTween)
		{
			_intentFadeTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._shakeTween)
		{
			_shakeTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._isRemotePlayerOrPet)
		{
			_isRemotePlayerOrPet = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._isInBestiary)
		{
			_isInBestiary = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._tempScale)
		{
			_tempScale = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._scaleTween)
		{
			_scaleTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._isInMultiselect)
		{
			_isInMultiselect = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._selectionReticle)
		{
			_selectionReticle = VariantUtils.ConvertTo<NSelectionReticle>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		Control from;
		if (name == PropertyName.Hitbox)
		{
			from = Hitbox;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.OrbManager)
		{
			value = VariantUtils.CreateFrom<NOrbManager>(OrbManager);
			return true;
		}
		bool from2;
		if (name == PropertyName.IsInteractable)
		{
			from2 = IsInteractable;
			value = VariantUtils.CreateFrom(in from2);
			return true;
		}
		Vector2 from3;
		if (name == PropertyName.VfxSpawnPosition)
		{
			from3 = VfxSpawnPosition;
			value = VariantUtils.CreateFrom(in from3);
			return true;
		}
		if (name == PropertyName.PowerAppliedVfxSpawnPosition)
		{
			from3 = PowerAppliedVfxSpawnPosition;
			value = VariantUtils.CreateFrom(in from3);
			return true;
		}
		if (name == PropertyName.Visuals)
		{
			value = VariantUtils.CreateFrom<NCreatureVisuals>(Visuals);
			return true;
		}
		if (name == PropertyName.Body)
		{
			value = VariantUtils.CreateFrom<Node2D>(Body);
			return true;
		}
		if (name == PropertyName.IntentContainer)
		{
			from = IntentContainer;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.IsPlayingDeathAnimation)
		{
			from2 = IsPlayingDeathAnimation;
			value = VariantUtils.CreateFrom(in from2);
			return true;
		}
		if (name == PropertyName.HasSpineAnimation)
		{
			from2 = HasSpineAnimation;
			value = VariantUtils.CreateFrom(in from2);
			return true;
		}
		if (name == PropertyName.IsFocused)
		{
			from2 = IsFocused;
			value = VariantUtils.CreateFrom(in from2);
			return true;
		}
		if (name == PropertyName.PlayerIntentHandler)
		{
			value = VariantUtils.CreateFrom<NMultiplayerPlayerIntentHandler>(PlayerIntentHandler);
			return true;
		}
		if (name == PropertyName._stateDisplay)
		{
			value = VariantUtils.CreateFrom(in _stateDisplay);
			return true;
		}
		if (name == PropertyName._intentFadeTween)
		{
			value = VariantUtils.CreateFrom(in _intentFadeTween);
			return true;
		}
		if (name == PropertyName._shakeTween)
		{
			value = VariantUtils.CreateFrom(in _shakeTween);
			return true;
		}
		if (name == PropertyName._isRemotePlayerOrPet)
		{
			value = VariantUtils.CreateFrom(in _isRemotePlayerOrPet);
			return true;
		}
		if (name == PropertyName._isInBestiary)
		{
			value = VariantUtils.CreateFrom(in _isInBestiary);
			return true;
		}
		if (name == PropertyName._tempScale)
		{
			value = VariantUtils.CreateFrom(in _tempScale);
			return true;
		}
		if (name == PropertyName._scaleTween)
		{
			value = VariantUtils.CreateFrom(in _scaleTween);
			return true;
		}
		if (name == PropertyName._isInMultiselect)
		{
			value = VariantUtils.CreateFrom(in _isInMultiselect);
			return true;
		}
		if (name == PropertyName._selectionReticle)
		{
			value = VariantUtils.CreateFrom(in _selectionReticle);
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
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._stateDisplay, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._intentFadeTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._shakeTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isRemotePlayerOrPet, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isInBestiary, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._tempScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._scaleTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Hitbox, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.OrbManager, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isInMultiselect, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._selectionReticle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsInteractable, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName.VfxSpawnPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName.PowerAppliedVfxSpawnPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Visuals, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Body, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.IntentContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsPlayingDeathAnimation, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.HasSpineAnimation, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsFocused, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.PlayerIntentHandler, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.Hitbox, Variant.From<Control>(Hitbox));
		info.AddProperty(PropertyName.OrbManager, Variant.From<NOrbManager>(OrbManager));
		info.AddProperty(PropertyName.IsInteractable, Variant.From<bool>(IsInteractable));
		info.AddProperty(PropertyName.Visuals, Variant.From<NCreatureVisuals>(Visuals));
		info.AddProperty(PropertyName.IntentContainer, Variant.From<Control>(IntentContainer));
		info.AddProperty(PropertyName.IsFocused, Variant.From<bool>(IsFocused));
		info.AddProperty(PropertyName.PlayerIntentHandler, Variant.From<NMultiplayerPlayerIntentHandler>(PlayerIntentHandler));
		info.AddProperty(PropertyName._stateDisplay, Variant.From(in _stateDisplay));
		info.AddProperty(PropertyName._intentFadeTween, Variant.From(in _intentFadeTween));
		info.AddProperty(PropertyName._shakeTween, Variant.From(in _shakeTween));
		info.AddProperty(PropertyName._isRemotePlayerOrPet, Variant.From(in _isRemotePlayerOrPet));
		info.AddProperty(PropertyName._isInBestiary, Variant.From(in _isInBestiary));
		info.AddProperty(PropertyName._tempScale, Variant.From(in _tempScale));
		info.AddProperty(PropertyName._scaleTween, Variant.From(in _scaleTween));
		info.AddProperty(PropertyName._isInMultiselect, Variant.From(in _isInMultiselect));
		info.AddProperty(PropertyName._selectionReticle, Variant.From(in _selectionReticle));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.Hitbox, out var value))
		{
			Hitbox = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName.OrbManager, out var value2))
		{
			OrbManager = value2.As<NOrbManager>();
		}
		if (info.TryGetProperty(PropertyName.IsInteractable, out var value3))
		{
			IsInteractable = value3.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.Visuals, out var value4))
		{
			Visuals = value4.As<NCreatureVisuals>();
		}
		if (info.TryGetProperty(PropertyName.IntentContainer, out var value5))
		{
			IntentContainer = value5.As<Control>();
		}
		if (info.TryGetProperty(PropertyName.IsFocused, out var value6))
		{
			IsFocused = value6.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.PlayerIntentHandler, out var value7))
		{
			PlayerIntentHandler = value7.As<NMultiplayerPlayerIntentHandler>();
		}
		if (info.TryGetProperty(PropertyName._stateDisplay, out var value8))
		{
			_stateDisplay = value8.As<NCreatureStateDisplay>();
		}
		if (info.TryGetProperty(PropertyName._intentFadeTween, out var value9))
		{
			_intentFadeTween = value9.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._shakeTween, out var value10))
		{
			_shakeTween = value10.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._isRemotePlayerOrPet, out var value11))
		{
			_isRemotePlayerOrPet = value11.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._isInBestiary, out var value12))
		{
			_isInBestiary = value12.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._tempScale, out var value13))
		{
			_tempScale = value13.As<float>();
		}
		if (info.TryGetProperty(PropertyName._scaleTween, out var value14))
		{
			_scaleTween = value14.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._isInMultiselect, out var value15))
		{
			_isInMultiselect = value15.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._selectionReticle, out var value16))
		{
			_selectionReticle = value16.As<NSelectionReticle>();
		}
	}
}
