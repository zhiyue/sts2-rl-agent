using System.Collections.Generic;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Localization.DynamicVars;

public class OstyDamageVar : DynamicVar
{
	public const string defaultName = "OstyDamage";

	public ValueProp Props { get; set; }

	public OstyDamageVar(decimal damage, ValueProp props)
		: base("OstyDamage", damage)
	{
		Props = props;
	}

	public OstyDamageVar(string name, decimal damage, ValueProp props)
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
			CombatState combatState = card.CombatState ?? card.Owner.Creature.CombatState;
			num = Hook.ModifyDamage(card.Owner.RunState, combatState, target, card.Owner.Osty, base.BaseValue, Props, card, ModifyDamageHookType.All, previewMode, out IEnumerable<AbstractModel> _);
		}
		base.PreviewValue = num;
	}
}
