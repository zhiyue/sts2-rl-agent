using System;

namespace MegaCrit.Sts2.Core.Entities.Cards;

/// <summary>
/// Represents a card cost that should only exist temporarily.
/// Used for effects that set a card's cost to a specific value.
/// For effects that modify a card's existing cost by a certain amount, see <see cref="T:MegaCrit.Sts2.Core.Entities.Cards.TemporaryCardCostOffset" />.
/// Used for both temporary energy and star card costs.
/// </summary>
public class TemporaryCardCost
{
	/// <summary>
	/// The amount that this card's cost is set to.
	/// </summary>
	public int Cost { get; private set; }

	/// <summary>
	/// Should this temporary cost be cleared when the turn ends?
	/// This is true for effects that say something like "It costs N _this turn_" (like Attack Potion).
	/// </summary>
	public bool ClearsWhenTurnEnds { get; private set; }

	/// <summary>
	/// Should this temporary cost be cleared when the card is played?
	/// This is true for effects that say something like "It costs N _until played_" (like King's Gambit),
	/// AND for effects that say something like "It costs N _this turn_" (like Attack Potion).
	/// </summary>
	public bool ClearsWhenCardIsPlayed { get; private set; }

	/// <summary>
	/// A temporary cost that is cleared only when the card is played.
	/// </summary>
	public static TemporaryCardCost UntilPlayed(int cost)
	{
		return new TemporaryCardCost
		{
			Cost = Math.Max(cost, 0),
			ClearsWhenTurnEnds = false,
			ClearsWhenCardIsPlayed = true
		};
	}

	/// <summary>
	/// A temporary cost that is cleared when the card is played OR at the end of the current
	/// turn, whichever comes first.
	/// </summary>
	public static TemporaryCardCost ThisTurn(int cost)
	{
		return new TemporaryCardCost
		{
			Cost = Math.Max(cost, 0),
			ClearsWhenTurnEnds = true,
			ClearsWhenCardIsPlayed = true
		};
	}

	/// <summary>
	/// A temporary cost that remains for the entire combat.
	/// </summary>
	public static TemporaryCardCost ThisCombat(int cost)
	{
		return new TemporaryCardCost
		{
			Cost = Math.Max(cost, 0),
			ClearsWhenTurnEnds = false,
			ClearsWhenCardIsPlayed = false
		};
	}
}
