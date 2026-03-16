namespace MegaCrit.Sts2.Core.Entities.Actions;

public enum GameActionState
{
	None,
	WaitingForExecution,
	Executing,
	GatheringPlayerChoice,
	ReadyToResumeExecuting,
	Finished,
	Canceled
}
