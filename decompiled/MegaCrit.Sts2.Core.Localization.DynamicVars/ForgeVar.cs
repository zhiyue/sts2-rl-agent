namespace MegaCrit.Sts2.Core.Localization.DynamicVars;

public class ForgeVar : DynamicVar
{
	public const string defaultName = "Forge";

	public ForgeVar(int forge)
		: this("Forge", forge)
	{
	}

	public ForgeVar(string name, int forge)
		: base(name, forge)
	{
	}
}
