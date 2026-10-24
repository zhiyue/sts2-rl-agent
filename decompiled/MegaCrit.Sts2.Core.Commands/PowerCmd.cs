using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace MegaCrit.Sts2.Core.Commands;

public static class PowerCmd
{
	/// <summary>
	/// Apply a power to multiple creatures.
	/// </summary>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="targets">Creatures to apply the power to.</param>
	/// <param name="amount">Amount of the power to apply.</param>
	/// <param name="applier">Creature applying the power.</param>
	/// <param name="cardSource">Optional card that is applying the power.</param>
	/// <param name="silent">Pass true if no flashes should play for this power application.</param>
	/// <typeparam name="T">Type of power to apply.</typeparam>
	/// <returns>
	/// The power that was applied.
	/// Can be null if:
	/// * Combat is over.
	/// * Something blocked application (like Artifact).
	/// * The power's amount was changed to 0 (like if you had 3 Strength and applied -3 Strength).
	/// </returns>
	public static async Task<IReadOnlyList<T>> Apply<T>(PlayerChoiceContext choiceContext, IEnumerable<Creature> targets, decimal amount, Creature? applier, CardModel? cardSource, bool silent = false) where T : PowerModel
	{
		List<T> powers = new List<T>();
		foreach (Creature target in targets)
		{
			T val = await Apply<T>(choiceContext, target, amount, applier, cardSource, silent);
			if (val != null)
			{
				powers.Add(val);
			}
		}
		return powers;
	}

