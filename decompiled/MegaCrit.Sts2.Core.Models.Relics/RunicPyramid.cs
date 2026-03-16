using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class RunicPyramid : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	public override bool ShouldFlush(Player player)
	{
		if (player != base.Owner)
		{
			return true;
		}
		return false;
	}
}
