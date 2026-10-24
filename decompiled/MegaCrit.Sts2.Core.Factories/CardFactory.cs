using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Factories;

public static class CardFactory
{
	private static decimal UpgradedCardOddScaling => AscensionHelper.GetValueIfAscension(AscensionLevel.Scarcity, 0.125m, 0.25m);

	private static IEnumerable<CardModel> FilterForPlayerCount(IRunState runState, IEnumerable<CardModel> options)
	{
		if (runState.Players.Count > 1)
		{
			return options.Where((CardModel c) => c.MultiplayerConstraint != CardMultiplayerConstraint.SingleplayerOnly);
		}
		return options.Where((CardModel c) => c.MultiplayerConstraint != CardMultiplayerConstraint.MultiplayerOnly);
	}

	/// <summary>
	/// Creates a card to display to the player at the merchant. Takes in a card type, and rarity is rolled.
	/// </summary>
	/// <param name="player">The player for which we're creating cards to display.</param>
	/// <param name="options">The cards to pull from.</param>
	/// <param name="type">The card type to generate.</param>
	public static CardCreationResult CreateForMerchant(Player player, IEnumerable<CardModel> options, CardType type)
	{
		if (player.Character is Deprived)
		{
			throw new InvalidOperationException("Merchant inventory can't be generated for the test character. Update your test to use Ironclad.");
		}
		options = Hook.ModifyMerchantCardPool(player.RunState, player, options);
		options = options.Where((CardModel c) => c.Rarity != CardRarity.Basic);
		options = FilterForPlayerCount(player.RunState, options);
		CardModel[] optionsArr = options.ToArray();
		CardRarity rarity = Hook.ModifyMerchantCardRarity(player.RunState, player, player.PlayerOdds.CardRarity.RollWithoutChangingFutureOdds(CardRarityOddsType.Shop));
		CardRarity rarity2 = GetNextAllowedRarity(rarity, (CardRarity r) => optionsArr.Any((CardModel c) => c.Rarity == r && c.Type == type));
		if (rarity2 == CardRarity.None)
		{
			throw new InvalidOperationException($"Can't generate valid rarity for merchant card type {type} with card options: {string.Join(",", optionsArr.Select((CardModel c) => c.Id))}");
		}
		List<CardModel> items = optionsArr.Where((CardModel c) => c.Rarity == rarity2 && c.Type == type).ToList();
		CardModel cardModel = player.RunState.CreateCard(player.PlayerRng.Shops.NextItem(items), player);
		RollForUpgrade(player, cardModel, -999999999m);
		return new CardCreationResult(cardModel);
	}

	/// <summary>
	/// Creates a set of cards to display to the player at the merchant.
	/// The rarity of cards are affected by the source.
	/// </summary>
	/// <param name="player">The player for which we're creating cards to display.</param>
	/// <param name="options">The cards to pull from.</param>
	/// <param name="rarity">The card rarity to generate. This rarity may be modified by hooks.</param>
	public static CardCreationResult CreateForMerchant(Player player, IEnumerable<CardModel> options, CardRarity rarity)
	{
		options = Hook.ModifyMerchantCardPool(player.RunState, player, options);
		options = options.Where((CardModel c) => c.Rarity != CardRarity.Basic);
		options = FilterForPlayerCount(player.RunState, options);
		CardModel[] source = options.ToArray();
		CardRarity modifiedRarity = Hook.ModifyMerchantCardRarity(player.RunState, player, rarity);
		IEnumerable<CardModel> items = source.Where((CardModel c) => c.Rarity == modifiedRarity);
		CardModel cardModel = player.RunState.CreateCard(player.PlayerRng.Shops.NextItem(items), player);
		RollForUpgrade(player, cardModel, -999999999m);
		return new CardCreationResult(cardModel);
	}

	/// <summary>
	/// Creates a set of cards for the player to choose 1 from.
	/// The rarity of cards are affected by the source.
	/// </summary>
	/// <param name="player">The player for which we're creating rewards.</param>
	/// <param name="cardCount">How many choices to offer (usually 3).</param>
	/// <param name="options">The options to use when creating the cards.</param>
	public static IEnumerable<CardCreationResult> CreateForReward(Player player, int cardCount, CardCreationOptions options)
	{
		List<CardModel> list = new List<CardModel>();
		List<CardCreationResult> list2 = new List<CardCreationResult>();
		for (int i = 0; i < cardCount; i++)
		{
			CardModel cardModel = CreateForReward(player, list, options);
			list.Add(cardModel.CanonicalInstance);
			list2.Add(new CardCreationResult(cardModel));
			if (!options.Flags.HasFlag(CardCreationFlags.NoUpgradeRoll))
			{
				Rng rng = options.RngOverride ?? player.PlayerRng.Rewards;
				RollForUpgrade(player, cardModel, 0m, rng);
			}
		}
		if (!options.Flags.HasFlag(CardCreationFlags.NoModifyHooks) && Hook.TryModifyCardRewardOptions(player.RunState, player, list2, options, out List<AbstractModel> modifiers))
		{
			TaskHelper.RunSafely(Hook.AfterModifyingCardRewardOptions(player.RunState, modifiers));
		}
		return list2;
	}

