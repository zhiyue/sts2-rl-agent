using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

public class RandomBranchState : MonsterState
{
	public struct StateWeight
	{
		public string stateId;

		public MoveRepeatType repeatType;

		public int maxTimes;

		public Func<float> weightLambda;

		public int cooldown;

		public float GetWeight()
		{
			if (weightLambda != null)
			{
				return weightLambda();
			}
			throw new InvalidOperationException(stateId + " doesn't have a weight");
		}
	}

	public string StateId { get; }

	public List<StateWeight> States { get; set; } = new List<StateWeight>();

	public override bool ShouldAppearInLogs => false;

	public override string Id => StateId;

	public RandomBranchState(string id)
	{
		StateId = id;
	}

	public void AddBranch(MonsterState state, int cooldown, MoveRepeatType repeatType, Func<float> weight)
	{
		if (repeatType.Equals(MoveRepeatType.CanRepeatXTimes))
		{
			throw new ArgumentException("Use other constructor to specify number of repeats");
		}
		StateWeight item = new StateWeight
		{
			repeatType = repeatType,
			stateId = state.Id,
			weightLambda = weight,
			cooldown = cooldown
		};
		States.Add(item);
	}

	public void AddBranch(MonsterState state, int cooldown, int maxRepeats, Func<float> weight)
	{
		StateWeight item = new StateWeight
		{
			maxTimes = maxRepeats,
			repeatType = MoveRepeatType.CanRepeatXTimes,
			stateId = state.Id,
			weightLambda = weight,
			cooldown = cooldown
		};
		States.Add(item);
	}

	public void AddBranch(MonsterState state, int maxRepeats, Func<float> weight)
	{
		AddBranch(state, 0, maxRepeats, weight);
	}

	public void AddBranch(MonsterState state, int cooldown, MoveRepeatType repeatType, float weight)
	{
		AddBranch(state, cooldown, repeatType, () => weight);
	}

	public void AddBranch(MonsterState state, MoveRepeatType repeatType, float weight)
	{
		AddBranch(state, repeatType, () => weight);
	}

	public void AddBranch(MonsterState state, MoveRepeatType repeatType, Func<float> weight)
	{
		AddBranch(state, 0, repeatType, weight);
	}

	public void AddBranch(MonsterState state, int maxRepeats, float weight)
	{
		AddBranch(state, maxRepeats, () => weight);
	}

	public void AddBranch(MonsterState state, int cooldown, MoveRepeatType repeatType)
	{
		AddBranch(state, cooldown, repeatType, 1f);
	}

	public void AddBranch(MonsterState state, int maxRepeats)
	{
		AddBranch(state, maxRepeats, 1f);
	}

	public void AddBranch(MonsterState state, MoveRepeatType repeatType)
	{
		AddBranch(state, repeatType, 1f);
	}

	public override string GetNextState(Creature owner, Rng rng)
	{
		float max = States.Sum((StateWeight x) => GetStateWeight(x, owner));
		float num = rng.NextFloat(max);
		foreach (StateWeight state in States)
		{
			num -= GetStateWeight(state, owner);
			if (num <= 0f)
			{
				return state.stateId;
			}
		}
		throw new InvalidOperationException("No valid state found in RandomBranchState " + Id + "!");
	}

	private static float GetStateWeight(StateWeight stateWeight, Creature owner)
	{
		MonsterMoveStateMachine moveStateMachine = owner.Monster.MoveStateMachine;
		float num = 1f;
		if (stateWeight.repeatType.Equals(MoveRepeatType.UseOnlyOnce))
		{
			MonsterState item = moveStateMachine.States[stateWeight.stateId];
			if (moveStateMachine.StateLog.Contains(item))
			{
				num = 0f;
			}
		}
		else if (!stateWeight.repeatType.Equals(MoveRepeatType.CanRepeatForever))
		{
			float num2 = (stateWeight.repeatType.Equals(MoveRepeatType.CannotRepeat) ? 1 : stateWeight.maxTimes);
			num = (((float)moveStateMachine.StateLog.Count < num2) ? 1 : 0);
			int num3 = 0;
			while ((float)moveStateMachine.StateLog.Count >= num2 && (float)num3 < num2 && moveStateMachine.StateLog.Count - num3 > 0)
			{
				MonsterState monsterState = moveStateMachine.States[stateWeight.stateId];
				if (moveStateMachine.StateLog[moveStateMachine.StateLog.Count - 1 - num3] != monsterState)
				{
					num = 1f;
					break;
				}
				num3++;
			}
		}
		if (stateWeight.cooldown > 0)
		{
			IEnumerable<MonsterState> source = moveStateMachine.StateLog.Where((MonsterState state) => state.IsMove).Reverse().Take(stateWeight.cooldown);
			if (source.Any((MonsterState move) => move.Id == stateWeight.stateId))
			{
				return 0f;
			}
		}
		return num * stateWeight.GetWeight();
	}

	public override void RegisterStates(Dictionary<string, MonsterState> monsterStates)
	{
		monsterStates.Add(Id, this);
	}
}
