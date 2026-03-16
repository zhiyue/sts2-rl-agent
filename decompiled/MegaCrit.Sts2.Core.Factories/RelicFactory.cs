using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Factories;

public static class RelicFactory
{
	private static RelicModel FallbackRelic => ModelDb.Relic<Circlet>();

	public static RelicModel PullNextRelicFromFront(Player player, Rng rng)
	{
		return PullNextRelicFromFront(player, RollRarity(rng));
	}

	public static RelicModel PullNextRelicFromFront(Player player)
	{
		return PullNextRelicFromFront(player, RollRarity(player));
	}

	public static RelicModel PullNextRelicFromFront(Player player, RelicRarity rarity)
	{
		RelicModel relicModel = TestRngInjector.ConsumeRelicOverride() ?? player.RelicGrabBag.PullFromFront(rarity, player.RunState) ?? FallbackRelic;
		player.RunState.SharedRelicGrabBag.Remove(relicModel);
		return relicModel;
	}

	public static RelicModel PullNextRelicFromBack(Player player)
	{
		return PullNextRelicFromBack(player, RollRarity(player), Array.Empty<RelicModel>());
	}

	public static RelicModel PullNextRelicFromBack(Player player, RelicRarity rarity, IEnumerable<RelicModel> blacklist)
	{
		RelicModel relicModel = TestRngInjector.ConsumeRelicOverride() ?? player.RelicGrabBag.PullFromBack(rarity, blacklist, player.RunState) ?? FallbackRelic;
		player.RunState.SharedRelicGrabBag.Remove(relicModel);
		return relicModel;
	}

	public static RelicRarity RollRarity(Player player)
	{
		return RollRarity(player.PlayerRng.Rewards);
	}

	public static RelicRarity RollRarity(Rng rng)
	{
		RelicRarity? relicRarityOverride = TestRngInjector.GetRelicRarityOverride();
		if (relicRarityOverride.HasValue)
		{
			return relicRarityOverride.GetValueOrDefault();
		}
		float num = rng.NextFloat();
		return (num < 0.5f) ? RelicRarity.Common : ((!(num < 0.83f)) ? RelicRarity.Rare : RelicRarity.Uncommon);
	}
}