	/// <summary>
	/// Get a set of distinct cards for use in combat generation effects like Attack Potion or Discovery.
	/// </summary>
	/// <param name="player">The owner of the newly created cards.</param>
	/// <param name="cards">Cards to choose from.</param>
	/// <param name="count">Number of cards to get.</param>
	/// <param name="rng">RNG to use.</param>
	/// <returns>Mutable cards owned by the player and created within its combat state, for combat generation effects.</returns>
	public static IEnumerable<CardModel> GetDistinctForCombat(Player player, IEnumerable<CardModel> cards, int count, Rng rng)
	{
		List<CardModel> list = TestRngInjector.ConsumeCombatCardGenerationOverride();
		if (list != null)
		{
			return list;
		}
		cards = FilterForPlayerCount(player.RunState, cards);
		return from c in FilterForCombat(cards).TakeRandom(count, rng)
			select player.Creature.CombatState.CreateCard(c, player);
	}

	/// <summary>
	/// Get a set of cards for use in combat generation effects like Calamity.
	/// Can include multiple of the same card, unlike <see cref="M:MegaCrit.Sts2.Core.Factories.CardFactory.GetDistinctForCombat(MegaCrit.Sts2.Core.Entities.Players.Player,System.Collections.Generic.IEnumerable{MegaCrit.Sts2.Core.Models.CardModel},System.Int32,MegaCrit.Sts2.Core.Random.Rng)" />.
	/// </summary>
	/// <param name="player">The owner of the newly created cards.</param>
	/// <param name="cards">Cards to choose from.</param>
	/// <param name="count">Number of cards to get.</param>
	/// <param name="rng">RNG to use.</param>
	/// <returns>Cards for combat generation effects.</returns>
	public static IEnumerable<CardModel> GetForCombat(Player player, IEnumerable<CardModel> cards, int count, Rng rng)
	{
		List<CardModel> options = FilterForCombat(cards).ToList();
		options = FilterForPlayerCount(player.RunState, options).ToList();
		List<CardModel> list = new List<CardModel>();
		for (int i = 0; i < count; i++)
		{
			CardModel canonicalCard = rng.NextItem(options);
			CardModel item = player.Creature.CombatState.CreateCard(canonicalCard, player);
			list.Add(item);
		}
		return list;
	}

	/// <summary>
	/// Filter out cards that should not be included in combat card generation effects like Attack Potion or Discovery.
	/// </summary>
	/// <param name="cards">Cards to filter.</param>
	/// <returns>Cards for combat generation effects.</returns>
	public static IEnumerable<CardModel> FilterForCombat(IEnumerable<CardModel> cards)
	{
		return cards.Where((CardModel c) => c.CanBeGeneratedInCombat && c.Rarity != CardRarity.Basic && c.Rarity != CardRarity.Ancient && c.Rarity != CardRarity.Event).Distinct();
	}

	/// <summary>
	/// Get the default set of cards that the specified card should be able to be transformed into.
	/// </summary>
	/// <param name="original">Card that will be transformed.</param>
	/// <param name="isInCombat">Whether the transformation is happening in combat.</param>
	/// <returns>The set of cards that the card can be transformed into.</returns>
	public static IEnumerable<CardModel> GetDefaultTransformationOptions(CardModel original, bool isInCombat)
	{
		CardPoolModel cardPoolModel = ((original.Type != CardType.Quest && original.Rarity != CardRarity.Event && original.Rarity != CardRarity.Ancient && original.Rarity != CardRarity.Token) ? original.Pool : ModelDb.CardPool<ColorlessCardPool>());
		IEnumerable<CardModel> unlockedCards = cardPoolModel.GetUnlockedCards(original.Owner.UnlockState, original.RunState.CardMultiplayerConstraint);
		return GetFilteredTransformationOptions(original, unlockedCards, isInCombat);
	}

	public static CardModel CreateRandomCardForTransform(CardModel original, bool isInCombat, Rng rng)
	{
		IEnumerable<CardModel> defaultTransformationOptions = GetDefaultTransformationOptions(original, isInCombat);
		return original.CardScope.CreateCard(rng.NextItem(defaultTransformationOptions), original.Owner);
	}

	public static CardModel CreateRandomCardForTransform(CardModel original, IEnumerable<CardModel> options, bool isInCombat, Rng rng)
	{
		CardModel[] filteredTransformationOptions = GetFilteredTransformationOptions(original, options, isInCombat);
		return original.CardScope.CreateCard(rng.NextItem(filteredTransformationOptions), original.Owner);
	}

