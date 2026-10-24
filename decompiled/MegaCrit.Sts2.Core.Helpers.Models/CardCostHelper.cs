using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Helpers.Models;

public static class CardCostHelper
{
	/// <summary>
	/// Get the color that should be used for the text in a card's energy cost.
	/// Depends on a whole bunch of tricky rules, see comments and tests for details.
	/// WARNING: If you make a change to this method, you should probably make a similar change to
	/// <see cref="M:MegaCrit.Sts2.Core.Helpers.Models.CardCostHelper.GetStarCostColor(MegaCrit.Sts2.Core.Models.CardModel,MegaCrit.Sts2.Core.Combat.ICombatState)" />, or write a comment explaining why the two methods are different.
	/// </summary>
	/// <param name="card">Card whose energy cost color we want.</param>
	/// <param name="state">Combat state that the color depends on. Null outside of combat (like in the Card Library).</param>
	/// <returns>Energy cost color for the specified card in the specified combat state.</returns>
	public static CardCostColor GetEnergyCostColor(CardModel card, ICombatState? state)
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

	/// <summary>
	/// Get the color that should be used for the text in a card's star cost.
	/// Depends on a whole bunch of tricky rules, see comments and tests for details.
	/// WARNING: If you make a change to this method, you should probably make a similar change to
	/// <see cref="M:MegaCrit.Sts2.Core.Helpers.Models.CardCostHelper.GetEnergyCostColor(MegaCrit.Sts2.Core.Models.CardModel,MegaCrit.Sts2.Core.Combat.ICombatState)" />, or write a comment explaining why the two methods are different.
	/// </summary>
	/// <param name="card">Card whose star cost color we want.</param>
	/// <param name="state">Combat state that the color depends on. Null outside of combat (like in the Card Library).</param>
	/// <returns>Star cost color for the specified card in the specified combat state.</returns>
	public static CardCostColor GetStarCostColor(CardModel card, ICombatState? state)
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

	private static bool TryModifyEnergyCostWithHooks(CardModel card, ICombatState state, out decimal hookModifiedCost)
	{
		hookModifiedCost = card.EnergyCost.GetWithModifiers(CostModifiers.None);
		bool flag = false;
		foreach (AbstractModel item in state.IterateHookListeners())
		{
			flag |= item.TryModifyEnergyCostInCombat(card, hookModifiedCost, out hookModifiedCost);
		}
		foreach (AbstractModel item2 in state.IterateHookListeners())
		{
			flag |= item2.TryModifyEnergyCostInCombatLate(card, hookModifiedCost, out hookModifiedCost);
		}
		return flag;
	}

	private static bool TryModifyStarCostWithHooks(CardModel card, ICombatState state, out decimal hookModifiedCost)
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
