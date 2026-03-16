using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

public class MoveState : MonsterState
{
	private bool _performedAtLeastOnce;

	private readonly Func<IReadOnlyList<Creature>, Task> _onPerform;

	public IReadOnlyList<AbstractIntent> Intents { get; private set; }

	public string StateId { get; }

	public bool MustPerformOnceBeforeTransitioning { get; set; }

	public string? FollowUpStateId { get; init; }

	public MonsterState? FollowUpState { get; set; }

	public override bool CanTransitionAway
	{
		get
		{
			if (MustPerformOnceBeforeTransitioning)
			{
				return _performedAtLeastOnce;
			}
			return true;
		}
	}

	public override bool IsMove => true;

	public override string Id => StateId;

	public MoveState()
		: this("UNSET_MOVE", UnsetMove)
	{
	}

	public MoveState(string stateId, Func<IReadOnlyList<Creature>, Task> onPerform, params AbstractIntent[] intents)
	{
		_onPerform = onPerform;
		Intents = intents;
		StateId = stateId;
	}

	public async Task PerformMove(IEnumerable<Creature> targets)
	{
		_performedAtLeastOnce = true;
		Creature[] arg = (targets as Creature[]) ?? targets.ToArray();
		await _onPerform(arg);
	}

	public override void OnExitState()
	{
		_performedAtLeastOnce = false;
	}

	public override string GetNextState(Creature owner, Rng rng)
	{
		return (FollowUpState?.Id ?? FollowUpStateId) ?? throw new InvalidOperationException("No valid followup state.");
	}

	public override void RegisterStates(Dictionary<string, MonsterState> monsterStates)
	{
		monsterStates.Add(Id, this);
	}

	private static Task UnsetMove(IEnumerable<Creature> c)
	{
		throw new InvalidOperationException("No move has been set for the monster");
	}
}
