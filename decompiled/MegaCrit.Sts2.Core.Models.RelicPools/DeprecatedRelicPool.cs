using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models.Relics;

namespace MegaCrit.Sts2.Core.Models.RelicPools;

public sealed class DeprecatedRelicPool : RelicPoolModel
{
	public override string EnergyColorName => "colorless";

	protected override IEnumerable<RelicModel> GenerateAllRelics()
	{
		return new global::_003C_003Ez__ReadOnlySingleElementList<RelicModel>(ModelDb.Relic<DeprecatedRelic>());
	}
}
