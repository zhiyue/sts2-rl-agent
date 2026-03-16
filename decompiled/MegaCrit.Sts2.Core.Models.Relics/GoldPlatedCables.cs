using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class GoldPlatedCables : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Uncommon;

	public override int ModifyOrbPassiveTriggerCounts(OrbModel orb, int triggerCount)
	{
		if (orb.Owner != base.Owner)
		{
			return triggerCount;
		}
		if (orb != base.Owner.PlayerCombatState.OrbQueue.Orbs[0])
		{
			return triggerCount;
		}
		return triggerCount + 1;
	}

	public override Task AfterModifyingOrbPassiveTriggerCount(OrbModel orb)
	{
		Flash();
		return Task.CompletedTask;
	}
}
