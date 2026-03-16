using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class RampartPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override bool ShouldScaleInMultiplayer => true;

	public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
	{
		if (side != CombatSide.Player)
		{
			return;
		}
		IEnumerable<Creature> enumerable = base.CombatState.Enemies.Where((Creature c) => c.Monster is TurretOperator);
		foreach (Creature item in enumerable)
		{
			await CreatureCmd.GainBlock(item, base.Amount, ValueProp.Unpowered, null);
		}
	}
}
