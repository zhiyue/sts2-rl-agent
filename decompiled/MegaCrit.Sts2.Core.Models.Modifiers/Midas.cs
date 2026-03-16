using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Modifiers;

public class Midas : ModifierModel
{
	public override bool TryModifyRewardsLate(Player player, List<Reward> rewards, AbstractRoom? room)
	{
		List<Reward> list = new List<Reward>();
		foreach (Reward reward in rewards)
		{
			if (reward is GoldReward goldReward)
			{
				list.Add(new GoldReward(goldReward.Amount * 2, player));
			}
			else
			{
				list.Add(reward);
			}
		}
		rewards.Clear();
		rewards.AddRange(list);
		return true;
	}

	public override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
	{
		List<SmithRestSiteOption> list = options.OfType<SmithRestSiteOption>().ToList();
		foreach (SmithRestSiteOption item in list)
		{
			options.Remove(item);
		}
		return true;
	}
}
