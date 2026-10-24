using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Entities.Actions;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;

namespace MegaCrit.Sts2.Core.GameActions.Multiplayer;

/// <summary>
/// Contains one action queue per player.
/// We use multiple action queues to resolve the following scenario:
/// - One player queues up three or more card plays, the second of which requires player choice
/// - Their queue blocks on the player choice. The third action awaits completion of the player choice
/// - Other players may freely queue card plays to their queues and have them executed
/// </summary>
public class ActionQueueSet
{
	private class ActionQueue
	{
		public List<GameAction> actions = new List<GameAction>();

		public ulong ownerId;

		public bool isCancellingPlayCardActions;

		public bool isCancellingPlayerDrivenCombatActions;

		public bool isCancellingCombatActions = true;

		public bool isPaused;
	}

	private struct ActionWaitingForResumption
	{
		public uint oldId;

		public uint newId;
	}

	private readonly Logger _logger;

	private readonly List<ActionQueue> _actionQueues = new List<ActionQueue>();

	private readonly List<ActionWaitingForResumption> _actionsWaitingForResumption = new List<ActionWaitingForResumption>();

	private TaskCompletionSource? _queuesEmptyCompletionSource;

	private uint _nextId;

	private bool _isInCombat;

	public bool IsEmpty
	{
		get
		{
			if (_queuesEmptyCompletionSource != null)
			{
				return _queuesEmptyCompletionSource.Task.IsCompleted;
			}
			return true;
		}
	}

	public uint NextActionId => _nextId;

	public event Action? ActionQueueChanged;

	public event Action<GameAction>? ActionEnqueued;

	public event Action<uint>? ActionResumed;

	public ActionQueueSet(IReadOnlyList<Player> players)
	{
		_logger = new Logger("ActionQueueSet", LogType.Actions);
		foreach (Player player in players)
		{
			_actionQueues.Add(new ActionQueue
			{
				actions = new List<GameAction>(),
				ownerId = player.NetId
			});
		}
	}

	/// <summary>
	/// Only use this if you really know what you're doing! Improper use can lead to state divergence.
	/// Enqueues an action directly to the owner's action queue.
	/// </summary>
	/// <param name="gameAction">The action to enqueue.</param>
	public void EnqueueWithoutSynchronizing(GameAction gameAction)
	{
		if (_queuesEmptyCompletionSource == null || _queuesEmptyCompletionSource.Task.IsCompleted)
		{
			_queuesEmptyCompletionSource = new TaskCompletionSource();
		}
		if (gameAction.Id.HasValue)
		{
			throw new InvalidOperationException($"Attempting to enqueue GameAction {gameAction} which already has an ID {gameAction.Id}, indicating it was previously enqueued to the queue!");
		}
		gameAction.OnEnqueued(PopAction, GetAndIncrementActionId());
		ActionQueue queue = GetQueue(gameAction.OwnerId);
		try
		{
			this.ActionEnqueued?.Invoke(gameAction);
		}
		catch (Exception ex)
		{
			Log.Error($"Exception encountered in ActionEnqueued for action {gameAction}: {ex}");
			SentryService.CaptureException(ex);
		}
		if (queue.isCancellingPlayCardActions && gameAction is PlayCardAction)
		{
			_logger.Debug($"Attempted to enqueue PlayCardAction {gameAction} to player queue owned by {gameAction.OwnerId}, but it's currently cancelling all play card actions due to player choice");
			gameAction.Cancel();
			return;
		}
		if (queue.isCancellingPlayerDrivenCombatActions && IsGameActionPlayerDriven(gameAction) && gameAction.ActionType != GameActionType.NonCombat && gameAction.ActionType != GameActionType.Any)
		{
			_logger.Debug($"Attempted to enqueue GameAction {gameAction} to player queue owned by {gameAction.OwnerId}, but it's currently cancelling all non-hook actions due to end of turn");
			gameAction.Cancel();
			return;
		}
		bool isCancellingCombatActions = queue.isCancellingCombatActions;
		bool flag = isCancellingCombatActions;
		if (flag)
		{
			GameActionType actionType = gameAction.ActionType;
			bool flag2 = (uint)(actionType - 1) <= 1u;
			flag = flag2;
		}
		if (flag)
		{
			_logger.Debug($"Attempted to enqueue GameAction {gameAction} to player queue owned by {gameAction.OwnerId}, but it's currently cancelling all combat actions");
			gameAction.Cancel();
		}
		else
		{
			_logger.Debug($"Enqueueing action {gameAction} to player queue owned by {gameAction.OwnerId}");
			queue.actions.Add(gameAction);
			this.ActionQueueChanged?.Invoke();
		}
	}

