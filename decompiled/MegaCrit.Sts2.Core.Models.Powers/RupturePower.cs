using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class RupturePower : PowerModel
{
	private class Data
	{
		public readonly Dictionary<CardModel, int> playedCards = new Dictionary<CardModel, int>();
	}

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<StrengthPower>());

	protected override object InitInternalData()
	{
		return new Data();
	}

	public override Task BeforeCardPlayed(CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner.Creature != base.Owner)
		{
			return Task.CompletedTask;
		}
		if (base.CombatState.CurrentSide != base.Owner.Side)
		{
			return Task.CompletedTask;
		}
		GetInternalData<Data>().playedCards.Add(cardPlay.Card, 0);
		return Task.CompletedTask;
	}

	public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target == base.Owner && result.UnblockedDamage > 0 && base.CombatState.CurrentSide == base.Owner.Side)
		{
			if (cardSource == null || !GetInternalData<Data>().playedCards.ContainsKey(cardSource))
			{
				await PowerCmd.Apply<StrengthPower>(base.Owner, base.Amount, base.Owner, null);
			}
			else
			{
				GetInternalData<Data>().playedCards[cardSource] += base.Amount;
			}
		}
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner.Creature == base.Owner && GetInternalData<Data>().playedCards.Remove(cardPlay.Card, out var value))
		{
			await PowerCmd.Apply<StrengthPower>(base.Owner, value, base.Owner, null);
		}
	}
}
