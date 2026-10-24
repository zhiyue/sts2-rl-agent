using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Achievements;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Ftue;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Combat;

public class CombatManager
{
	private sealed record PendingLossState(CombatState State, CombatRoom Room);

	public const int baseHandDrawCount = 5;

	private readonly Lock _playerReadyLock = new Lock();

	private readonly HashSet<Player> _playersReadyToEndTurn = new HashSet<Player>();

	private readonly HashSet<Player> _playersReadyToBeginEnemyTurn = new HashSet<Player>();

	private readonly List<Player> _playersTakingExtraTurn = new List<Player>();

	/// <summary>
	/// True while <see cref="M:MegaCrit.Sts2.Core.Combat.CombatManager.StartTurn(System.Func{System.Threading.Tasks.Task})" /> is setting up the player turn but has not yet moved to the Play phase.
	/// Guarded by <see cref="F:MegaCrit.Sts2.Core.Combat.CombatManager._playerReadyLock" />.
	/// </summary>
	private bool _inPlayerTurnSetup;

	/// <summary>
	/// If a card ends the turn while <see cref="F:MegaCrit.Sts2.Core.Combat.CombatManager._inPlayerTurnSetup" /> is true (e.g. Void Form auto-played by
	/// Whispering Earring during the AutoPrePlay phase), the end-of-turn transition is stored in this field and run
	/// only once <see cref="M:MegaCrit.Sts2.Core.Combat.CombatManager.StartTurn(System.Func{System.Threading.Tasks.Task})" /> has caused us to move to the Play phase, so it never runs concurrently with
	/// the end of StartTurn (this would be a race condition, and could leave the turn-transition sequence stuck
	/// mid-way, never advancing to the next turn).
	/// A normal end-turn happens after the Play phase has started, so it is never deferred.
	/// Guarded by <see cref="F:MegaCrit.Sts2.Core.Combat.CombatManager._playerReadyLock" />.
	/// </summary>
	private Func<Task>? _deferredEndTurnTransition;

	private CombatState? _state;

	private CancellationTokenSource? _combatCts;

	private PendingLossState? _pendingLoss;

	/// <summary>
	/// Set to true when the player should not be able to interact with their hand or any potions.
	/// </summary>
	private bool _playerActionsDisabled;

	private readonly Dictionary<Player, int> _cardOrPotionEffectDepth = new Dictionary<Player, int>();

	public static CombatManager Instance { get; } = new CombatManager();

	private CancellationToken CombatCt => _combatCts?.Token ?? default(CancellationToken);

	/// <summary>
	/// WARNING: ONLY USE THIS IN TESTS!
	/// See <see cref="M:MegaCrit.Sts2.Core.Combat.CombatManager.DebugForceTopCardOnNextShuffle(MegaCrit.Sts2.Core.Models.CardModel)" />.
	/// </summary>
	public CardModel? DebugForcedTopCardOnNextShuffle { get; private set; }

	public bool IsPaused { get; private set; }

	public bool PlayerActionsDisabled
	{
		get
		{
			return _playerActionsDisabled;
		}
		private set
		{
			if (_playerActionsDisabled != value)
			{
				_playerActionsDisabled = value;
				this.PlayerActionsDisabledChanged?.Invoke(_state);
			}
		}
	}

	/// <summary>
	/// The list of players in the current turn that are taking an extra turn.
	/// Normally empty; only non-empty if there are players that used extra-turn-taking effects like
	/// <see cref="T:MegaCrit.Sts2.Core.Models.Relics.PaelsEye" />.
	/// Returns a snapshot copy for thread safety.
	/// </summary>
	public IReadOnlyList<Player> PlayersTakingExtraTurn
	{
		get
		{
			using (_playerReadyLock.EnterScope())
			{
				return _playersTakingExtraTurn.ToList();
			}
		}
	}

	/// <summary>
	/// True when the enemy turn has started (TurnStarted has fired for the enemy side).
	/// Set right before TurnStarted fires for enemy turns, cleared when switching to player turn.
	/// </summary>
	public bool IsEnemyTurnStarted { get; private set; }

	/// <summary>
	/// Set to true in the time between when all players are ready to begin the enemy turn and when the enemy turn begins.
	/// </summary>
	public bool EndingPlayerTurnPhaseTwo { get; private set; }

	/// <summary>
	/// Set to true in the time during phase one of the end of the player's turn.
	/// </summary>
	public bool EndingPlayerTurnPhaseOne { get; private set; }

	public CombatStateTracker StateTracker { get; }

	public CombatHistory History { get; }

	/// <summary>
	/// Is the combat currently in progress?
	/// True when the combat is done being initialized and has fully started.
	/// False when:
	/// * The combat is first being initialized.
	/// * The combat is ending (the last monster has been killed).
	/// * We're in a non-combat room.
	/// </summary>
	public bool IsInProgress { get; private set; }

	/// <summary>
	/// Is a new combat currently being set up?
	/// True from the start of <see cref="M:MegaCrit.Sts2.Core.Combat.CombatManager.SetUpCombat(MegaCrit.Sts2.Core.Combat.CombatState)" /> until <see cref="P:MegaCrit.Sts2.Core.Combat.CombatManager.IsInProgress" /> flips true in
	/// <see cref="M:MegaCrit.Sts2.Core.Combat.CombatManager.StartCombatInternal" />. During this window <see cref="P:MegaCrit.Sts2.Core.Combat.CombatManager.IsInProgress" /> is still false, so it lets
	/// callers distinguish "combat is starting" (where combat hooks that run during setup, like the initial deck
	/// shuffle, must still fire) from "combat is over or ending".
	/// </summary>
	public bool IsStarting { get; private set; }

	/// <summary>
	/// Is combat about to end due to player death?
	/// True when LoseCombat() has been called but the loss hasn't been processed yet.
	/// This allows effects to bail out early while still letting the current action complete.
	/// </summary>
	public bool IsAboutToLose => _pendingLoss != null;

	/// <summary>
	/// Is the combat in the process of ending (but still in progress)?
	/// True when combat is in progress but all the enemies are dead, and there is nothing stopping combat from ending
	/// (e.g. Phrog Parasite spawning in new enemies).
	/// Also true when a pending loss is waiting to be processed.
	/// False when
	/// * Combat is in progress and 1+ primary enemies are still alive.
	/// * Combat is not in progress.
	/// </summary>
	public bool IsEnding
	{
		get
		{
			if (!IsInProgress)
			{
				return false;
			}
			if (_pendingLoss != null)
			{
				return true;
			}
			if (_state != null && _state.Enemies.Any((Creature e) => e != null && e.IsAlive && e.IsPrimaryEnemy))
			{
				return false;
			}
			if (Hook.ShouldStopCombatFromEnding(_state))
			{
				return false;
			}
			return true;
		}
	}

	/// <summary>
	/// Has this combat ended (or is it in the process of ending)?
	/// When you want to skip/cancel an effect because combat is not in progress, you should usually use this instead of
	/// <see cref="P:MegaCrit.Sts2.Core.Combat.CombatManager.IsEnding" /> or !<see cref="P:MegaCrit.Sts2.Core.Combat.CombatManager.IsInProgress" />, because they can return unexpected results at certain
	/// boundary points.
	/// </summary>
	public bool IsOverOrEnding
	{
		get
		{
			if (!IsEnding)
			{
				return !IsInProgress;
			}
			return true;
		}
	}

