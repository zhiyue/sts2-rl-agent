using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace MegaCrit.Sts2.Core.Commands;

public static class PowerCmd
{
	public static async Task<IReadOnlyList<T>> Apply<T>(IEnumerable<Creature> targets, decimal amount, Creature? applier, CardModel? cardSource, bool silent = false) where T : PowerModel
	{
		List<T> powers = new List<T>();
		foreach (Creature target in targets)
		{
			T val = await Apply<T>(target, amount, applier, cardSource, silent);
			if (val != null)
			{
				powers.Add(val);
			}
		}
		return powers;
	}

	public static async Task<T?> Apply<T>(Creature target, decimal amount, Creature? applier, CardModel? cardSource, bool silent = false) where T : PowerModel
	{
		if (CombatManager.Instance.IsEnding)
		{
			return null;
		}
		if (!target.CanReceivePowers)
		{
			return null;
		}
		PowerModel powerModel = ModelDb.Power<T>();
		PowerModel power;
		if (powerModel.IsInstanced || !target.HasPower<T>())
		{
			power = powerModel.ToMutable();
			await Apply(power, target, amount, applier, cardSource, silent);
		}
		else
		{
			power = target.GetPower<T>();
			if (power == null)
			{
				throw new InvalidOperationException("Creature missing expected power.");
			}
			if (await ModifyAmount(power, amount, applier, cardSource, silent) == 0)
			{
				power = null;
			}
		}
		return power as T;
	}

	public static async Task Apply(PowerModel power, Creature target, decimal amount, Creature? applier, CardModel? cardSource, bool silent = false)
	{
		if (CombatManager.Instance.IsEnding || amount == 0m || !target.CanReceivePowers)
		{
			return;
		}
		CombatState combatState = target.CombatState;
		if (combatState == null)
		{
			return;
		}
		if (!power.IsInstanced && target.HasPower(power.Id))
		{
			PowerModel power2 = target.GetPower(power.Id);
			if (power2 == null)
			{
				throw new InvalidOperationException("Creature missing expected power.");
			}
			await ModifyAmount(power2, amount, applier, cardSource);
			return;
		}
		power.AssertMutable();
		power.Applier = applier;
		await Hook.BeforePowerAmountChanged(combatState, power, amount, target, applier, cardSource);
		decimal modifiedAmount = amount;
		IEnumerable<AbstractModel> givenModifiers = null;
		if (applier != null && combatState.ContainsCreature(applier))
		{
			modifiedAmount = Hook.ModifyPowerAmountGiven(combatState, power, applier, modifiedAmount, target, cardSource, out givenModifiers);
		}
		modifiedAmount = Hook.ModifyPowerAmountReceived(combatState, power, target, modifiedAmount, applier, out IEnumerable<AbstractModel> receivedModifiers);
		await power.BeforeApplied(target, modifiedAmount, applier, cardSource);
		if (modifiedAmount != 0m)
		{
			CombatManager.Instance.History.PowerReceived(combatState, power, modifiedAmount, applier);
		}
		power.ApplyInternal(target, modifiedAmount, silent);
		if (power.IsVisible && CombatManager.Instance.IsInProgress)
		{
			await Cmd.CustomScaledWait(0.1f, 0.25f);
		}
		if (target.Side == CombatSide.Player && power.Type == PowerType.Debuff)
		{
			power.SkipNextDurationTick = true;
		}
		if (givenModifiers != null)
		{
			await Hook.AfterModifyingPowerAmountGiven(combatState, givenModifiers, power);
		}
		await Hook.AfterModifyingPowerAmountReceived(combatState, receivedModifiers, power);
		if (modifiedAmount != 0m)
		{
			await power.AfterApplied(applier, cardSource);
			await Hook.AfterPowerAmountChanged(combatState, power, modifiedAmount, applier, cardSource);
		}
	}

	public static async Task Decrement(PowerModel power)
	{
		await ModifyAmount(power, -1m, null, null);
	}

	public static async Task TickDownDuration(PowerModel power)
	{
		if (power.SkipNextDurationTick)
		{
			power.SkipNextDurationTick = false;
		}
		else
		{
			await Decrement(power);
		}
	}

	public static async Task<int> ModifyAmount(PowerModel power, decimal offset, Creature? applier, CardModel? cardSource, bool silent = false)
	{
		if (CombatManager.Instance.IsEnding)
		{
			return 0;
		}
		Creature owner = power.Owner;
		CombatState combatState = owner.CombatState;
		if (combatState == null)
		{
			return 0;
		}
		await Hook.BeforePowerAmountChanged(combatState, power, offset, owner, applier, cardSource);
		decimal modifiedOffset = offset;
		IEnumerable<AbstractModel> modifiers = null;
		if (applier != null && combatState.ContainsCreature(applier))
		{
			modifiedOffset = Hook.ModifyPowerAmountGiven(combatState, power, applier, modifiedOffset, owner, cardSource, out modifiers);
		}
		modifiedOffset = Hook.ModifyPowerAmountReceived(combatState, power, owner, modifiedOffset, applier, out IEnumerable<AbstractModel> receivedModifiers);
		CombatManager.Instance.History.PowerReceived(combatState, power, modifiedOffset, applier);
		int newAmount = power.Amount + (int)modifiedOffset;
		power.SetAmount(newAmount, silent);
		if (modifiers != null)
		{
			await Hook.AfterModifyingPowerAmountGiven(combatState, modifiers, power);
		}
		await Hook.AfterModifyingPowerAmountReceived(combatState, receivedModifiers, power);
		if ((int)modifiedOffset != 0)
		{
			await Hook.AfterPowerAmountChanged(combatState, power, modifiedOffset, applier, cardSource);
		}
		if (power.ShouldRemoveDueToAmount())
		{
			await Remove(power);
		}
		if (CombatManager.Instance.IsInProgress && owner != null && owner.IsMonster && owner.IsAlive)
		{
			NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(owner);
			if (nCreature != null)
			{
				await nCreature.UpdateIntent(combatState.Allies);
			}
		}
		if (power.IsVisible && CombatManager.Instance.IsInProgress)
		{
			await Cmd.CustomScaledWait(0.1f, 0.25f);
		}
		return newAmount;
	}

	public static async Task<T?> SetAmount<T>(Creature target, decimal amount, Creature? applier, CardModel? cardSource) where T : PowerModel
	{
		if (CombatManager.Instance.IsEnding)
		{
			return null;
		}
		T existingPower = target.GetPower<T>();
		if (existingPower == null)
		{
			return await Apply<T>(target, amount, applier, cardSource);
		}
		await ModifyAmount(existingPower, amount - (decimal)existingPower.Amount, applier, cardSource);
		return existingPower;
	}

	public static async Task Remove<T>(Creature creature) where T : PowerModel
	{
		await Remove(creature.GetPower<T>());
	}

	public static async Task Remove(PowerModel? power)
	{
		if (power != null)
		{
			power.RemoveInternal();
			await Cmd.CustomScaledWait(0.2f, 0.4f);
			await power.AfterRemoved(power.Owner);
		}
	}
}
