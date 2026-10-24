using System.Linq;
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
		if (!options.Flags.HasFlag(CardCreationFlags.IsCardReward))
		{
			return options;
		}
		if (options.CardPools.Contains(ModelDb.CardPool<ColorlessCardPool>()))
		{
			return options;
		}
		if (options.CustomCardPool != null)
		{
			return options;
		}
		return options.WithCardPools(options.CardPools.ToList().Concat(new global::_003C_003Ez__ReadOnlySingleElementList<CardPoolModel>(ModelDb.CardPool<ColorlessCardPool>())), options.CardPoolFilter);
	}
}
