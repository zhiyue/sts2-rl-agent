using System;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Singleton;

/// <summary>
/// Scales block gained by enemies.
/// </summary>
public class MultiplayerScalingModel : SingletonModel
{
	/// <summary>
	/// The <see cref="T:MegaCrit.Sts2.Core.Runs.RunState" /> that this model is scaling.
	/// Should always be set when creating a run, so we mark it as non-nullable.
	/// </summary>
	private RunState _runState;

	/// <summary>
	/// The CombatState that this model is scaling.
	/// Only set when we are in combat.
	/// </summary>
	private CombatState? _combatState;

	public override bool ShouldReceiveCombatHooks => true;

	/// <summary>
	/// Initialize with the specified <see cref="T:MegaCrit.Sts2.Core.Runs.RunState" />.
	/// </summary>
	public void Initialize(RunState state)
	{
		if (_runState != null)
		{
			throw new InvalidOperationException("Already initialized");
		}
		_runState = state;
	}

	public void OnCombatEntered(CombatState combatState)
	{
		_combatState = combatState;
	}

	public void OnCombatFinished()
	{
		_combatState = null;
	}

	public override decimal ModifyBlockMultiplicative(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
	{
		if (target != null && !target.IsPrimaryEnemy && !target.IsSecondaryEnemy)
		{
			return 1m;
		}
		if (!props.IsPoweredCardOrMonsterMoveBlock())
		{
			return 1m;
		}
		int count = _runState.Players.Count;
		if (count == 1)
		{
			return 1m;
		}
		return (decimal)count * GetMultiplayerScaling(_combatState.Encounter, _runState.CurrentActIndex);
	}

	public static decimal GetMultiplayerScaling(EncounterModel? encounter, int actIndex)
	{
		switch (actIndex)
		{
		case 0:
			return 1.1m;
		case 1:
			return 1.2m;
		case 2:
			if (encounter != null && encounter.RoomType == RoomType.Boss)
			{
				return 1.3m;
			}
			return 1.2m;
		default:
			throw new ArgumentOutOfRangeException("actIndex", actIndex, "Invalid act index for HP scaling");
		}
	}
}
