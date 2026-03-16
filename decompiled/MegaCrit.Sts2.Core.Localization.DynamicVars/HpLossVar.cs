namespace MegaCrit.Sts2.Core.Localization.DynamicVars;

public class HpLossVar : DynamicVar
{
	public const string defaultName = "HpLoss";

	public HpLossVar(decimal hpLoss)
		: base("HpLoss", hpLoss)
	{
	}

	public HpLossVar(string name, decimal hpLoss)
		: base(name, hpLoss)
	{
	}
}
