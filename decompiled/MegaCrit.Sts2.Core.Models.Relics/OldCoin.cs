using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class OldCoin : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Rare;

	public override bool IsAllowedInShops => false;

	public override bool HasUponPickupEffect => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new GoldVar(300));

	public override bool IsAllowed(IRunState runState)
	{
		return RelicModel.IsBeforeAct3TreasureChest(runState);
	}

	public override async Task AfterObtained()
	{
		await PlayerCmd.GainGold(base.DynamicVars.Gold.BaseValue, base.Owner);
	}
}
