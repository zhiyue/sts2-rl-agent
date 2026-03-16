using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class MiniatureTent : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Shop;

	public override bool ShouldDisableRemainingRestSiteOptions(Player player)
	{
		if (player != base.Owner)
		{
			return true;
		}
		Flash();
		return false;
	}
}
