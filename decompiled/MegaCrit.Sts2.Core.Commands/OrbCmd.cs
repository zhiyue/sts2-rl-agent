using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Orbs;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace MegaCrit.Sts2.Core.Commands;

public static class OrbCmd
{
	/// <summary>
	/// Add orb slots to a creature.
	/// </summary>
	/// <param name="player">Player to add orb slots to.</param>
	/// <param name="amount">Number of orb slots to add.</param>
	public static Task AddSlots(Player player, int amount)
	{
		if (CombatManager.Instance.IsOverOrEnding)
		{
			return Task.CompletedTask;
		}
		amount = Math.Min(10 - player.PlayerCombatState.OrbQueue.Capacity, amount);
		player.PlayerCombatState.OrbQueue.AddCapacity(amount);
		NCombatRoom.Instance?.GetCreatureNode(player.Creature).OrbManager?.AddSlotAnim(amount);
		return Task.CompletedTask;
	}

	/// <summary>
	/// Remove orb slots from the creature. Starts from the back of the list.
	/// Orb slots with orbs already in them are also removed.
	/// </summary>
	/// <param name="player">Player to remove orb slots from.</param>
	/// <param name="amount">Number of orb slots to remove.</param>
	public static void RemoveSlots(Player player, int amount)
	{
		if (!CombatManager.Instance.IsOverOrEnding)
		{
			amount = Math.Min(player.PlayerCombatState.OrbQueue.Capacity, amount);
			player.PlayerCombatState.OrbQueue.RemoveCapacity(amount);
			NCombatRoom.Instance?.GetCreatureNode(player.Creature).OrbManager?.RemoveSlotAnim(amount);
		}
	}

	/// <summary>
	/// Channel an orb of the specified type.
	/// </summary>
	/// <param name="choiceContext">The context with which to handle player choices.</param>
	/// <param name="player">Player who is channeling the orb.</param>
	/// <typeparam name="T">Type of orb to channel.</typeparam>
	public static async Task Channel<T>(PlayerChoiceContext choiceContext, Player player) where T : OrbModel
	{
		await Channel(choiceContext, ModelDb.Orb<T>().ToMutable(), player);
	}

	/// <summary>
	/// Channel an orb.
	/// </summary>
	/// <param name="choiceContext">The context with which to handle player choices.</param>
	/// <param name="orb">Orb to channel.</param>
	/// <param name="player">Player who is channeling the orb.</param>
	public static async Task Channel(PlayerChoiceContext choiceContext, OrbModel orb, Player player)
	{
		if (!CombatManager.Instance.IsOverOrEnding)
		{
			ICombatState combatState = player.Creature.CombatState;
			OrbQueue orbQueue = player.PlayerCombatState.OrbQueue;
			if (player.Character.BaseOrbSlotCount == 0 && orbQueue.Capacity == 0)
			{
				await AddSlots(player, 1);
			}
			orb.AssertMutable();
			orb.Owner = player;
			if (orbQueue.Orbs.Count >= orbQueue.Capacity)
			{
				await EvokeNext(choiceContext, player);
			}
			if (await player.PlayerCombatState.OrbQueue.TryEnqueue(orb))
			{
				CombatManager.Instance.History.OrbChanneled(combatState, orb);
				orb.PlayChannelSfx();
				NCombatRoom.Instance?.GetCreatureNode(player.Creature)?.OrbManager?.AddOrbAnim();
				await Hook.AfterOrbChanneled(combatState, choiceContext, player, orb);
			}
		}
	}

	public static async Task EvokeNext(PlayerChoiceContext choiceContext, Player player, bool dequeue = true)
	{
		OrbQueue orbQueue = player.PlayerCombatState.OrbQueue;
		if (orbQueue.Orbs.Count > 0)
		{
			OrbModel orb = orbQueue.Orbs.First();
			choiceContext.PushModel(orb);
			await Evoke(choiceContext, player, orb, dequeue);
			choiceContext.PopModel(orb);
		}
	}

	public static async Task EvokeLast(PlayerChoiceContext choiceContext, Player player, bool dequeue = true)
	{
		OrbQueue orbQueue = player.PlayerCombatState.OrbQueue;
		if (orbQueue.Orbs.Count > 0)
		{
			OrbModel orb = orbQueue.Orbs.Last();
			choiceContext.PushModel(orb);
			await Evoke(choiceContext, player, orb, dequeue);
			choiceContext.PopModel(orb);
		}
	}

	/// <summary>
	/// Evoke the orb.
	/// </summary>
	/// <param name="choiceContext">The context with which to handle player choices.</param>
	/// <param name="player">Player whose next orb we're evoking.</param>
	/// <param name="evokedOrb">orb being evoked.</param>
	/// <param name="dequeue">Whether or not to dequeue the orb from the creature's orb queue after evoking (usually true).</param>
	private static async Task Evoke(PlayerChoiceContext choiceContext, Player player, OrbModel evokedOrb, bool dequeue = true)
	{
		if (CombatManager.Instance.IsOverOrEnding)
		{
			return;
		}
		OrbQueue orbQueue = player.PlayerCombatState.OrbQueue;
		if (orbQueue.Orbs.Count <= 0)
		{
			return;
		}
		bool removed = false;
		if (dequeue)
		{
			removed = orbQueue.Remove(evokedOrb);
			NCombatRoom.Instance?.GetCreatureNode(player.Creature)?.OrbManager?.EvokeOrbAnim(evokedOrb);
		}
		choiceContext.PushModel(evokedOrb);
		IEnumerable<Creature> targets = await evokedOrb.Evoke(choiceContext);
		choiceContext.PopModel(evokedOrb);
		if (player.Creature.CombatState != null)
		{
			await Hook.AfterOrbEvoked(choiceContext, player.Creature.CombatState, evokedOrb, targets);
			if (removed)
			{
				evokedOrb.RemoveInternal();
			}
		}
	}

	public static async Task Passive(PlayerChoiceContext choiceContext, OrbModel orb, Creature? target)
	{
		if (!CombatManager.Instance.IsOverOrEnding)
		{
			choiceContext.PushModel(orb);
			await orb.Passive(choiceContext, target);
			choiceContext.PopModel(orb);
		}
	}
}
