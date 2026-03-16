using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Relics;

public class Driftwood : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	public override bool TryModifyRewardsLate(Player player, List<Reward> rewards, AbstractRoom? room)
	{
		if (player != base.Owner)
		{
			return false;
		}
		foreach (CardReward item in rewards.OfType<CardReward>())
		{
			item.CanReroll = true;
		}
		return true;
	}
}