	/// <summary>
	/// Fired after combat is set up.
	/// Note that this happens a little bit before combat actually begins.
	/// </summary>
	public event Action<CombatState>? CombatSetUp;

	/// <summary>
	/// Fired when combat ends.
	/// </summary>
	public event Action<CombatRoom>? CombatEnded;

	/// <summary>
	/// Fired when combat is won.
	/// </summary>
	public event Action<CombatRoom>? CombatWon;

	/// <summary>
	/// Fired whenever the arrangement of creatures in the combat changes. Specifically, when:
	/// * A creature is added.
	/// * A creature is removed.
	/// * A creature's position changes.
	/// </summary>
	public event Action<CombatState>? CreaturesChanged;

	/// <summary>
	/// Fired whenever a new turn starts.
	/// </summary>
	public event Action<CombatState>? TurnStarted;

	/// <summary>
	/// Fired whenever a turn ends.
	/// </summary>
	public event Action<CombatState>? TurnEnded;

	/// <summary>
	/// Fired whenever a player ends their turn. Remember that, in multiplayer, this is not the same as switching to the
	/// enemy's turn.
	/// </summary>
	public event Action<Player, bool>? PlayerEndedTurn;

	/// <summary>
	/// Fired whenever a player un-does the end of their turn.
	/// </summary>
	public event Action<Player>? PlayerUnendedTurn;

	/// <summary>
	/// Fired when all players have fully committed to ending turn and all player actions are done (including end of turn
	/// hooks like Well-Laid Plans), but before the player hand flush.
	/// </summary>
	public event Action<CombatState>? AboutToSwitchToEnemyTurn;

	/// <summary>
	/// Fired when the local player's actions become disabled or enabled.
	/// </summary>
	public event Action<CombatState>? PlayerActionsDisabledChanged;

	/// <summary>
	/// THIS IS TEMPORARY AND SHOULD ONLY BE USED IN TESTS
	/// </summary>
	/// <returns></returns>
	public CombatState? DebugOnlyGetState()
	{
		return _state;
	}

	/// <summary>
	/// Sets <see cref="P:MegaCrit.Sts2.Core.Entities.Players.PlayerCombatState.Phase" /> to the same value for all players.
	/// </summary>
	private void SetPhaseForAllPlayers(PlayerTurnPhase phase)
	{
		if (_state == null)
		{
			return;
		}
		foreach (Player player in _state.Players)
		{
			if (player.PlayerCombatState != null)
			{
				player.PlayerCombatState.Phase = phase;
			}
		}
	}

	/// <summary>
	/// True while a <see cref="M:MegaCrit.Sts2.Core.Models.CardModel.OnPlay(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Entities.Cards.CardPlay)" /> or a <see cref="M:MegaCrit.Sts2.Core.Models.PotionModel.OnUse(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Entities.Creatures.Creature)" /> effect body is currently
	/// executing for <paramref name="player" />, including nested auto-plays (e.g. a Sly card auto-played when
	/// discarded). Used to avoid premature hand-empty triggers while that player's effect is mid-resolution.
	/// </summary>
	public bool IsExecutingCardOrPotionEffect(Player player)
	{
		return _cardOrPotionEffectDepth.GetValueOrDefault(player) > 0;
	}

	/// <summary>
	/// Marks the start of a <see cref="M:MegaCrit.Sts2.Core.Models.CardModel.OnPlay(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Entities.Cards.CardPlay)" /> or <see cref="M:MegaCrit.Sts2.Core.Models.PotionModel.OnUse(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Entities.Creatures.Creature)" /> effect body for
	/// <paramref name="player" />, incrementing their effect-nesting depth. Must be paired with a
	/// <see cref="M:MegaCrit.Sts2.Core.Combat.CombatManager.EndCardOrPotionEffect(MegaCrit.Sts2.Core.Entities.Players.Player)" /> in a finally block so the depth stays balanced even if the effect throws or
	/// the player dies mid-play.
	/// </summary>
	public void BeginCardOrPotionEffect(Player player)
	{
		_cardOrPotionEffectDepth[player] = _cardOrPotionEffectDepth.GetValueOrDefault(player) + 1;
	}

	/// <summary>
	/// Marks the end of a <see cref="M:MegaCrit.Sts2.Core.Models.CardModel.OnPlay(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Entities.Cards.CardPlay)" /> or <see cref="M:MegaCrit.Sts2.Core.Models.PotionModel.OnUse(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext,MegaCrit.Sts2.Core.Entities.Creatures.Creature)" /> effect body for
	/// <paramref name="player" />, decrementing their effect-nesting depth (and removing the entry once it reaches
	/// zero). Pairs with <see cref="M:MegaCrit.Sts2.Core.Combat.CombatManager.BeginCardOrPotionEffect(MegaCrit.Sts2.Core.Entities.Players.Player)" />; call it from a finally block.
	/// </summary>
	public void EndCardOrPotionEffect(Player player)
	{
		int num = _cardOrPotionEffectDepth.GetValueOrDefault(player) - 1;
		if (num <= 0)
		{
			_cardOrPotionEffectDepth.Remove(player);
		}
		else
		{
			_cardOrPotionEffectDepth[player] = num;
		}
	}

	private CombatManager()
	{
		History = new CombatHistory();
		StateTracker = new CombatStateTracker(this);
	}

	public void SetUpCombat(CombatState state)
	{
		if (_state != null)
		{
			throw new InvalidOperationException("Make sure to reset the combat before setting up a new one.");
		}
		IsStarting = true;
		_state = state;
		_state.MultiplayerScalingModel?.OnCombatEntered(_state);
		StateTracker.SetState(state);
		using (_playerReadyLock.EnterScope())
		{
			_playersTakingExtraTurn.Clear();
		}
		foreach (Player player in state.Players)
		{
			player.ResetCombatState();
		}
		foreach (Player player2 in state.Players)
		{
			player2.PopulateCombatState(player2.RunState.Rng.Shuffle, state);
		}
		NetCombatCardDb.Instance.StartCombat(state.Players);
		foreach (Creature creature in state.Creatures)
		{
			AddCreature(creature);
		}
		this.CombatSetUp?.Invoke(state);
	}

	public void AfterCombatRoomLoaded()
	{
		_combatCts?.Cancel();
		_combatCts = new CancellationTokenSource();
		TaskHelper.RunSafely(StartCombatInternal());
	}

