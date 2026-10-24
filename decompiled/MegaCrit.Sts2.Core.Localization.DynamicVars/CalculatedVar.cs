using System;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Localization.DynamicVars;

/// <summary>
/// A special type of DynamicVar that is used for cards that include a calculation in their base behavior.
/// For example, <see cref="T:MegaCrit.Sts2.Core.Models.Cards.PerfectedStrike" /> uses a subclass of this (<see cref="T:MegaCrit.Sts2.Core.Localization.DynamicVars.CalculatedDamageVar" />) for its
/// 6 base damage + 2 extra damage for each Strike.
/// </summary>
public class CalculatedVar : DynamicVar
{
	private Func<CardModel, Creature?, decimal>? _multiplierCalc;

	public CalculatedVar(string name)
		: base(name, 0m)
	{
	}

	public override void SetOwner(AbstractModel owner)
	{
		base.SetOwner(owner);
		UpdateValues();
	}

	/// <summary>
	/// Set the function that will be used for the multiplier value of this var.
	/// This will be multiplied by the <see cref="M:MegaCrit.Sts2.Core.Localization.DynamicVars.CalculatedVar.GetExtraVar" /> value.
	/// This is the "dynamic" part of the calculation, and will often be based on something in the CombatState.
	/// </summary>
	/// <example>
	/// <see cref="T:MegaCrit.Sts2.Core.Models.Cards.PerfectedStrike" /> says:
	/// "Deal 6 damage, plus 2 additional damage for ALL your cards containing "Strike"."
	/// In this case, the extra multiplier is the number of cards containing "Strike".
	/// </example>
	/// <param name="multiplierCalc">
	/// Function that will be used to calculate the extra multiplier value.
	/// Argument 1 is the card instance that this calculation is occurring on.
	/// Argument 2 is the creature that is being targeted for this calculation (null when there is no target).
	/// </param>
	public CalculatedVar WithMultiplier(Func<CardModel, Creature?, decimal> multiplierCalc)
	{
		if (_multiplierCalc != null)
		{
			throw new InvalidOperationException($"Tried to set extra multiplier calc on {this} twice!");
		}
		if (multiplierCalc.Target is AbstractModel)
		{
			throw new InvalidOperationException("Multiplier calc must be static!");
		}
		_multiplierCalc = multiplierCalc;
		return this;
	}

	/// <summary>
	/// Calculate this var's value based on the specified target.
	/// Effects that target multiple creatures (or no creatures) should pass null for the target.
	/// </summary>
	public decimal Calculate(Creature? target)
	{
		if (_multiplierCalc == null)
		{
			throw new InvalidOperationException("Extra multiplier calc must be specified!");
		}
		CardModel cardModel = (CardModel)_owner;
		decimal num = ((CombatManager.Instance.IsInProgress && cardModel.CombatState != null) ? _multiplierCalc(cardModel, target) : 0m);
		return GetBaseVar().BaseValue + GetExtraVar().BaseValue * num;
	}

	/// <summary>
	/// Re-run the calculations to determine if any of the dependent values have been upgraded or enchanted.
	/// </summary>
	public void RecalculateForUpgradeOrEnchant()
	{
		decimal baseValue = GetBaseVar().BaseValue;
		if (baseValue != base.BaseValue)
		{
			base.WasJustUpgraded = true;
		}
		base.BaseValue = baseValue;
	}

	public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
	{
		base.PreviewValue = Calculate(target);
	}

	/// <summary>
	/// Get the DynamicVar that should be used for this calculation's base value.
	/// </summary>
	protected virtual DynamicVar GetBaseVar()
	{
		return ((CardModel)_owner).DynamicVars.CalculationBase;
	}

	/// <summary>
	/// Get the DynamicVar that should be used for this calculation's extra value.
	/// </summary>
	protected virtual DynamicVar GetExtraVar()
	{
		return ((CardModel)_owner).DynamicVars.CalculationExtra;
	}

	protected override decimal GetBaseValueForIConvertible()
	{
		return Calculate(null);
	}

	public override string ToString()
	{
		return Calculate(null).ToString();
	}

	private void UpdateValues()
	{
		if (_owner != null)
		{
			base.BaseValue = GetBaseVar().BaseValue;
		}
	}
}
