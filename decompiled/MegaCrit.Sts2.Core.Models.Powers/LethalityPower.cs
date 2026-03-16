using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class LethalityPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (!props.IsPoweredAttack())
		{
			return 1m;
		}
		if (cardSource == null)
		{
			return 1m;
		}
		if (cardSource.Owner.Creature != base.Owner)
		{
			return 1m;
		}
		int num = CombatManager.Instance.History.CardPlaysStarted.Count((CardPlayStartedEntry e) => e.HappenedThisTurn(base.CombatState) && e.CardPlay.Card.Type == CardType.Attack && e.CardPlay.Card.Owner.Creature == base.Owner);
		int num2 = ((cardSource.Pile.Type == PileType.Play) ? 1 : 0);
		if (num > num2)
		{
			return 1m;
		}
		return 1m + (decimal)base.Amount / 100m;
	}
}
