namespace MegaCrit.Sts2.Core.Localization.DynamicVars;

public class EnergyVar : DynamicVar
{
	public const string defaultName = "Energy";

	public string ColorPrefix { get; set; } = string.Empty;

	public EnergyVar(int energy)
		: this("Energy", energy)
	{
	}

	public EnergyVar(string name, int energy)
		: base(name, energy)
	{
	}
}
