using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Combat.History.Entries;

public class CardDiscardedEntry : CombatHistoryEntry
{
	public CardModel Card { get; }

	public override string Description => base.Actor.Player.Character.Id.Entry + " discarded " + Card.Id.Entry;

	public CardDiscardedEntry(CardModel card, int roundNumber, CombatSide currentSide, CombatHistory history, IEnumerable<Player> players)
		: base(card.Owner.Creature, roundNumber, currentSide, history, players)
	{
		Card = card;
	}
}
