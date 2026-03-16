using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class SpiritGrafter : EventModel
{
	private const string _rejectionHpLossKey = "RejectionHpLoss";

	private const string _letItInHealAmountKey = "LetItInHealAmount";

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new HpLossVar("RejectionHpLoss", 9m),
		new HealVar("LetItInHealAmount", 25m)
	});

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[2]
		{
			new EventOption(this, StepInside, "SPIRIT_GRAFTER.pages.INITIAL.options.LET_IT_IN", HoverTipFactory.FromCardWithCardHoverTips<Metamorphosis>()),
			new EventOption(this, StickArmIn, "SPIRIT_GRAFTER.pages.INITIAL.options.REJECTION").ThatDoesDamage(base.DynamicVars["RejectionHpLoss"].BaseValue)
		});
	}

	private async Task StepInside()
	{
		await CreatureCmd.Heal(base.Owner.Creature, base.DynamicVars["LetItInHealAmount"].BaseValue);
		CardModel card = base.Owner.RunState.CreateCard<Metamorphosis>(base.Owner);
		CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));
		SetEventFinished(L10NLookup("SPIRIT_GRAFTER.pages.LET_IT_IN.description"));
	}

	private async Task StickArmIn()
	{
		List<CardModel> cards = (await CardSelectCmd.FromDeckForRemoval(prefs: new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, 1), player: base.Owner)).ToList();
		await CardPileCmd.RemoveFromDeck(cards);
		await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), base.Owner.Creature, base.DynamicVars["RejectionHpLoss"].BaseValue, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
		SetEventFinished(L10NLookup("SPIRIT_GRAFTER.pages.REJECTION.description"));
	}
}
