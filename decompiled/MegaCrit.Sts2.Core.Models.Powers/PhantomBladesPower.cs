using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class PhantomBladesPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
	{
		HoverTipFactory.FromCard<Shiv>(),
		HoverTipFactory.FromKeyword(CardKeyword.Retain)
	});

	public override Task AfterCardEnteredCombat(CardModel card)
	{
		if (!card.Tags.Contains(CardTag.Shiv))
		{
			return Task.CompletedTask;
		}
		if (card.Owner != base.Owner.Player)
		{
			return Task.CompletedTask;
		}
		CardCmd.ApplyKeyword(card, CardKeyword.Retain);
		return Task.CompletedTask;
	}

	public override Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		foreach (CardModel item in base.Owner.Player.PlayerCombatState.AllCards.Where((CardModel c) => c.Tags.Contains(CardTag.Shiv)))
		{
			CardCmd.ApplyKeyword(item, CardKeyword.Retain);
		}
		return Task.CompletedTask;
	}

	public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (!props.IsPoweredAttack())
		{
			return 0m;
		}
		if (cardSource == null || !cardSource.Tags.Contains(CardTag.Shiv))
		{
			return 0m;
		}
		if (dealer != base.Owner)
		{
			return 0m;
		}
		int num = CombatManager.Instance.History.CardPlaysFinished.Count((CardPlayFinishedEntry e) => e.HappenedThisTurn(base.CombatState) && e.CardPlay.Card.Tags.Contains(CardTag.Shiv) && e.CardPlay.Card.Owner.Creature == base.Owner);
		if (num > 0)
		{
			return 0m;
		}
		return base.Amount;
	}
}
