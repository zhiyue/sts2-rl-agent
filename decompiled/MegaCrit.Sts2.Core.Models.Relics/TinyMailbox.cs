using System.Collections.Generic;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Rewards;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class TinyMailbox : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Uncommon;

	public override bool TryModifyRestSiteHealRewards(Player player, List<Reward> rewards, bool isMimicked)
	{
		if (player != base.Owner)
		{
			return false;
		}
		rewards.Add(new PotionReward(player));
		rewards.Add(new PotionReward(player));
		Flash();
		return true;
	}

	public override IReadOnlyList<LocString> ModifyExtraRestSiteHealText(Player player, IReadOnlyList<LocString> currentExtraText)
	{
		if (!LocalContext.IsMe(base.Owner))
		{
			return currentExtraText;
		}
		int num = 0;
		LocString[] array = new LocString[1 + currentExtraText.Count];
		foreach (LocString item in currentExtraText)
		{
			array[num] = item;
			num++;
		}
		array[num] = base.AdditionalRestSiteHealText;
		return new global::_003C_003Ez__ReadOnlyArray<LocString>(array);
	}
}
