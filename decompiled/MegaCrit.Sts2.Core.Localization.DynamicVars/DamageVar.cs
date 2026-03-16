using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Localization.DynamicVars;

public class DamageVar : DynamicVar
{
	public const string defaultName = "Damage";

	public ValueProp Props { get; set; }

	public DamageVar(decimal damage, ValueProp props)
		: base("Damage", damage)
	{
		Props = props;
	}

	public DamageVar(string name, decimal damage, ValueProp props)
		: base(name, damage)
	{
		Props = props;
	}

	public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
	{
		decimal num = base.BaseValue;
		EnchantmentModel enchantment = card.Enchantment;
		if (enchantment != null)
		{
			num += enchantment.EnchantDamageAdditive(num, Props);
			num *= enchantment.EnchantDamageMultiplicative(num, Props);
			if (!card.IsEnchantmentPreview)
			{
				base.EnchantedValue = num;
			}
		}
		if (runGlobalHooks)
		{
			num = Hook.ModifyDamage(card.Owner.RunState, card.CombatState, target, card.Owner.Creature, base.BaseValue, Props, card, ModifyDamageHookType.All, previewMode, out IEnumerable<AbstractModel> _);
		}
		base.PreviewValue = num;
	}
}
