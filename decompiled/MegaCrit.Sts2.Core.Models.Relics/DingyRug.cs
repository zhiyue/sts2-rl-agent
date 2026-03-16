using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class DingyRug : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Shop;

	public override CardCreationOptions ModifyCardRewardCreationOptions(Player player, CardCreationOptions options)
	{
		if (base.Owner != player)
		{
			return options;
		}
		if (options.Flags.HasFlag(CardCreationFlags.NoCardPoolModifications))
		{
			return options;
		}
		List<CardModel> list = options.GetPossibleCards(player).ToList();
		CardPoolModel cardPoolModel = ModelDb.CardPool<ColorlessCardPool>();
		List<CardModel> list2 = cardPoolModel.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint).ToList();
		if (options.Flags.HasFlag(CardCreationFlags.NoRarityModification))
		{
			HashSet<CardRarity> allowedRarities = (from c in options.GetPossibleCards(player)
				select c.Rarity).ToHashSet();
			list2 = list2.Where((CardModel c) => allowedRarities.Contains(c.Rarity)).ToList();
		}
		foreach (CardModel item in list2)
		{
			if (!list.Contains(item))
			{
				list.Add(item);
			}
		}
		return options.WithCustomPool(list);
	}
}