	/// <summary>
	/// Returns true if a GameAction is player driven, false if it is emitted automatically.
	/// If this ends up being used in a few different places, it can be moved somewhere else; for now, it's obscure enough
	/// that it should only be used here.
	/// </summary>
	/// <returns></returns>
	public static bool IsGameActionPlayerDriven(GameAction gameAction)
	{
		if (!(gameAction is GenericHookGameAction))
		{
			return !(gameAction is ReadyToBeginEnemyTurnAction);
		}
		return false;
	}

	/// <summary>
	/// Returns the next action on top of a player's queue that should be executed.
	/// Selection works like this:
	/// - If any player's queue is awaiting a player choice, then that queue is skipped.
	/// - Otherwise, we return the action with the minimum ID (the first queued by the host) at the top of any player's queue.
	/// This method should not be called when an action is already being executed.
	/// </summary>
	public GameAction? GetReadyAction()
	{
		GameAction gameAction = null;
		_logger.VeryDebug("Attempting to find ready action");
		foreach (ActionQueue actionQueue in _actionQueues)
		{
			if (actionQueue.actions.Count <= 0)
			{
				_logger.VeryDebug($"Queue for player {actionQueue.ownerId} is empty");
				continue;
			}
			while (actionQueue.actions.Count > 0 && actionQueue.actions[0].State == GameActionState.Canceled)
			{
				_logger.Warn($"Removing canceled action {actionQueue.actions[0]} from front of player queue {actionQueue.ownerId}");
				actionQueue.actions.RemoveAt(0);
			}
			if (actionQueue.actions.Count <= 0)
			{
				continue;
			}
			GameAction gameAction2 = actionQueue.actions[0];
			if (_isInCombat && gameAction2.ActionType == GameActionType.NonCombat)
			{
				_logger.VeryDebug($"We are currently in combat and candidate action {gameAction2} has type {gameAction2.ActionType}");
				continue;
			}
			if (actionQueue.isPaused && gameAction2.ActionType == GameActionType.CombatPlayPhaseOnly)
			{
				_logger.VeryDebug($"Queue for player {actionQueue.ownerId} is paused and candidate action {gameAction2} has type {gameAction2.ActionType}");
				continue;
			}
			if (gameAction2.State == GameActionState.GatheringPlayerChoice)
			{
				_logger.VeryDebug($"Action {gameAction2} at front of player queue {actionQueue.ownerId} is waiting for player choice");
				continue;
			}
			if (gameAction2.State != GameActionState.WaitingForExecution && gameAction2.State != GameActionState.ReadyToResumeExecuting)
			{
				throw new InvalidOperationException($"GameAction {gameAction2} at the front of player action queue {actionQueue.ownerId} is in invalid state {gameAction2.State}!");
			}
			if (gameAction == null || gameAction2.Id < gameAction.Id)
			{
				_logger.VeryDebug($"Action {gameAction2} with id {gameAction2.Id.Value} belonging to {actionQueue.ownerId} becomes new ready action");
				gameAction = gameAction2;
			}
			else
			{
				_logger.VeryDebug($"Action {gameAction2} has id {gameAction2.Id.Value} greater than current ready action {gameAction} with ID {gameAction.Id.Value}");
			}
		}
		if (gameAction != null)
		{
			_logger.VeryDebug($"Got ready action {gameAction} ({gameAction.Id})");
		}
		else
		{
			_logger.VeryDebug("No action is ready");
		}
		return gameAction;
	}