	public async Task StartCombatInternal()
	{
		await RunManager.Instance.ActionExecutor.FinishedExecutingActions();
		if (_state.Encounter.HasBgm)
		{
			NRunMusicController.Instance?.PlayCustomMusic(_state.Encounter.CustomBgm);
		}
		foreach (Creature creature in _state.Creatures)
		{
			await AfterCreatureAdded(creature);
			CombatCt.ThrowIfCancellationRequested();
		}
		RunManager.Instance.ActionExecutor.Pause();
		RunManager.Instance.ActionQueueSynchronizer.SetCombatState(ActionSynchronizerCombatState.NotPlayPhase);
		IsInProgress = true;
		IsStarting = false;
		await Hook.BeforeCombatStart(_state.RunState, _state);
		CombatCt.ThrowIfCancellationRequested();
		NRunMusicController.Instance?.UpdateTrack();
		NCombatRulesFtue ftue = null;
		if (SaveManager.Instance.SeenFtue("combat_rules_ftue"))
		{
			NCombatRoom.Instance?.AddChildSafely(NCombatStartBanner.Create());
		}
		else
		{
			ftue = NCombatRulesFtue.Create();
			NModalContainer.Instance?.Add(ftue, showBackstop: false);
		}
		await Cmd.CustomScaledWait(0.5f, 1f);
		CombatCt.ThrowIfCancellationRequested();
		await StartTurn();
		ftue?.Start();
	}

	private async Task StartTurn(Func<Task>? actionDuringEnemyTurn = null)
	{
		if (!IsInProgress)
		{
			return;
		}
		CombatCt.ThrowIfCancellationRequested();
		SetPhaseForAllPlayers(PlayerTurnPhase.None);
		bool isExtraPlayerTurn;
		List<Creature> creaturesStartingTurn;
		List<Player> playersStartingTurn;
		using (_playerReadyLock.EnterScope())
		{
			isExtraPlayerTurn = _playersTakingExtraTurn.Count > 0;
			CombatState? state = _state;
			if (state != null && state.CurrentSide == CombatSide.Player && isExtraPlayerTurn)
			{
				creaturesStartingTurn = _playersTakingExtraTurn.Select((Player p) => p.Creature).ToList();
				playersStartingTurn = _playersTakingExtraTurn.ToList();
			}
			else
			{
				creaturesStartingTurn = _state?.CreaturesOnCurrentSide.ToList() ?? new List<Creature>();
				CombatState? state2 = _state;
				playersStartingTurn = ((state2 == null || state2.CurrentSide != CombatSide.Player) ? new List<Player>() : (_state?.Players.ToList() ?? new List<Player>()));
			}
		}
		foreach (Creature item2 in creaturesStartingTurn)
		{
			if (_state != null)
			{
				item2.BeforeTurnStart(_state.CurrentSide);
			}
		}
		if (_state != null)
		{
			await Hook.BeforeSideTurnStart(_state, _state.CurrentSide, creaturesStartingTurn);
			CombatCt.ThrowIfCancellationRequested();
		}
		CombatState? state3 = _state;
		if (state3 != null && state3.CurrentSide == CombatSide.Player)
		{
			SetPhaseForAllPlayers(PlayerTurnPhase.Start);
			PlayerActionsDisabled = false;
			using (_playerReadyLock.EnterScope())
			{
				_playersReadyToEndTurn.Clear();
				_playersReadyToBeginEnemyTurn.Clear();
				_inPlayerTurnSetup = true;
				_deferredEndTurnTransition = null;
			}
			int num = LocalContext.GetMe(playersStartingTurn)?.PlayerCombatState?.TurnNumber ?? (-1);
			if (num > 1)
			{
				NCombatRoom.Instance?.AddChildSafely(NPlayerTurnBanner.Create(num));
			}
			if (!isExtraPlayerTurn)
			{
				foreach (Creature enemy in _state.Enemies)
				{
					enemy.PrepareForNextTurn(_state.PlayerCreatures);
				}
			}
		}
		else
		{
			NCombatRoom.Instance?.AddChildSafely(NEnemyTurnBanner.Create());
		}
		await Cmd.CustomScaledWait(0.5f, 0.8f);
		CombatCt.ThrowIfCancellationRequested();
		foreach (Creature item3 in creaturesStartingTurn)
		{
			if (_state != null)
			{
				await item3.AfterTurnStart(_state.CurrentSide);
				CombatCt.ThrowIfCancellationRequested();
			}
		}
		foreach (Creature item4 in creaturesStartingTurn)
		{
			if (_state != null)
			{
				await Hook.AfterBlockCleared(_state, item4);
				CombatCt.ThrowIfCancellationRequested();
			}
		}
		List<(HookPlayerChoiceContext, Task)> setupPlayerTurnContext = new List<(HookPlayerChoiceContext, Task)>();
		foreach (Player item5 in playersStartingTurn)
		{
			if (LocalContext.NetId.HasValue)
			{
				HookPlayerChoiceContext playerChoiceContext = new HookPlayerChoiceContext(item5, LocalContext.NetId.Value, GameActionType.CombatPlayPhaseOnly);
				Task task = SetupPlayerTurn(item5, playerChoiceContext);
				await playerChoiceContext.WaitForPauseOrCompletionWithoutAssigningTask(task);
				CombatCt.ThrowIfCancellationRequested();
				setupPlayerTurnContext.Add((playerChoiceContext, task));
			}
		}
		if (_state != null)
		{
			await Hook.AfterSideTurnStart(_state, _state.CurrentSide, creaturesStartingTurn);
			CombatCt.ThrowIfCancellationRequested();
		}
		CombatState? state4 = _state;
		if (state4 != null && state4.CurrentSide == CombatSide.Player)
		{
			foreach (Player item6 in playersStartingTurn)
			{
				if (item6.PlayerCombatState != null && LocalContext.NetId.HasValue)
				{
					HookPlayerChoiceContext hookPlayerChoiceContext = new HookPlayerChoiceContext(item6, LocalContext.NetId.Value, GameActionType.CombatPlayPhaseOnly);
					Task task2 = item6.PlayerCombatState.OrbQueue.AfterTurnStart(hookPlayerChoiceContext);
					await hookPlayerChoiceContext.AssignTaskAndWaitForPauseOrCompletion(task2);
					CombatCt.ThrowIfCancellationRequested();
				}
			}
			RunManager.Instance.ChecksumTracker.GenerateChecksum("After player turn start", null);
			if (_state == null)
			{
				return;
			}
			foreach (Player player2 in _state.Players)
			{
				if (player2.Creature.IsDead || !playersStartingTurn.Contains(player2))
				{
					Log.Info($"Setting player {player2.NetId} to ready at start of turn. IsDead: {player2.Creature.IsDead}. IsStartingTurn: {playersStartingTurn.Contains(player2)}");
					SetReadyToEndTurn(player2, canBackOut: false);
					if (AllPlayersReadyToEndTurn())
					{
						ReleaseDeferredEndTurnTransitionIfNeeded();
						return;
					}
				}
			}
			foreach (var item7 in playersStartingTurn.Zip(setupPlayerTurnContext))
			{
				(HookPlayerChoiceContext, Task) item = item7.Second;
				var (player, _) = item7;
				var (hookPlayerChoiceContext2, setupPlayerTurnTask) = item;
				if (_state == null)
				{
					ReleaseDeferredEndTurnTransitionIfNeeded();
					return;
				}
				if (!player.Creature.IsDead)
				{
					Task task3 = RunAutoPrePlayPhase(hookPlayerChoiceContext2, setupPlayerTurnTask, player);
					await hookPlayerChoiceContext2.AssignTaskAndWaitForPauseOrCompletion(task3);
					CombatCt.ThrowIfCancellationRequested();
				}
			}
			await CheckWinCondition();
			CombatCt.ThrowIfCancellationRequested();
			if (IsInProgress)
			{
				RunManager.Instance.ActionExecutor.Unpause();
				RunManager.Instance.ActionQueueSynchronizer.SetCombatState(ActionSynchronizerCombatState.PlayPhase);
				IsEnemyTurnStarted = false;
				using (_playerReadyLock.EnterScope())
				{
					_inPlayerTurnSetup = false;
				}
				this.TurnStarted?.Invoke(_state);
			}
			ReleaseDeferredEndTurnTransitionIfNeeded();
		}
		else
		{
			IsEnemyTurnStarted = true;
			if (_state != null)
			{
				this.TurnStarted?.Invoke(_state);
			}
			RunManager.Instance.ChecksumTracker.GenerateChecksum("After enemy turn start", null);
			await WaitForUnpause();
			CombatCt.ThrowIfCancellationRequested();
			await CheckWinCondition();
			CombatCt.ThrowIfCancellationRequested();
			if (IsInProgress)
			{
				await ExecuteEnemyTurn(actionDuringEnemyTurn);
			}
		}
	}

