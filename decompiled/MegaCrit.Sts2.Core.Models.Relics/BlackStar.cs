using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class BlackStar : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	public override bool TryModifyRewards(Player player, List<Reward> rewards, AbstractRoom? room)
	{
		if (player != base.Owner)
		{
			return false;
		}
		if (room == null || room.RoomType != RoomType.Elite)
		{
			return false;
		}
		rewards.Add(new RelicReward(player));
		return true;
	}
}
