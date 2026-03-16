using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Rewards;

public class CardRemovalReward : Reward
{
	private static string RewardIcon => ImageHelper.GetImagePath("ui/reward_screen/reward_icon_card_removal.png");

	protected override RewardType RewardType => RewardType.RemoveCard;

	public override int RewardsSetIndex => 7;

	protected override string IconPath => RewardIcon;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(RewardIcon);

	public override bool IsPopulated => true;

	public override LocString Description => new LocString("gameplay_ui", "COMBAT_REWARD_CARD_REMOVAL");

	public CardRemovalReward(Player player)
		: base(player)
	{
	}

	public override Task Populate()
	{
		return Task.CompletedTask;
	}

	protected override async Task<bool> OnSelect()
	{
		Log.Info("Obtained card removal from reward");
		return await RunManager.Instance.RewardSynchronizer.DoLocalCardRemoval();
	}

	public override void MarkContentAsSeen()
	{
	}
}
