using System;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Actions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes;

namespace MegaCrit.Sts2.Core.GameActions;

public class ActionExecutor
{
	private readonly ActionQueueSet _actionQueueSet;

	private bool _isPaused;

	private CancellationTokenSource? _actionCancelToken;

	private TaskCompletionSource<bool>? _queueTaskCompletionSource;

	private readonly MegaCrit.Sts2.Core.Logging.Logger _logger;

	public bool IsPaused => _isPaused;

	public bool IsRunning
	{
		get
		{
			TaskCompletionSource<bool> queueTaskCompletionSource = _queueTaskCompletionSource;
			if (queueTaskCompletionSource != null)
			{
				Task<bool> task = queueTaskCompletionSource.Task;
				if (task != null)
				{
					return !task.IsCompleted;
				}
			}
			return false;
		}
	}

	public GameAction? CurrentlyRunningAction { get; private set; }

	public event Action<GameAction>? BeforeActionExecuted;

	public event Action<GameAction>? AfterActionExecuted;

	public ActionExecutor(ActionQueueSet actionQueueSet)
	{
		actionQueueSet.ActionQueueChanged += ActionQueueChanged;
		_actionQueueSet = actionQueueSet;
		_logger = new MegaCrit.Sts2.Core.Logging.Logger("ActionExecutor", LogType.Actions);
	}

	public void Pause()
	{
		if (!NonInteractiveMode.IsActive)
		{
			_logger.Debug("Pausing queue");
			_isPaused = true;
		}
	}

	public void Unpause()
	{
		_logger.Debug("Un-pausing queue");
		_isPaused = false;
	}

	public Task FinishedExecutingActions()
	{
		if (_queueTaskCompletionSource == null)
		{
			return Task.CompletedTask;
		}
		return _queueTaskCompletionSource.Task;
	}

	public void Cancel()
	{
		_logger.Debug("Cancelling queue");
		_actionCancelToken?.Cancel();
	}

	private void ActionQueueChanged()
	{
		if (!IsRunning)
		{
			_logger.Debug("Action queue changed, beginning ExecuteActions");
			TaskHelper.RunSafely(ExecuteActions());
		}
	}

	private async Task ExecuteActions()
	{
		_queueTaskCompletionSource = new TaskCompletionSource<bool>();
		_actionCancelToken = new CancellationTokenSource();
		try
		{
			GameAction readyAction = _actionQueueSet.GetReadyAction();
			while (readyAction != null)
			{
				await WaitForUnpause();
				if (readyAction.State == GameActionState.Canceled)
				{
					readyAction = _actionQueueSet.GetReadyAction();
					continue;
				}
				_logger.Debug($"Executing action: {readyAction}");
				this.BeforeActionExecuted?.Invoke(readyAction);
				if (NonInteractiveMode.IsActive)
				{
					CurrentlyRunningAction = readyAction;
					await readyAction.Execute();
					AfterActionFinished(readyAction);
				}
				else
				{
					CurrentlyRunningAction = readyAction;
					readyAction.AfterFinished += AfterActionFinished;
					Task actionTask = readyAction.Execute();
					while (!actionTask.IsCompleted && !_actionCancelToken.IsCancellationRequested)
					{
						await Engine.GetMainLoop().ToSignal(Engine.GetMainLoop(), SceneTree.SignalName.ProcessFrame);
					}
					if (actionTask.IsFaulted)
					{
						Log.Error($"GameAction {readyAction} completed with exception: {actionTask.Exception}");
					}
				}
				if (CombatManager.Instance.IsInProgress)
				{
					await CombatManager.Instance.CheckWinCondition();
				}
				if (readyAction.State == GameActionState.Finished)
				{
					_logger.Debug($"Completed execution of action {readyAction}, attempting to find new action");
				}
				else
				{
					_logger.Debug($"Paused execution of action {readyAction} (state is {readyAction.State}), attempting to find new action");
				}
				readyAction.AfterFinished -= AfterActionFinished;
				readyAction = _actionQueueSet.GetReadyAction();
			}
			_queueTaskCompletionSource?.SetResult(result: true);
		}
		catch (TaskCanceledException innerException)
		{
			InvalidOperationException ex = new InvalidOperationException("ActionExecutor.ExecuteActions should never be canceled!", innerException);
			_queueTaskCompletionSource?.SetException(ex);
			throw ex;
		}
		catch (Exception exception)
		{
			_queueTaskCompletionSource?.SetException(exception);
			throw;
		}
	}

	private void AfterActionFinished(GameAction action)
	{
		if (CurrentlyRunningAction != action)
		{
			Log.Error($"Currently running action {CurrentlyRunningAction} did not match recently finished action {action}!");
		}
		else
		{
			if (action.State == GameActionState.Finished)
			{
				this.AfterActionExecuted?.Invoke(action);
			}
			CurrentlyRunningAction = null;
		}
	}

	private async Task WaitForUnpause()
	{
		if (!NonInteractiveMode.IsActive)
		{
			while (_isPaused)
			{
				await NGame.Instance.ToSignal(NGame.Instance.GetTree(), SceneTree.SignalName.ProcessFrame);
			}
		}
	}
}
