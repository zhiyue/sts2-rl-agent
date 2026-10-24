namespace MegaCrit.Sts2.Core.Entities.Cards;

public struct ResourceInfo
{
	/// <summary>
	/// The amount of energy that was spent when playing this card.
	/// </summary>
	public required int EnergySpent { get; init; }

	/// <summary>
	/// The amount of energy that this card cost when it was played.
	/// Note that this is not necessarily the same as <see cref="P:MegaCrit.Sts2.Core.Entities.Cards.ResourceInfo.EnergySpent" />.
	/// For example, if you auto-play a 3-energy-cost card, this will be 3, while <see cref="P:MegaCrit.Sts2.Core.Entities.Cards.ResourceInfo.EnergySpent" /> will be 0.
	/// </summary>
	public required int EnergyValue { get; init; }

	/// <summary>
	/// The number of stars that were spent when playing this card.
	/// </summary>
	public required int StarsSpent { get; init; }

	/// <summary>
	/// The number of stars that this card cost when it was played.
	/// Note that this is not necessarily the same as <see cref="P:MegaCrit.Sts2.Core.Entities.Cards.ResourceInfo.StarsSpent" />.
	/// For example, if you auto-play a 3-star-cost card, this will be 3, while <see cref="P:MegaCrit.Sts2.Core.Entities.Cards.ResourceInfo.StarsSpent" /> will be 0.
	/// </summary>
	public required int StarValue { get; init; }
}
