using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class ThieveryPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override bool IsInstanced => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new GoldVar(0));

	public async Task Steal()
	{
		if (base.Target != null && !base.Target.IsDead && base.Target.Player.Gold > 0)
		{
			int amount = Math.Min(base.Amount, base.Target.Player.Gold);
			await PlayerCmd.LoseGold(amount, base.Target.Player, GoldLossType.Stolen);
			base.DynamicVars.Gold.BaseValue += (decimal)amount;
		}
	}
}
