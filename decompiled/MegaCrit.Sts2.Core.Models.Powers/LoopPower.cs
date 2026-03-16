using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class LoopPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player == base.Owner.Player && player.PlayerCombatState.OrbQueue.Orbs.Count != 0)
		{
			for (int i = 0; i < base.Amount; i++)
			{
				await OrbCmd.Passive(choiceContext, player.PlayerCombatState.OrbQueue.Orbs[0], null);
				await Cmd.Wait(0.25f);
			}
		}
	}
}
