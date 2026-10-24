using System;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Factories;

public static class RelicFactory
{
	public static RelicModel FallbackRelic => ModelDb.Relic<Circlet>();

	/// <summary>
	/// Roll rarity and pull the next matching relic from the front of the list.
	/// The pulled relic will never be seen again on this run.
	/// Used by most relic reward sources (elite combat rewards etc).
	/// </summary>
	/// <returns>RelicModel</returns>
	public static RelicModel PullNextRelicFromFront(Player player, Rng rng)
	{
		return PullNextRelicFromFront(player, RollRarity(rng), (RelicModel _) => true);
	}

	public static RelicModel PullNextRelicFromFront(Player player)
	{
		return PullNextRelicFromFront(player, RollRarity(player), (RelicModel _) => true);
	}

	public static RelicModel PullNextRelicFromFront(Player player, RelicRarity rarity)
	{
		return PullNextRelicFromFront(player, rarity, (RelicModel _) => true);
	}

	/// <summary>
	/// Pull the next matching relic from the front of the list.
	/// The pulled relic will never be seen again on this run.
	/// Used by most relic reward sources (elite combat rewards etc).
	/// </summary>
	/// <param name="player">The player for which we're obtaining a relic.</param>
	/// <param name="rarity">Rarity of the relic we want.</param>
	/// <param name="filter">Filter for what relics are allowed.</param>
	/// <returns>RelicModel</returns>
	public static RelicModel PullNextRelicFromFront(Player player, RelicRarity rarity, Func<RelicModel, bool> filter)
	{
		RelicModel relicModel = TestRngInjector.ConsumeRelicOverride() ?? player.RelicGrabBag.PullFromFront(rarity, filter, player.RunState) ?? FallbackRelic;
		player.RunState.SharedRelicGrabBag.Remove(relicModel);
		return relicModel;
	}

	/// <summary>
	/// Roll rarity and pull the next matching relic from the back of the list.
	/// The pulled relic will never be seen again on this run.
	/// Used in shops.
	/// </summary>
	/// <param name="player">The player for which we're obtaining a relic.</param>
	/// <returns>RelicModel</returns>
	public static RelicModel PullNextRelicFromBack(Player player)
	{
		return PullNextRelicFromBack(player, RollRarity(player), (RelicModel _) => true);
	}

	/// <summary>
	/// Pull the next matching relic from the back of the list.
	/// The pulled relic will never be seen again on this run.
	/// Used in shops.
	/// </summary>
	/// <param name="player">The player for which we're obtaining a relic.</param>
	/// <param name="rarity">Rarity of the relic we want.</param>
	/// <param name="filter">Only relics that match the filter will be allowed to be returned.</param>
	/// <returns>RelicModel</returns>
	public static RelicModel PullNextRelicFromBack(Player player, RelicRarity rarity, Func<RelicModel, bool> filter)
	{
		RelicModel relicModel = TestRngInjector.ConsumeRelicOverride() ?? player.RelicGrabBag.PullFromBack(rarity, filter, player.RunState) ?? FallbackRelic;
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
