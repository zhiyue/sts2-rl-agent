namespace MegaCrit.Sts2.Core.Localization.DynamicVars;

public class CardsVar : DynamicVar
{
	public const string defaultName = "Cards";

	public CardsVar(int cards)
		: base("Cards", cards)
	{
	}

	public CardsVar(string name, int cards)
		: base(name, cards)
	{
	}
}
