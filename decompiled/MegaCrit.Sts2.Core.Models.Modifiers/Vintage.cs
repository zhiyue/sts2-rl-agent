using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Modifiers;

public class Vintage : ModifierModel
{
	public override bool TryModifyRewardsLate(Player player, List<Reward> rewards, AbstractRoom? room)
	{
		if (!(room is CombatRoom combatRoom))
		{
			return false;
		}
		if (combatRoom.Encounter.RoomType != RoomType.Monster)
		{
			return false;
		}
		for (int i = 0; i < rewards.Count; i++)
		{
			if (rewards[i] is CardReward)
			{
				rewards.RemoveAt(i);
				RelicReward item = new RelicReward(player);
				rewards.Insert(i, item);
			}
		}
		return true;
	}
}
