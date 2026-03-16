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
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName ConnectSpineAnimatorSignals = "ConnectSpineAnimatorSignals";

		public static readonly StringName UpdateBounds = "UpdateBounds";

		public static readonly StringName UpdateNavigation = "UpdateNavigation";

		public static readonly StringName OnFocus = "OnFocus";

		public static readonly StringName OnUnfocus = "OnUnfocus";

		public static readonly StringName OnTargetingStarted = "OnTargetingStarted";

		public static readonly StringName SetRemotePlayerFocused = "SetRemotePlayerFocused";

		public static readonly StringName HideHoverTips = "HideHoverTips";

		public static readonly StringName SetAnimationTrigger = "SetAnimationTrigger";

		public static readonly StringName GetCurrentAnimationLength = "GetCurrentAnimationLength";

		public static readonly StringName GetCurrentAnimationTimeRemaining = "GetCurrentAnimationTimeRemaining";

		public static readonly StringName ToggleIsInteractable = "ToggleIsInteractable";

		public static readonly StringName AnimDisableUi = "AnimDisableUi";

		public static readonly StringName AnimEnableUi = "AnimEnableUi";

		public static readonly StringName StartDeathAnim = "StartDeathAnim";

		public static readonly StringName StartReviveAnim = "StartReviveAnim";

		public static readonly StringName AnimTempRevive = "AnimTempRevive";

		public static readonly StringName ImmediatelySetIdle = "ImmediatelySetIdle";

		public static readonly StringName AnimHideIntent = "AnimHideIntent";

		public static readonly StringName SetScaleAndHue = "SetScaleAndHue";

		public static readonly StringName ScaleTo = "ScaleTo";

		public static readonly StringName SetDefaultScaleTo = "SetDefaultScaleTo";

		public static readonly StringName OstyScaleToSize = "OstyScaleToSize";

		public static readonly StringName AnimShake = "AnimShake";

		public static readonly StringName DoScaleTween = "DoScaleTween";

		public static readonly StringName SetOrbManagerPosition = "SetOrbManagerPosition";

		public static readonly StringName GetTopOfHitbox = "GetTopOfHitbox";

		public static readonly StringName GetBottomOfHitbox = "GetBottomOfHitbox";

		public static readonly StringName ShowMultiselectReticle = "ShowMultiselectReticle";

		public static readonly StringName HideMultiselectReticle = "HideMultiselectReticle";

		public static readonly StringName ShowSingleSelectReticle = "ShowSingleSelectReticle";

		public static readonly StringName HideSingleSelectReticle = "HideSingleSelectReticle";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName Hitbox = "Hitbox";

		public static readonly StringName OrbManager = "OrbManager";

		public static readonly StringName IsInteractable = "IsInteractable";

		public static readonly StringName VfxSpawnPosition = "VfxSpawnPosition";

		public static readonly StringName Visuals = "Visuals";

		public static readonly StringName Body = "Body";

		public static readonly StringName IntentContainer = "IntentContainer";

		public static readonly StringName IsPlayingDeathAnimation = "IsPlayingDeathAnimation";

		public static readonly StringName HasSpineAnimation = "HasSpineAnimation";

		public static readonly StringName IsFocused = "IsFocused";

		public static readonly StringName PlayerIntentHandler = "PlayerIntentHandler";

		public static readonly StringName _stateDisplay = "_stateDisplay";

		public static readonly StringName _intentFadeTween = "_intentFadeTween";

		public static readonly StringName _shakeTween = "_shakeTween";

		public static readonly StringName _isRemotePlayerOrPet = "_isRemotePlayerOrPet";

		public static readonly StringName _tempScale = "_tempScale";

		public static readonly StringName _scaleTween = "_scaleTween";

		public static readonly StringName _isInMultiselect = "_isInMultiselect";

		public static readonly StringName _selectionReticle = "_selectionReticle";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("combat/creature");

	private NCreatureStateDisplay _stateDisplay;

	private Tween? _intentFadeTween;

	private Tween? _shakeTween;

	private CreatureAnimator? _spineAnimator;

	private bool _isRemotePlayerOrPet;

	private float _tempScale = 1f;

	private Tween? _scaleTween;

	private bool _isInMultiselect;

	private NSelectionReticle _selectionReticle;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(_scenePath);

	public Task? DeathAnimationTask { get; set; }

	public CancellationTokenSource DeathAnimCancelToken { get; } = new CancellationTokenSource();

	public Control Hitbox { get; private set; }

	public NOrbManager? OrbManager { get; private set; }

	public bool IsInteractable { get; private set; } = true;

	public Creature Entity { get; private set; }

	public Vector2 VfxSpawnPosition => Visuals.VfxSpawnPosition.GlobalPosition;

	public NCreatureVisuals Visuals { get; private set; }

	public Node2D Body => Visuals.Body;

	public Control IntentContainer { get; private set; }

	public bool IsPlayingDeathAnimation => DeathAnimationTask != null;

	public bool HasSpineAnimation => Visuals.HasSpineAnimation;

	public MegaSprite? SpineController => Visuals.SpineBody;

	public bool IsFocused { get; private set; }

	public NMultiplayerPlayerIntentHandler? PlayerIntentHandler { get; private set; }

	public T? GetSpecialNode<T>(string name) where T : Node
	{
		return Visuals.GetNode<T>(name);
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
			CombatState? combatState = Entity.CombatState;
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
		MoveChild(Visuals, 0);
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
				_spineAnimator = Entity.Player.Character.GenerateAnimator(SpineController);
			}
			else
			{
				_spineAnimator = Entity.Monster.GenerateAnimator(SpineController);
				Visuals.SetUpSkin(Entity.Monster);
			}
			ConnectSpineAnimatorSignals();
			if (Entity.IsDead)
			{
				SetAnimationTrigger("Dead");
				MegaTrackEntry current = Visuals.SpineBody.GetAnimationState().GetCurrent(0);
				current.SetTrackTime(current.GetAnimationEnd());
			}
		}
		SetOrbManagerPosition();
		if (Entity.Monster != null)
		{
			ToggleIsInteractable(Entity.Monster.IsHealthBarVisible);
		}
		UpdateBounds(Visuals);
	}

	public override void _EnterTree()
	{
		base._EnterTree();
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
		DeathAnimCancelToken.Cancel();
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

	private void UpdateBounds(Node boundsContainer)
	{
		Control node = boundsContainer.GetNode<Control>("Bounds");
		Vector2 size = node.Size * Visuals.Scale / _tempScale;
		Vector2 vector = (node.GlobalPosition - base.GlobalPosition) / _tempScale;
		Hitbox.Size = size;
		Hitbox.GlobalPosition = base.GlobalPosition + vector;
		_selectionReticle.Size = size;
		_selectionReticle.GlobalPosition = base.GlobalPosition + vector;
		_selectionReticle.PivotOffset = _selectionReticle.Size / 2f;
		IntentContainer.Position = boundsContainer.GetNode<Marker2D>("IntentPos").Position - IntentContainer.Size / 2f;
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
		float num = (float)GetHashCode() / 100f;
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
		AnimHideIntent(0.4f);
		await Cmd.CustomScaledWait(0.25f, 0.4f);
	}

	public async Task RefreshIntents()
	{
		await UpdateIntent(Entity.CombatState.Players.Select((Player p) => p.Creature));
		await RevealIntents();
	}

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
		if (IsFocused)
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
		NPowerAppliedVfx vfx = NPowerAppliedVfx.Create(power, amount);
		if (vfx != null)
		{
			Callable.From(delegate
			{
				NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(vfx);
			}).CallDeferred();
		}
		if (power.ShouldPlayVfx)
		{
			SfxCmd.Play((power.GetTypeForAmount(amount) == PowerType.Buff) ? "event:/sfx/buff" : "event:/sfx/debuff");
		}
		if (power.GetTypeForAmount(power.Amount) == PowerType.Debuff)
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
		return SpineController.GetAnimationState().GetCurrent(0).GetAnimation()
			.GetDuration();
	}

	public float GetCurrentAnimationTimeRemaining()
	{
		MegaTrackEntry current = SpineController.GetAnimationState().GetCurrent(0);
		return current.GetTrackComplete() - current.GetTrackTime();
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
		tween.TweenProperty(Visuals, "modulate:a", 0f, 0.20000000298023224);
		tween.TweenCallback(Callable.From(ImmediatelySetIdle));
		tween.TweenProperty(Visuals, "modulate:a", 1f, 0.20000000298023224);
	}

	private void ImmediatelySetIdle()
	{
		_spineAnimator?.SetTrigger("Idle");
		MegaTrackEntry current = Visuals.SpineBody.GetAnimationState().GetCurrent(0);
		current.SetMixDuration(0f);
		current.SetTrackTime(current.GetAnimationEnd());
	}

	private async Task AnimDie(bool shouldRemove, CancellationToken cancelToken)
	{
		Tween disableUiTween = AnimDisableUi();
		Hitbox.MouseFilter = MouseFilterEnum.Ignore;
		if (!RunManager.Instance.IsSinglePlayerOrFakeMultiplayer)
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
			if (cancelToken.IsCancellationRequested)
			{
				return;
			}
		}
		else
		{
			MonsterModel monster = Entity.Monster;
			if (monster != null && monster.HasDeathAnimLengthOverride)
			{
				await Cmd.Wait(Entity.Monster.DeathAnimLengthOverride, cancelToken, ignoreCombatEnd: true);
			}
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
				parent.MoveChild(nMonsterDeathVfx, GetIndex());
				fadeVfx = nMonsterDeathVfx?.PlayVfx();
			}
			if (SaveManager.Instance.PrefsSave.FastMode != FastModeType.Instant)
			{
				if (disableUiTween.IsValid() && disableUiTween.IsRunning())
				{
					await ToSignal(disableUiTween, Tween.SignalName.Finished);
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
		if (Entity.Monster is Osty)
		{
			OstyScaleToSize(0f, 0.75f);
		}
	}

	public void AnimHideIntent(float delay = 0f)
	{
		_intentFadeTween?.Kill();
		_intentFadeTween = CreateTween().SetParallel();
		PropertyTweener propertyTweener = _intentFadeTween.TweenProperty(IntentContainer, "modulate:a", 0f, 0.5);
		if (delay > 0f)
		{
			propertyTweener.SetDelay(delay);
		}
	}

	public void SetScaleAndHue(float scale, float hue)
	{
		Visuals.SetScaleAndHue(scale, hue);
		UpdateBounds(Visuals);
	}

	public void ScaleTo(float size, float duration)
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

	public void OstyScaleToSize(float ostyHealth, float duration)
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
		if ((_shakeTween == null || !_shakeTween.IsRunning()) && !Visuals.IsPlayingHurtAnimation())
		{
			Visuals.Position = Vector2.Zero;
			_shakeTween = CreateTween();
			_shakeTween.TweenMethod(Callable.From(delegate(float t)
			{
				Visuals.Position = Vector2.Right * 10f * Mathf.Sin(t * 4f) * Mathf.Sin(t / 2f);
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

	public Vector2 GetTopOfHitbox()
	{
		return Hitbox.GlobalPosition + new Vector2(Hitbox.Size.X * 0.5f, 0f);
	}

	public Vector2 GetBottomOfHitbox()
	{
		return Hitbox.GlobalPosition + new Vector2(Hitbox.Size.X * 0.5f, Hitbox.Size.Y);
	}

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

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(35);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ConnectSpineAnimatorSignals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateBounds, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "boundsNodeName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
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
			AnimHideIntent(VariantUtils.ConvertTo<float>(in args[0]));
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
			ScaleTo(VariantUtils.ConvertTo<float>(in args[0]), VariantUtils.ConvertTo<float>(in args[1]));
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
			OstyScaleToSize(VariantUtils.ConvertTo<float>(in args[0]), VariantUtils.ConvertTo<float>(in args[1]));
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
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

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
		return base.HasGodotClassMethod(in method);
	}

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
		if (name == PropertyName.VfxSpawnPosition)
		{
			value = VariantUtils.CreateFrom<Vector2>(VfxSpawnPosition);
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

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._stateDisplay, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._intentFadeTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._shakeTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isRemotePlayerOrPet, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._tempScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._scaleTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Hitbox, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.OrbManager, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isInMultiselect, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._selectionReticle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsInteractable, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName.VfxSpawnPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Visuals, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Body, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.IntentContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsPlayingDeathAnimation, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.HasSpineAnimation, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsFocused, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.PlayerIntentHandler, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

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
		info.AddProperty(PropertyName._tempScale, Variant.From(in _tempScale));
		info.AddProperty(PropertyName._scaleTween, Variant.From(in _scaleTween));
		info.AddProperty(PropertyName._isInMultiselect, Variant.From(in _isInMultiselect));
		info.AddProperty(PropertyName._selectionReticle, Variant.From(in _selectionReticle));
	}

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
		if (info.TryGetProperty(PropertyName._tempScale, out var value12))
		{
			_tempScale = value12.As<float>();
		}
		if (info.TryGetProperty(PropertyName._scaleTween, out var value13))
		{
			_scaleTween = value13.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._isInMultiselect, out var value14))
		{
			_isInMultiselect = value14.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._selectionReticle, out var value15))
		{
			_selectionReticle = value15.As<NSelectionReticle>();
		}
	}
}
