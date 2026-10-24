using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Events.Custom;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class FakeMerchant : EventModel
{
	public const int relicCost = 50;

	/// <summary>
	/// Note: don't add <see cref="T:MegaCrit.Sts2.Core.Models.Relics.FakeMerchantsRug" /> to this list, it's a combat reward, not a relic you can buy.
	/// </summary>
	private static readonly RelicModel[] _inventoryRelics = new RelicModel[9]
	{
		ModelDb.Relic<FakeAnchor>(),
		ModelDb.Relic<FakeBloodVial>(),
		ModelDb.Relic<FakeHappyFlower>(),
		ModelDb.Relic<FakeLeesWaffle>(),
		ModelDb.Relic<FakeMango>(),
		ModelDb.Relic<FakeOrichalcum>(),
		ModelDb.Relic<FakeSneckoEye>(),
		ModelDb.Relic<FakeStrikeDummy>(),
		ModelDb.Relic<FakeVenerableTeaSet>()
	};

	private static MerchantDialogueSet? _dialogue;

	private MerchantInventory? _inventory;

	private bool _startedFight;

	public static MerchantDialogueSet Dialogue
	{
		get
		{
			if (_dialogue != null)
			{
				return _dialogue;
			}
			LocTable table = LocManager.Instance.GetTable("events");
			string keyPrefix = StringHelper.Slugify("FakeMerchant") + ".talk.";
			IReadOnlyList<LocString> locStringsWithPrefix = table.GetLocStringsWithPrefix(keyPrefix);
			_dialogue = MerchantDialogueSet.CreateFromLocStrings(locStringsWithPrefix);
			return _dialogue;
		}
	}

	public override EventLayoutType LayoutType => EventLayoutType.Custom;

	public override bool IsShared => true;

	public MerchantInventory Inventory
	{
		get
		{
			return _inventory;
		}
		private set
		{
			AssertMutable();
			_inventory = value;
		}
	}

	public bool StartedFight
	{
		get
		{
			return _startedFight;
		}
		private set
		{
			AssertMutable();
			_startedFight = value;
		}
	}

	public override IEnumerable<LocString> GameInfoOptions => Array.Empty<LocString>();

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return Array.Empty<EventOption>();
	}

	public override bool IsAllowed(IRunState runState)
	{
		if (runState.CurrentActIndex < 1)
		{
			return false;
		}
		if (runState.Players.Count > 1)
		{
			return false;
		}
		return runState.Players.All((Player player) => player.Gold >= 100 || player.Potions.Any((PotionModel potion) => potion is FoulPotion));
	}

	protected override Task BeforeEventStarted(bool isPreFinished)
	{
		Inventory = new MerchantInventory(base.Owner);
		List<RelicModel> list = _inventoryRelics.ToList().UnstableShuffle(base.Rng).Take(6)
			.ToList();
		foreach (RelicModel item in list)
		{
			MerchantRelicEntry entry = new MerchantRelicEntry(item.ToMutable(), base.Owner);
			Inventory.AddRelicEntry(entry);
		}
		return Task.CompletedTask;
	}

	public async Task FoulPotionThrown(FoulPotion potion)
	{
		if (LocalContext.IsMine(this) && base.Node is NFakeMerchant nFakeMerchant)
		{
			await nFakeMerchant.FoulPotionThrown();
		}
		StartedFight = true;
		List<Reward> list = new List<Reward>();
		foreach (Player player in base.Owner.RunState.Players)
		{
			list.Add(new RelicReward(ModelDb.Relic<FakeMerchantsRug>().ToMutable(), player));
			FakeMerchant fakeMerchant = (FakeMerchant)RunManager.Instance.EventSynchronizer.GetEventForPlayer(player);
			foreach (MerchantRelicEntry relicEntry in fakeMerchant.Inventory.RelicEntries)
			{
				if (relicEntry.IsStocked || base.Owner.RunState.Players.Count > 1)
				{
					list.Add(new RelicReward(relicEntry.Model, player));
				}
			}
		}
		EnterCombatWithoutExitingEvent<FakeMerchantEventEncounter>(list, shouldResumeAfterCombat: false);
	}
}
