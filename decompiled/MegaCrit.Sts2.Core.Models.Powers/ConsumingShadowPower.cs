using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class ConsumingShadowPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side == base.Owner.Side && base.Owner.Player.PlayerCombatState.OrbQueue.Orbs.Count != 0)
		{
			for (int i = 0; i < base.Amount; i++)
			{
				await OrbCmd.EvokeLast(choiceContext, base.Owner.Player);
				await Cmd.Wait(0.25f);
			}
		}
	}
}
