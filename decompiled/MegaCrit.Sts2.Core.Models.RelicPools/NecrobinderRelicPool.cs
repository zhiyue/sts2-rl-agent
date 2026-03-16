using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Timeline.Epochs;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Models.RelicPools;

public sealed class NecrobinderRelicPool : RelicPoolModel
{
	public override string EnergyColorName => "necrobinder";

	public override Color LabOutlineColor => StsColors.pink;

	protected override IEnumerable<RelicModel> GenerateAllRelics()
	{
		return new global::_003C_003Ez__ReadOnlyArray<RelicModel>(new RelicModel[8]
		{
			ModelDb.Relic<BigHat>(),
			ModelDb.Relic<BoneFlute>(),
			ModelDb.Relic<BookRepairKnife>(),
			ModelDb.Relic<Bookmark>(),
			ModelDb.Relic<BoundPhylactery>(),
			ModelDb.Relic<FuneraryMask>(),
			ModelDb.Relic<IvoryTile>(),
			ModelDb.Relic<UndyingSigil>()
		});
	}

	public override IEnumerable<RelicModel> GetUnlockedRelics(UnlockState unlockState)
	{
		List<RelicModel> list = base.AllRelics.ToList();
		if (!unlockState.IsEpochRevealed<Necrobinder3Epoch>())
		{
			list.RemoveAll((RelicModel r) => Necrobinder3Epoch.Relics.Any((RelicModel relic) => relic.Id == r.Id));
		}
		if (!unlockState.IsEpochRevealed<Necrobinder6Epoch>())
		{
			list.RemoveAll((RelicModel r) => Necrobinder6Epoch.Relics.Any((RelicModel relic) => relic.Id == r.Id));
		}
		return list;
	}
}
