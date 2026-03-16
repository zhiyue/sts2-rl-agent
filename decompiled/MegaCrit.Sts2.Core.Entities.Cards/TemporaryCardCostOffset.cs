namespace MegaCrit.Sts2.Core.Entities.Cards;

public class TemporaryCardCostOffset
{
	public int Offset { get; private set; }

	public bool ClearsWhenTurnEnds { get; private set; }

	public bool ClearsWhenCardIsPlayed { get; private set; }

	public static TemporaryCardCostOffset UntilPlayed(int offset)
	{
		return new TemporaryCardCostOffset
		{
			Offset = offset,
			ClearsWhenTurnEnds = false,
			ClearsWhenCardIsPlayed = true
		};
	}

	public static TemporaryCardCostOffset ThisTurn(int offset)
	{
		return new TemporaryCardCostOffset
		{
			Offset = offset,
			ClearsWhenTurnEnds = true,
			ClearsWhenCardIsPlayed = true
		};
	}

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
