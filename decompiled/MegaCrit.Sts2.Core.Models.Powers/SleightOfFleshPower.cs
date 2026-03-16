using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class SleightOfFleshPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

	public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		if (!(amount == 0m) && power.GetTypeForAmount(amount) == PowerType.Debuff && power.Owner.IsEnemy && applier == base.Owner && !(power is ITemporaryPower))
		{
			Flash();
			await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), power.Owner, base.Amount, ValueProp.Unpowered, base.Owner, null);
		}
	}
}
