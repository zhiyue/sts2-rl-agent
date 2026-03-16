using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class NecroMasteryPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task AfterCurrentHpChanged(Creature creature, decimal delta)
	{
		if (!(delta >= 0m) && creature.Monster is Osty && creature.PetOwner == base.Owner.Player)
		{
			await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), creature.CombatState.HittableEnemies, -delta * (decimal)base.Amount, ValueProp.Unblockable | ValueProp.Unpowered, base.Owner, null);
		}
	}
}