	/// <summary>
	/// Awaits the player's setup task, then runs the auto-pre-play hooks, transitioning the player's phase
	/// from <see cref="F:MegaCrit.Sts2.Core.Combat.PlayerTurnPhase.Start" /> -&gt; <see cref="F:MegaCrit.Sts2.Core.Combat.PlayerTurnPhase.AutoPrePlay" /> -&gt; <see cref="F:MegaCrit.Sts2.Core.Combat.PlayerTurnPhase.Play" />.
	/// The setup await ensures a player whose setup is paused (making a <see cref="T:MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext" />)
	/// stays in <see cref="F:MegaCrit.Sts2.Core.Combat.PlayerTurnPhase.Start" /> until their setup actually completes.
	/// </summary>
	private async Task RunAutoPrePlayPhase(HookPlayerChoiceContext playerChoiceContext, Task setupPlayerTurnTask, Player player)
	{
		await setupPlayerTurnTask;
		player.PlayerCombatState.Phase = PlayerTurnPhase.AutoPrePlay;
		await Hook.AfterAutoPrePlayPhaseEntered(playerChoiceContext, _state, player);
		player.PlayerCombatState.Phase = PlayerTurnPhase.Play;
	}

	/// <summary>
	/// Sets up a player's turn by resetting energy, drawing cards, and firing start-of-turn hooks.
	/// If the player's turn start executes a player choice (e.g. Mayhem plays Cosmic Indifference), then the entire
	/// sequence is paused for this player. However, other players' turn start sequences may continue, and they may
	/// play cards while this is occuring.
	/// </summary>
	/// <param name="player">The player whose turn to setup.</param>
	/// <param name="playerChoiceContext">The player choice context to pass to hooks that take it.</param>
	private async Task SetupPlayerTurn(Player player, HookPlayerChoiceContext playerChoiceContext)
	{
		if (player.Creature.IsDead)
		{
			return;
		}
		if (_state == null || player.PlayerCombatState == null)
		{
			Log.Warn($"Combat state is null. Assuming that the run has been cleaned up. (CombatState: {_state} PlayerCombatState: {player.PlayerCombatState})");
			return;
		}
		CombatState state = _state;
		if (Hook.ShouldPlayerResetEnergy(state, player))
		{
			SfxCmd.Play("event:/sfx/ui/gain_energy");
			player.PlayerCombatState.ResetEnergy();
		}
		else
		{
			player.PlayerCombatState.AddMaxEnergyToCurrent();
		}
		await Hook.AfterEnergyReset(state, player);
		CombatCt.ThrowIfCancellationRequested();
		await Hook.BeforeHandDraw(state, player, playerChoiceContext);
		CombatCt.ThrowIfCancellationRequested();
		decimal handDraw = Hook.ModifyHandDraw(state, player, 5m, out IEnumerable<AbstractModel> modifiers);
		await Hook.AfterModifyingHandDraw(state, modifiers);
		CombatCt.ThrowIfCancellationRequested();
		if (player.PlayerCombatState.TurnNumber == 1)
		{
			CardPile pile = PileType.Draw.GetPile(player);
			List<CardModel> list = pile.Cards.Where((CardModel c) => c.Enchantment?.ShouldStartAtBottomOfDrawPile ?? false).ToList();
			foreach (CardModel item in list)
			{
				pile.MoveToBottomInternal(item);
			}
			List<CardModel> list2 = pile.Cards.Where((CardModel c) => c.Keywords.Contains(CardKeyword.Innate)).Except(list).ToList();
			foreach (CardModel item2 in list2)
			{
				pile.MoveToTopInternal(item2);
			}
			handDraw = Math.Max(handDraw, list2.Count);
			handDraw = Math.Min(handDraw, CardPile.MaxCardsInHand);
		}
		await CardPileCmd.Draw(playerChoiceContext, handDraw, player, fromHandDraw: true);
		CombatCt.ThrowIfCancellationRequested();
		await Hook.AfterPlayerTurnStart(state, playerChoiceContext, player);
	}

	/// <summary>
	/// Called in EndPlayerTurnAction to indicate that the player is ready to execute end-of-turn events.
	/// </summary>
	/// <param name="player">The player that readied up.</param>
	/// <param name="canBackOut">In multiplayer, notes if the player is allowed to back out of ending their turn.</param>
	/// <param name="actionDuringEnemyTurn">Optional action to execute during the enemy turn. This is useful for tests.</param>
	public void SetReadyToEndTurn(Player player, bool canBackOut, Func<Task>? actionDuringEnemyTurn = null)
	{
		using (_playerReadyLock.EnterScope())
		{
			if (_playersReadyToEndTurn.Contains(player))
			{
				return;
			}
			_playersReadyToEndTurn.Add(player);
		}
		this.PlayerEndedTurn?.Invoke(player, canBackOut);
		if (!AllPlayersReadyToEndTurn())
		{
			return;
		}
		Log.Debug("All players ready to end turn");
		GameAction runningAction = RunManager.Instance.ActionExecutor.CurrentlyRunningAction;
		Func<Task> func = ((runningAction == null || !ActionQueueSet.IsGameActionPlayerDriven(runningAction)) ? ((Func<Task>)(() => AfterAllPlayersReadyToEndTurn(actionDuringEnemyTurn))) : ((Func<Task>)(() => WaitForActionThenEndTurn(runningAction, actionDuringEnemyTurn))));
		bool inPlayerTurnSetup;
		using (_playerReadyLock.EnterScope())
		{
			inPlayerTurnSetup = _inPlayerTurnSetup;
			if (inPlayerTurnSetup)
			{
				_deferredEndTurnTransition = func;
			}
		}
		if (!inPlayerTurnSetup)
		{
			TaskHelper.RunSafely(func());
		}
	}

