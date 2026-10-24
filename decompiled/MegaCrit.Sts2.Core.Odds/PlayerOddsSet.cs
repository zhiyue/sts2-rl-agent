using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Odds;

public class PlayerOddsSet
{
	/// <summary>
	/// Manages the odds of rolling various rarities when generating cards (encounter rewards, shops, etc.) for each player.
	/// </summary>
	public CardRarityOdds CardRarity { get; private init; }

	/// <summary>
	/// Manages the odds of whether or not a potion drops after beating an encounter for each player.
	/// </summary>
	public PotionRewardOdds PotionReward { get; private init; }

	private PlayerOddsSet()
	{
	}

	public PlayerOddsSet(PlayerRngSet rng)
	{
		CardRarity = new CardRarityOdds(rng.Rewards);
		PotionReward = new PotionRewardOdds(rng.Rewards);
	}

	public SerializablePlayerOddsSet ToSerializable()
	{
		return new SerializablePlayerOddsSet
		{
			CardRarityOddsValue = CardRarity.CurrentValue,
			PotionRewardOddsValue = PotionReward.CurrentValue
		};
	}

	public static PlayerOddsSet FromSerializable(SerializablePlayerOddsSet save, PlayerRngSet rng)
	{
		return new PlayerOddsSet
		{
			CardRarity = new CardRarityOdds(save.CardRarityOddsValue, rng.Rewards),
			PotionReward = new PotionRewardOdds(save.PotionRewardOddsValue, rng.Rewards)
		};
	}

	public void LoadFromSerializable(SerializablePlayerOddsSet save)
	{
		CardRarity.OverrideCurrentValue(save.CardRarityOddsValue);
		PotionReward.OverrideCurrentValue(save.PotionRewardOddsValue);
	}
}
