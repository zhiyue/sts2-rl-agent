using System;
using System.Collections.Generic;
using System.Linq;
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

	public static CardCreationResult CreateForMerchant(Player player, IEnumerable<CardModel> options, CardType type)
	{
		if (player.Character is Deprived)
		{
			throw new InvalidOperationException("Merchant inventory can't be generated for the test character. Update your test to use Ironclad.");
		}
		options = Hook.ModifyMerchantCardPool(player.RunState, player, options);
		options = options.Where((CardModel c) => c.Rarity != CardRarity.Basic);
		options = FilterForPlayerCount(player.RunState, options);
		CardModel[] source = options.ToArray();
		CardRarity rolledRarity = Hook.ModifyMerchantCardRarity(player.RunState, player, player.PlayerOdds.CardRarity.RollWithoutChangingFutureOdds(CardRarityOddsType.Shop));
		List<CardModel> list = source.Where((CardModel c) => c.Rarity == rolledRarity && c.Type == type).ToList();
		while (list.Count == 0)
		{
			rolledRarity = rolledRarity.GetNextHighestRarity();
			if (rolledRarity == CardRarity.None)
			{
				throw new InvalidOperationException("Can't generate a valid rarity for the merchant card options passed.");
			}
			list = source.Where((CardModel c) => c.Rarity == rolledRarity && c.Type == type).ToList();
		}
		CardModel cardModel = player.RunState.CreateCard(player.PlayerRng.Shops.NextItem(list), player);
		RollForUpgrade(player, cardModel, -999999999m);
		return new CardCreationResult(cardModel);
	}

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

	public static IEnumerable<CardModel> GetDistinctForCombat(Player player, IEnumerable<CardModel> cards, int count, Rng rng)
	{
		cards = FilterForPlayerCount(player.RunState, cards);
		return from c in FilterForCombat(cards).TakeRandom(count, rng)
			select player.Creature.CombatState.CreateCard(c, player);
	}

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

	public static IEnumerable<CardModel> FilterForCombat(IEnumerable<CardModel> cards)
	{
		return cards.Where((CardModel c) => c.CanBeGeneratedInCombat && c.Rarity != CardRarity.Basic && c.Rarity != CardRarity.Ancient && c.Rarity != CardRarity.Event).Distinct();
	}

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
		CardRarity cardRarity = ((!flag2) ? player.PlayerOdds.CardRarity.RollWithBaseOdds(rollMethod) : player.PlayerOdds.CardRarity.Roll(rollMethod));
		while (!allowedRarities.Contains(cardRarity) && cardRarity != CardRarity.None)
		{
			cardRarity = cardRarity.GetNextHighestRarity();
		}
		return cardRarity;
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
