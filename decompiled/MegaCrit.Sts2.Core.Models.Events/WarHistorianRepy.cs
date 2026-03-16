using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class WarHistorianRepy : EventModel
{
	public override bool IsShared => true;

	public override bool IsAllowed(RunState runState)
	{
		return false;
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[2]
		{
			new EventOption(this, UnlockCage, "WAR_HISTORIAN_REPY.pages.INITIAL.options.UNLOCK_CAGE", HoverTipFactory.FromRelic<HistoryCourse>().Concat(new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromCard<LanternKey>()))),
			new EventOption(this, UnlockChest, "WAR_HISTORIAN_REPY.pages.INITIAL.options.UNLOCK_CHEST", HoverTipFactory.FromCard<LanternKey>())
		});
	}

	private async Task UnlockCage()
	{
		SetEventFinished(L10NLookup("WAR_HISTORIAN_REPY.pages.UNLOCK_CAGE.description"));
		base.Owner.RunState.ExtraFields.FreedRepy = true;
		await RemoveLanternKey();
		await RelicCmd.Obtain(ModelDb.Relic<HistoryCourse>().ToMutable(), base.Owner);
	}

	private async Task UnlockChest()
	{
		SetEventFinished(L10NLookup("WAR_HISTORIAN_REPY.pages.UNLOCK_CHEST.description"));
		await RemoveLanternKey();
		List<Reward> list = new List<Reward>();
		list.Add(new PotionReward(base.Owner));
		list.Add(new PotionReward(base.Owner));
		list.Add(new RelicReward(base.Owner));
		list.Add(new RelicReward(base.Owner));
		await RewardsCmd.OfferCustom(base.Owner, list);
	}

	private async Task RemoveLanternKey()
	{
		List<CardModel> list = base.Owner.Deck.Cards.Where((CardModel c) => c is LanternKey).ToList();
		foreach (CardModel item in list)
		{
			PlayerCmd.CompleteQuest(item);
			await CardPileCmd.RemoveFromDeck(item);
		}
	}
}
