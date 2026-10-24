using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace MegaCrit.Sts2.Core.Models.Powers.Mocks;

public sealed class MockExtraTurnPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override bool ShouldTakeExtraTurn(Player player)
	{
		return player == base.Owner.Player;
	}

	public override async Task AfterTakingExtraTurn(Player player)
	{
		if (player == base.Owner.Player)
		{
			await PowerCmd.Decrement(this);
		}
	}
}
