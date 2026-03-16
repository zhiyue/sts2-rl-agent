using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class PrayerWheel : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Rare;

	public override bool TryModifyRewards(Player player, List<Reward> rewards, AbstractRoom? room)
	{
		if (player != base.Owner)
		{
			return false;
		}
		if (room == null || room.RoomType != RoomType.Monster)
		{
			return false;
		}
		rewards.Add(new CardReward(CardCreationOptions.ForRoom(player, RoomType.Monster), 3, player));
		return true;
	}
}
