using System;

namespace MegaCrit.Sts2.Core.Entities.Cards;

/// <summary>
/// An enum representing the types of modifiers that should be included when calculating a card's energy cost.
/// </summary>
[Flags]
public enum CostModifiers
{
	/// <summary>
	/// No modifiers at all. This will just return the card's unmodified energy cost.
	///
	/// What is the difference between this and <see cref="P:MegaCrit.Sts2.Core.Entities.Cards.CardEnergyCost.Canonical" />? A card's canonical cost is what
	/// would be "printed" on the card if it was printed out on paper, which means it cannot include _permanent_
	/// modifications. The most prolific permanent modification is an upgrade; cards like <see cref="T:MegaCrit.Sts2.Core.Models.Cards.Apotheosis" />
	/// reduce their cost by 1 when upgraded. This change will be reflected when using <see cref="F:MegaCrit.Sts2.Core.Entities.Cards.CostModifiers.None" />,
	/// but not when calling <see cref="P:MegaCrit.Sts2.Core.Entities.Cards.CardEnergyCost.Canonical" />.
	/// </summary>
	None = 0,
	/// <summary>
	/// Include any modifiers that have been applied directly to the card.
	/// These modifiers live locally on the card itself, and will persist regardless of changes to other models in the
	/// combat state.
	/// </summary>
	/// <example>
	/// See <see cref="T:MegaCrit.Sts2.Core.Entities.Cards.LocalCostModifierExpiration" /> for examples.
	/// </example>
	Local = 2,
	/// <summary>
	/// Include any modifiers that are persistently applied by other models in the global game state, and that may
	/// change or wear off based on changes to other models in the game state.
	/// </summary>
	/// <example>
	/// <see cref="T:MegaCrit.Sts2.Core.Models.Cards.Enthralled" />, which applies a persistent "cost 1 more energy" modifier to all cards in the player's
	/// hand. This effect will wear off if Enthralled leaves the player's hand.
	/// </example>
	Global = 4,
	/// <summary>
	/// Include all modifiers.
	/// </summary>
	All = -1
}
