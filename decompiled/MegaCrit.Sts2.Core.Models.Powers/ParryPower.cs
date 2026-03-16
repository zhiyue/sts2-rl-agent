using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public class ParryPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public async Task AfterSovereignBladePlayed(Creature? dealer, IEnumerable<DamageResult> damageResults)
	{
		if (dealer != null && dealer == base.Owner)
		{
			Flash();
			await CreatureCmd.GainBlock(dealer, base.Amount, ValueProp.Unpowered, null);
		}
	}
}
