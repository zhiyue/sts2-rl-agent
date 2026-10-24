using System;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Odds;

public class CardRarityOdds : AbstractOdds
{
	public static float regularCommonOdds = AscensionHelper.GetValueIfAscension(AscensionLevel.Scarcity, 0.615f, 0.6f);

	public const float regularUncommonOdds = 0.37f;

	public const float eliteUncommonOdds = 0.4f;

	public const float bossCommonOdds = 0f;

	public const float bossUncommonOdds = 0f;

	public const float bossRareOdds = 1f;

	public const float shopUncommonOdds = 0.37f;

	private const float _baseRarityOffset = -0.05f;

	private const float _maxRarityOffset = 0.4f;

	public float RarityGrowth => AscensionHelper.GetValueIfAscension(AscensionLevel.Scarcity, 0.005f, 0.01f);

	public static float RegularRareOdds => AscensionHelper.GetValueIfAscension(AscensionLevel.Scarcity, 0.0149f, 0.03f);

	public static float EliteCommonOdds => AscensionHelper.GetValueIfAscension(AscensionLevel.Scarcity, 0.549f, 0.5f);

	public static float EliteRareOdds => AscensionHelper.GetValueIfAscension(AscensionLevel.Scarcity, 0.05f, 0.1f);

	public static float ShopCommonOdds => AscensionHelper.GetValueIfAscension(AscensionLevel.Scarcity, 0.585f, 0.54f);

	public static float ShopRareOdds => AscensionHelper.GetValueIfAscension(AscensionLevel.Scarcity, 0.045f, 0.09f);

	/// <summary>
	/// For creating at the start of a run.
	/// </summary>
	/// <param name="rng">RNG to use for rolls.</param>
	public CardRarityOdds(Rng rng)
		: base(-0.05f, rng)
	{
	}

	/// <summary>
	/// For restoring from save.
	/// </summary>
	/// <param name="initialValue">Restored value at the saved spot in the run.</param>
	/// <param name="rng">RNG to use for rolls.</param>
	public CardRarityOdds(float initialValue, Rng rng)
		: base(initialValue, rng)
	{
	}

	/// <summary>
	/// Roll for what rarity of card should be generated.
	/// Using this will NOT modify the odds of future generated card rarities.
	/// Use for things like encounter rewards.
	/// </summary>
	/// <param name="type">Odds type to roll for.</param>
	/// <returns>What rarity a generated card should be.</returns>
	public CardRarity Roll(CardRarityOddsType type)
	{
		CardRarity cardRarity = RollWithoutChangingFutureOdds(type, (type == CardRarityOddsType.BossEncounter) ? 0f : base.CurrentValue);
		if (cardRarity == CardRarity.Rare)
		{
			base.CurrentValue = -0.05f;
		}
		else
		{
			base.CurrentValue = Math.Min(base.CurrentValue + RarityGrowth, 0.4f);
		}
		return cardRarity;
	}

	public CardRarity RollWithoutChangingFutureOdds(CardRarityOddsType oddsType)
	{
		return RollWithoutChangingFutureOdds(oddsType, base.CurrentValue);
	}

	/// <summary>
	/// Roll for what rarity of card should be generated.
	/// Using this will NOT modify the odds of future generated card rarities.
	/// Use for things like shops.
	/// </summary>
	/// <param name="type">Odds type to roll for.</param>
	/// <param name="offset">offset for rare odds</param>
	/// <returns>What rarity a generated card should be.</returns>
	public CardRarity RollWithoutChangingFutureOdds(CardRarityOddsType type, float offset)
	{
		float num = _rng.NextFloat();
		float num2 = GetBaseOdds(type, CardRarity.Rare) + offset;
		Log.Info($"Card rarity: Rolled {num}, need < {num2} for rare (offset = {offset})");
		if (num < num2)
		{
			return CardRarity.Rare;
		}
		if (num < GetBaseOdds(type, CardRarity.Uncommon) + num2)
		{
			return CardRarity.Uncommon;
		}
		return CardRarity.Common;
	}

	/// <summary>
	/// Roll for what rarity of card should be generated.
	/// Using this will use only the base odds for the given card source.
	/// Used for some events.
	/// </summary>
	/// <param name="type">Odds type to roll for.</param>
	/// <returns>What rarity a generated card should be.</returns>
	public CardRarity RollWithBaseOdds(CardRarityOddsType type)
	{
		float num = _rng.NextFloat();
		if (num < GetBaseOdds(type, CardRarity.Rare))
		{
			return CardRarity.Rare;
		}
		if (num < GetBaseOdds(type, CardRarity.Uncommon))
		{
			return CardRarity.Uncommon;
		}
		return CardRarity.Common;
	}

	private static float GetBaseOdds(CardRarityOddsType type, CardRarity rarity)
	{
		return type switch
		{
			CardRarityOddsType.EliteEncounter => rarity switch
			{
				CardRarity.Common => EliteCommonOdds, 
				CardRarity.Uncommon => 0.4f, 
				CardRarity.Rare => EliteRareOdds, 
				_ => throw new ArgumentOutOfRangeException("rarity"), 
			}, 
			CardRarityOddsType.BossEncounter => rarity switch
			{
				CardRarity.Common => 0f, 
				CardRarity.Uncommon => 0f, 
				CardRarity.Rare => 1f, 
				_ => throw new ArgumentOutOfRangeException("rarity"), 
			}, 
			CardRarityOddsType.Shop => rarity switch
			{
				CardRarity.Common => ShopCommonOdds, 
				CardRarity.Uncommon => 0.37f, 
				CardRarity.Rare => ShopRareOdds, 
				_ => throw new ArgumentOutOfRangeException("rarity"), 
			}, 
			CardRarityOddsType.RegularEncounter => rarity switch
			{
				CardRarity.Common => regularCommonOdds, 
				CardRarity.Uncommon => 0.37f, 
				CardRarity.Rare => RegularRareOdds, 
				_ => throw new ArgumentOutOfRangeException("rarity"), 
			}, 
			CardRarityOddsType.Uniform => rarity switch
			{
				CardRarity.Common => 0.33f, 
				CardRarity.Uncommon => 0.33f, 
				CardRarity.Rare => 0.33f, 
				_ => throw new ArgumentOutOfRangeException("rarity"), 
			}, 
			_ => throw new ArgumentOutOfRangeException("type"), 
		};
	}
}
