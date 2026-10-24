using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Cards;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class SwordSagePower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips
	{
		get
		{
			List<IHoverTip> list = new List<IHoverTip>();
			list.AddRange(HoverTipFactory.FromCardWithCardHoverTips<SovereignBlade>());
			list.Add(HoverTipFactory.Static(StaticHoverTip.ReplayStatic));
			return new _003C_003Ez__ReadOnlyList<IHoverTip>(list);
		}
	}

	public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		if (!(power is SwordSagePower))
		{
			return Task.CompletedTask;
		}
		if (power.Owner != base.Owner)
		{
			return Task.CompletedTask;
		}
		IEnumerable<CardModel> enumerable = base.Owner.Player?.PlayerCombatState?.AllCards ?? Array.Empty<CardModel>();
		foreach (CardModel item in enumerable)
		{
			TryAddReplays(item, (int)amount);
		}
		return Task.CompletedTask;
	}

	public override Task AfterCardEnteredCombat(CardModel card)
	{
		if (card.IsClone)
		{
			return Task.CompletedTask;
		}
		TryAddReplays(card, base.Amount);
		return Task.CompletedTask;
	}

	public override Task AfterRemoved(Creature oldOwner)
	{
		IEnumerable<CardModel> enumerable = oldOwner.Player?.PlayerCombatState?.AllCards ?? Array.Empty<CardModel>();
		foreach (CardModel item in enumerable)
		{
			if (item is SovereignBlade sovereignBlade)
			{
				sovereignBlade.BaseReplayCount -= base.Amount;
			}
		}
		return Task.CompletedTask;
	}

	private bool TryAddReplays(CardModel card, int amount)
	{
		if (card.Owner != base.Owner.Player)
		{
			return false;
		}
		if (!(card is SovereignBlade sovereignBlade))
		{
			return false;
		}
		sovereignBlade.BaseReplayCount += amount;
		return true;
	}
}
