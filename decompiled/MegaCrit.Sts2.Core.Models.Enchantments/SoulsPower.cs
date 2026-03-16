using System.Collections.Generic;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;

namespace MegaCrit.Sts2.Core.Models.Enchantments;

public sealed class SoulsPower : EnchantmentModel
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromKeyword(CardKeyword.Exhaust));

	public override bool CanEnchant(CardModel card)
	{
		if (!base.CanEnchant(card))
		{
			return false;
		}
		if (!card.Keywords.Contains(CardKeyword.Exhaust))
		{
			return false;
		}
		return true;
	}

	protected override void OnEnchant()
	{
		CardCmd.RemoveKeyword(base.Card, CardKeyword.Exhaust);
	}
}
