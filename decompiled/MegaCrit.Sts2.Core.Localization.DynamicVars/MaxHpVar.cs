namespace MegaCrit.Sts2.Core.Localization.DynamicVars;

public class MaxHpVar : DynamicVar
{
	public const string defaultName = "MaxHp";

	public MaxHpVar(decimal maxHp)
		: base("MaxHp", maxHp)
	{
	}

	public MaxHpVar(string name, decimal maxHp)
		: base(name, maxHp)
	{
	}
}
