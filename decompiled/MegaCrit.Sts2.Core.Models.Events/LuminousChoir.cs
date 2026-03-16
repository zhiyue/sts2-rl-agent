using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class LuminousChoir : EventModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new GoldVar(149));

	public override bool IsAllowed(RunState runState)
	{
		return runState.Players.All((Player p) => (decimal)p.Gold >= base.DynamicVars.Gold.BaseValue && p.RelicGrabBag.HasAvailableRelics(runState));
	}

	public override void CalculateVars()
	{
		base.DynamicVars.Gold.BaseValue -= (decimal)base.Rng.NextInt(0, 50);
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		int num = 1;
		List<EventOption> list = new List<EventOption>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<EventOption> span = CollectionsMarshal.AsSpan(list);
		int index = 0;
		span[index] = new EventOption(this, ReachIntoTheFlesh, "LUMINOUS_CHOIR.pages.INITIAL.options.REACH_INTO_THE_FLESH", HoverTipFactory.FromCardWithCardHoverTips<SporeMind>());
		List<EventOption> list2 = list;
		if (base.Owner.Gold >= base.DynamicVars.Gold.IntValue)
		{
			list2.Add(new EventOption(this, OfferTribute, "LUMINOUS_CHOIR.pages.INITIAL.options.OFFER_TRIBUTE"));
		}
		else
		{
			list2.Add(new EventOption(this, null, "LUMINOUS_CHOIR.pages.INITIAL.options.OFFER_TRIBUTE_LOCKED"));
		}
		return list2;
	}

	private async Task ReachIntoTheFlesh()
	{
		List<CardModel> cards = (await CardSelectCmd.FromDeckForRemoval(prefs: new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, 2), player: base.Owner)).ToList();
		await CardPileCmd.RemoveFromDeck(cards);
		await CardPileCmd.AddCurseToDeck<SporeMind>(base.Owner);
		SetEventFinished(L10NLookup("LUMINOUS_CHOIR.pages.REACH_INTO_THE_FLESH.description"));
	}

	private async Task OfferTribute()
	{
		await PlayerCmd.LoseGold(base.DynamicVars.Gold.IntValue, base.Owner, GoldLossType.Spent);
		RelicModel relic = RelicFactory.PullNextRelicFromFront(base.Owner).ToMutable();
		await RelicCmd.Obtain(relic, base.Owner);
		SetEventFinished(L10NLookup("LUMINOUS_CHOIR.pages.OFFER_TRIBUTE.description"));
	}
}
