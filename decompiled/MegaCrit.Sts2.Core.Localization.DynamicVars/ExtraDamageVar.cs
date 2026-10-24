using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Localization.DynamicVars;

/// <summary>
/// A special version of <see cref="T:MegaCrit.Sts2.Core.Localization.DynamicVars.DamageVar" /> that is used to represent extra damage done by a calculated damage card.
/// For example, <see cref="T:MegaCrit.Sts2.Core.Models.Cards.PerfectedStrike" /> uses this for its 2 extra damage done for each Strike.
/// </summary>
public class ExtraDamageVar : DynamicVar
{
	public const string defaultName = "ExtraDamage";

	public bool IsFromOsty { get; private set; }

	/// <summary>
	/// Set this damage to come from Osty.
	/// </summary>
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
