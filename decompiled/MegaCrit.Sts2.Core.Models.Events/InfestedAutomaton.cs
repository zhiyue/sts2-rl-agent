using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class InfestedAutomaton : EventModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[2]
		{
			new EventOption(this, Study, "INFESTED_AUTOMATON.pages.INITIAL.options.STUDY"),
			new EventOption(this, TouchCore, "INFESTED_AUTOMATON.pages.INITIAL.options.TOUCH_CORE")
		});
	}

	private async Task Study()
	{
		CardCreationOptions options = CardCreationOptions.ForNonCombatWithDefaultOdds(new global::_003C_003Ez__ReadOnlySingleElementList<CardPoolModel>(base.Owner.Character.CardPool), (CardModel c) => c.Type == CardType.Power);
		CardModel cardModel = CardFactory.CreateForReward(base.Owner, 1, options).FirstOrDefault()?.Card;
		if (cardModel != null)
		{
			CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(cardModel, PileType.Deck), 1.2f, CardPreviewStyle.EventLayout);
		}
		SetEventFinished(L10NLookup("INFESTED_AUTOMATON.pages.STUDY.description"));
	}

	private async Task TouchCore()
	{
		CardCreationOptions options = CardCreationOptions.ForNonCombatWithDefaultOdds(new global::_003C_003Ez__ReadOnlySingleElementList<CardPoolModel>(base.Owner.Character.CardPool), delegate(CardModel c)
		{
			CardEnergyCost energyCost = c.EnergyCost;
			return energyCost != null && energyCost.Canonical == 0 && !energyCost.CostsX;
		});
		CardModel cardModel = CardFactory.CreateForReward(base.Owner, 1, options).FirstOrDefault()?.Card;
		if (cardModel != null)
		{
			CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(cardModel, PileType.Deck), 1.2f, CardPreviewStyle.EventLayout);
		}
		SetEventFinished(L10NLookup("INFESTED_AUTOMATON.pages.TOUCH_CORE.description"));
	}
}
