using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class ForbiddenGrimoirePower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override Task AfterCombatEnd(CombatRoom room)
	{
		for (int i = 0; i < base.Amount; i++)
		{
			room.AddExtraReward(base.Owner.Player, new CardRemovalReward(base.Owner.Player));
		}
		return Task.CompletedTask;
	}
}
