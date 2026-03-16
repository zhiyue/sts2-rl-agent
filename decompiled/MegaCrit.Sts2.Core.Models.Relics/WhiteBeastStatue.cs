using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class WhiteBeastStatue : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Rare;

	public override bool IsAllowed(IRunState runState)
	{
		return RelicModel.IsBeforeAct3TreasureChest(runState);
	}

	public override bool ShouldForcePotionReward(Player player, RoomType roomType)
	{
		if (player != base.Owner)
		{
			return false;
		}
		if (!roomType.IsCombatRoom())
		{
			return false;
		}
		return true;
	}
}
