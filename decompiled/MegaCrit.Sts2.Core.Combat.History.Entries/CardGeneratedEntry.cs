using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Combat.History.Entries;

public class CardGeneratedEntry : CombatHistoryEntry
{
	public CardModel Card { get; }

	public bool GeneratedByPlayer { get; }

	public override string Description => base.Actor.Player.Character.Id.Entry + " generated " + Card.Id.Entry + " during combat";

	public CardGeneratedEntry(CardModel card, bool generatedByPlayer, int roundNumber, CombatSide currentSide, CombatHistory history)
		: base(card.Owner.Creature, roundNumber, currentSide, history)
	{
		Card = card;
		GeneratedByPlayer = generatedByPlayer;
	}
}
