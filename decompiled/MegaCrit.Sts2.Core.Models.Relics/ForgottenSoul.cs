using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class ForgottenSoul : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Event;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DamageVar(1m, ValueProp.Unpowered));

	public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool _)
	{
		if (card.Owner == base.Owner)
		{
			Flash();
			Creature creature = base.Owner.RunState.Rng.CombatTargets.NextItem(base.Owner.Creature.CombatState.HittableEnemies);
			if (creature != null)
			{
				VfxCmd.PlayOnCreatureCenter(creature, "vfx/vfx_attack_blunt");
				await CreatureCmd.Damage(choiceContext, creature, base.DynamicVars.Damage, base.Owner.Creature);
			}
		}
	}
}
