using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Actions;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;

namespace MegaCrit.Sts2.Core.GameActions.Multiplayer;

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
		this.ActionEnqueued?.Invoke(gameAction);
		if (queue.isCancellingPlayCardActions && gameAction is PlayCardAction)
		{
			_logger.Debug($"Attempted to enqueue PlayCardAction {gameAction} to player queue owned by {gameAction.OwnerId}, but it's currently cancelling all play card actions due to player choice");
			gameAction.Cancel();
			return;
		}
		if (queue.isCancellingPlayerDrivenCombatActions && IsGameActionPlayerDriven(gameAction) && gameAction.ActionType != GameActionType.NonCombat)
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

	public static bool IsGameActionPlayerDriven(GameAction gameAction)
	{
		if (!(gameAction is GenericHookGameAction))
		{
			return !(gameAction is ReadyToBeginEnemyTurnAction);
		}
		return false;
	}

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
			GameAction gameAction2 = actionQueue.actions[0];
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

	public Task BecameEmpty()
	{
		if (_queuesEmptyCompletionSource == null)
		{
			return Task.CompletedTask;
		}
		return _queuesEmptyCompletionSource.Task;
	}

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

	public void StartCancellingAllPlayerDrivenCombatActions()
	{
		_logger.Debug("Setting all player queues to cancel all non-hook actions");
		foreach (ActionQueue actionQueue in _actionQueues)
		{
			actionQueue.isCancellingPlayerDrivenCombatActions = true;
			for (int i = 0; i < actionQueue.actions.Count; i++)
			{
				GameAction gameAction = actionQueue.actions[i];
				if (IsGameActionPlayerDriven(gameAction) && gameAction.ActionType != GameActionType.NonCombat && gameAction.State == GameActionState.WaitingForExecution)
				{
					_logger.VeryDebug($"Cancelling non-hook action {actionQueue.actions[i]}");
					gameAction.Cancel();
					actionQueue.actions.RemoveAt(i);
					i--;
				}
			}
		}
	}

	public bool ActionQueueIsPaused(ulong playerId)
	{
		return GetQueue(playerId).isPaused;
	}

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

	public void CombatEnded()
	{
		_logger.Debug("Cancelling all non-executing combat actions in all queues");
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

	public void CombatStarted()
	{
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

	private ActionQueue GetQueue(ulong playerId)
	{
		ActionQueue actionQueue = _actionQueues.FirstOrDefault((ActionQueue q) => q.ownerId == playerId);
		if (actionQueue == null)
		{
			throw new InvalidOperationException($"Tried to get local action queue for nonexistent player with ID {playerId}!");
		}
		return actionQueue;
	}

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

	public void FastForwardNextActionId(uint nextId)
	{
		_nextId = nextId;
	}

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

	private uint GetAndIncrementActionId()
	{
		uint nextId = _nextId;
		_nextId++;
		return nextId;
	}
}
