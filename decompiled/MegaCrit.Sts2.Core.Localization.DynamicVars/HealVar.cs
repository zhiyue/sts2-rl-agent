namespace MegaCrit.Sts2.Core.Localization.DynamicVars;

public class HealVar : DynamicVar
{
	public const string defaultName = "Heal";

	public HealVar(decimal healAmount)
		: base("Heal", healAmount)
	{
	}

	public HealVar(string name, decimal healAmount)
		: base(name, healAmount)
	{
	}
}
