namespace MegaCrit.Sts2.Core.Entities.Cards;

/// <summary>
/// Represents a card cost offset that should only exist temporarily.
/// Used for effects that modify a card's cost by a certain amount.
/// For effects that set a card's cost to a specific value, see <see cref="T:MegaCrit.Sts2.Core.Entities.Cards.TemporaryCardCost" />.
/// </summary>
public class TemporaryCardCostOffset
{
	/// <summary>
	/// The amount that this card's cost is modified by.
	/// Positive for cost increases, negative for cost decreases.
	/// </summary>
	public int Offset { get; private set; }

	/// <summary>
	/// Should this temporary cost offset be cleared when the turn ends?
	/// This is true for effects that say something like "It costs N more _this turn_".
	/// </summary>
	public bool ClearsWhenTurnEnds { get; private set; }

	/// <summary>
	/// Should this temporary cost offset be cleared when the card is played?
	/// This is true for effects that say something like "It costs N more _until played_",
	/// AND for effects that say something like "It costs N more _this turn_".
	/// </summary>
	public bool ClearsWhenCardIsPlayed { get; private set; }

	/// <summary>
	/// A temporary cost offset that is cleared only when the card is played.
	/// </summary>
	public static TemporaryCardCostOffset UntilPlayed(int offset)
	{
		return new TemporaryCardCostOffset
		{
			Offset = offset,
			ClearsWhenTurnEnds = false,
			ClearsWhenCardIsPlayed = true
		};
	}

	/// <summary>
	/// A temporary cost offset that is cleared when the card is played OR at the end of the current
	/// turn, whichever comes first.
	/// </summary>
	public static TemporaryCardCostOffset ThisTurn(int offset)
	{
		return new TemporaryCardCostOffset
		{
			Offset = offset,
			ClearsWhenTurnEnds = true,
			ClearsWhenCardIsPlayed = true
		};
	}

	/// <summary>
	/// A temporary cost offset that remains for the entire combat.
	/// </summary>
	public static TemporaryCardCostOffset ThisCombat(int offset)
	{
		return new TemporaryCardCostOffset
		{
			Offset = offset,
			ClearsWhenTurnEnds = false,
			ClearsWhenCardIsPlayed = false
		};
	}
}
