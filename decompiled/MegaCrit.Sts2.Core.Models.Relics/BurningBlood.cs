using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class BurningBlood : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Starter;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new HealVar(6m));

	public override async Task AfterCombatVictory(CombatRoom _)
	{
		if (!base.Owner.Creature.IsDead)
		{
			Flash();
			await CreatureCmd.Heal(base.Owner.Creature, base.DynamicVars.Heal.BaseValue);
		}
	}
}
