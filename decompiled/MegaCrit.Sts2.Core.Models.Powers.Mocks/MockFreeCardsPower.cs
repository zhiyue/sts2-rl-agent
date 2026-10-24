using MegaCrit.Sts2.Core.Entities.Powers;

namespace MegaCrit.Sts2.Core.Models.Powers.Mocks;

public sealed class MockFreeCardsPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public override bool TryModifyEnergyCostInCombatLate(CardModel card, decimal originalCost, out decimal modifiedCost)
	{
		modifiedCost = default(decimal);
		return true;
	}
}
