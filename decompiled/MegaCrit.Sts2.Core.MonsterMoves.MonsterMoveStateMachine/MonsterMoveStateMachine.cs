using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

public class MonsterMoveStateMachine
{
	private MonsterState _currentState;

	private readonly MonsterState _initialState;

	private bool _performedFirstMove;

	public Dictionary<string, MonsterState> States { get; } = new Dictionary<string, MonsterState>();

	public List<MonsterState> StateLog { get; } = new List<MonsterState>();

	public MonsterMoveStateMachine(IEnumerable<MonsterState> states, MonsterState initialState)
	{
		foreach (MonsterState state in states)
		{
			state.RegisterStates(States);
		}
		_initialState = initialState;
		_currentState = _initialState;
		if (_currentState.ShouldAppearInLogs)
		{
			StateLog.Add(_currentState);
		}
	}

	public MoveState RollMove(IEnumerable<Creature> targets, Creature owner, Rng rng)
	{
		FindNextMoveState(targets, owner, rng, logMove: true);
		if (!_currentState.IsMove)
		{
			throw new InvalidOperationException(_currentState.Id + " is not a valid move state");
		}
		return (MoveState)_currentState;
	}

	public void ForceCurrentState(MonsterState state)
	{
		SetCurrentState(state);
	}

	public void OnMovePerformed(MoveState _)
	{
		_performedFirstMove = true;
	}

	private void FindNextMoveState(IEnumerable<Creature> targets, Creature owner, Rng rng, bool logMove)
	{
		if (_currentState == null)
		{
			throw new InvalidOperationException("Cannot find next move state when current state is null.");
		}
		if (!_currentState.CanTransitionAway || (!_performedFirstMove && _currentState.IsMove))
		{
			return;
		}
		MonsterState monsterState = null;
		do
		{
			string nextState = _currentState.GetNextState(owner, rng);
			if (!string.IsNullOrEmpty(nextState) && !States.ContainsKey(nextState))
			{
				throw new InvalidOperationException("no valid state found: " + nextState);
			}
			SetCurrentState(string.IsNullOrEmpty(nextState) ? _initialState : States[nextState]);
			monsterState = ((monsterState == null && _currentState.ShouldAppearInLogs) ? _currentState : monsterState);
		}
		while (!_currentState.IsMove);
		if (logMove && monsterState != null)
		{
			StateLog.Add(monsterState);
		}
	}

	private void SetCurrentState(MonsterState state)
	{
		_currentState.OnExitState();
		_currentState = state;
		_currentState.OnEnterState();
	}
}
