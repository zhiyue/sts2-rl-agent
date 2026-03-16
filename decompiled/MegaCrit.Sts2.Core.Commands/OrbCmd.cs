using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Orbs;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace MegaCrit.Sts2.Core.Commands;

public static class OrbCmd
{
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

	public static void RemoveSlots(Player player, int amount)
	{
		if (!CombatManager.Instance.IsOverOrEnding)
		{
			amount = Math.Min(player.PlayerCombatState.OrbQueue.Capacity, amount);
			player.PlayerCombatState.OrbQueue.RemoveCapacity(amount);
			NCombatRoom.Instance?.GetCreatureNode(player.Creature).OrbManager?.RemoveSlotAnim(amount);
		}
	}

	public static async Task Channel<T>(PlayerChoiceContext choiceContext, Player player) where T : OrbModel
	{
		await Channel(choiceContext, ModelDb.Orb<T>().ToMutable(), player);
	}

	public static async Task Channel(PlayerChoiceContext choiceContext, OrbModel orb, Player player)
	{
		if (!CombatManager.Instance.IsOverOrEnding)
		{
			CombatState combatState = player.Creature.CombatState;
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

	private static async Task Evoke(PlayerChoiceContext choiceContext, Player player, OrbModel evokedOrb, bool dequeue = true)
	{
		if (CombatManager.Instance.IsOverOrEnding)
		{
			return;
		}
		OrbQueue orbQueue = player.PlayerCombatState.OrbQueue;
		if (orbQueue.Orbs.Count > 0)
		{
			bool removed = false;
			if (dequeue)
			{
				removed = orbQueue.Remove(evokedOrb);
				NCombatRoom.Instance?.GetCreatureNode(player.Creature)?.OrbManager?.EvokeOrbAnim(evokedOrb);
			}
			choiceContext.PushModel(evokedOrb);
			IEnumerable<Creature> targets = await evokedOrb.Evoke(choiceContext);
			choiceContext.PopModel(evokedOrb);
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

	public static Task Replace(OrbModel oldOrb, OrbModel newOrb, Player player)
	{
		if (CombatManager.Instance.IsOverOrEnding)
		{
			return Task.CompletedTask;
		}
		OrbQueue orbQueue = player.PlayerCombatState.OrbQueue;
		int idx = orbQueue.Orbs.IndexOf(oldOrb);
		newOrb.AssertMutable();
		newOrb.Owner = player;
		if (orbQueue.Remove(oldOrb))
		{
			oldOrb.RemoveInternal();
		}
		orbQueue.Insert(idx, newOrb);
		NCombatRoom.Instance?.GetCreatureNode(player.Creature)?.OrbManager?.ReplaceOrb(oldOrb, newOrb);
		return Task.CompletedTask;
	}

	public static void IncreaseBaseOrbCount(Player player, int amount)
	{
		player.BaseOrbSlotCount += amount;
	}
}
