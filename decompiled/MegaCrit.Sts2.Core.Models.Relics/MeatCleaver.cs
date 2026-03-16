using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.HoverTips;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class MeatCleaver : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.Static(StaticHoverTip.Cook));

	public override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
	{
		if (player != base.Owner)
		{
			return false;
		}
		options.Add(new CookRestSiteOption(player));
		return true;
	}
}
