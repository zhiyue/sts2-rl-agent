using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Commands;

public static class DamageCmd
{
	/// <summary>
	/// Perform an attack.
	/// See <see cref="T:MegaCrit.Sts2.Core.Commands.Builders.AttackCommand" /> for how to chain methods to set the attacking card/creature, targets, and more.
	/// </summary>
	/// <param name="damagePerHit">Amount of damage that each hit does.</param>
	/// <returns>Resulting AttackCommand.</returns>
	public static AttackCommand Attack(decimal damagePerHit)
	{
		return new AttackCommand(damagePerHit);
	}

	/// <summary>
	/// Perform an attack whose damage is calculated by a <see cref="T:MegaCrit.Sts2.Core.Localization.DynamicVars.CalculatedDamageVar" />.
	/// Used for cards that do dynamic amounts of damage based on combat state, like <see cref="T:MegaCrit.Sts2.Core.Models.Cards.PerfectedStrike" />.
	/// This uses special logic to ensure that intermediate values are floored at the appropriate time.
	/// </summary>
	/// <param name="calculatedDamageVar">Dynamic var that calculates how much damage each hit does.</param>
	/// <returns>Resulting AttackCommand.</returns>
	public static AttackCommand Attack(CalculatedDamageVar calculatedDamageVar)
	{
		return new AttackCommand(calculatedDamageVar);
	}
}
