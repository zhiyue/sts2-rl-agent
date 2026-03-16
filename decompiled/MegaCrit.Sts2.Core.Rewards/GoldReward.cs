using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Rewards;

public class GoldReward : Reward
{
	public const int defaultMinGoldAmount = 10;

	public const int defaultMaxGoldAmount = 20;

	private readonly bool _wasGoldStolenBack;

	private readonly int _min;

	private readonly int _max;

	private static string RewardIcon => ImageHelper.GetImagePath("ui/reward_screen/reward_icon_money.png");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(RewardIcon);

	protected override RewardType RewardType => RewardType.Gold;

	public override int RewardsSetIndex => 1;

	protected override string IconPath => RewardIcon;

	public override Vector2 IconPosition => new Vector2(0f, -5f);

	public int Amount { get; private set; } = -1;

	public override LocString Description
	{
		get
		{
			LocString locString = new LocString("gameplay_ui", _wasGoldStolenBack ? "COMBAT_REWARD_GOLD_STOLEN" : "COMBAT_REWARD_GOLD");
			locString.Add("gold", Amount);
			return locString;
		}
	}

	public override bool IsPopulated => Amount >= 0;

	public GoldReward(int amount, Player player, bool wasGoldStolenBack = false)
		: base(player)
	{
		_min = amount;
		_max = amount;
		Amount = amount;
		_wasGoldStolenBack = wasGoldStolenBack;
	}

	public GoldReward(int min, int max, Player player, bool wasGoldStolenBack = false)
		: base(player)
	{
		_min = min;
		_max = max;
		_wasGoldStolenBack = wasGoldStolenBack;
	}

	public override Task Populate()
	{
		Rng rng = _rngOverride ?? base.Player.PlayerRng.Rewards;
		Amount = rng.NextInt(_min, _max + 1);
		return Task.CompletedTask;
	}

	protected override async Task<bool> OnSelect()
	{
		await PlayerCmd.GainGold(Amount, base.Player, _wasGoldStolenBack);
		RunManager.Instance.RewardSynchronizer.SyncLocalObtainedGold(Amount);
		Log.Info($"Obtained {Amount} gold from reward");
		return true;
	}

	public override SerializableReward ToSerializable()
	{
		return new SerializableReward
		{
			RewardType = RewardType.Gold,
			GoldAmount = Amount,
			WasGoldStolenBack = _wasGoldStolenBack
		};
	}

	public override void MarkContentAsSeen()
	{
	}
}
