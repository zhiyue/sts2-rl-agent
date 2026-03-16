using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Encounters;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class BattlewornDummyTimeLimitPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side != base.Owner.Side)
		{
			return;
		}
		if (base.Amount > 1)
		{
			await PowerCmd.Decrement(this);
			return;
		}
		if (base.Owner.CombatState.Encounter is BattlewornDummyEventEncounter battlewornDummyEventEncounter)
		{
			battlewornDummyEventEncounter.RanOutOfTime = true;
		}
		await CreatureCmd.Escape(base.Owner);
	}
}
