using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class Pear : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Uncommon;

	public override bool HasUponPickupEffect => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new MaxHpVar(10m));

	public override async Task AfterObtained()
	{
		await CreatureCmd.GainMaxHp(base.Owner.Creature, base.DynamicVars.MaxHp.BaseValue);
	}
}
