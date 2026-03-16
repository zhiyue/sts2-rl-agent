using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Cards;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class SwordSagePower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromCardWithCardHoverTips<SovereignBlade>();

	public override Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
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
			if (item is SovereignBlade sovereignBlade)
			{
				sovereignBlade.SetRepeats(base.Amount + 1);
			}
		}
		return Task.CompletedTask;
	}

	public override Task AfterCardEnteredCombat(CardModel card)
	{
		if (card.Owner != base.Owner.Player)
		{
			return Task.CompletedTask;
		}
		if (!(card is SovereignBlade sovereignBlade))
		{
			return Task.CompletedTask;
		}
		sovereignBlade.SetRepeats(base.Amount + 1);
		return Task.CompletedTask;
	}

	public override Task AfterRemoved(Creature oldOwner)
	{
		IEnumerable<CardModel> enumerable = oldOwner.Player?.PlayerCombatState?.AllCards ?? Array.Empty<CardModel>();
		foreach (CardModel item in enumerable)
		{
			if (item is SovereignBlade sovereignBlade)
			{
				sovereignBlade.SetRepeats(1m);
			}
		}
		return Task.CompletedTask;
	}

	public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
	{
		modifiedCost = originalCost;
		if (card.Owner.Creature != base.Owner)
		{
			return false;
		}
		if (!(card is SovereignBlade))
		{
			return false;
		}
		modifiedCost += (decimal)base.Amount;
		return true;
	}
}
