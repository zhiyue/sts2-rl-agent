using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class BurrowedPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public override bool ShouldClearBlock(Creature creature)
	{
		if (base.Owner != creature)
		{
			return true;
		}
		return false;
	}

	public override async Task AfterBlockBroken(Creature creature)
	{
		if (creature == base.Owner)
		{
			await CreatureCmd.TriggerAnim(base.Owner, "UnburrowAttack", 0.25f);
			await CreatureCmd.Stun(base.Owner, "BITE_MOVE");
			await PowerCmd.Remove<BurrowedPower>(base.Owner);
		}
	}

	public override async Task AfterRemoved(Creature oldOwner)
	{
		await CreatureCmd.LoseBlock(oldOwner, 999m);
	}
}
