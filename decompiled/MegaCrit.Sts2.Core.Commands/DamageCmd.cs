using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Commands;

public static class DamageCmd
{
	public static AttackCommand Attack(decimal damagePerHit)
	{
		return new AttackCommand(damagePerHit);
	}

	public static AttackCommand Attack(CalculatedDamageVar calculatedDamageVar)
	{
		return new AttackCommand(calculatedDamageVar);
	}
}
