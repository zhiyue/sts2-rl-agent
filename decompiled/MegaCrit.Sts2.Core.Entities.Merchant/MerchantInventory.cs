using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace MegaCrit.Sts2.Core.Entities.Merchant;

public class MerchantInventory
{
	private static readonly CardType[] _coloredCardTypes = new CardType[5]
	{
		CardType.Attack,
		CardType.Attack,
		CardType.Skill,
		CardType.Skill,
		CardType.Power
	};

	private static readonly CardRarity[] _colorlessCardRarities = new CardRarity[2]
	{
		CardRarity.Uncommon,
		CardRarity.Rare
	};

	private readonly List<MerchantCardEntry> _characterCardEntries = new List<MerchantCardEntry>();

	private readonly List<MerchantCardEntry> _colorlessCardEntries = new List<MerchantCardEntry>();

	private readonly List<MerchantRelicEntry> _relicEntries = new List<MerchantRelicEntry>();

	private readonly List<MerchantPotionEntry> _potionEntries = new List<MerchantPotionEntry>();

	public IReadOnlyList<MerchantCardEntry> CharacterCardEntries => _characterCardEntries;

	public IReadOnlyList<MerchantCardEntry> ColorlessCardEntries => _colorlessCardEntries;

	public IReadOnlyList<MerchantRelicEntry> RelicEntries => _relicEntries;

	public IReadOnlyList<MerchantPotionEntry> PotionEntries => _potionEntries;

	public MerchantCardRemovalEntry? CardRemovalEntry { get; private set; }

	public Player Player { get; }

	public IEnumerable<MerchantEntry> AllEntries
	{
		get
		{
			IEnumerable<MerchantEntry>[] obj = new IEnumerable<MerchantEntry>[4] { CardEntries, RelicEntries, PotionEntries, null };
			IEnumerable<MerchantEntry> enumerable2;
			if (CardRemovalEntry == null)
			{
				IEnumerable<MerchantEntry> enumerable = Array.Empty<MerchantEntry>();
				enumerable2 = enumerable;
			}
			else
			{
				IEnumerable<MerchantEntry> enumerable = new global::_003C_003Ez__ReadOnlySingleElementList<MerchantEntry>(CardRemovalEntry);
				enumerable2 = enumerable;
			}
			obj[3] = enumerable2;
			return obj.SelectMany((IEnumerable<MerchantEntry> e) => e);
		}
	}

	public IEnumerable<MerchantCardEntry> CardEntries => CharacterCardEntries.Concat(ColorlessCardEntries);

	public MerchantInventory(Player player)
	{
		Player = player;
	}

	public static MerchantInventory CreateForNormalMerchant(Player player)
	{
		MerchantInventory merchantInventory = new MerchantInventory(player);
		merchantInventory.PopulateCharacterCardEntries();
		merchantInventory.PopulateColorlessCardEntries();
		merchantInventory.PopulateRelicEntries();
		merchantInventory.PopulatePotionEntries();
		merchantInventory.CardRemovalEntry = new MerchantCardRemovalEntry(player);
		foreach (MerchantEntry allEntry in merchantInventory.AllEntries)
		{
			allEntry.PurchaseCompleted += merchantInventory.UpdateEntries;
		}
		return merchantInventory;
	}

	public void AddRelicEntry(MerchantRelicEntry entry)
	{
		_relicEntries.Add(entry);
	}

	private void PopulateCharacterCardEntries()
	{
		int num = Player.PlayerRng.Shops.NextInt(_coloredCardTypes.Length);
		List<CardModel> cardPool = Player.Character.CardPool.GetUnlockedCards(Player.UnlockState, Player.RunState.CardMultiplayerConstraint).ToList();
		for (int i = 0; i < _coloredCardTypes.Length; i++)
		{
			MerchantCardEntry merchantCardEntry = new MerchantCardEntry(Player, this, cardPool, _coloredCardTypes[i]);
			merchantCardEntry.Populate();
			_characterCardEntries.Add(merchantCardEntry);
			if (num == i)
			{
				merchantCardEntry.SetOnSale();
			}
		}
	}

	private void PopulateColorlessCardEntries()
	{
		List<CardModel> cardPool = ModelDb.CardPool<ColorlessCardPool>().GetUnlockedCards(Player.UnlockState, Player.RunState.CardMultiplayerConstraint).ToList();
		CardRarity[] colorlessCardRarities = _colorlessCardRarities;
		foreach (CardRarity cardRarity in colorlessCardRarities)
		{
			MerchantCardEntry merchantCardEntry = new MerchantCardEntry(Player, this, cardPool, cardRarity);
			merchantCardEntry.Populate();
			_colorlessCardEntries.Add(merchantCardEntry);
		}
	}

	private void PopulateRelicEntries()
	{
		RelicRarity[] array = new RelicRarity[3]
		{
			RelicFactory.RollRarity(Player),
			RelicFactory.RollRarity(Player),
			RelicRarity.Shop
		};
		RelicRarity[] array2 = array;
		foreach (RelicRarity rarity in array2)
		{
			AddRelicEntry(new MerchantRelicEntry(rarity, Player));
		}
	}

	private void PopulatePotionEntries()
	{
		List<PotionModel> list = PotionFactory.CreateRandomPotionsOutOfCombat(Player, 3, Player.PlayerRng.Shops);
		foreach (PotionModel item in list)
		{
			_potionEntries.Add(new MerchantPotionEntry(item.ToMutable(), Player));
		}
	}

	private void UpdateEntries(PurchaseStatus _, MerchantEntry __)
	{
		foreach (MerchantEntry allEntry in AllEntries)
		{
			allEntry.OnMerchantInventoryUpdated();
		}
	}
}
