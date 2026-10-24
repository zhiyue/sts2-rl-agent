namespace MegaCrit.Sts2.Core.Entities.Cards;

/// <summary>
/// What algorithm should be used to apply a <see cref="T:MegaCrit.Sts2.Core.Entities.Cards.LocalCostModifier" /> to a card's cost.
/// </summary>
public enum LocalCostType
{
	None,
	/// <summary>
	/// The card's local cost should be reset to this specific value.
	/// </summary>
	Absolute,
	/// <summary>
	/// The card's current local cost should be offset by this amount.
	/// </summary>
	Relative
}
