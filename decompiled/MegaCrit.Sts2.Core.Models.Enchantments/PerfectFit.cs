using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Players;

namespace MegaCrit.Sts2.Core.Models.Enchantments;

public sealed class PerfectFit : EnchantmentModel
{
	public override bool HasExtraCardText => true;

	public override void ModifyShuffleOrder(Player player, List<CardModel> cards, bool isInitialShuffle)
	{
		if (!isInitialShuffle && cards.Contains(base.Card))
		{
			cards.Remove(base.Card);
			cards.Insert(0, base.Card);
		}
	}
}
