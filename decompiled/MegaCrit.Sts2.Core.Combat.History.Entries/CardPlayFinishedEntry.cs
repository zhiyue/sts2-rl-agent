using System.Collections.Generic;
using System.Text;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;

namespace MegaCrit.Sts2.Core.Combat.History.Entries;

public class CardPlayFinishedEntry : CombatHistoryEntry
{
	/// <summary>
	/// The instance of the card play that finished.
	/// </summary>
	public CardPlay CardPlay { get; }

	/// <summary>
	/// Tracks if the card was Ethereal at the time it was played.
	///
	/// This is a HACK in order to make <see cref="T:MegaCrit.Sts2.Core.Models.Cards.BansheesCry" /> work.
	/// If we end up with more WasX properties like this, we should add some sort of snapshot mechanism so that
	/// <see cref="P:MegaCrit.Sts2.Core.Combat.History.Entries.CardPlayFinishedEntry.CardPlay" /> represents the state of the card at the time it was played.
	/// </summary>
	public bool WasEthereal { get; }

	public override string Description
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder(base.Actor.Player.Character.Id.Entry + " played " + CardPlay.Card.Id.Entry);
			if (CardPlay.Target != null)
			{
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(11, 1, stringBuilder2);
				handler.AppendLiteral(" targeting ");
				handler.AppendFormatted(CardPlay.Target.Monster.Id.Entry);
				stringBuilder2.Append(ref handler);
			}
			return stringBuilder.ToString();
		}
	}

	public CardPlayFinishedEntry(CardPlay cardPlay, int roundNumber, CombatSide currentSide, CombatHistory history, IEnumerable<Player> players)
		: base(cardPlay.Card.Owner.Creature, roundNumber, currentSide, history, players)
	{
		CardPlay = cardPlay;
		WasEthereal = cardPlay.Card.Keywords.Contains(CardKeyword.Ethereal);
	}
}
