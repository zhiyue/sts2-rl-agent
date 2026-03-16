using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Entities.TreasureRelicPicking;

public class RelicPickingResult
{
	public RelicPickingResultType type;

	public required RelicModel relic;

	public required Player player;

	public RelicPickingFight? fight;

	public static RelicPickingResult GenerateRelicFight(List<Player> players, RelicModel relic, Func<RelicPickingFightMove> generateMove)
	{
		RelicPickingFight relicPickingFight = new RelicPickingFight();
		relicPickingFight.playersInvolved.AddRange(players);
		HashSet<Player> hashSet = new HashSet<Player>();
		foreach (Player player in players)
		{
			hashSet.Add(player);
		}
		HashSet<Player> hashSet2 = hashSet;
		while (hashSet2.Count > 1)
		{
			RelicPickingFightRound relicPickingFightRound = new RelicPickingFightRound();
			foreach (Player player2 in players)
			{
				if (hashSet2.Contains(player2))
				{
					RelicPickingFightMove value = generateMove();
					relicPickingFightRound.moves.Add(value);
				}
				else
				{
					relicPickingFightRound.moves.Add(null);
				}
			}
			relicPickingFight.rounds.Add(relicPickingFightRound);
			List<RelicPickingFightMove> list = relicPickingFightRound.moves.OfType<RelicPickingFightMove>().Distinct().ToList();
			if (list.Count != 2)
			{
				continue;
			}
			RelicPickingFightMove losingMove = GetLosingMove(list[0], list[1]);
			for (int i = 0; i < players.Count; i++)
			{
				if (relicPickingFightRound.moves[i] == losingMove)
				{
					hashSet2.Remove(players[i]);
				}
			}
		}
		return new RelicPickingResult
		{
			type = RelicPickingResultType.FoughtOver,
			player = hashSet2.First(),
			relic = relic,
			fight = relicPickingFight
		};
	}

	private static RelicPickingFightMove GetLosingMove(RelicPickingFightMove move1, RelicPickingFightMove move2)
	{
		if ((int)(move1 + 1) % 3 == (int)move2)
		{
			return move1;
		}
		return move2;
	}
}
