using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;

namespace MegaCrit.Sts2.Core.Models.Enchantments;

public sealed class Steady : EnchantmentModel
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromKeyword(CardKeyword.Retain));

	protected override void OnEnchant()
	{
		base.Card.AddKeyword(CardKeyword.Retain);
	}
}
