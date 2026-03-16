using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class FakeLeesWaffle : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Event;

	public override bool HasUponPickupEffect => true;

	public override int MerchantCost => 50;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new HealVar(10m));

	public override async Task AfterObtained()
	{
		await CreatureCmd.Heal(base.Owner.Creature, (decimal)base.Owner.Creature.MaxHp * (base.DynamicVars.Heal.BaseValue / 100m));
	}
}
