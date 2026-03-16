using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class ParryingShield : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Uncommon;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new BlockVar(10m, ValueProp.Unpowered),
		new DamageVar(6m, ValueProp.Unpowered)
	});

	public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side == CombatSide.Player && !((decimal)base.Owner.Creature.Block < base.DynamicVars.Block.BaseValue))
		{
			Creature creature = base.Owner.RunState.Rng.CombatTargets.NextItem(base.Owner.Creature.CombatState.HittableEnemies);
			if (creature != null)
			{
				VfxCmd.PlayOnCreatureCenter(creature, "vfx/vfx_attack_blunt");
				await CreatureCmd.Damage(choiceContext, creature, base.DynamicVars.Damage, base.Owner.Creature);
			}
		}
	}
}
