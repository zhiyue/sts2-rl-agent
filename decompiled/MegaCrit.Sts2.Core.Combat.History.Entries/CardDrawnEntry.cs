using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Combat.History.Entries;

public class CardDrawnEntry : CombatHistoryEntry
{
	public CardModel Card { get; }

	public bool FromHandDraw { get; }

	public override string Description => base.Actor.Player.Character.Id.Entry + " discarded " + Card.Id.Entry;

	public CardDrawnEntry(CardModel card, int roundNumber, CombatSide currentSide, bool fromHandDraw, CombatHistory history, IEnumerable<Player> players)
		: base(card.Owner.Creature, roundNumber, currentSide, history, players)
	{
		Card = card;
		FromHandDraw = fromHandDraw;
	}
}
