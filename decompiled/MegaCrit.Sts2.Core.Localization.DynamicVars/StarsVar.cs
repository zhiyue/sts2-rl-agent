namespace MegaCrit.Sts2.Core.Localization.DynamicVars;

public class StarsVar : DynamicVar
{
	public const string defaultName = "Stars";

	public StarsVar(int stars)
		: this("Stars", stars)
	{
	}

	public StarsVar(string name, int stars)
		: base(name, stars)
	{
	}
}
