using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Players;

namespace MegaCrit.Sts2.Core.Entities.TreasureRelicPicking;

public class RelicPickingFight
{
	public List<Player> playersInvolved = new List<Player>();

	public List<RelicPickingFightRound> rounds = new List<RelicPickingFightRound>();
}
