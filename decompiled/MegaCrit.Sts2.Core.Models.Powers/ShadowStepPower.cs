using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class ShadowStepPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
	{
		if (side == CombatSide.Player)
		{
			await PowerCmd.Apply<DoubleDamagePower>(base.Owner, base.Amount, base.Owner, null);
			await PowerCmd.Remove(this);
		}
	}
}
