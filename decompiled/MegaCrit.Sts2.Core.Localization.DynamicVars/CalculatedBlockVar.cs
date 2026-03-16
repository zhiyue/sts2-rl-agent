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
			baseValue += enchantment.EnchantBlockAdditive(baseValue, Props);
			baseValue *= enchantment.EnchantBlockMultiplicative(baseValue, Props);
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
				num += enchantment.EnchantBlockAdditive(num, Props);
				num *= enchantment.EnchantBlockMultiplicative(num, Props);
			}
			base.PreviewValue = num;
		}
	}
}
