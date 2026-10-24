using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace MegaCrit.Sts2.Core.Models.Powers.Mocks;

public sealed class MockUnhittablePower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public override bool ShouldAllowHitting(Creature creature)
	{
		if (creature != base.Owner)
		{
			return true;
		}
		return false;
	}
}
