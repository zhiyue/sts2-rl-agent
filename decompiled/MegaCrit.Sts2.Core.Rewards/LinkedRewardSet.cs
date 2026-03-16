using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace MegaCrit.Sts2.Core.Rewards;

public class LinkedRewardSet : Reward
{
	private readonly List<Reward> _rewards;

	protected override RewardType RewardType => RewardType.None;

	public override int RewardsSetIndex => Rewards.Max((Reward r) => r.RewardsSetIndex);

	private static LocString HoverTipTitle => new LocString("static_hover_tips", "LINKED_REWARDS.title");

	private static LocString HoverTipDesc => new LocString("static_hover_tips", "LINKED_REWARDS.description");

	public static HoverTip HoverTip => new HoverTip(HoverTipTitle, HoverTipDesc);

	public IReadOnlyList<Reward> Rewards => _rewards.ToList();

	public override bool IsPopulated => _rewards.All((Reward r) => r.IsPopulated);

	public override LocString Description => new LocString("gameplay_ui", "COMBAT_REWARD_LINKED");

	public LinkedRewardSet(List<Reward> rewards, Player player)
		: base(player)
	{
		_rewards = rewards;
		foreach (Reward reward in _rewards)
		{
			reward.ParentRewardSet = this;
		}
	}

	public override async Task Populate()
	{
		foreach (Reward reward in _rewards)
		{
			await reward.Populate();
		}
	}

	public void RemoveReward(Reward reward)
	{
		_rewards.Remove(reward);
	}

	protected override Task<bool> OnSelect()
	{
		return Task.FromResult(result: true);
	}

	public override void OnSkipped()
	{
		foreach (Reward reward in _rewards)
		{
			reward.OnSkipped();
		}
	}

	public override void MarkContentAsSeen()
	{
		foreach (Reward reward in _rewards)
		{
			reward.MarkContentAsSeen();
		}
	}
}
