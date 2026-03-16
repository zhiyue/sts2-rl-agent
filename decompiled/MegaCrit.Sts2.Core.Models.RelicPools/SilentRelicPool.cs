using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Timeline.Epochs;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Models.RelicPools;

public sealed class SilentRelicPool : RelicPoolModel
{
	public override string EnergyColorName => "silent";

	public override Color LabOutlineColor => StsColors.green;

	protected override IEnumerable<RelicModel> GenerateAllRelics()
	{
		return new global::_003C_003Ez__ReadOnlyArray<RelicModel>(new RelicModel[8]
		{
			ModelDb.Relic<HelicalDart>(),
			ModelDb.Relic<NinjaScroll>(),
			ModelDb.Relic<PaperKrane>(),
			ModelDb.Relic<RingOfTheSnake>(),
			ModelDb.Relic<SneckoSkull>(),
			ModelDb.Relic<Tingsha>(),
			ModelDb.Relic<ToughBandages>(),
			ModelDb.Relic<TwistedFunnel>()
		});
	}

	public override IEnumerable<RelicModel> GetUnlockedRelics(UnlockState unlockState)
	{
		List<RelicModel> list = base.AllRelics.ToList();
		if (!unlockState.IsEpochRevealed<Silent3Epoch>())
		{
			list.RemoveAll((RelicModel r) => Silent3Epoch.Relics.Any((RelicModel relic) => relic.Id == r.Id));
		}
		if (!unlockState.IsEpochRevealed<Silent6Epoch>())
		{
			list.RemoveAll((RelicModel r) => Silent6Epoch.Relics.Any((RelicModel relic) => relic.Id == r.Id));
		}
		return list;
	}
}
