using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;

namespace MegaCrit.Sts2.Core.Models.Enchantments;

public sealed class TezcatarasEmber : EnchantmentModel
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromKeyword(CardKeyword.Eternal));

	protected override void OnEnchant()
	{
		base.Card.EnergyCost.UpgradeBy(-base.Card.EnergyCost.GetWithModifiers(CostModifiers.None));
		base.Card.AddKeyword(CardKeyword.Eternal);
	}
}
