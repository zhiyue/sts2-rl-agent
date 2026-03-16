using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class ZenWeaver : EventModel
{
	private const string _breathingTechniquesCostKey = "BreathingTechniquesCost";

	private const string _emotionalAwarenessCostKey = "EmotionalAwarenessCost";

	private const string _arachnidAcupunctureCostKey = "ArachnidAcupunctureCost";

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[3]
	{
		new DynamicVar("BreathingTechniquesCost", 50m),
		new DynamicVar("EmotionalAwarenessCost", 125m),
		new DynamicVar("ArachnidAcupunctureCost", 250m)
	});

	public override bool IsAllowed(RunState runState)
	{
		return runState.Players.All((Player p) => (decimal)p.Gold >= base.DynamicVars["EmotionalAwarenessCost"].BaseValue);
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		int gold = base.Owner.Gold;
		EventOption eventOption = new EventOption(this, BreathingTechniques, "ZEN_WEAVER.pages.INITIAL.options.BREATHING_TECHNIQUES", HoverTipFactory.FromCardWithCardHoverTips<Enlightenment>());
		EventOption eventOption2 = ((gold < base.DynamicVars["EmotionalAwarenessCost"].IntValue) ? CreateLockedOption() : new EventOption(this, EmotionalAwareness, "ZEN_WEAVER.pages.INITIAL.options.EMOTIONAL_AWARENESS"));
		EventOption eventOption3 = ((gold < base.DynamicVars["ArachnidAcupunctureCost"].IntValue) ? CreateLockedOption() : new EventOption(this, ArachnidAcupuncture, "ZEN_WEAVER.pages.INITIAL.options.ARACHNID_ACUPUNCTURE"));
		return new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[3] { eventOption, eventOption2, eventOption3 });
	}

	private async Task BreathingTechniques()
	{
		await PlayerCmd.LoseGold(base.DynamicVars["BreathingTechniquesCost"].IntValue, base.Owner, GoldLossType.Spent);
		CardModel[] array = new CardModel[2];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = base.Owner.RunState.CreateCard<Enlightenment>(base.Owner);
		}
		CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(array, PileType.Deck));
		SetEventFinished(L10NLookup("ZEN_WEAVER.pages.BREATHING_TECHNIQUES.description"));
	}

	private async Task EmotionalAwareness()
	{
		await RemoveCardsAndProceed(base.DynamicVars["EmotionalAwarenessCost"].IntValue, 1);
		SetEventFinished(L10NLookup("ZEN_WEAVER.pages.EMOTIONAL_AWARENESS.description"));
	}

	private async Task ArachnidAcupuncture()
	{
		await RemoveCardsAndProceed(base.DynamicVars["ArachnidAcupunctureCost"].IntValue, 2);
		SetEventFinished(L10NLookup("ZEN_WEAVER.pages.ARACHNID_ACUPUNCTURE.description"));
	}

	private async Task RemoveCardsAndProceed(int cost, int count)
	{
		await CardPileCmd.RemoveFromDeck((await CardSelectCmd.FromDeckForRemoval(base.Owner, new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, count))).ToList());
		await PlayerCmd.LoseGold(cost, base.Owner, GoldLossType.Spent);
	}

	private EventOption CreateLockedOption()
	{
		return new EventOption(this, null, "ZEN_WEAVER.pages.INITIAL.options.LOCKED");
	}
}
