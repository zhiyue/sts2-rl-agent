using System;

namespace MegaCrit.Sts2.Core.Entities.Cards;

public class TemporaryCardCost
{
	public int Cost { get; private set; }

	public bool ClearsWhenTurnEnds { get; private set; }

	public bool ClearsWhenCardIsPlayed { get; private set; }

	public static TemporaryCardCost UntilPlayed(int cost)
	{
		return new TemporaryCardCost
		{
			Cost = Math.Max(cost, 0),
			ClearsWhenTurnEnds = false,
			ClearsWhenCardIsPlayed = true
		};
	}

	public static TemporaryCardCost ThisTurn(int cost)
	{
		return new TemporaryCardCost
		{
			Cost = Math.Max(cost, 0),
			ClearsWhenTurnEnds = true,
			ClearsWhenCardIsPlayed = true
		};
	}

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
