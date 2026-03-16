using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.GameActions;

public class GenericHookGameAction : GameAction
{
	private readonly TaskCompletionSource _executionStartedSource = new TaskCompletionSource();

	private readonly TaskCompletionSource _choiceContextSetSource = new TaskCompletionSource();

	private readonly GameActionType _gameActionType;

	public override bool RecordableToReplay => false;

	public HookPlayerChoiceContext? ChoiceContext { get; private set; }

	public override ulong OwnerId { get; }

	public override GameActionType ActionType => _gameActionType;

	public uint HookId { get; }

	public Task ExecutionStartedTask => _executionStartedSource.Task;

	public GenericHookGameAction(uint hookId, ulong ownerId, GameActionType gameActionType)
	{
		_gameActionType = gameActionType;
		HookId = hookId;
		OwnerId = ownerId;
		if (_gameActionType != GameActionType.Combat && _gameActionType != GameActionType.CombatPlayPhaseOnly)
		{
			throw new InvalidOperationException($"Unexpected GameActionType {_gameActionType} received for GenericHookGameAction!");
		}
	}

	public void SetChoiceContext(HookPlayerChoiceContext choiceContext)
	{
		if (choiceContext.Owner?.NetId != OwnerId)
		{
			throw new InvalidOperationException($"Assigned choice context with owner {choiceContext.Owner?.NetId} to GenericHookGameAction with owner {OwnerId}!");
		}
		ChoiceContext = choiceContext;
		_choiceContextSetSource.SetResult();
	}

	protected override async Task ExecuteAction()
	{
		await _choiceContextSetSource.Task;
		_executionStartedSource.SetResult();
		await ChoiceContext.Task;
	}

	public override INetAction ToNetAction()
	{
		throw new NotImplementedException();
	}

	public override string ToString()
	{
		return $"{"GenericHookGameAction"} id {HookId} owner {OwnerId}";
	}
}
