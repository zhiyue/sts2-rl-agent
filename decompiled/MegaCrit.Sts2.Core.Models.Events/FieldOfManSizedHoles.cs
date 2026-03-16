using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class FieldOfManSizedHoles : EventModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[4]
	{
		new GoldVar(75),
		new CardsVar(2),
		new StringVar("ResistCurse", ModelDb.Card<Normality>().Title),
		new StringVar("Enchantment", ModelDb.Enchantment<PerfectFit>().Title.GetFormattedText())
	});

	public override bool IsAllowed(RunState runState)
	{
		return runState.Players.All((Player p) => CardPile.Get(PileType.Deck, p).Cards.Any(ModelDb.Enchantment<PerfectFit>().CanEnchant));
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[2]
		{
			new EventOption(this, Resist, "FIELD_OF_MAN_SIZED_HOLES.pages.INITIAL.options.RESIST", HoverTipFactory.FromCardWithCardHoverTips<Normality>()),
			new EventOption(this, EnterYourHole, "FIELD_OF_MAN_SIZED_HOLES.pages.INITIAL.options.ENTER_YOUR_HOLE", HoverTipFactory.FromEnchantment<PerfectFit>())
		});
	}

	private async Task Resist()
	{
		List<CardModel> cards = (await CardSelectCmd.FromDeckForRemoval(prefs: new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, base.DynamicVars.Cards.IntValue), player: base.Owner)).ToList();
		await CardPileCmd.RemoveFromDeck(cards);
		await CardPileCmd.AddCursesToDeck(new global::_003C_003Ez__ReadOnlySingleElementList<CardModel>(ModelDb.Card<Normality>()), base.Owner);
		SetEventFinished(L10NLookup("FIELD_OF_MAN_SIZED_HOLES.pages.RESIST.description"));
	}

	private async Task EnterYourHole()
	{
		CardModel cardModel = (await CardSelectCmd.FromDeckForEnchantment(base.Owner, ModelDb.Enchantment<PerfectFit>(), 1, new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, 1))).FirstOrDefault();
		if (cardModel != null)
		{
			CardCmd.Enchant<PerfectFit>(cardModel, 1m);
			NCardEnchantVfx nCardEnchantVfx = NCardEnchantVfx.Create(cardModel);
			if (nCardEnchantVfx != null)
			{
				NRun.Instance?.GlobalUi.CardPreviewContainer.AddChildSafely(nCardEnchantVfx);
			}
		}
		SetEventFinished(L10NLookup("FIELD_OF_MAN_SIZED_HOLES.pages.ENTER_YOUR_HOLE.description"));
	}
}
