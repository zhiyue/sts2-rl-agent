using System;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Singleton;

public class MultiplayerScalingModel : SingletonModel
{
	private RunState _runState;

	private CombatState? _combatState;

	public override bool ShouldReceiveCombatHooks => true;

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

	public override decimal ModifyPowerAmountGiven(PowerModel power, Creature giver, decimal amount, Creature? target, CardModel? cardSource)
	{
		if (target == null)
		{
			return amount;
		}
		if (target != null && !target.IsPrimaryEnemy && !target.IsSecondaryEnemy)
		{
			return amount;
		}
		if (!power.ShouldScaleInMultiplayer)
		{
			return amount;
		}
		int count = _runState.Players.Count;
		if (count == 1)
		{
			return amount;
		}
		if ((power is ArtifactPower || power is SlipperyPower || power is PlatingPower || power is BufferPower) ? true : false)
		{
			return (decimal)((count - 1) * 2 + 1) * amount;
		}
		return amount * (decimal)count * GetMultiplayerScaling(_combatState.Encounter, _runState.CurrentActIndex);
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
