using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class VitruvianMinion : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Shop;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.Static(StaticHoverTip.Block));

	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (cardSource == null)
		{
			return 1m;
		}
		if (cardSource.Owner != base.Owner)
		{
			return 1m;
		}
		if (!cardSource.Tags.Contains(CardTag.Minion))
		{
			return 1m;
		}
		return 2m;
	}

	public override decimal ModifyBlockMultiplicative(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
	{
		if (cardSource == null)
		{
			return 1m;
		}
		if (cardSource.Owner != base.Owner)
		{
			return 1m;
		}
		if (!cardSource.Tags.Contains(CardTag.Minion))
		{
			return 1m;
		}
		return 2m;
	}
}
