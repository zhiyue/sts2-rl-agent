using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Helpers.Models;

public static class CardCostHelper
{
	public static CardCostColor GetEnergyCostColor(CardModel card, CombatState? state)
	{
		if (state == null)
		{
			return CardCostColor.Unmodified;
		}
		if (!card.CanPlay(out UnplayableReason reason, out AbstractModel _) && reason.HasFlag(UnplayableReason.EnergyCostTooHigh))
		{
			return CardCostColor.InsufficientResources;
		}
		if (card.EnergyCost.CostsX)
		{
			return CardCostColor.Unmodified;
		}
		if (TryModifyEnergyCostWithHooks(card, state, out var hookModifiedCost))
		{
			return GetColorForHookModifiedCost(hookModifiedCost, card.EnergyCost.GetWithModifiers(CostModifiers.None));
		}
		if (card.EnergyCost.HasLocalModifiers)
		{
			return GetColorForLocalCost(card.EnergyCost.GetWithModifiers(CostModifiers.Local), card.EnergyCost.GetWithModifiers(CostModifiers.None));
		}
		return CardCostColor.Unmodified;
	}

	public static CardCostColor GetStarCostColor(CardModel card, CombatState? state)
	{
		if (state == null)
		{
			return CardCostColor.Unmodified;
		}
		if (!card.CanPlay(out UnplayableReason reason, out AbstractModel _) && reason.HasFlag(UnplayableReason.StarCostTooHigh))
		{
			return CardCostColor.InsufficientResources;
		}
		if (card.HasStarCostX)
		{
			return CardCostColor.Unmodified;
		}
		if (TryModifyStarCostWithHooks(card, state, out var hookModifiedCost))
		{
			return GetColorForHookModifiedCost(hookModifiedCost, card.BaseStarCost);
		}
		if (card.TemporaryStarCost != null)
		{
			return GetColorForLocalCost(card.TemporaryStarCost.Cost, card.BaseStarCost);
		}
		return CardCostColor.Unmodified;
	}

	private static CardCostColor GetColorForLocalCost(int localCost, int baseCost)
	{
		if (localCost > baseCost)
		{
			return CardCostColor.Increased;
		}
		if (localCost < baseCost)
		{
			return CardCostColor.Decreased;
		}
		return CardCostColor.Unmodified;
	}

	private static CardCostColor GetColorForHookModifiedCost(decimal hookModifiedCost, int baseCost)
	{
		if (hookModifiedCost > (decimal)baseCost)
		{
			return CardCostColor.Increased;
		}
		if (hookModifiedCost < (decimal)baseCost)
		{
			return CardCostColor.Decreased;
		}
		return CardCostColor.Unmodified;
	}

	private static bool TryModifyEnergyCostWithHooks(CardModel card, CombatState state, out decimal hookModifiedCost)
	{
		hookModifiedCost = card.EnergyCost.GetWithModifiers(CostModifiers.None);
		bool flag = false;
		foreach (AbstractModel item in state.IterateHookListeners())
		{
			flag |= item.TryModifyEnergyCostInCombat(card, hookModifiedCost, out hookModifiedCost);
		}
		return flag;
	}

	private static bool TryModifyStarCostWithHooks(CardModel card, CombatState state, out decimal hookModifiedCost)
	{
		hookModifiedCost = card.BaseStarCost;
		bool flag = false;
		foreach (AbstractModel item in state.IterateHookListeners())
		{
			flag |= item.TryModifyStarCost(card, hookModifiedCost, out hookModifiedCost);
		}
		return flag;
	}
}
