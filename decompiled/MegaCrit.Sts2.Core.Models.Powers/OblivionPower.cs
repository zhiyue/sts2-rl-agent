using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class OblivionPower : PowerModel
{
	private class Data
	{
		/// <summary>
		/// Keep track of the cards we've seen played and the power amount at the time they were played.
		/// This lets Oblivion avoid triggering on itself.
		/// </summary>
		public readonly Dictionary<CardModel, int> amountsForPlayedCards = new Dictionary<CardModel, int>();
	}

	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override PowerInstanceType InstanceType => PowerInstanceType.InstancedPerApplier;

	protected override object InitInternalData()
	{
		return new Data();
	}

	public override Task BeforeCardPlayed(CardPlay cardPlay)
	{
		if (base.Applier?.Player == null)
		{
			return Task.CompletedTask;
		}
		if (cardPlay.Card.Owner != base.Applier.Player)
		{
			return Task.CompletedTask;
		}
		GetInternalData<Data>().amountsForPlayedCards.Add(cardPlay.Card, base.Amount);
		return Task.CompletedTask;
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (GetInternalData<Data>().amountsForPlayedCards.Remove(cardPlay.Card, out var value))
		{
			Flash();
			await PowerCmd.Apply<DoomPower>(choiceContext, base.Owner, value, base.Applier, null);
		}
	}

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (side == CombatSide.Player)
		{
			await PowerCmd.Remove(this);
		}
	}
}
