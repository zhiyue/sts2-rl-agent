using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Enchantments;

public sealed class Spiral : EnchantmentModel
{
	private const string _timesKey = "Times";

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new IntVar("Times", 1m));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.Static(StaticHoverTip.ReplayDynamic, base.DynamicVars["Times"]));

	public override bool CanEnchant(CardModel c)
	{
		if (base.CanEnchant(c) && c.Rarity == CardRarity.Basic)
		{
			if (!c.Tags.Contains(CardTag.Strike))
			{
				return c.Tags.Contains(CardTag.Defend);
			}
			return true;
		}
		return false;
	}

	public override int EnchantPlayCount(int originalPlayCount)
	{
		return originalPlayCount + base.DynamicVars["Times"].IntValue;
	}
}
