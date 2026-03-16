using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.GameActions.Multiplayer;

public class HookPlayerChoiceContext : PlayerChoiceContext
{
	private readonly ulong _localPlayerId;

	private GenericHookGameAction? _gameAction;

	private readonly TaskCompletionSource _taskAssignedCompletionSource = new TaskCompletionSource();

	private readonly TaskCompletionSource _pausedCompletionSource = new TaskCompletionSource();

	private ActionQueueSynchronizer? _actionQueueSynchronizer;

	private ActionQueueSet? _actionQueueSet;

	private ActionExecutor? _actionExecutor;

	private GameActionType _gameActionType;

	private ActionQueueSynchronizer ActionQueueSynchronizer => _actionQueueSynchronizer ?? RunManager.Instance.ActionQueueSynchronizer;

	private ActionQueueSet ActionQueueSet => _actionQueueSet ?? RunManager.Instance.ActionQueueSet;

	private ActionExecutor ActionExecutor => _actionExecutor ?? RunManager.Instance.ActionExecutor;

	public AbstractModel? Source { get; }

	public Task? Task { get; private set; }

	public Player? Owner { get; }

	public GenericHookGameAction? GameAction => _gameAction;

	public HookPlayerChoiceContext(Player owner, ulong localPlayerId, GameActionType gameActionType)
	{
		_gameActionType = gameActionType;
		_localPlayerId = localPlayerId;
		Owner = owner;
	}

	public HookPlayerChoiceContext(AbstractModel source, ulong localPlayerId, CombatState combatState, GameActionType gameActionType)
	{
		_localPlayerId = localPlayerId;
		Source = source;
		AbstractModel source2 = Source;
		Owner = ((source2 is CardModel cardModel) ? cardModel.Owner : ((source2 is RelicModel relicModel) ? relicModel.Owner : ((source2 is PotionModel potionModel) ? potionModel.Owner : ((source2 is AfflictionModel afflictionModel) ? afflictionModel.Card.Owner : ((!(source2 is EnchantmentModel enchantmentModel)) ? null : enchantmentModel.Card.Owner)))));
		if (Source is PowerModel powerModel)
		{
			if (powerModel.Owner.IsPlayer)
			{
				Owner = powerModel.Owner.Player;
			}
			else
			{
				Owner = combatState.Players[0];
			}
		}
		PushModel(Source);
		_gameActionType = gameActionType;
	}

	public void MockDependenciesForTest(ActionQueueSynchronizer? actionQueueSynchronizer, ActionQueueSet? actionQueueSet, ActionExecutor? actionExecutor)
	{
		_actionQueueSet = actionQueueSet;
		_actionQueueSynchronizer = actionQueueSynchronizer;
		_actionExecutor = actionExecutor;
	}

	public async Task<bool> AssignTaskAndWaitForPauseOrCompletion(Task task)
	{
		if (Source != null)
		{
			Task = ExecuteTaskThenInvokeExecutionFinished(task);
		}
		else
		{
			Task = task;
		}
		_taskAssignedCompletionSource.SetResult();
		await TaskHelper.WhenAny(task, _pausedCompletionSource.Task);
		return task.IsCompleted;
	}

	private async Task ExecuteTaskThenInvokeExecutionFinished(Task task)
	{
		await task;
		Source?.InvokeExecutionFinished();
	}

	public override async Task SignalPlayerChoiceBegun(PlayerChoiceOptions options)
	{
		if (Task == null)
		{
			await _taskAssignedCompletionSource.Task;
			if (Task == null)
			{
				throw new InvalidOperationException("HookPlayerChoiceContext was never passed a task to await!");
			}
		}
		if (_gameAction != null)
		{
			if (ActionExecutor.CurrentlyRunningAction != _gameAction)
			{
				Log.Error($"Tried to interrupt action {_gameAction} but the currently running action is {ActionExecutor.CurrentlyRunningAction}!");
				return;
			}
		}
		else
		{
			if (Owner == null)
			{
				throw new InvalidOperationException($"HookPlayerChoiceContext is assigned a model {Source} with no owner, but the model has requested a player choice! This is not supported");
			}
			_gameAction = ActionQueueSynchronizer.GenerateHookAction(Owner.NetId, _gameActionType);
			_gameAction.SetChoiceContext(this);
			if (_gameAction.OwnerId == _localPlayerId)
			{
				ActionQueueSynchronizer.RequestEnqueueHookAction(_gameAction);
			}
			_pausedCompletionSource.SetResult();
			await _gameAction.ExecutionStartedTask;
		}
		ActionQueueSet.PauseActionForPlayerChoice(_gameAction, options);
	}

	public override async Task SignalPlayerChoiceEnded()
	{
		if (_gameAction.OwnerId == _localPlayerId)
		{
			ActionQueueSynchronizer.RequestResumeActionAfterPlayerChoice(_gameAction);
		}
		await _gameAction.WaitForActionToResumeExecutingAfterPlayerChoice();
	}
}
