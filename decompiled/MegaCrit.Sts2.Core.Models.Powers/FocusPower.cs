using System;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class FocusPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override bool AllowNegative => true;

	public override decimal ModifyOrbValue(OrbModel orb, decimal value)
	{
		if (base.Owner.Player != orb.Owner)
		{
			return value;
		}
		return Math.Max(value + (decimal)base.Amount, 0m);
	}
}
