using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class DrawCardsNextTurnPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override decimal ModifyHandDraw(Player player, decimal count)
	{
		if (player != base.Owner.Player)
		{
			return count;
		}
		if (base.AmountOnTurnStart == 0)
		{
			return count;
		}
		return count + (decimal)base.Amount;
	}

	public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
	{
		if (side == base.Owner.Side && base.AmountOnTurnStart != 0)
		{
			await PowerCmd.Remove(this);
		}
	}
}
