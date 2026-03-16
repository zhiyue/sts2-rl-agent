using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class HeistPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override bool IsInstanced => true;

	public override Task BeforeDeath(Creature target)
	{
		if (base.Owner != target)
		{
			return Task.CompletedTask;
		}
		if (base.CombatState.RunState.CurrentRoom is CombatRoom combatRoom)
		{
			combatRoom.AddExtraReward(base.Target.Player, new GoldReward(base.Amount, base.Target.Player, wasGoldStolenBack: true));
		}
		return Task.CompletedTask;
	}
}
