using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class VoidFormPower : PowerModel
{
	private class Data
	{
		public int cardsPlayedThisTurn;
	}

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override object InitInternalData()
	{
		return new Data();
	}

	public override Task BeforePowerAmountChanged(PowerModel power, decimal amount, Creature target, Creature? applier, CardModel? cardSource)
	{
		if (power != this)
		{
			return Task.CompletedTask;
		}
		HideTemporaryZeroCostVisual();
		return Task.CompletedTask;
	}

	public override Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
	{
		HideTemporaryZeroCostVisual();
		return Task.CompletedTask;
	}

	public override bool TryModifyEnergyCostInCombatLate(CardModel card, decimal originalCost, out decimal modifiedCost)
	{
		modifiedCost = originalCost;
		if (ShouldSkip(card))
		{
			return false;
		}
		modifiedCost = default(decimal);
		return true;
	}

	public override bool TryModifyStarCost(CardModel card, decimal originalCost, out decimal modifiedCost)
	{
		modifiedCost = originalCost;
		if (ShouldSkip(card))
		{
			return false;
		}
		modifiedCost = default(decimal);
		return true;
	}

	public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner.Creature == base.Owner && cardPlay != null && !cardPlay.IsAutoPlay && cardPlay.IsLastInSeries)
		{
			GetInternalData<Data>().cardsPlayedThisTurn++;
		}
		return Task.CompletedTask;
	}

	public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (!participants.Contains(base.Owner))
		{
			return Task.CompletedTask;
		}
		GetInternalData<Data>().cardsPlayedThisTurn = 0;
		return Task.CompletedTask;
	}

	private bool ShouldSkip(CardModel card)
	{
		bool flag = card.Owner.Creature != base.Owner;
		bool flag2 = flag;
		if (!flag2)
		{
			bool flag3;
			switch (card.Pile?.Type)
			{
			case PileType.Hand:
			case PileType.Play:
				flag3 = true;
				break;
			default:
				flag3 = false;
				break;
			}
			flag2 = !flag3;
		}
		if (!flag2)
		{
			return GetInternalData<Data>().cardsPlayedThisTurn >= base.Amount;
		}
		return true;
	}

	/// <summary>
	/// HACK: If Void Form is the first card played in a turn, there's a brief period before the turn is auto-ended
	/// where we show a zero energy cost on all cards.
	/// To avoid this, we max out cardsPlayedThisTurn, which doesn't matter anyways since the turn is ending.
	/// </summary>
	private void HideTemporaryZeroCostVisual()
	{
		GetInternalData<Data>().cardsPlayedThisTurn = 999999999;
	}
}
