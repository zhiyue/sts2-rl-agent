namespace MegaCrit.Sts2.Core.Localization.DynamicVars;

public class GoldVar : DynamicVar
{
	public const string defaultName = "Gold";

	public GoldVar(int gold)
		: base("Gold", gold)
	{
	}

	public GoldVar(string name, int gold)
		: base(name, gold)
	{
	}
}
