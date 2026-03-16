using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class PrismaticGem : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new EnergyVar(1));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.ForEnergy(this));

	public override decimal ModifyMaxEnergy(Player player, decimal amount)
	{
		if (player != base.Owner)
		{
			return amount;
		}
		return amount + (decimal)base.DynamicVars.Energy.IntValue;
	}

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
		if (options.CustomCardPool != null)
		{
			return options;
		}
		if (options.CardPools.All((CardPoolModel p) => p.IsColorless))
		{
			return options;
		}
		IEnumerable<CardPoolModel> pools = player.UnlockState.CharacterCardPools.Union(options.CardPools);
		return options.WithCardPools(pools, options.CardPoolFilter);
	}
}
