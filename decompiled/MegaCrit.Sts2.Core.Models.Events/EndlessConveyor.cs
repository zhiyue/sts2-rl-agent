using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class EndlessConveyor : EventModel
{
	private record struct Dish
	{
		public readonly string id;

		public readonly LocString title;

		public readonly string optionKey;

		public readonly IEnumerable<IHoverTip> hoverTips;

		public readonly float weight;

		public readonly Func<Task> action;

		public Dish(string id, Func<Task> action, IEnumerable<IHoverTip> hoverTips, float weight)
		{
			this.id = id;
			title = new LocString("events", "ENDLESS_CONVEYOR.DISHES." + id + ".title");
			optionKey = "ENDLESS_CONVEYOR.pages.ALL.options." + id;
			this.action = action;
			this.hoverTips = hoverTips;
			this.weight = weight;
		}
	}

	private const string _currentDishTitleKey = "CurrentDishTitle";

	private const string _lastDishTitleKey = "LastDishTitle";

	private const string _goldenFyshGoldKey = "GoldenFyshGold";

	private const string _clamRollHealKey = "ClamRollHeal";

	private const string _caviarMaxHpKey = "CaviarMaxHp";

	private string _lastDishId = "";

	private int _numOfGrabs;

	private Dish _currentDish;

	private int NumOfGrabs
	{
		get
		{
			return _numOfGrabs;
		}
		set
		{
			AssertMutable();
			_numOfGrabs = value;
		}
	}

	private Dish CurrentDish
	{
		get
		{
			return _currentDish;
		}
		set
		{
			AssertMutable();
			_currentDish = value;
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[6]
	{
		new GoldVar(35),
		new GoldVar("GoldenFyshGold", 75),
		new HealVar("ClamRollHeal", 10m),
		new MaxHpVar("CaviarMaxHp", 4m),
		new StringVar("CurrentDishTitle"),
		new StringVar("LastDishTitle")
	});

	public override bool IsAllowed(RunState runState)
	{
		return runState.Players.All((Player p) => p.Gold >= 105);
	}

	public override void CalculateVars()
	{
		RollDish();
		((StringVar)base.DynamicVars["CurrentDishTitle"]).StringValue = CurrentDish.title.GetFormattedText();
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[2]
		{
			GenerateGrabSomethingOffTheBeltOption(),
			new EventOption(this, ObserveChef, "ENDLESS_CONVEYOR.pages.INITIAL.options.OBSERVE_CHEF")
		});
	}

	private async Task GrabSomethingOffTheBelt()
	{
		if (_currentDish.id != "GOLDEN_FYSH")
		{
			await PlayerCmd.LoseGold(base.DynamicVars.Gold.IntValue, base.Owner, GoldLossType.Spent);
		}
		await CurrentDish.action();
		RollDish();
		((StringVar)base.DynamicVars["LastDishTitle"]).StringValue = ((StringVar)base.DynamicVars["CurrentDishTitle"]).StringValue;
		((StringVar)base.DynamicVars["CurrentDishTitle"]).StringValue = CurrentDish.title.GetFormattedText();
		SetEventState(L10NLookup("ENDLESS_CONVEYOR.pages.GRAB_SOMETHING_OFF_THE_BELT.description"), new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[2]
		{
			GenerateGrabSomethingOffTheBeltOption(),
			new EventOption(this, Leave, "ENDLESS_CONVEYOR.pages.GRAB_SOMETHING_OFF_THE_BELT.options.LEAVE")
		}));
	}

	private EventOption GenerateGrabSomethingOffTheBeltOption()
	{
		if (base.Owner.Gold >= base.DynamicVars.Gold.IntValue)
		{
			return new EventOption(this, GrabSomethingOffTheBelt, _currentDish.optionKey, _currentDish.hoverTips);
		}
		return new EventOption(this, null, "ENDLESS_CONVEYOR.pages.ALL.options.LOCKED");
	}

	private async Task ClamRoll()
	{
		await CreatureCmd.Heal(base.Owner.Creature, base.DynamicVars["ClamRollHeal"].IntValue);
	}

	private async Task Caviar()
	{
		await CreatureCmd.GainMaxHp(base.Owner.Creature, base.DynamicVars["CaviarMaxHp"].IntValue);
	}

	private async Task SuspiciousCondiment()
	{
		IEnumerable<PotionModel> items = base.Owner.Character.PotionPool.GetUnlockedPotions(base.Owner.UnlockState).Concat(ModelDb.PotionPool<SharedPotionPool>().GetUnlockedPotions(base.Owner.UnlockState));
		PotionModel potionModel = base.Owner.PlayerRng.Rewards.NextItem(items);
		if (potionModel != null)
		{
			await RewardsCmd.OfferCustom(base.Owner, new List<Reward>(1)
			{
				new PotionReward(potionModel.ToMutable(), base.Owner)
			});
		}
	}

	private async Task JellyLiver()
	{
		CardModel cardModel = (await CardSelectCmd.FromDeckForTransformation(base.Owner, new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 1))).FirstOrDefault();
		if (cardModel != null)
		{
			await CardCmd.TransformToRandom(cardModel, base.Rng, CardPreviewStyle.EventLayout);
		}
	}

	private async Task SeapunkSalad()
	{
		CardModel card = base.Owner.RunState.CreateCard<FeedingFrenzy>(base.Owner);
		CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck), 1.2f, CardPreviewStyle.EventLayout);
	}

	private async Task FriedEel()
	{
		CardCreationOptions options = CardCreationOptions.ForNonCombatWithDefaultOdds(new global::_003C_003Ez__ReadOnlySingleElementList<CardPoolModel>(ModelDb.CardPool<ColorlessCardPool>()));
		CardModel card = CardFactory.CreateForReward(base.Owner, 1, options).First().Card;
		CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck), 1.2f, CardPreviewStyle.EventLayout);
	}

	private async Task GoldenFysh()
	{
		await PlayerCmd.GainGold(base.DynamicVars["GoldenFyshGold"].BaseValue, base.Owner);
	}

	private Task SpicySnappy()
	{
		List<CardModel> list = PileType.Deck.GetPile(base.Owner).Cards.Where((CardModel c) => c.IsUpgradable).ToList();
		if (list.Count != 0)
		{
			CardModel card = base.Rng.NextItem(list);
			CardCmd.Upgrade(card);
		}
		return Task.CompletedTask;
	}

	private void RollDish()
	{
		NumOfGrabs++;
		if (NumOfGrabs % 5 == 0)
		{
			_lastDishId = "SEAPUNK_SALAD";
			_currentDish = new Dish("SEAPUNK_SALAD", SeapunkSalad, HoverTipFactory.FromCardWithCardHoverTips<FeedingFrenzy>(), 0f);
			return;
		}
		int num = 4;
		List<Dish> list = new List<Dish>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<Dish> span = CollectionsMarshal.AsSpan(list);
		int num2 = 0;
		span[num2] = new Dish("CAVIAR", Caviar, Array.Empty<IHoverTip>(), 6f);
		num2++;
		span[num2] = new Dish("SPICY_SNAPPY", SpicySnappy, Array.Empty<IHoverTip>(), 3f);
		num2++;
		span[num2] = new Dish("JELLY_LIVER", JellyLiver, new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.Static(StaticHoverTip.Transform)), 3f);
		num2++;
		span[num2] = new Dish("FRIED_EEL", FriedEel, Array.Empty<IHoverTip>(), 3f);
		List<Dish> list2 = list;
		if (base.Owner.HasOpenPotionSlots)
		{
			list2.Add(new Dish("SUSPICIOUS_CONDIMENT", SuspiciousCondiment, Array.Empty<IHoverTip>(), 3f));
		}
		if (base.Owner.Creature.CurrentHp != base.Owner.Creature.MaxHp)
		{
			list2.Add(new Dish("CLAM_ROLL", ClamRoll, Array.Empty<IHoverTip>(), 6f));
		}
		if (NumOfGrabs > 1)
		{
			list2.Add(new Dish("GOLDEN_FYSH", GoldenFysh, Array.Empty<IHoverTip>(), 1f));
		}
		list2.RemoveAll((Dish d) => d.id == _lastDishId);
		float num3 = 0f;
		foreach (Dish item in list2)
		{
			num3 += item.weight;
		}
		float num4 = base.Rng.NextFloat() * num3;
		float num5 = 0f;
		foreach (Dish item2 in list2)
		{
			num5 += item2.weight;
			if (num4 < num5)
			{
				_lastDishId = item2.id;
				_currentDish = item2;
				break;
			}
		}
	}

	private Task ObserveChef()
	{
		IEnumerable<CardModel> enumerable = base.Owner.Deck.Cards.Where((CardModel c) => c.IsUpgradable);
		IEnumerable<CardModel> enumerable2 = (enumerable as CardModel[]) ?? enumerable.ToArray();
		if (enumerable2.Any())
		{
			CardCmd.Upgrade(base.Rng.NextItem(enumerable2));
		}
		SetEventFinished(L10NLookup("ENDLESS_CONVEYOR.pages.OBSERVE_CHEF.description"));
		return Task.CompletedTask;
	}

	private Task Leave()
	{
		SetEventFinished(L10NLookup("ENDLESS_CONVEYOR.pages.LEAVE.description"));
		return Task.CompletedTask;
	}
}
