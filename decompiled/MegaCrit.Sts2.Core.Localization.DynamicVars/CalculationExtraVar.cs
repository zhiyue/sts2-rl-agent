namespace MegaCrit.Sts2.Core.Localization.DynamicVars;

public class CalculationExtraVar : DynamicVar
{
	public const string defaultName = "CalculationExtra";

	public CalculationExtraVar(decimal baseValue)
		: base("CalculationExtra", baseValue)
	{
	}
}
