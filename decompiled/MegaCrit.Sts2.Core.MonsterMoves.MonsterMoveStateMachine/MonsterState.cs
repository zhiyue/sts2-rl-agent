using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

public abstract class MonsterState
{
	public abstract string Id { get; }

	public virtual bool ShouldAppearInLogs => true;

	public virtual bool CanTransitionAway => true;

	public virtual bool IsMove => this is MoveState;

	public abstract string GetNextState(Creature owner, Rng rng);

	public abstract void RegisterStates(Dictionary<string, MonsterState> monsterStates);

	public virtual void OnEnterState()
	{
	}

	public virtual void OnExitState()
	{
	}
}
