namespace MegaCrit.Sts2.Core.Localization.DynamicVars;

public class RepeatVar : DynamicVar
{
	public const string defaultName = "Repeat";

	public RepeatVar(int times)
		: base("Repeat", times)
	{
	}

	public RepeatVar(string name, int times)
		: base(name, times)
	{
	}
}
