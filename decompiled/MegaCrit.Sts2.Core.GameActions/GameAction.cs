using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Actions;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;

namespace MegaCrit.Sts2.Core.GameActions;

public abstract class GameAction
{
	private static readonly Logger _logger = new Logger("GameAction", LogType.Actions);

	private TaskCompletionSource? _pauseForPlayerChoiceTaskSource;

	private TaskCompletionSource? _executeAfterResumptionTaskSource;

	private TaskCompletionSource _completionSource = new TaskCompletionSource();

	private Task? _executionTask;

	public GameActionState State { get; private set; }

	public abstract ulong OwnerId { get; }

	public abstract GameActionType ActionType { get; }

	public uint? Id { get; private set; }

	public Task CompletionTask => _completionSource.Task;

	public Exception? Exception => _executionTask?.Exception;

	public virtual bool RecordableToReplay => true;

	public event Action<GameAction>? AfterFinished;

	public event Action<GameAction>? BeforeExecuted;

	public event Action<GameAction>? BeforeCancelled;

	public event Action<GameAction>? BeforePausedForPlayerChoice;

	public event Action<GameAction>? BeforeReadyToResumeAfterPlayerChoice;

	public event Action<GameAction>? BeforeResumedAfterPlayerChoice;

	public void OnEnqueued(Action<GameAction> afterFinished, uint id)
	{
		if (State != GameActionState.None)
		{
			throw new InvalidOperationException($"GameAction {this} was enqueued to the queue twice!");
		}
		Log.VeryDebug($"Action {this} enqueued with id {id}");
		Id = id;
		AfterFinished += afterFinished;
		State = GameActionState.WaitingForExecution;
	}

	public async Task Execute()
	{
		_pauseForPlayerChoiceTaskSource = new TaskCompletionSource();
		switch (State)
		{
		case GameActionState.WaitingForExecution:
			_logger.VeryDebug($"Action {this} began executing");
			State = GameActionState.Executing;
			this.BeforeExecuted?.Invoke(this);
			_executionTask = TaskHelper.RunSafely(ExecuteAction());
			break;
		case GameActionState.ReadyToResumeExecuting:
			_logger.VeryDebug($"Action {this} resumed execution");
			State = GameActionState.Executing;
			_executeAfterResumptionTaskSource.SetResult();
			break;
		default:
			throw new InvalidOperationException($"Attempted to execute GameAction {this} from invalid state {State}! Expected WaitingForExecution or ReadyToResumeExecuting");
		}
		try
		{
			await TaskHelper.WhenAny(_executionTask, _pauseForPlayerChoiceTaskSource.Task);
		}
		finally
		{
			if (_executionTask.IsCompleted)
			{
				_logger.VeryDebug($"Action {this} finished execution");
				State = GameActionState.Finished;
				_completionSource.SetResult();
				this.AfterFinished?.Invoke(this);
			}
			else
			{
				_logger.VeryDebug($"Action {this} paused execution");
			}
		}
	}

	public void ResumeAfterGatheringPlayerChoice(uint newId)
	{
		if (State != GameActionState.GatheringPlayerChoice)
		{
			throw new InvalidOperationException($"Tried setting GameAction {this} ready from invalid state {State}! Expected GatheringPlayerChoice");
		}
		_logger.VeryDebug($"Action {this} finished gathering player choice, and is assigned new id {newId}");
		Id = newId;
		this.BeforeReadyToResumeAfterPlayerChoice?.Invoke(this);
		State = GameActionState.ReadyToResumeExecuting;
	}

	public async Task WaitForActionToResumeExecutingAfterPlayerChoice()
	{
		_logger.VeryDebug($"Action {this} waiting to resume execution after player choice");
		await _executeAfterResumptionTaskSource.Task;
		_executeAfterResumptionTaskSource = null;
		this.BeforeResumedAfterPlayerChoice?.Invoke(this);
	}

	public void PauseForPlayerChoice()
	{
		if (State != GameActionState.Executing)
		{
			throw new InvalidOperationException($"Tried to pause GameAction {this} from invalid state {State}! Expected Executing");
		}
		_logger.VeryDebug($"Action {this} gathering player choice");
		_executeAfterResumptionTaskSource = new TaskCompletionSource();
		this.BeforePausedForPlayerChoice?.Invoke(this);
		State = GameActionState.GatheringPlayerChoice;
		_pauseForPlayerChoiceTaskSource.SetResult();
	}

	protected abstract Task ExecuteAction();

	public void Cancel()
	{
		State = GameActionState.Canceled;
		this.BeforeCancelled?.Invoke(this);
		CancelAction();
	}

	protected virtual void CancelAction()
	{
	}

	public abstract INetAction ToNetAction();
}
