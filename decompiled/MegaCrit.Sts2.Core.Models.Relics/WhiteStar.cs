using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class WhiteStar : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Rare;

	public override bool IsAllowed(IRunState runState)
	{
		return RelicModel.IsBeforeAct3TreasureChest(runState);
	}

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
		rewards.Add(new CardReward(CardCreationOptions.ForRoom(base.Owner, RoomType.Boss), 3, player));
		return true;
	}
}
