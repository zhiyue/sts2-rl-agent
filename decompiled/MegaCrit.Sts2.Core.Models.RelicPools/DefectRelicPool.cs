using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Timeline.Epochs;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Models.RelicPools;

public sealed class DefectRelicPool : RelicPoolModel
{
	public override string EnergyColorName => "defect";

	public override Color LabOutlineColor => StsColors.blue;

	protected override IEnumerable<RelicModel> GenerateAllRelics()
	{
		return new global::_003C_003Ez__ReadOnlyArray<RelicModel>(new RelicModel[8]
		{
			ModelDb.Relic<CrackedCore>(),
			ModelDb.Relic<DataDisk>(),
			ModelDb.Relic<EmotionChip>(),
			ModelDb.Relic<GoldPlatedCables>(),
			ModelDb.Relic<PowerCell>(),
			ModelDb.Relic<Metronome>(),
			ModelDb.Relic<RunicCapacitor>(),
			ModelDb.Relic<SymbioticVirus>()
		});
	}

	public override IEnumerable<RelicModel> GetUnlockedRelics(UnlockState unlockState)
	{
		List<RelicModel> list = base.AllRelics.ToList();
		if (!unlockState.IsEpochRevealed<Defect3Epoch>())
		{
			list.RemoveAll((RelicModel r) => Defect3Epoch.Relics.Any((RelicModel relic) => relic.Id == r.Id));
		}
		if (!unlockState.IsEpochRevealed<Defect6Epoch>())
		{
			list.RemoveAll((RelicModel r) => Defect6Epoch.Relics.Any((RelicModel relic) => relic.Id == r.Id));
		}
		return list;
	}
}
