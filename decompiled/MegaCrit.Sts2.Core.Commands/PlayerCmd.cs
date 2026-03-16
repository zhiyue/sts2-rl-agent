using System;
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

	public static Task GainEnergy(decimal amount, Player player)
	{
		if (amount <= 0m)
		{
			return Task.CompletedTask;
		}
		if (CombatManager.Instance.IsEnding)
		{
			return Task.CompletedTask;
		}
		SfxCmd.Play("event:/sfx/ui/gain_energy");
		player.PlayerCombatState.GainEnergy(amount);
		return Task.CompletedTask;
	}

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

	public static async Task GainStars(decimal amount, Player player)
	{
		if (!CombatManager.Instance.IsEnding && Hook.ShouldGainStars(player.Creature.CombatState, amount, player))
		{
			player.PlayerCombatState.GainStars(amount);
			await Hook.AfterStarsGained(player.Creature.CombatState, (int)amount, player);
		}
	}

	public static Task LoseStars(decimal amount, Player player)
	{
		if (CombatManager.Instance.IsEnding)
		{
			return Task.CompletedTask;
		}
		player.PlayerCombatState.LoseStars(amount);
		return Task.CompletedTask;
	}

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

	public static async Task GainGold(decimal amount, Player player, bool wasStolenBack = false)
	{
		if (!Hook.ShouldGainGold(player.RunState, player.Creature.CombatState, amount, player))
		{
			return;
		}
		IRunState runState = player.RunState;
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
				break;
			}
		}
		player.Gold = int.Max(0, player.Gold - (int)amount);
		return Task.CompletedTask;
	}

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

	public static async Task<Creature> AddPet<T>(Player player) where T : MonsterModel
	{
		Creature pet = player.Creature.CombatState.CreateCreature((T)ModelDb.Monster<T>().ToMutable(), player.Creature.Side, null);
		await AddPet(pet, player);
		return pet;
	}

	public static async Task AddPet(Creature pet, Player player)
	{
		if (pet.CombatState == null)
		{
			throw new InvalidOperationException("Pet must already be added to a combat state.");
		}
		player.PlayerCombatState.AddPetInternal(pet);
		await CreatureCmd.Add(pet);
	}

	public static async Task MimicRestSiteHeal(Player player, bool playSfx = true)
	{
		if (playSfx)
		{
			HealRestSiteOption.PlayRestSiteHealSfx();
		}
		await HealRestSiteOption.ExecuteRestSiteHeal(player, isMimicked: true);
	}

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
