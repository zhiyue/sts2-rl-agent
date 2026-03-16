using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class HauntPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (cardPlay.Card is Soul)
		{
			IReadOnlyList<Creature> hittableEnemies = base.CombatState.HittableEnemies;
			if (hittableEnemies.Count != 0)
			{
				Creature item = base.Owner.Player.RunState.Rng.CombatTargets.NextItem(hittableEnemies);
				await CreatureCmd.Damage(context, new global::_003C_003Ez__ReadOnlySingleElementList<Creature>(item), base.Amount, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
			}
		}
	}
}
