using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Actions;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;

namespace MegaCrit.Sts2.Core.GameActions;

/// <summary>
/// A GameAction is a thin wrapper around an async task that should be run in response to player input.
/// THIS IS DIFFERENT from GameActions in STS1 (and the original Unity version of STS2).
///
/// In STS1, a GameAction represented a small unit of game logic, like dealing damage, gaining block, etc.
///
/// In STS2, these small units of logic are handled by Commands (see the MegaCrit.Sts2.Core.Commands namespace).
/// A GameAction WRAPS these commands, and should ONLY be used for player input.
///
/// Examples of things that SHOULD be wrapped in a game action:
/// * Playing a card.
/// * Drinking a potion.
/// * Clicking the "end player turn" button.
///
/// Examples of things that should NOT be wrapped in a game action:
/// * Dealing damage (this can happen as PART of a game action, but never directly from player input).
/// * Gaining block (same reason).
/// * Applying a power to a creature (same reason).
/// * A monster making a move (once the player's turn ends, monster moves are run in sequence. They wait on each other
///   to execute, but never on player input, so no GameActions are necessary).
/// </summary>
public abstract class GameAction
{
	private static readonly Logger _logger = new Logger("GameAction", LogType.Actions);

	private TaskCompletionSource? _pauseForPlayerChoiceTaskSource;

	private TaskCompletionSource? _executeAfterResumptionTaskSource;

	private TaskCompletionSource _completionSource = new TaskCompletionSource();

	private Task? _executionTask;

	public GameActionState State { get; private set; }

	public abstract ulong OwnerId { get; }

	/// <summary>
	/// The type of game action, which dictates cancellation and enqueuing timings. See comments in the enum for
	/// details.
	/// </summary>
	public abstract GameActionType ActionType { get; }

	public uint? Id { get; private set; }

	/// <summary>
	/// Use this to wait for the GameAction to be fully completed.
	/// </summary>
	public Task CompletionTask => _completionSource.Task;

	public Exception? Exception => _executionTask?.Exception;

	public virtual bool RecordableToReplay => true;

	/// <summary>
	/// Called when the GameAction is fully run to completion, but just before we complete _completionSource.
	/// This timing is important for checksum generation.
	/// </summary>
	public event Action<GameAction>? JustBeforeFinished;

	/// <summary>
	/// Called when the GameAction is fully run to completion.
	/// </summary>
	public event Action<GameAction>? AfterFinished;

	/// <summary>
	/// Called just before the GameAction begins execution for the first time.
	/// Not called when the GameAction is resumed.
	/// </summary>
	public event Action<GameAction>? BeforeExecuted;

	/// <summary>
	/// Called when the GameAction is cancelled.
	/// </summary>
	public event Action<GameAction>? BeforeCancelled;

	/// <summary>
	/// Called when the GameAction is paused for player choice.
	/// </summary>
	public event Action<GameAction>? BeforePausedForPlayerChoice;

	/// <summary>
	/// Called when the GameAction is ready to resume after player choice.
	/// </summary>
	public event Action<GameAction>? BeforeReadyToResumeAfterPlayerChoice;

	/// <summary>
	/// Called when the GameAction resumes execution after player choice.
	/// </summary>
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
				this.JustBeforeFinished?.Invoke(this);
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
