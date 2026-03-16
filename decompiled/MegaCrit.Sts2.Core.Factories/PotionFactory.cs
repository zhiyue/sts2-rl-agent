using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Factories;

public static class PotionFactory
{
	private const float _rareChance = 0.1f;

	private const float _rareThreshold = 0.1f;

	private const float _uncommonChance = 0.25f;

	private const float _uncommonThreshold = 0.35f;

	public static PotionModel CreateRandomPotionOutOfCombat(Player player, Rng rng, IEnumerable<PotionModel>? blacklist = null)
	{
		return CreateRandomPotion(GetPotionOptions(player, blacklist ?? Array.Empty<PotionModel>()), 1, rng).First();
	}

	public static List<PotionModel> CreateRandomPotionsOutOfCombat(Player player, int count, Rng rng, IEnumerable<PotionModel>? blacklist = null)
	{
		return CreateRandomPotion(GetPotionOptions(player, blacklist ?? Array.Empty<PotionModel>()), count, rng);
	}

	public static PotionModel CreateRandomPotionInCombat(Player player, Rng rng, IEnumerable<PotionModel>? blacklist = null)
	{
		return CreateRandomPotion(from p in GetPotionOptions(player, blacklist ?? Array.Empty<PotionModel>())
			where p.CanBeGeneratedInCombat
			select p, 1, rng).First();
	}

	private static List<PotionModel> CreateRandomPotion(IEnumerable<PotionModel> options, int count, Rng rng)
	{
		List<PotionModel> list = options.ToList();
		List<PotionModel> list2 = new List<PotionModel>();
		for (int i = 0; i < count; i++)
		{
			float num = rng.NextFloat();
			PotionRarity potionRarity = ((num <= 0.1f) ? PotionRarity.Rare : ((!(num <= 0.35f)) ? PotionRarity.Common : PotionRarity.Uncommon));
			PotionRarity rarity = potionRarity;
			PotionModel item = rng.NextItem(list.Where((PotionModel potion) => potion.Rarity == rarity));
			list2.Add(item);
			list.Remove(item);
		}
		return list2;
	}

	public static IEnumerable<PotionModel> GetPotionOptions(Player player, IEnumerable<PotionModel> blacklist)
	{
		return from p in player.Character.PotionPool.GetUnlockedPotions(player.UnlockState).Concat(ModelDb.PotionPool<SharedPotionPool>().GetUnlockedPotions(player.UnlockState))
			where !blacklist.Contains(p)
			select p;
	}
}
