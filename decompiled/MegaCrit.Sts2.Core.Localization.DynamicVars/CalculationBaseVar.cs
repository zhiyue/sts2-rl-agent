namespace MegaCrit.Sts2.Core.Localization.DynamicVars;

/// <summary>
/// A special version of <see cref="T:MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVar" /> that is used to represent base value in a calculated value card.
/// For example, <see cref="T:MegaCrit.Sts2.Core.Models.Cards.PerfectedStrike" /> uses this for its 6 base damage.
/// </summary>
public class CalculationBaseVar : DynamicVar
{
	public const string defaultName = "CalculationBase";

	public CalculationBaseVar(decimal baseValue)
		: base("CalculationBase", baseValue)
	{
	}
}
