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

	private bool ShouldGetSecondReward
	{
		get
		{
			if (base.Owner.Deck.Cards.Any((CardModel c) => c is LanternKey))
			{
				return base.Owner.RunState.Players.Count <= 1;
			}
			return false;
		}
	}

	public override bool IsAllowed(IRunState runState)
	{
		return false;
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[2]
		{
			new EventOption(this, InitialUnlockCage, "WAR_HISTORIAN_REPY.pages.INITIAL.options.UNLOCK_CAGE", HoverTipFactory.FromRelic<HistoryCourse>().Concat(HoverTipFactory.FromCardWithCardHoverTips<LanternKey>())),
			new EventOption(this, InitialUnlockChest, "WAR_HISTORIAN_REPY.pages.INITIAL.options.UNLOCK_CHEST", HoverTipFactory.FromCardWithCardHoverTips<LanternKey>())
		});
	}

	private async Task InitialUnlockCage()
	{
		await RemoveLanternKeysForInitialChoice();
		await UnlockCage();
		if (ShouldGetSecondReward)
		{
			SetEventState(L10NLookup("WAR_HISTORIAN_REPY.pages.UNLOCK_CAGE.description"), new global::_003C_003Ez__ReadOnlySingleElementList<EventOption>(new EventOption(this, SecondUnlockChest, "WAR_HISTORIAN_REPY.pages.INITIAL.options.UNLOCK_CHEST", HoverTipFactory.FromCardWithCardHoverTips<LanternKey>())));
		}
		else
		{
			SetEventFinished(L10NLookup("WAR_HISTORIAN_REPY.pages.UNLOCK_CAGE.description"));
		}
	}

	private async Task InitialUnlockChest()
	{
		await RemoveLanternKeysForInitialChoice();
		await UnlockChest();
		if (ShouldGetSecondReward)
		{
			SetEventState(L10NLookup("WAR_HISTORIAN_REPY.pages.UNLOCK_CHEST.description"), new global::_003C_003Ez__ReadOnlySingleElementList<EventOption>(new EventOption(this, SecondUnlockCage, "WAR_HISTORIAN_REPY.pages.INITIAL.options.UNLOCK_CAGE", HoverTipFactory.FromRelic<HistoryCourse>().Concat(HoverTipFactory.FromCardWithCardHoverTips<LanternKey>()))));
		}
		else
		{
			SetEventFinished(L10NLookup("WAR_HISTORIAN_REPY.pages.UNLOCK_CHEST.description"));
		}
	}

	private async Task SecondUnlockCage()
	{
		SetEventFinished(L10NLookup("WAR_HISTORIAN_REPY.pages.EXTRA_UNLOCK_CAGE.description"));
		await RemoveLanternKeysForSecondChoice();
		await UnlockCage();
	}

	private async Task SecondUnlockChest()
	{
		SetEventFinished(L10NLookup("WAR_HISTORIAN_REPY.pages.EXTRA_UNLOCK_CHEST.description"));
		await RemoveLanternKeysForSecondChoice();
		await UnlockChest();
	}

	private async Task UnlockChest()
	{
		List<Reward> list = new List<Reward>();
		list.Add(new PotionReward(base.Owner));
		list.Add(new PotionReward(base.Owner));
		list.Add(new RelicReward(base.Owner));
		list.Add(new RelicReward(base.Owner));
		await RewardsCmd.OfferCustom(base.Owner, list);
	}

	private async Task UnlockCage()
	{
		base.Owner.RunState.ExtraFields.FreedRepy = true;
		await RelicCmd.Obtain<HistoryCourse>(base.Owner);
	}

	private async Task RemoveLanternKeysForInitialChoice()
	{
		if (base.Owner.RunState.Players.Count > 1)
		{
			await RemoveLanternKeysForSecondChoice();
		}
		else
		{
			await RemoveFirstLanternKey();
		}
	}

	private async Task RemoveFirstLanternKey()
	{
		CardModel cardModel = base.Owner.Deck.Cards.FirstOrDefault((CardModel c) => c is LanternKey);
		if (cardModel != null)
		{
			PlayerCmd.CompleteQuest(cardModel);
			await CardPileCmd.RemoveFromDeck(cardModel);
		}
	}

	private async Task RemoveLanternKeysForSecondChoice()
	{
		List<CardModel> list = base.Owner.Deck.Cards.Where((CardModel c) => c is LanternKey).ToList();
		foreach (CardModel item in list)
		{
			PlayerCmd.CompleteQuest(item);
			await CardPileCmd.RemoveFromDeck(item);
		}
	}
}
