using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class SlothPower : PowerModel
{
	private int _cardsPlayedThisTurn;

	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override int DisplayAmount => _cardsPlayedThisTurn;

	public override bool ShouldPlay(CardModel card, AutoPlayType _)
	{
		if (card.Owner.Creature != base.Owner)
		{
			return true;
		}
		return _cardsPlayedThisTurn < base.Amount;
	}

	public override Task BeforeCardPlayed(CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner != base.Owner.Player)
		{
			return Task.CompletedTask;
		}
		_cardsPlayedThisTurn++;
		InvokeDisplayAmountChanged();
		return Task.CompletedTask;
	}

	public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
	{
		if (side != base.Owner.Side)
		{
			return Task.CompletedTask;
		}
		_cardsPlayedThisTurn = 0;
		InvokeDisplayAmountChanged();
		return Task.CompletedTask;
	}
}
