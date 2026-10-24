using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Modifiers;

public class Terminal : ModifierModel
{
	public override async Task AfterRoomEntered(AbstractRoom room)
	{
		if (base.RunState.BaseRoom == room)
		{
			foreach (Player player in base.RunState.Players)
			{
				Creature creature = player.Creature;
				if (creature == null || creature.CurrentHp > 1 || creature.MaxHp > 1)
				{
					await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), player.Creature, 1m, isFromCard: false);
				}
			}
		}
		if (!(room is CombatRoom combatRoom))
		{
			return;
		}
		foreach (Creature playerCreature in combatRoom.CombatState.PlayerCreatures)
		{
			await PowerCmd.Apply<PlatingPower>(new ThrowingPlayerChoiceContext(), playerCreature, 5m, null, null);
		}
	}
}
