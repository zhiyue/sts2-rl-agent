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

public sealed class GravityPower : PowerModel
{
	private class Data
	{
		/// <summary>
		/// Keep track of the cards we've seen played and the power amount at the time they were played.
		/// This lets Gravity avoid triggering on cards that started play before it was applied, and avoid applying
		/// extra damage on multiple plays of Gravity Well.
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
		GetInternalData<Data>().amountsForPlayedCards.Add(cardPlay.Card, base.Amount);
		return Task.CompletedTask;
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		GetInternalData<Data>().amountsForPlayedCards.Remove(cardPlay.Card, out var value);
		if (value > 0)
		{
			await CreatureCmd.Damage(choiceContext, base.Owner.CombatState.HittableEnemies, value, ValueProp.Unpowered, base.Owner, null);
		}
	}

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (participants.Contains(base.Owner))
		{
			Flash();
			await PowerCmd.Remove(this);
		}
	}
}
