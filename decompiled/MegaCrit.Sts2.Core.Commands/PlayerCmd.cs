using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Commands;

public static class PlayerCmd
{
	public const string goldSmallSfx = "event:/sfx/ui/gold/gold_1";

	public const string goldMediumSfx = "event:/sfx/ui/gold/gold_2";

	public const string goldLargeSfx = "event:/sfx/ui/gold/gold_3";

	/// <summary>
	/// Increase the current amount of energy that the player has.
	/// </summary>
	/// <param name="amount">Amount of energy to give.</param>
	/// <param name="player">Player to give the energy to.</param>
	public static async Task GainEnergy(decimal amount, Player player)
	{
		if (!(amount <= 0m) && !CombatManager.Instance.IsEnding)
		{
			ICombatState combatState = player.Creature.CombatState;
			IEnumerable<AbstractModel> modifiers;
			decimal finalAmount = Hook.ModifyEnergyGain(combatState, player, amount, out modifiers);
			await Hook.AfterModifyingEnergyGain(combatState, modifiers);
			if (finalAmount > 0m)
			{
				SfxCmd.Play("event:/sfx/ui/gain_energy");
				player.PlayerCombatState.GainEnergy(finalAmount);
			}
		}
	}

	/// <summary>
	/// Decrease the current amount of energy that the player has.
	/// </summary>
	/// <param name="amount">Amount of energy to remove.</param>
	/// <param name="player">Player to remove the energy from.</param>
	public static Task LoseEnergy(decimal amount, Player player)
	{
		if (amount <= 0m)
		{
			return Task.CompletedTask;
		}
		if (CombatManager.Instance.IsEnding)
		{
			return Task.CompletedTask;
		}
		player.PlayerCombatState.LoseEnergy(amount);
		return Task.CompletedTask;
	}

	/// <summary>
	/// Set the player to have a specific amount of energy.
	/// </summary>
	/// <param name="amount">New amount of energy.</param>
	/// <param name="player">Player whose energy we're setting.</param>
	public static async Task SetEnergy(decimal amount, Player player)
	{
		if (!CombatManager.Instance.IsEnding)
		{
			int energy = player.PlayerCombatState.Energy;
			if ((decimal)energy < amount)
			{
				await GainEnergy(amount - (decimal)energy, player);
			}
			else if ((decimal)energy > amount)
			{
				await LoseEnergy((decimal)energy - amount, player);
			}
		}
	}

	/// <summary>
	/// Increase the current amount of stars that the player has.
	/// </summary>
	/// <param name="amount">Amount of stars to give.</param>
	/// <param name="player">Player to give the stars to.</param>
	public static async Task GainStars(decimal amount, Player player)
	{
		if (!CombatManager.Instance.IsEnding && Hook.ShouldGainStars(player.Creature.CombatState, amount, player))
		{
			player.PlayerCombatState.GainStars(amount);
			await Hook.AfterStarsGained(player.Creature.CombatState, (int)amount, player);
		}
	}

	/// <summary>
	/// Decrease the current amount of stars that the player has.
	/// </summary>
	/// <param name="amount">Amount of stars to remove.</param>
	/// <param name="player">Player to remove the stars from.</param>
	public static Task LoseStars(decimal amount, Player player)
	{
		if (CombatManager.Instance.IsEnding)
		{
			return Task.CompletedTask;
		}
		player.PlayerCombatState.LoseStars(amount);
		return Task.CompletedTask;
	}

	/// <summary>
	/// Set the player to have a specific amount of stars.
	/// </summary>
	/// <param name="amount">New amount of stars.</param>
	/// <param name="player">Player whose stars we're setting.</param>
	public static async Task SetStars(decimal amount, Player player)
	{
		if (!CombatManager.Instance.IsEnding)
		{
			int stars = player.PlayerCombatState.Stars;
			if ((decimal)stars < amount)
			{
				await GainStars(amount - (decimal)stars, player);
			}
			else if ((decimal)stars > amount)
			{
				await LoseStars((decimal)stars - amount, player);
			}
		}
	}

	/// <summary>
	/// Increase the current amount of gold that the player has.
	/// </summary>
	/// <param name="amount">Amount of gold to give.</param>
	/// <param name="player">Player to give the gold to.</param>
	/// <param name="wasStolenBack">Was the gold stolen back from an enemy.</param>
	public static async Task GainGold(decimal amount, Player player, bool wasStolenBack = false)
	{
		IRunState runState = player.RunState;
		amount = Hook.ModifyGoldGained(runState, player.Creature.CombatState, amount, player, out IEnumerable<AbstractModel> modifiers);
		await Hook.AfterModifyingGoldGained(runState, player.Creature.CombatState, modifiers, player, amount);
		if (!(amount > 0m))
		{
			return;
		}
		if (player == LocalContext.GetMe(runState))
		{
			string text = ((amount >= 100m) ? "event:/sfx/ui/gold/gold_3" : ((!(amount > 30m)) ? "event:/sfx/ui/gold/gold_1" : "event:/sfx/ui/gold/gold_2"));
			string sfx = text;
			SfxCmd.Play(sfx);
		}
		PlayerMapPointHistoryEntry playerMapPointHistoryEntry = runState.CurrentMapPointHistoryEntry?.GetEntry(player.NetId);
		if (playerMapPointHistoryEntry != null)
		{
			if (wasStolenBack)
			{
				playerMapPointHistoryEntry.GoldStolen -= (int)amount;
			}
			else
			{
				playerMapPointHistoryEntry.GoldGained += (int)amount;
			}
		}
		player.Gold += (int)amount;
		await Hook.AfterGoldGained(runState, player);
	}

