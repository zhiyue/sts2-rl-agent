using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models.Potions.Mocks;

namespace MegaCrit.Sts2.Core.Models.PotionPools;

/// <summary>
/// Used for test-only potions.
/// </summary>
public sealed class MockPotionPool : PotionPoolModel
{
	public override string EnergyColorName => "colorless";

	protected override IEnumerable<PotionModel> GenerateAllPotions()
	{
		return new global::_003C_003Ez__ReadOnlySingleElementList<PotionModel>(ModelDb.Potion<MockDiscardAndAddShivsPotion>());
	}
}
