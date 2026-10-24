using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class SubroutinePower : PowerModel
{
	private class Data
	{
		/// <summary>
		/// Keep track of the Power cards we've seen played and the power amount at the time they were played.
		/// This lets SubroutinePower avoid triggering on cards that started play before it was applied, and avoid
		/// granting extra energy on multiple plays of Subroutine.
		/// </summary>
		public readonly Dictionary<CardModel, int> amountsForPlayedCards = new Dictionary<CardModel, int>();
	}

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override object InitInternalData()
	{
		return new Data();
	}

	public override Task BeforeCardPlayed(CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner != base.Owner.Player)
		{
			return Task.CompletedTask;
		}
		if (cardPlay.Card.Type != CardType.Power)
		{
			return Task.CompletedTask;
		}
		GetInternalData<Data>().amountsForPlayedCards.Add(cardPlay.Card, base.Amount);
		return Task.CompletedTask;
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner == base.Owner.Player && GetInternalData<Data>().amountsForPlayedCards.Remove(cardPlay.Card, out var energy) && energy > 0)
		{
			Flash();
			for (int i = 0; i < energy; i++)
			{
				await PlayerCmd.GainEnergy(1m, base.Owner.Player);
			}
		}
	}
}
