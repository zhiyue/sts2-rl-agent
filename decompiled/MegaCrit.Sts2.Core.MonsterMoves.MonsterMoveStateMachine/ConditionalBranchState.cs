using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

public class ConditionalBranchState : MonsterState
{
	private readonly struct ConditionalBranch
	{
		public readonly string id;

		private readonly Func<bool> _conditionalLambda;

		public ConditionalBranch(MonsterState state, Func<bool> condition)
		{
			id = state.Id;
			_conditionalLambda = condition;
		}

		public float Evaluate()
		{
			if (_conditionalLambda != null)
			{
				return _conditionalLambda() ? 1 : 0;
			}
			return 1f;
		}
	}

	public string BranchId { get; }

	public override string Id => BranchId;

	private List<ConditionalBranch> States { get; } = new List<ConditionalBranch>();

	public override bool ShouldAppearInLogs => false;

	public ConditionalBranchState(string stateId)
	{
		BranchId = stateId;
	}

	public void AddState(MonsterState move, Func<bool> condition)
	{
		States.Add(new ConditionalBranch(move, condition));
	}

	public override string GetNextState(Creature _, Rng __)
	{
		foreach (ConditionalBranch state in States)
		{
			if (state.Evaluate() > 0f)
			{
				return state.id;
			}
		}
		throw new InvalidOperationException("No valid next state found.");
	}

	public override void RegisterStates(Dictionary<string, MonsterState> monsterStates)
	{
		monsterStates.Add(Id, this);
	}
}
