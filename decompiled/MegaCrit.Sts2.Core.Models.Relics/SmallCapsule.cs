using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Rewards;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class SmallCapsule : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	public override async Task AfterObtained()
	{
		await RewardsCmd.OfferCustom(base.Owner, new List<Reward>(1)
		{
			new RelicReward(base.Owner)
		});
	}
}
