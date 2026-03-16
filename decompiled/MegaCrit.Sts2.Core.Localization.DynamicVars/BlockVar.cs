using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Localization.DynamicVars;

public class BlockVar : DynamicVar
{
	public const string defaultName = "Block";

	public ValueProp Props { get; }

	public BlockVar(decimal block, ValueProp props)
		: base("Block", block)
	{
		Props = props;
	}

	public BlockVar(string name, decimal block, ValueProp props)
		: base(name, block)
	{
		Props = props;
	}

	public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
	{
		decimal num = base.BaseValue;
		EnchantmentModel enchantment = card.Enchantment;
		if (enchantment != null)
		{
			num += enchantment.EnchantBlockAdditive(num, Props);
			num *= enchantment.EnchantBlockMultiplicative(num, Props);
			if (!card.IsEnchantmentPreview)
			{
				base.EnchantedValue = num;
			}
		}
		if (runGlobalHooks)
		{
			num = Hook.ModifyBlock(card.CombatState, card.Owner.Creature, base.BaseValue, Props, card, null, out IEnumerable<AbstractModel> _);
		}
		base.PreviewValue = num;
	}
}
