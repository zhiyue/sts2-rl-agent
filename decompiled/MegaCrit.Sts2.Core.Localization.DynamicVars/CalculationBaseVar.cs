namespace MegaCrit.Sts2.Core.Localization.DynamicVars;

public class CalculationBaseVar : DynamicVar
{
	public const string defaultName = "CalculationBase";

	public CalculationBaseVar(decimal baseValue)
		: base("CalculationBase", baseValue)
	{
	}
}