	/// <summary>
	/// Decrease the current amount of gold that the player has.
	/// </summary>
	/// <param name="amount">Amount of gold to lose.</param>
	/// <param name="player">Player to take the gold from.</param>
	/// <param name="goldLossType">How the player lost the gold</param>
	public static Task LoseGold(decimal amount, Player player, GoldLossType goldLossType = GoldLossType.Lost)
	{
		SfxCmd.Play("event:/sfx/ui/gold/gold_1");
		PlayerMapPointHistoryEntry playerMapPointHistoryEntry = player.RunState.CurrentMapPointHistoryEntry?.GetEntry(player.NetId);
		if (playerMapPointHistoryEntry != null)
		{
			switch (goldLossType)
			{
			case GoldLossType.Spent:
				playerMapPointHistoryEntry.GoldSpent += (int)amount;
				break;
			case GoldLossType.Lost:
				playerMapPointHistoryEntry.GoldLost += (int)amount;
				break;
			case GoldLossType.Stolen:
				playerMapPointHistoryEntry.GoldStolen += (int)amount;
				playerMapPointHistoryEntry.MarkLootStolen((int)amount);
				break;
			}
		}
		player.Gold = int.Max(0, player.Gold - (int)amount);
		return Task.CompletedTask;
	}

	/// <summary>
	/// Set the player to have a specific amount of gold.
	/// </summary>
	/// <param name="amount">New amount of gold.</param>
	/// <param name="player">Player whose gold we're setting.</param>
	public static async Task SetGold(decimal amount, Player player)
	{
		int gold = player.Gold;
		if ((decimal)gold < amount)
		{
			await GainGold(amount - (decimal)gold, player);
		}
		else if ((decimal)gold > amount)
		{
			await LoseGold((decimal)gold - amount, player);
		}
	}

	public static Task GainMaxPotionCount(int amount, Player player)
	{
		player.AddToMaxPotionCount(amount);
		return Task.CompletedTask;
	}

	public static Task LoseMaxPotionCount(int amount, Player player)
	{
		player.SubtractFromMaxPotionCount(amount);
		return Task.CompletedTask;
	}

	/// <summary>
	/// Give the player a new pet (aww).
	/// </summary>
	/// <param name="player">Player to give the pet to.</param>
	/// <typeparam name="T">Type of pet to give them.</typeparam>
	public static async Task<Creature> AddPet<T>(Player player) where T : MonsterModel
	{
		Creature pet = player.Creature.CombatState.CreateCreature((T)ModelDb.Monster<T>().ToMutable(), player.Creature.Side, null);
		await AddPet(pet, player);
		return pet;
	}

	/// <summary>
	/// Give the player a new pet (aww).
	/// </summary>
	/// <param name="pet">Pet creature to give to the player.</param>
	/// <param name="player">Player to give the pet to.</param>
	public static async Task AddPet(Creature pet, Player player)
	{
		if (pet.CombatState == null)
		{
			throw new InvalidOperationException("Pet must already be added to a combat state.");
		}
		player.PlayerCombatState.AddPetInternal(pet);
		await CreatureCmd.Add(pet);
	}

	/// <summary>
	/// Heal the player as if they were resting at a rest site.
	/// </summary>
	/// <param name="player">Player to heal.</param>
	/// <param name="playSfx">If true, we'll play the default rest site SFX.</param>
	public static async Task MimicRestSiteHeal(Player player, bool playSfx = true)
	{
		if (playSfx)
		{
			HealRestSiteOption.PlayRestSiteHealSfx();
		}
		await HealRestSiteOption.ExecuteRestSiteHeal(player, isMimicked: true);
	}

	/// <summary>
	/// Ends the turn for a given player.
	/// </summary>
	/// <param name="player">Player who is ending their turn</param>
	/// <param name="canBackOut">If the player is allowed to un-end their turn, particularly in multiplayer.</param>
	/// <param name="actionDuringEnemyTurn">Optional action to execute during the enemy turn. This is useful for tests.</param>
	public static void EndTurn(Player player, bool canBackOut, Func<Task>? actionDuringEnemyTurn = null)
	{
		if (!CombatManager.Instance.IsPlayerReadyToEndTurn(player))
		{
			if (LocalContext.IsMe(player))
			{
				CombatManager.Instance.OnEndedTurnLocally();
			}
			CombatManager.Instance.SetReadyToEndTurn(player, canBackOut, actionDuringEnemyTurn);
		}
	}

	public static void CompleteQuest(CardModel questCard)
	{
		questCard.Owner.RunState.CurrentMapPointHistoryEntry?.GetEntry(questCard.Owner.NetId).CompletedQuests.Add(questCard.Id);
	}
}