	/// <summary>
	/// Runs any end-of-turn transition that was deferred while the player turn was being set up (see
	/// <see cref="M:MegaCrit.Sts2.Core.Combat.CombatManager.SetReadyToEndTurn(MegaCrit.Sts2.Core.Entities.Players.Player,System.Boolean,System.Func{System.Threading.Tasks.Task})" />). Must be called on every exit path of the player-turn setup in
	/// <see cref="M:MegaCrit.Sts2.Core.Combat.CombatManager.StartTurn(System.Func{System.Threading.Tasks.Task})" /> so a deferred transition is never dropped.
	/// </summary>
	private void ReleaseDeferredEndTurnTransitionIfNeeded()
	{
		Func<Task> deferredEndTurnTransition;
		using (_playerReadyLock.EnterScope())
		{
			_inPlayerTurnSetup = false;
			deferredEndTurnTransition = _deferredEndTurnTransition;
			_deferredEndTurnTransition = null;
		}
		if (deferredEndTurnTransition != null)
		{
			TaskHelper.RunSafely(deferredEndTurnTransition());
		}
	}

	public void UndoReadyToEndTurn(Player player)
	{
		using (_playerReadyLock.EnterScope())
		{
			_playersReadyToEndTurn.Remove(player);
		}
		if (LocalContext.IsMe(player))
		{
			PlayerActionsDisabled = false;
		}
		this.PlayerUnendedTurn?.Invoke(player);
	}

	/// <summary>
	/// Call this when the end turn button is pressed to disable local player actions until the start of the next turn.
	/// In multiplayer, this prevents the player from playing cards after they have ended turn.
	/// In both SP and MP, this prevents the player from playing cards before the AfterTurnStart hook has run.
	/// It's important that we do this when the end turn button is pressed, instead of when the EndTurnAction is
	/// processed, because the player might try to execute actions while the end turn action is waiting in the queue.
	/// This is a little fragile; if actions do slip through in MP, it has the potential to cause a state divergence.
	/// Revisit if needed - we might need to discard actions on the host side (which ends up being way more complicated).
	/// </summary>
	public void OnEndedTurnLocally()
	{
		PlayerActionsDisabled = true;
	}

	/// <summary>
	/// Called in ReadyToBeginEnemyTurnAction to indicate that the player is ready to switch to the monster turn (or
	/// extra player turn, if necessary). Note that this is called automatically, and is not player-driven.
	/// </summary>
	/// <param name="player">The player that is ready to switch sides.</param>
	/// <param name="actionDuringEnemyTurn">Optional action to execute during the enemy turn. This is useful for tests.</param>
	public void SetReadyToBeginEnemyTurn(Player player, Func<Task>? actionDuringEnemyTurn = null)
	{
		if (!IsInProgress)
		{
			Log.Error("Trying to set player ready to begin enemy turn, but combat is over!");
		}
		bool flag;
		using (_playerReadyLock.EnterScope())
		{
			_playersReadyToBeginEnemyTurn.Add(player);
			flag = _playersReadyToBeginEnemyTurn.Count == _state.Players.Count && _state.CurrentSide == CombatSide.Player;
		}
		if (flag || RunManager.Instance.NetService.Type == NetGameType.Singleplayer)
		{
			TaskHelper.RunSafely(AfterAllPlayersReadyToBeginEnemyTurn(actionDuringEnemyTurn));
		}
	}

	/// <returns>
	/// True if the passed player has hit the end turn button, and the next player turn has not yet begun.
	/// </returns>
	public bool IsPlayerReadyToEndTurn(Player player)
	{
		using (_playerReadyLock.EnterScope())
		{
			return _playersReadyToEndTurn.Contains(player);
		}
	}

	public bool AllPlayersReadyToEndTurn()
	{
		bool flag;
		using (_playerReadyLock.EnterScope())
		{
			flag = _playersReadyToEndTurn.Count == _state.Players.Count;
		}
		if (!RunManager.Instance.IsSingleplayerOrFakeMultiplayer)
		{
			if (flag)
			{
				return _state.CurrentSide == CombatSide.Player;
			}
			return false;
		}
		return true;
	}

	private async Task EndEnemyTurn()
	{
		if (IsInProgress)
		{
			CombatCt.ThrowIfCancellationRequested();
			if (_state.CurrentSide != CombatSide.Enemy)
			{
				throw new InvalidOperationException($"EndEnemyTurn called while the current side is {_state.CurrentSide}!");
			}
			await WaitForUnpause();
			CombatCt.ThrowIfCancellationRequested();
			await EndEnemyTurnInternal();
			CombatCt.ThrowIfCancellationRequested();
			await CheckWinCondition();
			if (!IsEnding)
			{
				SwitchSides();
				await WaitForUnpause();
				CombatCt.ThrowIfCancellationRequested();
				await StartTurn();
			}
		}
	}

	public void AddCreature(Creature creature)
	{
		if (!_state.ContainsCreature(creature))
		{
			throw new InvalidOperationException("CombatState must already contain creature.");
		}
		creature.Monster?.SetUpForCombat();
		if (creature.SlotName != null)
		{
			_state.SortEnemiesBySlotName();
		}
		StateTracker.Subscribe(creature);
		this.CreaturesChanged?.Invoke(_state);
	}

	/// <summary>
	/// Called after both the Creature has been added to the room _and_ the NCreature is spawned.
	/// </summary>
	/// <param name="creature"></param>
	public async Task AfterCreatureAdded(Creature creature)
	{
		await creature.AfterAddedToRoom();
		if (creature.IsEnemy && _state.CurrentSide == CombatSide.Player)
		{
			creature.Monster.RollMove(_state.Players.Select((Player p) => p.Creature));
		}
	}

	/// <summary>
	/// Check for the player's hand to be empty and run the appropriate hooks if it is.
	///
	/// We can't just do this check every time the hand size changes, because sometimes we're in the middle of a
	/// sequence of effects and we want to wait to check until they're all done.
	///
	/// For example, if we have <see cref="T:MegaCrit.Sts2.Core.Models.Relics.UnceasingTop" /> and the last card in our hand is <see cref="T:MegaCrit.Sts2.Core.Models.Cards.PommelStrike" />
	/// and we play it, we have to wait to check hand size until Pommel Strike is done being played, otherwise we'll
	/// draw two cards (one when your hand becomes "empty" immediately after Pommel Strike moves to the Play pile, and
	/// another after Pommel Strike's draw command executes).
	///
	/// So, instead of automatically doing this check every time the hand size changes, we manually check after a card
	/// is played, and after a potion is used, since these are the two ways a player can manually interact with combat
	/// state (besides ending turn, which should not trigger an empty hand check). If we ever add more ways, we should
	/// add this check in those too, and update this comment.
	/// </summary>
	/// <param name="choiceContext">Object that keeps context of the action this is called from.</param>
	/// <param name="player">Player whose hand we want to check.</param>
	public async Task CheckForEmptyHand(PlayerChoiceContext choiceContext, Player player)
	{
		if (IsInProgress && !IsExecutingCardOrPotionEffect(player) && !PileType.Hand.GetPile(player).Cards.Any())
		{
			await Hook.AfterHandEmptied(_state, choiceContext, player);
		}
	}