	/// <summary>
	/// Pauses an action for player choice. The action will be removed from the ActionExecutor and a new action will be
	/// allowed to execute, if there are any actions that are ready.
	/// To resume the action after this is called, call RequestResumeActionAfterPlayerChoice.
	/// </summary>
	/// <param name="action">The GameAction to pause.</param>
	/// <param name="options">Whether to cancel card play actions that enter the queue while obtaining player choice.</param>
	/// <exception cref="T:System.InvalidOperationException">Thrown if the action is not at the front of the owner's action queue.</exception>
	public void PauseActionForPlayerChoice(GameAction action, PlayerChoiceOptions options)
	{
		ActionQueue queue = GetQueue(action.OwnerId);
		if (action != queue.actions[0])
		{
			throw new InvalidOperationException($"Attempting to pause action {action} that is not at the front of the owner {action.Id}'s queue!");
		}
		_logger.Debug($"Pausing action {action} for player choice");
		action.PauseForPlayerChoice();
		ActionWaitingForResumption? actionWaitingForResumption = null;
		for (int i = 0; i < _actionsWaitingForResumption.Count; i++)
		{
			if (_actionsWaitingForResumption[i].oldId == action.Id)
			{
				actionWaitingForResumption = _actionsWaitingForResumption[i];
				_actionsWaitingForResumption.RemoveAt(i);
				break;
			}
		}
		if (options.HasFlag(PlayerChoiceOptions.CancelPlayCardActions))
		{
			CancelNonExecutingActionsOfType<PlayCardAction>(action.OwnerId, actionWaitingForResumption?.newId);
			queue.isCancellingPlayCardActions = true;
		}
		this.ActionQueueChanged?.Invoke();
		if (actionWaitingForResumption.HasValue)
		{
			_logger.Debug($"Immediately resuming action {action} - already had resumption waiting");
			action.ResumeAfterGatheringPlayerChoice(actionWaitingForResumption.Value.newId);
			queue.isCancellingPlayCardActions = false;
			this.ActionQueueChanged?.Invoke();
		}
	}

	/// <summary>
	/// Returns a task which completes when all player queues are empty.
	/// Be careful of running this in tests that involve player choice! If actions are not resumed, the task will never complete.
	/// </summary>
	public Task BecameEmpty()
	{
		if (_queuesEmptyCompletionSource == null)
		{
			return Task.CompletedTask;
		}
		return _queuesEmptyCompletionSource.Task;
	}

	/// <summary>
	/// Pauses execution of all actions on all player queues, including those that are queued after this is called.
	/// Note that this does not pause execution of the currently executing action - only those that come after it.
	/// </summary>
	public void PauseAllPlayerQueues()
	{
		_logger.Debug("Pausing all player queues");
		foreach (ActionQueue actionQueue in _actionQueues)
		{
			actionQueue.isPaused = true;
			actionQueue.isCancellingPlayerDrivenCombatActions = false;
		}
		this.ActionQueueChanged?.Invoke();
	}

	/// <summary>
	/// Cancels all manual combat actions that are enqueued to all queues.
	/// Manual combat actions are any GameActions that have ActionType != GameActionType.NonCombat and ActionType !=
	/// GameActionType.Any, and are manually played by the player (i.e. everything except for GenericHookGameAction and
	/// ReadyToSwitchToEnemyTurnAction). The flag becomes unset when either PauseAllPlayerQueues or UnpauseAllPlayerQueues
	/// is called.
	/// See the comments in EnqueueWithoutSynchronizing for why this is necessary.
	/// </summary>
	public void StartCancellingAllPlayerDrivenCombatActions()
	{
		_logger.Debug("Setting all player queues to cancel all non-hook actions");
		foreach (ActionQueue actionQueue in _actionQueues)
		{
			actionQueue.isCancellingPlayerDrivenCombatActions = true;
			for (int i = 0; i < actionQueue.actions.Count; i++)
			{
				GameAction gameAction = actionQueue.actions[i];
				if (IsGameActionPlayerDriven(gameAction) && gameAction.ActionType != GameActionType.NonCombat && gameAction.ActionType != GameActionType.Any && gameAction.State == GameActionState.WaitingForExecution)
				{
					_logger.VeryDebug($"Cancelling non-hook action {actionQueue.actions[i]}");
					gameAction.Cancel();
					actionQueue.actions.RemoveAt(i);
					i--;
				}
			}
		}
	}

	/// <returns>Returns true if the action queue for the given player is paused.</returns>
	public bool ActionQueueIsPaused(ulong playerId)
	{
		return GetQueue(playerId).isPaused;
	}

	/// <summary>
	/// Resumes execution of all player queues without synchronization.
	/// It's fine to call this at the end of combat, but call this very carefully during combat, as incorrect usage
	/// can lead to state divergences.
	/// </summary>
	public void UnpauseAllPlayerQueues()
	{
		_logger.Debug("Unpausing all player queues");
		foreach (ActionQueue actionQueue in _actionQueues)
		{
			actionQueue.isPaused = false;
			actionQueue.isCancellingPlayerDrivenCombatActions = false;
		}
		this.ActionQueueChanged?.Invoke();
	}

