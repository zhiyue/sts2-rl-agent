using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class ConstrictPower : PowerModel
{
	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (participants.Contains(base.Owner))
		{
			await CreatureCmd.Damage(choiceContext, base.Owner, base.Amount, ValueProp.Unpowered, base.Owner, null);
		}
	}

	public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
	{
		if (!wasRemovalPrevented && creature == base.Applier)
		{
			await PowerCmd.Remove(this);
		}
	}
}
