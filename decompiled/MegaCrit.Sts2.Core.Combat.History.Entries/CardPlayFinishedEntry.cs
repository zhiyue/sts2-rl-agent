using System.Text;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace MegaCrit.Sts2.Core.Combat.History.Entries;

public class CardPlayFinishedEntry : CombatHistoryEntry
{
	public CardPlay CardPlay { get; }

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

	public CardPlayFinishedEntry(CardPlay cardPlay, int roundNumber, CombatSide currentSide, CombatHistory history)
		: base(cardPlay.Card.Owner.Creature, roundNumber, currentSide, history)
	{
		CardPlay = cardPlay;
		WasEthereal = cardPlay.Card.Keywords.Contains(CardKeyword.Ethereal);
	}
}