	private static CardModel[] GetFilteredTransformationOptions(CardModel original, IEnumerable<CardModel> originalOptions, bool isInCombat)
	{
		IEnumerable<CardModel> source = originalOptions;
		CardRarity rarity = original.Rarity;
		if ((uint)(rarity - 8) > 1u)
		{
			source = source.Where(delegate(CardModel c)
			{
				CardRarity rarity2 = c.Rarity;
				return (uint)(rarity2 - 2) <= 2u;
			});
		}
		if (isInCombat)
		{
			source = source.Where((CardModel c) => c.CanBeGeneratedInCombat);
		}
		source = source.Where((CardModel c) => c.Id != original.Id).ToList();
		CardModel[] array = FilterForPlayerCount(original.Owner.RunState, source).ToArray();
		if (array.Length == 0)
		{
			throw new InvalidOperationException("All transformation options provided are invalid! Original options: " + string.Join(",", originalOptions));
		}
		return array;
	}

	private static CardModel CreateForReward(Player player, IEnumerable<CardModel> blacklist, CardCreationOptions options)
	{
		options = Hook.ModifyCardRewardCreationOptions(player.RunState, player, options);
		IEnumerable<CardModel> options2 = options.GetPossibleCards(player).Except(blacklist).ToList();
		options2 = FilterForPlayerCount(player.RunState, options2).ToArray();
		CardRarity? selectedRarity = null;
		IEnumerable<CardModel> items;
		if (options.RarityOdds == CardRarityOddsType.Uniform)
		{
			items = options2.Where((CardModel c) => c.Rarity != CardRarity.Basic && c.Rarity != CardRarity.Ancient);
		}
		else
		{
			HashSet<CardRarity> allowedRarities = options2.Select((CardModel c) => c.Rarity).ToHashSet();
			selectedRarity = RollForRarity(player, options.RarityOdds, options.Source, allowedRarities, options.Flags.HasFlag(CardCreationFlags.ForceRarityOddsChange));
			if (selectedRarity == CardRarity.None)
			{
				throw new InvalidOperationException($"Tried to create a card for a reward, but we couldn't generate a valid rarity! Odds: {options.RarityOdds} Card pool: {string.Join(",", options2)}, blacklist: {string.Join(",", blacklist)}");
			}
			items = options2.Where((CardModel card) => card.Rarity == selectedRarity);
		}
		Rng rng = options.RngOverride ?? player.PlayerRng.Rewards;
		CardModel cardModel = rng.NextItem(items);
		if (cardModel == null)
		{
			throw new InvalidOperationException($"Tried to create a card for a reward, but we couldn't generate a valid card! Selected rarity: {selectedRarity}, card pool: {string.Join(",", options2)}, blacklist: {string.Join(",", blacklist)}, odds: {options.RarityOdds}");
		}
		return player.RunState.CreateCard(cardModel, player);
	}

	private static CardRarity RollForRarity(Player player, CardRarityOddsType rollMethod, CardCreationSource source, HashSet<CardRarity> allowedRarities, bool forceRarityOddsChange)
	{
		bool flag = forceRarityOddsChange;
		bool flag2 = flag;
		if (!flag2)
		{
			bool flag3 = source == CardCreationSource.Encounter;
			bool flag4 = flag3;
			if (flag4)
			{
				bool flag5 = (uint)(rollMethod - 1) <= 2u;
				flag4 = flag5;
			}
			flag2 = flag4;
		}
		CardRarity rarity = ((!flag2) ? player.PlayerOdds.CardRarity.RollWithBaseOdds(rollMethod) : player.PlayerOdds.CardRarity.Roll(rollMethod));
		return GetNextAllowedRarity(rarity, allowedRarities.Contains);
	}

	private static CardRarity GetNextAllowedRarity(CardRarity rarity, Func<CardRarity, bool> isAllowed)
	{
		int num = 1;
		List<CardRarity> list = new List<CardRarity>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<CardRarity> span = CollectionsMarshal.AsSpan(list);
		int index = 0;
		span[index] = rarity;
		List<CardRarity> list2 = list;
		while (!isAllowed(rarity) && rarity != CardRarity.None)
		{
			rarity = rarity.GetNextHighestRarityWithWrapping();
			if (list2.Contains(rarity))
			{
				return CardRarity.None;
			}
		}
		return rarity;
	}

	private static void RollForUpgrade(Player player, CardModel card, decimal baseChance)
	{
		RollForUpgrade(player, card, baseChance, player.PlayerRng.Rewards);
	}

	private static void RollForUpgrade(Player player, CardModel card, decimal baseChance, Rng rng)
	{
		decimal num = (decimal)rng.NextFloat();
		if (card.IsUpgradable)
		{
			decimal originalOdds = baseChance;
			if (card.Rarity != CardRarity.Rare)
			{
				int currentActIndex = player.RunState.CurrentActIndex;
				originalOdds += (decimal)currentActIndex * UpgradedCardOddScaling;
			}
			originalOdds = Hook.ModifyCardRewardUpgradeOdds(player.RunState, player, card, originalOdds);
			if (num <= originalOdds)
			{
				CardCmd.Upgrade(card);
			}
		}
	}
}
