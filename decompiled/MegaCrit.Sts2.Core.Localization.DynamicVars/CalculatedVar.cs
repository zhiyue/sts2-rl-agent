using System;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Localization.DynamicVars;

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

	protected virtual DynamicVar GetBaseVar()
	{
		return ((CardModel)_owner).DynamicVars.CalculationBase;
	}

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