	/// <summary>
	/// Reset the combat manager to prepare for the next combat.
	/// </summary>
	/// <param name="graceful">Usually true. Only pass false if we're exiting the game completely.</param>
	public void Reset(bool graceful)
	{
		_combatCts?.Cancel();
		if (graceful && _state != null)
		{
			SetPhaseForAllPlayers(PlayerTurnPhase.None);
			foreach (Creature item in _state.Creatures.ToList())
			{
				item.Reset();
				RemoveCreature(item);
				_state.RemoveCreature(item);
			}
			_state = null;
		}
		_pendingLoss = null;
		DebugForcedTopCardOnNextShuffle = null;
		IsInProgress = false;
		IsStarting = false;
		IsEnemyTurnStarted = false;
		History.Clear();
		_cardOrPotionEffectDepth.Clear();
		RunManager.Instance.ActionQueueSynchronizer.SetCombatState(ActionSynchronizerCombatState.NotInCombat);
	}

	public async Task HandlePlayerDeath(Player player)
	{
		if (IsInProgress)
		{
			CardModel[] cards = new CardPile[5]
			{
				player.PlayerCombatState.Hand,
				player.PlayerCombatState.DrawPile,
				player.PlayerCombatState.DiscardPile,
				player.PlayerCombatState.ExhaustPile,
				player.PlayerCombatState.PlayPile
			}.SelectMany((CardPile p) => p.Cards).ToArray();
			await CardPileCmd.RemoveFromCombat(cards);
			await PlayerCmd.SetEnergy(0m, player);
			await PlayerCmd.SetStars(0m, player);
		}
	}

	/// <summary>
	/// Marks combat as pending loss. The actual loss processing happens at the next safe point
	/// (in CheckWinCondition) to avoid race conditions where effects try to run after IsInProgress is false.
	/// </summary>
	public void LoseCombat()
	{
		if (!(_pendingLoss != null))
		{
			_pendingLoss = new PendingLossState(_state, (CombatRoom)_state.RunState.CurrentRoom);
		}
	}

	/// <summary>
	/// Processes a pending combat loss. Called from CheckWinCondition at safe points.
	/// </summary>
	private void ProcessPendingLoss()
	{
		if (!(_pendingLoss == null))
		{
			PendingLossState pendingLoss = _pendingLoss;
			_pendingLoss = null;
			IsInProgress = false;
			this.CombatEnded?.Invoke(pendingLoss.Room);
		}
	}

	/// <summary>
	/// DO NOT CALL THIS unless you're in this class or ModelTest.
	/// </summary>
	public async Task EndCombatInternal()
	{
		CombatState combatState = _state;
		Player localPlayer = LocalContext.GetMe(combatState);
		int turnsTaken = localPlayer.PlayerCombatState.TurnNumber;
		IRunState runState = combatState.RunState;
		CombatRoom room = (CombatRoom)runState.CurrentRoom;
		IsInProgress = false;
		SetPhaseForAllPlayers(PlayerTurnPhase.None);
		PlayerActionsDisabled = false;
		using (_playerReadyLock.EnterScope())
		{
			_playersTakingExtraTurn.Clear();
		}
		foreach (Player player in combatState.Players)
		{
			await player.ReviveBeforeCombatEnd();
		}
		await Hook.AfterCombatEnd(runState, combatState, room);
		History.Clear();
		room.OnCombatEnded();
		if (RunManager.Instance.NetService.Type != NetGameType.Replay)
		{
			RunManager.Instance.WriteReplay(stopRecording: true);
		}
		foreach (Player player2 in combatState.Players)
		{
			player2.AfterCombatEnd();
		}
		await Hook.AfterCombatVictory(runState, combatState, room);
		NHoverTipSet.Clear();
		if (runState.CurrentMapPointHistoryEntry != null)
		{
			runState.CurrentMapPointHistoryEntry.Rooms.Last().TurnsTaken = turnsTaken;
		}
		bool flag = runState.Map.SecondBossMapPoint != null && runState.CurrentMapCoord == runState.Map.SecondBossMapPoint.coord;
		bool flag2 = runState.Map.SecondBossMapPoint == null && runState.CurrentMapCoord == runState.Map.BossMapPoint.coord;
		if (room.RoomType == RoomType.Boss && runState.CurrentActIndex == runState.Acts.Count - 1 && (flag || flag2))
		{
			RunManager.Instance.WinTime = RunManager.Instance.RunTime;
		}
		room.MarkPreFinished();
		await SaveManager.Instance.SaveRun(room, saveProgress: false);
		NMapScreen.Instance?.SetTravelEnabled(enabled: true);
		SaveManager.Instance.UpdateProgressAfterCombatWon(localPlayer, room);
		AchievementsHelper.CheckForDefeatedAllEnemiesAchievement(runState.Act, localPlayer);
		SaveManager.Instance.SaveProgressFile();
		if (room.RoomType == RoomType.Boss)
		{
			AchievementsHelper.AfterBossDefeated(localPlayer);
		}
		combatState.MultiplayerScalingModel?.OnCombatFinished();
		if (_state != null)
		{
			this.CombatWon?.Invoke(room);
		}
		RunManager.Instance.ActionExecutor.Unpause();
		RunManager.Instance.ActionQueueSynchronizer.SetCombatState(ActionSynchronizerCombatState.NotInCombat);
		NRunMusicController.Instance?.UpdateTrack();
		if (_state != null)
		{
			this.CombatEnded?.Invoke(room);
		}
	}

	public void RemoveCreature(Creature creature)
	{
		if (creature.IsMonster)
		{
			creature.Monster.BeforeRemovedFromRoom();
			creature.Monster.ResetStateMachine();
		}
		StateTracker.Unsubscribe(creature);
		this.CreaturesChanged?.Invoke(_state);
	}

	public async Task<bool> CheckWinCondition()
	{
		if (_pendingLoss != null)
		{
			ProcessPendingLoss();
			return true;
		}
		if (IsEnding)
		{
			await EndCombatInternal();
			return true;
		}
		return false;
	}

	private async Task ExecuteEnemyTurn(Func<Task>? actionDuringEnemyTurn = null)
	{
		if (!IsInProgress)
		{
			return;
		}
		CombatCt.ThrowIfCancellationRequested();
		if (actionDuringEnemyTurn != null)
		{
			await actionDuringEnemyTurn();
		}
		foreach (Creature enemy in _state.Enemies.ToList())
		{
			if (_state.ContainsCreature(enemy))
			{
				NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(enemy);
				if (nCreature != null)
				{
					await nCreature.PerformIntent();
				}
				await enemy.TakeTurn();
				CombatCt.ThrowIfCancellationRequested();
				await WaitForUnpause();
				await CheckWinCondition();
				if (!IsInProgress)
				{
					return;
				}
			}
		}
		RunManager.Instance.ChecksumTracker.GenerateChecksum("After enemy turn end", null);
		await EndEnemyTurn();
	}

	private async Task WaitForActionThenEndTurn(GameAction action, Func<Task>? actionDuringEnemyTurn)
	{
		await action.CompletionTask;
		await AfterAllPlayersReadyToEndTurn(actionDuringEnemyTurn);
	}

