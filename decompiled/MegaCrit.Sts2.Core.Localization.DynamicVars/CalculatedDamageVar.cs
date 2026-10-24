using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Localization.DynamicVars;

public class CalculatedDamageVar : CalculatedVar
{
	public const string defaultName = "CalculatedDamage";

	public ValueProp Props { get; }

	public bool IsFromOsty { get; private set; }

	/// <summary>
	/// Create a new <see cref="T:MegaCrit.Sts2.Core.Localization.DynamicVars.CalculatedDamageVar" />.
	/// This will only work if the owner is a <see cref="T:MegaCrit.Sts2.Core.Models.CardModel" /> whose <see cref="T:MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVarSet" /> also has a
	/// <see cref="T:MegaCrit.Sts2.Core.Localization.DynamicVars.CalculationBaseVar" /> and a <see cref="T:MegaCrit.Sts2.Core.Localization.DynamicVars.ExtraDamageVar" />.
	/// Note: For cards whose values are entirely dynamic and have no base value (like <see cref="T:MegaCrit.Sts2.Core.Models.Cards.BodySlam" />), you
	/// should use a <see cref="T:MegaCrit.Sts2.Core.Localization.DynamicVars.CalculationBaseVar" /> of 0 and a <see cref="T:MegaCrit.Sts2.Core.Localization.DynamicVars.ExtraDamageVar" /> of 1.
	/// </summary>
	public CalculatedDamageVar(ValueProp props)
		: base("CalculatedDamage")
	{
		Props = props;
	}

	/// <summary>
	/// Set this damage to come from Osty.
	/// </summary>
	public CalculatedDamageVar FromOsty()
	{
		IsFromOsty = true;
		return this;
	}

	public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
	{
		EnchantmentModel enchantment = card.Enchantment;
		if (enchantment != null)
		{
			decimal baseValue = GetBaseVar().BaseValue;
			baseValue += enchantment.EnchantDamageAdditive(baseValue, Props);
			baseValue *= enchantment.EnchantDamageMultiplicative(baseValue, Props);
			baseValue = Math.Max(baseValue, 0m);
			if (card.IsEnchantmentPreview)
			{
				base.PreviewValue = baseValue;
			}
			else
			{
				base.EnchantedValue = baseValue;
			}
		}
		decimal num = Calculate(target);
		if (runGlobalHooks)
		{
			ICombatState combatState = card.CombatState ?? card.Owner.Creature.CombatState;
			base.PreviewValue = Hook.ModifyDamage(card.Owner.RunState, combatState, target, IsFromOsty ? card.Owner.Osty : card.Owner.Creature, num, Props, card, ModifyDamageHookType.All, previewMode, out IEnumerable<AbstractModel> _);
		}
		else if (!card.IsEnchantmentPreview)
		{
			if (enchantment != null)
			{
				num += enchantment.EnchantDamageAdditive(num, Props);
				num *= enchantment.EnchantDamageMultiplicative(num, Props);
			}
			base.PreviewValue = num;
		}
		base.PreviewValue = Math.Max(base.PreviewValue, 0m);
	}

	protected override DynamicVar GetExtraVar()
	{
		return ((CardModel)_owner).DynamicVars.ExtraDamage;
	}
}
