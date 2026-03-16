using System;
using System.Collections.Generic;

namespace MegaCrit.Sts2.Core.Rewards;

public static class RewardTypeExtensions
{
	public static IEnumerable<string> GetAssetPaths(this RewardType rewardType)
	{
		return rewardType switch
		{
			RewardType.Card => CardReward.AssetPaths, 
			RewardType.Gold => GoldReward.AssetPaths, 
			RewardType.RemoveCard => CardRemovalReward.AssetPaths, 
			RewardType.SpecialCard => SpecialCardReward.AssetPaths, 
			_ => Array.Empty<string>(), 
		};
	}
}
