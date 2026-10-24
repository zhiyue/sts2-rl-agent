using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Powers;

/// <summary>
/// Just a visual timer for when <see cref="T:MegaCrit.Sts2.Core.Models.Monsters.ThievingHopper" /> will escape.
/// </summary>
public sealed class EscapeArtistPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (participants.Contains(base.Owner))
		{
			if (base.Amount > 1)
			{
				await PowerCmd.Decrement(this);
			}
			if (base.Amount == 1)
			{
				StartPulsing();
			}
		}
	}
}
