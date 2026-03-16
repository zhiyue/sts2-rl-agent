using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class PotionBelt : RelicModel
{
	private const string _potionSlotsKey = "PotionSlots";

	public override RelicRarity Rarity => RelicRarity.Common;

	public override bool HasUponPickupEffect => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("PotionSlots", 2m));

	public override async Task AfterObtained()
	{
		await PlayerCmd.GainMaxPotionCount(base.DynamicVars["PotionSlots"].IntValue, base.Owner);
	}
}
