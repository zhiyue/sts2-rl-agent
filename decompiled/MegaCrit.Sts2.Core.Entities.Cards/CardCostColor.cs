namespace MegaCrit.Sts2.Core.Entities.Cards;

public enum CardCostColor
{
	/// <summary>
	/// "Normal" cost. Colored white.
	/// </summary>
	Unmodified,
	/// <summary>
	/// The current cost is modified to above normal. Colored blue.
	/// </summary>
	Increased,
	/// <summary>
	/// The current cost is modified to at/below normal. Colored green.
	/// </summary>
	Decreased,
	/// <summary>
	/// The card cannot be played because the player does not have enough resources.
	/// </summary>
	InsufficientResources
}
