using System;

namespace MegaCrit.Sts2.Core.Entities.Cards;

/// <summary>
/// A class representing a single local modifier to a card's cost.
///
/// A <see cref="T:MegaCrit.Sts2.Core.Models.CardModel" />'s <see cref="T:MegaCrit.Sts2.Core.Entities.Cards.CardEnergyCost" /> contains a list of these local modifiers, which are
/// executed in the order they were applied to determine the card's final cost (before global modifiers if requested).
///
/// For example, imagine the following scenario:
///
/// You have a card with a base cost of 5. The following sequence of events occurs:
/// 1. An effect sets the card's cost to 2 (absolute) for the whole combat.
/// 2. Another effect increases the card's cost by 1 (relative) for the whole combat.
/// 3. Another effect sets the card's cost to 0 (absolute) just for this turn.
/// 4. Another effect increases the card's cost by 1 (relative) for the whole combat.
///
/// The CardEnergyCost's list of local modifiers would look like this:
///
/// [
///     LocalCostModifier(2, Absolute, WholeCombat),
///     LocalCostModifier(1, Relative, WholeCombat),
///     LocalCostModifier(0, Absolute, ThisTurn),
///     LocalCostModifier(1, Relative, WholeCombat)
/// ]
///
/// And the card's calculated cost would be 1.
///
/// Then, after the turn ends, the LocalCostModifier(0, Absolute, ThisTurn) modifier would wear off, and the list of
/// local modifiers would look like this:
///
/// [
///     LocalCostModifier(2, Absolute, WholeCombat),
///     LocalCostModifier(1, Relative, WholeCombat),
///     LocalCostModifier(1, Relative, WholeCombat)
/// ]
///
/// And the card's calculated cost would be 4.
/// </summary>
public class LocalCostModifier
{
	/// <summary>
	/// This modifier's amount.
	/// For <see cref="F:MegaCrit.Sts2.Core.Entities.Cards.LocalCostType.Absolute" />, this is the cost that the card should be set to.
	/// For <see cref="F:MegaCrit.Sts2.Core.Entities.Cards.LocalCostType.Relative" />, this is the amount that the card's cost should be offset by.
	/// </summary>
	public int Amount { get; set; }

	/// <summary>
	/// What algorithm should be used to apply this modifier to its card's cost.
	/// </summary>
	public LocalCostType Type { get; }

	/// <summary>
	/// When should this modifier wear off?
	/// Depending on this value, the modifier may wear off after certain game events. For example, a modifier with
	/// <see cref="F:MegaCrit.Sts2.Core.Entities.Cards.LocalCostModifierExpiration.WhenPlayed" /> will wear off after the card is played.
	/// </summary>
	public LocalCostModifierExpiration Expiration { get; }

	/// <summary>
	/// Should this modifier only be included in the cost calculation if it would lower the cost?
	/// Usually false, but true for effects that have the word "Reduce" in them.
	/// If you're not sure whether to set this to true or false, check if the effect uses the word "Reduce".
	/// If it does, set it to true. If not, set it to false.
	/// If you're still not sure, read the example below.
	/// If you're still not sure, set it to false.
	/// </summary>
	/// <example>
	/// <see cref="T:MegaCrit.Sts2.Core.Models.Cards.Enlightenment" /> says "Reduce the cost of ALL cards in your Hand to 1".
	/// If you have a 3-cost card in your hand that's been temporarily reduced to 0 via other
	/// <see cref="T:MegaCrit.Sts2.Core.Entities.Cards.LocalCostModifier" />s, <see cref="T:MegaCrit.Sts2.Core.Models.Cards.Enlightenment" /> should not raise it up to 1.
	/// However, once the other modifiers wear off, <see cref="T:MegaCrit.Sts2.Core.Models.Cards.Enlightenment" /> should kick in and reduce the cost to 1.
	/// </example>
	public bool IsReduceOnly { get; }

	public LocalCostModifier(int amount, LocalCostType type, LocalCostModifierExpiration expiration, bool reduceOnly)
	{
		Amount = amount;
		Type = type;
		Expiration = expiration;
		IsReduceOnly = reduceOnly;
	}

	/// <summary>
	/// Modify the passed cost.
	/// </summary>
	public int Modify(int currentCost)
	{
		return Type switch
		{
			LocalCostType.Absolute => IsReduceOnly ? Math.Min(currentCost, Amount) : Amount, 
			LocalCostType.Relative => IsReduceOnly ? Math.Min(currentCost, currentCost + Amount) : (currentCost + Amount), 
			_ => throw new ArgumentOutOfRangeException("Type", Type, null), 
		};
	}

	public LocalCostModifier Clone()
	{
		return new LocalCostModifier(Amount, Type, Expiration, IsReduceOnly);
	}
}
