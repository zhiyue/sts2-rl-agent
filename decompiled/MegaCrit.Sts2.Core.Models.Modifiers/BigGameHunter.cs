using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Modifiers;

public class BigGameHunter : ModifierModel
{
	public override ActMap ModifyGeneratedMap(IRunState runState, ActMap map, int actIndex)
	{
		Rng rng = new Rng(runState.Rng.Seed, $"act_{runState.CurrentActIndex + 1}_map");
		MapPointTypeCounts mapPointTypeCountsOverride = new MapPointTypeCounts(rng)
		{
			NumOfElites = (int)Math.Round((float)map.GetAllMapPoints().Count((MapPoint p) => p.PointType == MapPointType.Elite) * 2.5f),
			PointTypesThatIgnoreRules = new HashSet<MapPointType> { MapPointType.Elite }
		};
		bool shouldReplaceTreasureWithElites = map is StandardActMap standardActMap && standardActMap.ShouldReplaceTreasureWithElites;
		return new StandardActMap(rng, runState.Act, runState.Players.Count > 1, shouldReplaceTreasureWithElites, runState.Act.HasSecondBoss, mapPointTypeCountsOverride);
	}

	public override CardCreationOptions ModifyCardRewardCreationOptions(Player player, CardCreationOptions options)
	{
		if (options.Source != CardCreationSource.Encounter || options.RarityOdds != CardRarityOddsType.EliteEncounter)
		{
			return options;
		}
		if (options.Flags.HasFlag(CardCreationFlags.NoCardPoolModifications) || options.Flags.HasFlag(CardCreationFlags.NoRarityModification))
		{
			return options;
		}
		List<CardModel> list = (from c in options.GetPossibleCards(player)
			where c.Rarity == CardRarity.Rare
			select c).ToList();
		if (list.Count <= 0)
		{
			list = (from c in player.Character.CardPool.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
				where c.Rarity == CardRarity.Rare
				select c).ToList();
		}
		return options.WithCustomPool(list, CardRarityOddsType.Uniform);
	}
}
