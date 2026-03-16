using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class WelcomeToWongos : EventModel
{
	private const int _wongoPointsForBadge = 2000;

	private const string _bargainBinCostKey = "BargainBinCost";

	private const string _featuredItemCostKey = "FeaturedItemCost";

	private const string _mysteryBoxCostKey = "MysteryBoxCost";

	private const string _mysteryBoxRelicCountKey = "MysteryBoxRelicCount";

	private const string _mysteryBoxCombatCountKey = "MysteryBoxCombatCount";

	private const string _wongoPointAmountKey = "WongoPointAmount";

	private const string _remainingWongoPointAmountKey = "RemainingWongoPointAmount";

	private const string _totalWongoBadgeAmountKey = "TotalWongoBadgeAmount";

	private const string _randomRelicKey = "RandomRelic";

	private RelicModel? _featuredItem;

	private RelicModel? FeaturedItem
	{
		get
		{
			return _featuredItem;
		}
		set
		{
			AssertMutable();
			_featuredItem = value;
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[9]
	{
		new DynamicVar("BargainBinCost", 100m),
		new DynamicVar("MysteryBoxCost", 300m),
		new DynamicVar("FeaturedItemCost", 200m),
		new DynamicVar("MysteryBoxRelicCount", 3m),
		new DynamicVar("MysteryBoxCombatCount", 5m),
		new DynamicVar("WongoPointAmount", 0m),
		new DynamicVar("RemainingWongoPointAmount", 0m),
		new DynamicVar("TotalWongoBadgeAmount", 0m),
		new StringVar("RandomRelic")
	});

	public override bool IsAllowed(RunState runState)
	{
		if (runState.CurrentActIndex == 1)
		{
			return runState.Players.All((Player p) => p.Gold >= 100);
		}
		return false;
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		Player owner = base.Owner;
		FeaturedItem = RelicFactory.PullNextRelicFromFront(owner, RelicRarity.Rare);
		((StringVar)base.DynamicVars["RandomRelic"]).StringValue = FeaturedItem.Title.GetFormattedText();
		List<EventOption> list = new List<EventOption>();
		if ((decimal)owner.Gold >= base.DynamicVars["BargainBinCost"].BaseValue)
		{
			list.Add(new EventOption(this, BuyBargainBin, "WELCOME_TO_WONGOS.pages.INITIAL.options.BARGAIN_BIN"));
		}
		else
		{
			list.Add(new EventOption(this, null, "WELCOME_TO_WONGOS.pages.INITIAL.options.BARGAIN_BIN_LOCKED"));
		}
		if ((decimal)owner.Gold >= base.DynamicVars["FeaturedItemCost"].BaseValue)
		{
			list.Add(new EventOption(this, BuyFeaturedItem, "WELCOME_TO_WONGOS.pages.INITIAL.options.FEATURED_ITEM", FeaturedItem.HoverTips));
		}
		else
		{
			list.Add(new EventOption(this, null, "WELCOME_TO_WONGOS.pages.INITIAL.options.FEATURED_ITEM_LOCKED"));
		}
		if ((decimal)owner.Gold >= base.DynamicVars["MysteryBoxCost"].BaseValue)
		{
			list.Add(new EventOption(this, BuyMysteryBox, "WELCOME_TO_WONGOS.pages.INITIAL.options.MYSTERY_BOX"));
		}
		else
		{
			list.Add(new EventOption(this, null, "WELCOME_TO_WONGOS.pages.INITIAL.options.MYSTERY_BOX_LOCKED"));
		}
		list.Add(new EventOption(this, Leave, "WELCOME_TO_WONGOS.pages.INITIAL.options.LEAVE"));
		return list;
	}

	private async Task<LocString> CheckObtainWongoBadge(int pointsEarned)
	{
		int wongoPoints = SaveManager.Instance.Progress.WongoPoints;
		int num = wongoPoints % 2000;
		int num2 = num + pointsEarned;
		int num3 = wongoPoints + pointsEarned;
		base.DynamicVars["WongoPointAmount"].BaseValue = num2;
		base.DynamicVars["RemainingWongoPointAmount"].BaseValue = 2000 - num2;
		base.DynamicVars["TotalWongoBadgeAmount"].BaseValue = num3 / 2000;
		base.Owner.ExtraFields.WongoPoints = pointsEarned;
		if (num2 >= 2000)
		{
			await RelicCmd.Obtain<WongoCustomerAppreciationBadge>(base.Owner);
			return L10NLookup("WELCOME_TO_WONGOS.pages.AFTER_BUY_RECEIVE_BADGE.description");
		}
		if (base.DynamicVars["TotalWongoBadgeAmount"].BaseValue > 0m)
		{
			return L10NLookup("WELCOME_TO_WONGOS.pages.AFTER_BUY_BADGE_COUNTER.description");
		}
		return L10NLookup("WELCOME_TO_WONGOS.pages.AFTER_BUY.description");
	}

	private async Task BuyBargainBin()
	{
		await PlayerCmd.LoseGold(base.DynamicVars["BargainBinCost"].BaseValue, base.Owner, GoldLossType.Spent);
		RelicModel relic = RelicFactory.PullNextRelicFromFront(base.Owner, RelicRarity.Common).ToMutable();
		await RelicCmd.Obtain(relic, base.Owner);
		SetEventFinished(await CheckObtainWongoBadge(32));
	}

	private async Task BuyMysteryBox()
	{
		await PlayerCmd.LoseGold(base.DynamicVars["MysteryBoxCost"].BaseValue, base.Owner, GoldLossType.Spent);
		await RelicCmd.Obtain<WongosMysteryTicket>(base.Owner);
		SetEventFinished(await CheckObtainWongoBadge(8));
	}

	private async Task BuyFeaturedItem()
	{
		await PlayerCmd.LoseGold(base.DynamicVars["FeaturedItemCost"].BaseValue, base.Owner, GoldLossType.Spent);
		await RelicCmd.Obtain(FeaturedItem.ToMutable(), base.Owner);
		SetEventFinished(await CheckObtainWongoBadge(16));
	}

	private async Task Leave()
	{
		Player owner = base.Owner;
		CardModel cardModel = base.Rng.NextItem(owner.Deck.Cards.Where((CardModel c) => c.IsUpgraded));
		if (cardModel != null)
		{
			CardCmd.Downgrade(cardModel);
			CardCmd.Preview(cardModel);
			await Cmd.CustomScaledWait(0.5f, 1.2f);
		}
		SetEventFinished(L10NLookup("WELCOME_TO_WONGOS.pages.LEAVE.description"));
	}
}
