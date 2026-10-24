using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class StranglePower : PowerModel
{
	private class Data
	{
		/// <summary>
		/// Keep track of the cards we've seen played and the power amount at the time they were played.
		/// This lets Strangle avoid triggering on itself, especially when stacking.
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
			await CreatureCmd.Damage(choiceContext, base.Owner, value, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
		}
	}

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (participants.Contains(base.Owner))
		{
			await PowerCmd.Remove(this);
		}
	}
}
