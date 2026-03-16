using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace MegaCrit.Sts2.Core.Models.Powers.Mocks;

public sealed class MockPreventDeathPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override bool ShouldDie(Creature creature)
	{
		if (creature != base.Owner)
		{
			return true;
		}
		return false;
	}

	public override async Task AfterPreventingDeath(Creature creature)
	{
		await CreatureCmd.Heal(creature, base.Amount);
		await PowerCmd.Remove(this);
	}
}