	private async Task AfterAllPlayersReadyToEndTurn(Func<Task>? actionDuringEnemyTurn = null)
	{
		if (IsInProgress)
		{
			CombatCt.ThrowIfCancellationRequested();
			EndingPlayerTurnPhaseOne = true;
			RunManager.Instance.ActionQueueSynchronizer.SetCombatState(ActionSynchronizerCombatState.EndTurnPhaseOne);
			await WaitUntilQueueIsEmptyOrWaitingOnNonPlayerDrivenAction();
			await EndPlayerTurnPhaseOneInternal();
			if (IsInProgress && RunManager.Instance.NetService.Type != NetGameType.Replay)
			{
				RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(new ReadyToBeginEnemyTurnAction(LocalContext.GetMe(_state), actionDuringEnemyTurn));
			}
			EndingPlayerTurnPhaseOne = false;
		}
	}

	private async Task WaitUntilQueueIsEmptyOrWaitingOnNonPlayerDrivenAction()
	{
		GameAction currentlyRunningAction = RunManager.Instance.ActionExecutor.CurrentlyRunningAction;
		TaskCompletionSource completionSource;
		if (currentlyRunningAction != null && ActionQueueSet.IsGameActionPlayerDriven(currentlyRunningAction))
		{
			completionSource = new TaskCompletionSource();
			RunManager.Instance.ActionExecutor.AfterActionExecuted += AfterActionExecuted;
			await completionSource.Task;
			RunManager.Instance.ActionExecutor.AfterActionExecuted -= AfterActionExecuted;
		}
		void AfterActionExecuted(GameAction action)
		{
			GameAction readyAction = RunManager.Instance.ActionQueueSet.GetReadyAction();
			if (readyAction == null || !ActionQueueSet.IsGameActionPlayerDriven(readyAction))
			{
				completionSource.SetResult();
			}
		}
	}

	/// <summary>
	/// DO NOT CALL THIS unless you're in this class or ModelTest.
	/// This calls all end-of-turn hooks that could require player choices to be made.
	/// </summary>
	public async Task EndPlayerTurnPhaseOneInternal()
	{
		if (_state == null)
		{
			return;
		}
		CombatState? state = _state;
		if (state == null || state.CurrentSide != CombatSide.Player)
		{
			throw new InvalidOperationException($"EndPlayerTurn called while the current side is {_state?.CurrentSide}!");
		}
		await WaitForUnpause();
		List<Player> playersEndingTurn;
		using (_playerReadyLock.EnterScope())
		{
			playersEndingTurn = ((_playersTakingExtraTurn.Count > 0) ? _playersTakingExtraTurn.ToList() : (_state?.Players.ToList() ?? new List<Player>()));
		}
		List<(Player, HookPlayerChoiceContext)> autoPostPlayContexts = new List<(Player, HookPlayerChoiceContext)>();
		foreach (Player player in playersEndingTurn)
		{
			if (_state != null && LocalContext.NetId.HasValue)
			{
				player.PlayerCombatState.Phase = PlayerTurnPhase.AutoPostPlay;
				HookPlayerChoiceContext playerChoiceContext = new HookPlayerChoiceContext(player, LocalContext.NetId.Value, GameActionType.CombatPlayPhaseOnly);
				Task task = Hook.AfterAutoPostPlayPhaseEntered(playerChoiceContext, _state, player);
				await playerChoiceContext.AssignTaskAndWaitForPauseOrCompletion(task);
				autoPostPlayContexts.Add((player, playerChoiceContext));
			}
		}
		foreach (var (player, hookPlayerChoiceContext) in autoPostPlayContexts)
		{
			await hookPlayerChoiceContext.WaitForCompletion();
			player.PlayerCombatState.Phase = PlayerTurnPhase.End;
		}
		if (_state != null)
		{
			await Hook.BeforeTurnEnd(_state, _state.CurrentSide, playersEndingTurn.Select((Player p) => p.Creature));
		}
		if (await CheckWinCondition())
		{
			return;
		}
		List<HookPlayerChoiceContext> playerEndContexts = new List<HookPlayerChoiceContext>();
		foreach (Player item in playersEndingTurn)
		{
			if (LocalContext.NetId.HasValue)
			{
				HookPlayerChoiceContext playerChoiceContext = new HookPlayerChoiceContext(item, LocalContext.NetId.Value, GameActionType.Combat);
				Task task2 = DoTurnEnd(item, playerChoiceContext);
				await playerChoiceContext.AssignTaskAndWaitForPauseOrCompletion(task2);
				playerEndContexts.Add(playerChoiceContext);
			}
		}
		foreach (HookPlayerChoiceContext item2 in playerEndContexts)
		{
			await item2.WaitForCompletion();
		}
		if (_state != null)
		{
			foreach (Player item3 in playersEndingTurn)
			{
				await Hook.BeforeFlush(_state, item3);
			}
		}
		RunManager.Instance.ChecksumTracker.GenerateChecksum("After player turn phase one end", null);
		await CheckWinCondition();
	}

	/// <summary>
	/// Executes turn end hooks for a player.
	/// If player choice occurs during this method, it uses the passed choice context. This way, each player's turn end
	/// runs independently of all others.
	/// </summary>
	private async Task DoTurnEnd(Player player, PlayerChoiceContext choiceContext)
	{
		await player.PlayerCombatState.OrbQueue.BeforeTurnEnd(choiceContext);
		if (IsOverOrEnding)
		{
			return;
		}
		CardPile pile = PileType.Hand.GetPile(player);
		PileType.Discard.GetPile(player);
		List<CardModel> turnEndCards = new List<CardModel>();
		List<CardModel> list = new List<CardModel>();
		foreach (CardModel card in pile.Cards)
		{
			if (card.HasTurnEndInHandEffect)
			{
				turnEndCards.Add(card);
			}
			else if (card.Keywords.Contains(CardKeyword.Ethereal) && Hook.ShouldEtherealTrigger(player.Creature.CombatState, card))
			{
				list.Add(card);
			}
		}
		foreach (CardModel item in list)
		{
			await CardCmd.Exhaust(choiceContext, item, causedByEthereal: true);
		}
		foreach (CardModel item2 in turnEndCards)
		{
			await item2.OnTurnEndInHandWrapper(choiceContext);
		}
	}

	private async Task EndEnemyTurnInternal()
	{
		List<Creature> enemies = _state.CreaturesOnCurrentSide.ToList();
		await Hook.BeforeTurnEnd(_state, _state.CurrentSide, enemies);
		foreach (Player player in _state.Players)
		{
			player.PlayerCombatState.EndOfTurnCleanup();
		}
		await Hook.AfterTurnEnd(_state, _state.CurrentSide, enemies);
	}

	private async Task AfterAllPlayersReadyToBeginEnemyTurn(Func<Task>? actionDuringEnemyTurn = null)
	{
		if (IsInProgress)
		{
			CombatCt.ThrowIfCancellationRequested();
			EndingPlayerTurnPhaseTwo = true;
			RunManager.Instance.ActionQueueSynchronizer.SetCombatState(ActionSynchronizerCombatState.NotPlayPhase);
			this.AboutToSwitchToEnemyTurn?.Invoke(_state);
			await Task.Yield();
			await EndPlayerTurnPhaseTwoInternal();
			await SwitchFromPlayerToEnemySide(actionDuringEnemyTurn);
			EndingPlayerTurnPhaseTwo = false;
		}
	}

