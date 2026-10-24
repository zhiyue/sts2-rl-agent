namespace MegaCrit.Sts2.Core.Localization.DynamicVars;

/// <summary>
/// A special version of <see cref="T:MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVar" /> that is used to represent extra value in a calculated value card.
/// For example, <see cref="T:MegaCrit.Sts2.Core.Models.Cards.PerfectedStrike" /> uses this for its 2 extra damage done for each Strike.
/// </summary>
public class CalculationExtraVar : DynamicVar
{
	public const string defaultName = "CalculationExtra";

	public CalculationExtraVar(decimal baseValue)
		: base("CalculationExtra", baseValue)
	{
	}
}
