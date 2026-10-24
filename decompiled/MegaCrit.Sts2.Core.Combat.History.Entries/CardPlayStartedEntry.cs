using System.Collections.Generic;
using System.Text;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;

namespace MegaCrit.Sts2.Core.Combat.History.Entries;

public class CardPlayStartedEntry : CombatHistoryEntry
{
	/// <summary>
	/// The instance of the card play that started.
	/// </summary>
	public CardPlay CardPlay { get; }

	public override string Description
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder(base.Actor.Player.Character.Id.Entry + " started playing " + CardPlay.Card.Id.Entry);
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

	public CardPlayStartedEntry(CardPlay cardPlay, int roundNumber, CombatSide currentSide, CombatHistory history, IEnumerable<Player> players)
		: base(cardPlay.Card.Owner.Creature, roundNumber, currentSide, history, players)
	{
		CardPlay = cardPlay;
	}
}
