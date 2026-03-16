using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Localization.DynamicVars;

public class ExtraDamageVar : DynamicVar
{
	public const string defaultName = "ExtraDamage";

	public bool IsFromOsty { get; private set; }

	public ExtraDamageVar FromOsty()
	{
		IsFromOsty = true;
		return this;
	}

	public ExtraDamageVar(decimal damage)
		: base("ExtraDamage", damage)
	{
	}

	public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
	{
		decimal baseValue = base.BaseValue;
		EnchantmentModel enchantment = card.Enchantment;
		if (enchantment != null)
		{
			baseValue *= enchantment.EnchantDamageMultiplicative(baseValue, ValueProp.Move);
			if (!card.IsEnchantmentPreview)
			{
				base.EnchantedValue = baseValue;
			}
		}
		base.PreviewValue = baseValue;
	}
}