	/// <summary>
	/// Cancels all combat actions on the queue and continues cancelling them until CombatStarted is called.
	/// </summary>
	public void CombatEnded()
	{
		_logger.Debug("Combat ended. Cancelling all non-executing combat actions in all queues");
		_isInCombat = false;
		foreach (ActionQueue actionQueue in _actionQueues)
		{
			for (int i = 0; i < actionQueue.actions.Count; i++)
			{
				GameAction gameAction = actionQueue.actions[i];
				GameActionType actionType = gameAction.ActionType;
				bool flag = (uint)(actionType - 1) <= 1u;
				if (flag && gameAction.State != GameActionState.Executing)
				{
					_logger.VeryDebug($"Cancelling action {gameAction}");
					gameAction.Cancel();
					actionQueue.actions.RemoveAt(i);
					i--;
				}
				else
				{
					_logger.VeryDebug($"Not cancelling action {gameAction}, type: {gameAction.ActionType}, state: {gameAction.State}");
				}
			}
			actionQueue.isCancellingPlayCardActions = false;
			actionQueue.isCancellingPlayerDrivenCombatActions = false;
			actionQueue.isCancellingCombatActions = true;
		}
		CheckIfQueuesEmpty();
		this.ActionQueueChanged?.Invoke();
	}

	/// <summary>
	/// Allows combat actions to be added to player queues.
	/// </summary>
	public void CombatStarted()
	{
		_logger.Debug("Combat started.");
		_isInCombat = true;
		foreach (ActionQueue actionQueue in _actionQueues)
		{
			actionQueue.isCancellingCombatActions = false;
		}
	}

	public void Reset()
	{
		_actionQueues.Clear();
		CheckIfQueuesEmpty();
		this.ActionQueueChanged?.Invoke();
	}

	/// <summary>
	/// Cancels all non-executing actions for a specific player's queue.
	/// </summary>
	public void CancelNonExecutingActionsForPlayer(ulong playerId)
	{
		_logger.Debug($"Cancelling all non-executing actions owned by {playerId}");
		ActionQueue queue = GetQueue(playerId);
		for (int i = 0; i < queue.actions.Count; i++)
		{
			if (queue.actions[i].State == GameActionState.WaitingForExecution)
			{
				_logger.VeryDebug($"Cancelling action {queue.actions[i]}");
				queue.actions[i].Cancel();
				queue.actions.RemoveAt(i);
				i--;
			}
		}
		CheckIfQueuesEmpty();
	}

	/// <summary>
	/// Cancel all queued actions of the specified type owned by a specific player.
	/// Used when we are selecting a card, e.g. for Survivor, but cards have been queued up for play.
	/// Note that this does not cancel the action at the front of the queue if it is executing.
	/// This is private because the timing must be synchronized with action enqueues across peers.
	/// </summary>
	/// <typeparam name="T">Type of actions to cancel.</typeparam>
	/// <param name="ownerId">The owner of the actions to cancel.</param>
	/// <param name="maxActionId">If non-null, only actions with ID less than this value will be cancelled.</param>
	private void CancelNonExecutingActionsOfType<T>(ulong ownerId, uint? maxActionId) where T : GameAction
	{
		_logger.Debug($"Cancelling non-executing actions of type {typeof(T)} owned by {ownerId}");
		foreach (ActionQueue actionQueue in _actionQueues)
		{
			if (actionQueue.ownerId != ownerId)
			{
				continue;
			}
			for (int i = 0; i < actionQueue.actions.Count; i++)
			{
				GameAction gameAction = actionQueue.actions[i];
				if (gameAction is T && gameAction.State == GameActionState.WaitingForExecution && (!maxActionId.HasValue || !(gameAction.Id.Value >= maxActionId)))
				{
					_logger.VeryDebug($"Cancelling action {actionQueue.actions[i]}");
					gameAction.Cancel();
					actionQueue.actions.RemoveAt(i);
					i--;
				}
			}
		}
		CheckIfQueuesEmpty();
	}

