using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class FestivePopper : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Common;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DamageVar(9m, ValueProp.Unpowered));

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player == base.Owner)
		{
			CombatState combatState = player.Creature.CombatState;
			if (combatState.RoundNumber == 1)
			{
				Flash();
				VfxCmd.PlayOnCreatureCenters(combatState.HittableEnemies, "vfx/vfx_attack_slash");
				await CreatureCmd.Damage(choiceContext, combatState.HittableEnemies, base.DynamicVars.Damage, base.Owner.Creature);
			}
		}
	}
}
