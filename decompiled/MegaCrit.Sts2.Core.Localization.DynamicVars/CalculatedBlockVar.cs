using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Localization.DynamicVars;

public class CalculatedBlockVar : CalculatedVar
{
	public const string defaultName = "CalculatedBlock";

	public ValueProp Props { get; }

	/// <summary>
	/// Create a new <see cref="T:MegaCrit.Sts2.Core.Localization.DynamicVars.CalculatedBlockVar" />.
	/// This will only work if the owner is a <see cref="T:MegaCrit.Sts2.Core.Models.CardModel" /> whose <see cref="T:MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVarSet" /> also has a
	/// <see cref="T:MegaCrit.Sts2.Core.Localization.DynamicVars.CalculationBaseVar" /> and a <see cref="T:MegaCrit.Sts2.Core.Localization.DynamicVars.CalculationExtraVar" />.
	/// Note: For cards whose values are entirely dynamic and have no base value (like <see cref="T:MegaCrit.Sts2.Core.Models.Cards.Mirage" />), you
	/// should use a <see cref="T:MegaCrit.Sts2.Core.Localization.DynamicVars.CalculationBaseVar" /> of 0 and a <see cref="T:MegaCrit.Sts2.Core.Localization.DynamicVars.CalculationExtraVar" /> of 1.
	/// </summary>
	public CalculatedBlockVar(ValueProp props)
		: base("CalculatedBlock")
	{
		Props = props;
	}

	public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
	{
		EnchantmentModel enchantment = card.Enchantment;
		if (enchantment != null)
		{
			decimal baseValue = GetBaseVar().BaseValue;
			baseValue += enchantment.EnchantBlockAdditive(baseValue);
			baseValue *= enchantment.EnchantBlockMultiplicative(baseValue);
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
			base.PreviewValue = Hook.ModifyBlock(card.CombatState, card.Owner.Creature, Calculate(target), Props, card, null, out IEnumerable<AbstractModel> _);
		}
		else if (!card.IsEnchantmentPreview)
		{
			if (enchantment != null)
			{
				num += enchantment.EnchantBlockAdditive(num);
				num *= enchantment.EnchantBlockMultiplicative(num);
			}
			base.PreviewValue = num;
		}
	}
}
