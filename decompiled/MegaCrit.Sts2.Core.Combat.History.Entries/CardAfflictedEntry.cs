using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Combat.History.Entries;

public class CardAfflictedEntry : CombatHistoryEntry
{
	public CardModel Card { get; }

	public AfflictionModel Affliction { get; }

	public override string Description => base.Actor.Player.Character.Id.Entry + " afflicted " + Card.Id.Entry;

	public CardAfflictedEntry(CardModel card, AfflictionModel affliction, int roundNumber, CombatSide currentSide, CombatHistory history, IEnumerable<Player> players)
		: base(card.Owner.Creature, roundNumber, currentSide, history, players)
	{
		Card = card;
		Affliction = affliction;
	}
}
