using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class RingingTriangle : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Shop;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromKeyword(CardKeyword.Retain));

	public override bool ShouldFlush(Player player)
	{
		if (player != base.Owner)
		{
			return true;
		}
		return player.Creature.CombatState.RoundNumber > 1;
	}
}
