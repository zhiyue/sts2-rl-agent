using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class JuggernautPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task AfterBlockGained(Creature creature, decimal amount, ValueProp props, CardModel? cardSource)
	{
		if (!(amount <= 0m) && creature == base.Owner)
		{
			IReadOnlyList<Creature> hittableEnemies = base.CombatState.HittableEnemies;
			if (hittableEnemies.Count != 0)
			{
				Creature target = base.Owner.Player.RunState.Rng.CombatTargets.NextItem(hittableEnemies);
				Flash();
				await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), target, base.Amount, ValueProp.Unpowered, base.Owner, null);
			}
		}
	}
}