	/// <summary>
	/// DO NOT CALL THIS unless you're in this class or ModelTest.
	/// This does all the player state cleanup for the end of their turn. It must not call any hooks that might cause
	/// player choices to occur.
	/// </summary>
	public async Task EndPlayerTurnPhaseTwoInternal()
	{
		if (_state.CurrentSide != CombatSide.Player)
		{
			throw new InvalidOperationException($"EndPlayerTurnPhaseTwo called while the current side is {_state.CurrentSide}!");
		}
		List<Player> playersEndingTurn;
		using (_playerReadyLock.EnterScope())
		{
			playersEndingTurn = ((_playersTakingExtraTurn.Count > 0) ? _playersTakingExtraTurn.ToList() : _state.Players.ToList());
		}
		List<HookPlayerChoiceContext> flushPlayerHandContexts = new List<HookPlayerChoiceContext>();
		foreach (Player item in playersEndingTurn)
		{
			if (_state != null && LocalContext.NetId.HasValue)
			{
				HookPlayerChoiceContext playerChoiceContext = new HookPlayerChoiceContext(item, LocalContext.NetId.Value, GameActionType.CombatPlayPhaseOnly);
				Task task = FlushPlayerHand(item, playerChoiceContext);
				await playerChoiceContext.AssignTaskAndWaitForPauseOrCompletion(task);
				flushPlayerHandContexts.Add(playerChoiceContext);
			}
		}
		foreach (HookPlayerChoiceContext item2 in flushPlayerHandContexts)
		{
			await item2.WaitForCompletion();
		}
		if (_state != null)
		{
			await Hook.AfterTurnEnd(_state, _state.CurrentSide, playersEndingTurn.Select((Player p) => p.Creature));
			CombatCt.ThrowIfCancellationRequested();
		}
		RunManager.Instance.ChecksumTracker.GenerateChecksum("after player turn phase two end", null);
	}

	private async Task FlushPlayerHand(Player player, HookPlayerChoiceContext playerChoiceContext)
	{
		if (player.Creature.IsDead)
		{
			return;
		}
		if (_state == null || player.PlayerCombatState == null)
		{
			Log.Warn($"Combat state is null. Assuming that the run has been cleaned up. (CombatState: {_state} PlayerCombatState: {player.PlayerCombatState})");
			return;
		}
		CombatState state = _state;
		List<CardModel> cardsToFlush = new List<CardModel>();
		List<CardModel> cardsToRetain = new List<CardModel>();
		bool flag = Hook.ShouldFlush(state, player);
		foreach (CardModel card in PileType.Hand.GetPile(player).Cards)
		{
			if (!flag || card.ShouldRetainThisTurn)
			{
				cardsToRetain.Add(card);
			}
			else
			{
				cardsToFlush.Add(card);
			}
		}
		if (cardsToFlush.Count > 0)
		{
			await CardPileCmd.Add(cardsToFlush, PileType.Discard);
			CombatCt.ThrowIfCancellationRequested();
		}
		await Hook.AfterFlush(state, player, playerChoiceContext, cardsToFlush, cardsToRetain);
		CombatCt.ThrowIfCancellationRequested();
		player.PlayerCombatState.EndOfTurnCleanup();
	}

	/// <summary>
	/// DO NOT CALL THIS unless you're in this class or ModelTest.
	/// This switches from the player side to the enemy side, handling extra player turns if necessary.
	/// </summary>
	/// <param name="actionDuringEnemyTurn">Optional action to execute during the enemy turn. This is useful for tests.</param>
	public async Task SwitchFromPlayerToEnemySide(Func<Task>? actionDuringEnemyTurn = null)
	{
		if (_state == null)
		{
			return;
		}
		List<Player> list;
		using (_playerReadyLock.EnterScope())
		{
			_playersTakingExtraTurn.Clear();
			foreach (Player player in _state.Players)
			{
				if (Hook.ShouldTakeExtraTurn(_state, player))
				{
					Log.Info($"Player {player.NetId} ({player.Character.Id.Entry}) is taking an extra turn");
					_playersTakingExtraTurn.Add(player);
				}
			}
			list = _playersTakingExtraTurn.ToList();
		}
		SwitchSides();
		foreach (Player item in list)
		{
			if (_state == null)
			{
				return;
			}
			await Hook.AfterTakingExtraTurn(_state, item);
		}
		await WaitForUnpause();
		await StartTurn(actionDuringEnemyTurn);
	}

	private void SwitchSides()
	{
		if (_state == null)
		{
			return;
		}
		bool flag;
		using (_playerReadyLock.EnterScope())
		{
			flag = _playersTakingExtraTurn.Count > 0;
		}
		if (_state.CurrentSide == CombatSide.Player && !flag)
		{
			_state.CurrentSide = CombatSide.Enemy;
		}
		else
		{
			_state.CurrentSide = CombatSide.Player;
			IReadOnlyList<Player> readOnlyList;
			if (flag)
			{
				readOnlyList = _playersTakingExtraTurn;
			}
			else
			{
				readOnlyList = _state.Players;
				_state.RoundNumber++;
			}
			foreach (Player item in readOnlyList)
			{
				item.PlayerCombatState.IncrementTurnNumber();
			}
		}
		foreach (Creature creature in _state.Creatures)
		{
			creature.OnSideSwitch();
		}
		this.TurnEnded?.Invoke(_state);
	}

	/// <summary>
	/// Pause combat.
	/// </summary>
	public void Pause()
	{
		if (!NonInteractiveMode.IsActive && IsInProgress)
		{
			IsPaused = true;
		}
	}

	/// <summary>
	/// Un-pause combat.
	/// </summary>
	public void Unpause()
	{
		if (!NonInteractiveMode.IsActive)
		{
			IsPaused = false;
		}
	}

	/// <summary>
	/// Returns true if the passed player is taking part in the current player turn.
	/// Returns false if some player is taking an extra turn, and it's not us.
	/// If it is not the player turn, then this returns false.
	/// </summary>
	public bool IsPartOfPlayerTurn(Player player)
	{
		CombatState? state = _state;
		if (state == null || state.CurrentSide != CombatSide.Player)
		{
			return false;
		}
		if (_playersTakingExtraTurn.Count == 0)
		{
			return true;
		}
		return _playersTakingExtraTurn.Contains(player);
	}

	public async Task WaitForUnpause()
	{
		if (!NonInteractiveMode.IsActive)
		{
			while (IsPaused && IsInProgress && _state != null)
			{
				await NGame.Instance.AwaitProcessFrame();
			}
		}
	}

	/// <summary>
	/// WARNING: ONLY CALL THIS IN TESTS!
	/// Force the specified card to be moved to the top of the next shuffle.
	/// Useful for tests for shuffle tests where the first card drawn afterwards matters.
	/// </summary>
	/// <param name="card">Card to force to the top.</param>
	public void DebugForceTopCardOnNextShuffle(CardModel card)
	{
		card.AssertMutable();
		DebugForcedTopCardOnNextShuffle = card;
	}

	/// <summary>
	/// WARNING: ONLY CALL THIS IN TESTS!
	/// Clear the forced specified card to be moved to the top of the next shuffle.
	/// Useful for tests for shuffle tests where the first card drawn afterwards matters.
	/// </summary>
	public void DebugClearForcedTopCardOnNextShuffle()
	{
		DebugForcedTopCardOnNextShuffle = null;
	}
}
