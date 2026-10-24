using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class ScrollBoxes : RelicModel
{
	private const int _clawBundleChancePercent = 1;

	public override RelicRarity Rarity => RelicRarity.Ancient;

	public override bool IsAllowedAtNeow(Player player)
	{
		if (CanGenerateBundles(player))
		{
			return base.IsAllowedAtNeow(player);
		}
		return false;
	}

	public override async Task AfterObtained()
	{
		List<IReadOnlyList<CardModel>> list = GenerateRandomBundles(base.Owner);
		List<IReadOnlyList<CardModel>> list2 = new List<IReadOnlyList<CardModel>>();
		foreach (IReadOnlyList<CardModel> item in list)
		{
			list2.Add(item.Select((CardModel c) => base.Owner.RunState.CreateCard(c, base.Owner)).ToList());
		}
		foreach (CardModel item2 in await CardSelectCmd.FromChooseABundleScreen(base.Owner, list2))
		{
			await CardPileCmd.Add(item2, PileType.Deck);
		}
	}

	/// <summary>
	/// Checks if random bundles can be generated for the given player.
	/// Requires at least 4 unlocked commons and 2 unlocked uncommons.
	/// </summary>
	public static bool CanGenerateBundles(Player player)
	{
		IEnumerable<CardModel> unlockedCards = GetCardPool(player.Character).GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint);
		int num = unlockedCards.Count((CardModel c) => c.Rarity == CardRarity.Common);
		int num2 = unlockedCards.Count((CardModel c) => c.Rarity == CardRarity.Uncommon);
		if (num >= 4)
		{
			return num2 >= 2;
		}
		return false;
	}

	/// <summary>
	/// Generates 2 random bundles for the player. Each bundle contains 2 commons and 1 uncommon.
	/// All 6 cards across both bundles are unique.
	/// For Defect, each bundle has a 1% chance to be 3x Claw instead.
	/// </summary>
	public static List<IReadOnlyList<CardModel>> GenerateRandomBundles(Player player)
	{
		Rng rewards = player.PlayerRng.Rewards;
		bool flag = player.Character is Defect;
		CardPoolModel cardPool = GetCardPool(player.Character);
		CardCreationOptions options = CardCreationOptions.ForNonCombatWithUniformOdds(new global::_003C_003Ez__ReadOnlySingleElementList<CardPoolModel>(cardPool), (CardModel c) => c.Rarity == CardRarity.Common).WithFlags(CardCreationFlags.NoRarityModification);
		options = Hook.ModifyCardRewardCreationOptions(player.RunState, player, options);
		CardCreationOptions options2 = CardCreationOptions.ForNonCombatWithUniformOdds(new global::_003C_003Ez__ReadOnlySingleElementList<CardPoolModel>(cardPool), (CardModel c) => c.Rarity == CardRarity.Uncommon).WithFlags(CardCreationFlags.NoRarityModification);
		options2 = Hook.ModifyCardRewardCreationOptions(player.RunState, player, options2);
		List<CardModel> source = options.GetPossibleCards(player).ToList();
		List<CardModel> source2 = options2.GetPossibleCards(player).ToList();
		List<IReadOnlyList<CardModel>> list = new List<IReadOnlyList<CardModel>>();
		HashSet<ModelId> usedCardIds = new HashSet<ModelId>();
		for (int num = 0; num < 2; num++)
		{
			if (flag && rewards.NextInt(100) < 1)
			{
				CardModel cardModel = ModelDb.Card<Claw>();
				list.Add(new global::_003C_003Ez__ReadOnlyArray<CardModel>(new CardModel[3] { cardModel, cardModel, cardModel }));
				continue;
			}
			List<CardModel> list2 = new List<CardModel>();
			List<CardModel> list3 = source.Where((CardModel c) => !usedCardIds.Contains(c.Id)).ToList();
			for (int num2 = 0; num2 < 2; num2++)
			{
				CardModel cardModel2 = rewards.NextItem(list3);
				list2.Add(cardModel2);
				usedCardIds.Add(cardModel2.Id);
				list3.Remove(cardModel2);
			}
			List<CardModel> items = source2.Where((CardModel c) => !usedCardIds.Contains(c.Id)).ToList();
			CardModel cardModel3 = rewards.NextItem(items);
			list2.Add(cardModel3);
			usedCardIds.Add(cardModel3.Id);
			list.Add(list2);
		}
		return list;
	}

	private static CardPoolModel GetCardPool(CharacterModel character)
	{
		if (TestMode.IsOn && character is Deprived)
		{
			return ModelDb.Character<Ironclad>().CardPool;
		}
		return character.CardPool;
	}
}
