using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class StarNextTurnPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task AfterEnergyReset(Player player)
	{
		if (player == base.Owner.Player)
		{
			await PlayerCmd.GainStars(base.Amount, player);
			await PowerCmd.Remove(this);
		}
	}
}
