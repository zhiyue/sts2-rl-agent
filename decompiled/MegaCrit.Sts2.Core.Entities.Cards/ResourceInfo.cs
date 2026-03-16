namespace MegaCrit.Sts2.Core.Entities.Cards;

public struct ResourceInfo
{
	public required int EnergySpent { get; init; }

	public required int EnergyValue { get; init; }

	public required int StarsSpent { get; init; }

	public required int StarValue { get; init; }
}
