using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Entities.Orbs;

public class OrbQueue
{
	public const int maxCapacity = 10;

	private readonly Player _owner;

	private readonly List<OrbModel> _orbs = new List<OrbModel>();

	public IReadOnlyList<OrbModel> Orbs => _orbs;

	public int Capacity { get; private set; }

	public OrbQueue(Player owner)
	{
		_owner = owner;
	}

	public void Clear()
	{
		_orbs.Clear();
		Capacity = 0;
	}

	public void AddCapacity(int capacity)
	{
		Capacity += capacity;
	}

	public void RemoveCapacity(int capacity)
	{
		Capacity = Math.Max(0, Capacity - capacity);
		while (Orbs.Count > Capacity)
		{
			Remove(_orbs.Last());
		}
	}

	public async Task<bool> TryEnqueue(OrbModel orb)
	{
		if (Capacity == 0)
		{
			return false;
		}
		orb.AssertMutable();
		if (Orbs.Count >= Capacity)
		{
			throw new InvalidOperationException("OrbQueue is full");
		}
		_orbs.Add(orb);
		await SmallWait();
		return true;
	}

	public bool Remove(OrbModel orb)
	{
		return _orbs.Remove(orb);
	}

	public void Insert(int idx, OrbModel orb)
	{
		if (idx >= Capacity)
		{
			throw new InvalidOperationException("idx cannot be greater than capacity");
		}
		_orbs.Insert(idx, orb);
	}

	public async Task BeforeTurnEnd(PlayerChoiceContext choiceContext)
	{
		foreach (OrbModel orb in Orbs.ToList())
		{
			if (_owner.Creature.CombatState == null)
			{
				return;
			}
			List<AbstractModel> modifyingModels;
			int triggerCount = Hook.ModifyOrbPassiveTriggerCount(_owner.Creature.CombatState, orb, 1, out modifyingModels);
			await Hook.AfterModifyingOrbPassiveTriggerCount(_owner.Creature.CombatState, orb, modifyingModels);
			if (_owner.Creature.CombatState == null)
			{
				return;
			}
			for (int i = 0; i < triggerCount; i++)
			{
				await orb.BeforeTurnEndOrbTrigger(choiceContext);
				await SmallWait();
			}
		}
	}

	public async Task AfterTurnStart(PlayerChoiceContext choiceContext)
	{
		foreach (OrbModel orb in Orbs.ToList())
		{
			if (_owner.Creature.CombatState == null)
			{
				return;
			}
			List<AbstractModel> modifyingModels;
			int triggerCount = Hook.ModifyOrbPassiveTriggerCount(_owner.Creature.CombatState, orb, 1, out modifyingModels);
			await Hook.AfterModifyingOrbPassiveTriggerCount(_owner.Creature.CombatState, orb, modifyingModels);
			if (_owner.Creature.CombatState == null)
			{
				return;
			}
			for (int i = 0; i < triggerCount; i++)
			{
				await orb.AfterTurnStartOrbTrigger(choiceContext);
				await SmallWait();
			}
		}
	}

	private async Task SmallWait()
	{
		if (LocalContext.IsMe(_owner))
		{
			await Cmd.CustomScaledWait(0.1f, 0.25f);
		}
		else
		{
			await Cmd.Wait(0.05f);
		}
	}
}
