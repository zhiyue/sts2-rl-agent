using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class AmethystAubergine : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Common;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new GoldVar(10));

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
		if (room == null)
		{
			return false;
		}
		if (!room.RoomType.IsCombatRoom())
		{
			return false;
		}
		if (room.RoomType == RoomType.Boss && player.RunState.CurrentActIndex >= player.RunState.Acts.Count - 1)
		{
			return false;
		}
		rewards.Add(new GoldReward(base.DynamicVars.Gold.IntValue, player));
		return true;
	}

	public override Task AfterModifyingRewards()
	{
		Flash();
		return Task.CompletedTask;
	}
}