	/// <summary>
	/// Apply a power to a creature.
	/// </summary>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="target">Creature to apply the power to.</param>
	/// <param name="amount">Amount of the power to apply.</param>
	/// <param name="applier">Creature applying the power.</param>
	/// <param name="cardSource">Optional card that is applying the power.</param>
	/// <param name="silent">Pass true if no flashes should play for this power application.</param>
	/// <typeparam name="T">Type of power to apply.</typeparam>
	/// <returns>
	/// The power that was applied.
	/// Can be null if:
	/// * Combat is over.
	/// * Something blocked application (like Artifact).
	/// * The power's amount was changed to 0 (like if you had 3 Strength and applied -3 Strength).
	/// </returns>
	public static async Task<T?> Apply<T>(PlayerChoiceContext choiceContext, Creature target, decimal amount, Creature? applier, CardModel? cardSource, bool silent = false) where T : PowerModel
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
		PowerModel power = FindExistingInstanceForStacking(powerModel, target, applier);
		if (power == null)
		{
			power = powerModel.ToMutable();
			await Apply(choiceContext, power, target, amount, applier, cardSource, silent);
		}
		else if (await ModifyAmount(choiceContext, power, amount, applier, cardSource, silent) == 0)
		{
			power = null;
		}
		return power as T;
	}

	/// <summary>
	/// Apply a power to a creature.
	/// </summary>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="power">Power to apply.</param>
	/// <param name="target">Creature to apply the power to.</param>
	/// <param name="amount">Amount of the power to apply.</param>
	/// <param name="applier">Creature applying the power.</param>
	/// <param name="cardSource">Optional card that is applying the power.</param>
	/// <param name="silent">Pass true if no flashes should play for this power application.</param>
	public static async Task Apply(PlayerChoiceContext choiceContext, PowerModel power, Creature target, decimal amount, Creature? applier, CardModel? cardSource, bool silent = false)
	{
		if (CombatManager.Instance.IsEnding || amount == 0m || !target.CanReceivePowers)
		{
			return;
		}
		ICombatState combatState = target.CombatState;
		if (combatState == null)
		{
			return;
		}
		PowerModel powerModel = FindExistingInstanceForStacking(power, target, applier);
		if (powerModel != null)
		{
			await ModifyAmount(choiceContext, powerModel, amount, applier, cardSource);
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
		if (combatState.Players.Count > 1 && (target.IsPrimaryEnemy || target.IsSecondaryEnemy) && power.ShouldScaleInMultiplayer)
		{
			modifiedAmount = power.GetScaledAmountForMultiplayer(combatState, applier, modifiedAmount, target, cardSource);
		}
		await power.BeforeApplied(target, modifiedAmount, applier, cardSource);
		if (target.CanReceivePowers)
		{
			power.ApplyInternal(target, modifiedAmount, silent);
			if (modifiedAmount != 0m)
			{
				CombatManager.Instance.History.PowerReceived(combatState, power, modifiedAmount, applier);
			}
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
				await Hook.AfterPowerAmountChanged(combatState, choiceContext, power, modifiedAmount, applier, cardSource);
			}
		}
	}

	/// <summary>
	/// Looks for a valid existing instance for a given power within the target
	///  for the sake of adjusting the amount on them.
	/// </summary>
	public static PowerModel? FindExistingInstanceForStacking(PowerModel basePower, Creature target, Creature? applier)
	{
		return basePower.InstanceType switch
		{
			PowerInstanceType.Instanced => null, 
			PowerInstanceType.InstancedPerApplier => target.GetPowerInstances(basePower.Id).FirstOrDefault((PowerModel p) => p.Applier == applier), 
			PowerInstanceType.None => target.GetPower(basePower.Id), 
			_ => throw new ArgumentOutOfRangeException("InstanceType"), 
		};
	}

	/// <summary>
	/// Decrement the amount of a power by 1.
	/// </summary>
	/// <param name="power">Power to decrement.</param>
	public static async Task Decrement(PowerModel power)
	{
		await ModifyAmount(new ThrowingPlayerChoiceContext(), power, -1m, null, null);
	}

	/// <summary>
	/// Tick down the amount of a duration-based power (Vulnerable, Weak, etc.).
	/// Similar to Decrement, but respects some extra rules about turn ordering.
	/// </summary>
	/// <param name="power">Power to tick down.</param>
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

	/// <summary>
	/// Modify the amount of a power.
	/// </summary>
	/// <param name="choiceContext">The context that is signalled in the event of a player choice.</param>
	/// <param name="power">Power to modify.</param>
	/// <param name="offset">Amount to modify it by (can be positive or negative).</param>
	/// <param name="applier">Creature applying the power.</param>
	/// <param name="cardSource">Optional card that is modifying the power amount.</param>
	/// <param name="silent">Pass true if no flashes should play for this power application.</param>
	/// <returns>
	/// The new amount of the power.
	/// For example, if you have 1 Strength and modify it by 3, this will return 4.
	/// </returns>
	public static async Task<int> ModifyAmount(PlayerChoiceContext choiceContext, PowerModel power, decimal offset, Creature? applier, CardModel? cardSource, bool silent = false)
	{
		if (CombatManager.Instance.IsEnding)
		{
			return 0;
		}
		Creature owner = power.Owner;
		ICombatState combatState = owner.CombatState;
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
			await Hook.AfterPowerAmountChanged(combatState, choiceContext, power, modifiedOffset, applier, cardSource);
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
				try
				{
					await nCreature.UpdateIntent(combatState.Allies);
				}
				catch (ObjectDisposedException ex)
				{
					Log.Error(ex.ToString());
				}
			}
		}
		if (power.IsVisible && CombatManager.Instance.IsInProgress)
		{
			await Cmd.CustomScaledWait(0.1f, 0.25f);
		}
		return newAmount;
	}

	/// <summary>
	/// Remove a power.
	/// </summary>
	/// <param name="creature">Creature to remove the power from.</param>
	/// <typeparam name="T">Type of power to remove.</typeparam>
	public static async Task Remove<T>(Creature creature) where T : PowerModel
	{
		await Remove(creature.GetPower<T>());
	}

	/// <summary>
	/// Remove a power.
	/// </summary>
	/// <param name="power">Power to remove.</param>
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
