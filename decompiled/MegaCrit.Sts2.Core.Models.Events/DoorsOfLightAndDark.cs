using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class DoorsOfLightAndDark : EventModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar(2));

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[2]
		{
			new EventOption(this, Light, "DOORS_OF_LIGHT_AND_DARK.pages.INITIAL.options.LIGHT"),
			new EventOption(this, Dark, "DOORS_OF_LIGHT_AND_DARK.pages.INITIAL.options.DARK")
		});
	}

	private Task Light()
	{
		IEnumerable<CardModel> enumerable = PileType.Deck.GetPile(base.Owner).Cards.Where((CardModel c) => c?.IsUpgradable ?? false).ToList().StableShuffle(base.Owner.RunState.Rng.Niche)
			.Take(base.DynamicVars.Cards.IntValue);
		foreach (CardModel item in enumerable)
		{
			CardCmd.Upgrade(item);
		}
		SetEventFinished(L10NLookup("DOORS_OF_LIGHT_AND_DARK.pages.LIGHT.description"));
		return Task.CompletedTask;
	}

	private async Task Dark()
	{
		List<CardModel> cards = (await CardSelectCmd.FromDeckForRemoval(prefs: new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, 1), player: base.Owner)).ToList();
		await CardPileCmd.RemoveFromDeck(cards);
		SetEventFinished(L10NLookup("DOORS_OF_LIGHT_AND_DARK.pages.DARK.description"));
	}
}
