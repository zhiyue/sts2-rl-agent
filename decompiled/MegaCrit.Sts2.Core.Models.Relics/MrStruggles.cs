using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class MrStruggles : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Event;

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player == base.Owner)
		{
			Flash();
			ICombatState combatState = player.Creature.CombatState;
			await CreatureCmd.Damage(choiceContext, combatState.HittableEnemies, base.Owner.PlayerCombatState.TurnNumber, ValueProp.Unpowered, base.Owner.Creature);
		}
	}
}
