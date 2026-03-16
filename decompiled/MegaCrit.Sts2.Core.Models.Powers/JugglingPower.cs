using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class JugglingPower : PowerModel
{
	private class Data
	{
		public int attacksPlayedThisTurn;
	}

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override object InitInternalData()
	{
		return new Data();
	}

	public override Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		GetInternalData<Data>().attacksPlayedThisTurn = CombatManager.Instance.History.CardPlaysStarted.Count((CardPlayStartedEntry e) => e.CardPlay.Card.Type == CardType.Attack && e.CardPlay.Card.Owner.Creature == base.Owner && e.HappenedThisTurn(base.CombatState));
		return Task.CompletedTask;
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner != base.Owner.Player || cardPlay.Card.Type != CardType.Attack)
		{
			return;
		}
		GetInternalData<Data>().attacksPlayedThisTurn++;
		if (GetInternalData<Data>().attacksPlayedThisTurn == 3)
		{
			Flash();
			for (int i = 0; i < base.Amount; i++)
			{
				CardModel card = cardPlay.Card.CreateClone();
				await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, addedByPlayer: true);
			}
		}
	}

	public override Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side == base.Owner.Side)
		{
			GetInternalData<Data>().attacksPlayedThisTurn = 0;
		}
		return Task.CompletedTask;
	}
}