	/// <summary>
	/// Resumes a GameAction after a player choice. This should never be called anywhere other than ActionQueueSynchronizer.
	/// </summary>
	public void ResumeActionWithoutSynchronizing(uint id)
	{
		this.ActionResumed?.Invoke(id);
		uint andIncrementActionId = GetAndIncrementActionId();
		if (TryGetAction(id, out GameAction gameAction, out ActionQueue queue) && gameAction.State == GameActionState.GatheringPlayerChoice)
		{
			_logger.Debug($"Resuming action {gameAction} after player choice");
			queue.isCancellingPlayCardActions = false;
			gameAction.ResumeAfterGatheringPlayerChoice(andIncrementActionId);
			this.ActionQueueChanged?.Invoke();
		}
		else
		{
			_logger.Debug($"Action with id {id} is not ready to resume, enqueueing resumption");
			ActionWaitingForResumption item = new ActionWaitingForResumption
			{
				oldId = id,
				newId = andIncrementActionId
			};
			_actionsWaitingForResumption.Add(item);
		}
	}

	/// <summary>
	/// Obtains an action by its ID.
	/// </summary>
	private bool TryGetAction(uint id, out GameAction? gameAction, out ActionQueue? queue)
	{
		foreach (ActionQueue actionQueue in _actionQueues)
		{
			foreach (GameAction action in actionQueue.actions)
			{
				if (action.Id == id)
				{
					queue = actionQueue;
					gameAction = action;
					return true;
				}
			}
		}
		queue = null;
		gameAction = null;
		return false;
	}

	/// <summary>
	/// Obtains an action queue by its owner's ID.
	/// </summary>
	private ActionQueue GetQueue(ulong playerId)
	{
		ActionQueue actionQueue = _actionQueues.FirstOrDefault((ActionQueue q) => q.ownerId == playerId);
		if (actionQueue == null)
		{
			throw new InvalidOperationException($"Tried to get local action queue for nonexistent player with ID {playerId}!");
		}
		return actionQueue;
	}

	/// <summary>
	/// Removes an action from the front of its owner's queue. Should be called when an action finishes execution.
	/// </summary>
	private void PopAction(GameAction action)
	{
		bool flag = false;
		foreach (ActionQueue actionQueue in _actionQueues)
		{
			if (actionQueue.actions.Count == 0)
			{
				continue;
			}
			if (actionQueue.actions[0] == action)
			{
				flag = true;
				actionQueue.actions.RemoveAt(0);
				continue;
			}
			foreach (GameAction action2 in actionQueue.actions)
			{
				if (action2 == action)
				{
					throw new InvalidOperationException($"Tried to pop action {action}, but it is not the top-most action for player {actionQueue.ownerId}!");
				}
			}
		}
		if (flag)
		{
			this.ActionQueueChanged?.Invoke();
			if (action.Exception != null)
			{
				_queuesEmptyCompletionSource?.SetException(action.Exception);
			}
			else
			{
				CheckIfQueuesEmpty();
			}
			return;
		}
		throw new InvalidOperationException($"Tried to pop action {action}, but we didn't find it in any queue!");
	}

	/// <summary>
	/// Used in replays so that we start with the correct action ID when replaying actions.
	/// </summary>
	/// <param name="nextId">The ID to assign to the next action.</param>
	public void FastForwardNextActionId(uint nextId)
	{
		_nextId = nextId;
	}

	/// <summary>
	/// Checks if all action queues are empty and sets the completion source for tests that wait.
	/// </summary>
	private void CheckIfQueuesEmpty()
	{
		if (!_actionQueues.All((ActionQueue q) => q.actions.Count == 0))
		{
			return;
		}
		TaskCompletionSource queuesEmptyCompletionSource = _queuesEmptyCompletionSource;
		if (queuesEmptyCompletionSource != null)
		{
			Task task = queuesEmptyCompletionSource.Task;
			if (task != null && !task.IsCompleted)
			{
				_queuesEmptyCompletionSource?.SetResult();
			}
		}
	}

	/// <summary>
	/// Called when an action is enqueued or resumed.
	/// It is extremely important that this message is called deterministically across all peers. Action IDs are not
	/// synchronized across the network; since all action messages (enqueue/resume) are received in the same order that
	/// they are sent, we can trust that all peers will generate the same action IDs.
	///
	/// Note that we re-assign an ID to an action when it becomes ready. The reason we do this is to avoid subtle timing
	/// bugs with message ready timings. Re-assigning the ID to an ID that is greater than all actions currently in the
	/// queue ensures that the existing actions in the queue will execute in a deterministic fashion, and the newly ready
	/// action will execute after them.
	/// </summary>
	private uint GetAndIncrementActionId()
	{
		uint nextId = _nextId;
		_nextId++;
		return nextId;
	}
}
