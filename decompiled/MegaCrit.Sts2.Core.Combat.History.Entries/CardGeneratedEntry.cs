using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Combat.History.Entries;

public class CardGeneratedEntry : CombatHistoryEntry
{
	public CardModel Card { get; }

	public Player? Creator { get; }

	public override string Description => base.Actor.Player.Character.Id.Entry + " generated " + Card.Id.Entry + " during combat";

	public CardGeneratedEntry(CardModel card, Player? creator, int roundNumber, CombatSide currentSide, CombatHistory history, IEnumerable<Player> players)
		: base(card.Owner.Creature, roundNumber, currentSide, history, players)
	{
		Card = card;
		Creator = creator;
	}
}
