using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class GraveOfTheForgotten : EventModel
{
	private const string _enchantmentKey = "Enchantment";

	private const string _relicKey = "Relic";

	private const string _curseKey = "Curse";

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[3]
	{
		new StringVar("Relic", ModelDb.Relic<ForgottenSoul>().Title.GetFormattedText()),
		new StringVar("Enchantment", ModelDb.Enchantment<SoulsPower>().Title.GetFormattedText()),
		new StringVar("Curse", ModelDb.Card<Decay>().Title)
	});

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		EnchantmentModel soulsPower = ModelDb.Enchantment<SoulsPower>();
		EventOption eventOption = ((!PileType.Deck.GetPile(base.Owner).Cards.Any((CardModel c) => soulsPower.CanEnchant(c))) ? new EventOption(this, null, "GRAVE_OF_THE_FORGOTTEN.pages.INITIAL.options.CONFRONT_LOCKED") : new EventOption(this, Confront, "GRAVE_OF_THE_FORGOTTEN.pages.INITIAL.options.CONFRONT", HoverTipFactory.FromEnchantment<SoulsPower>().Append(HoverTipFactory.FromCard<Decay>())));
		return new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[2]
		{
			eventOption,
			new EventOption(this, Accept, "GRAVE_OF_THE_FORGOTTEN.pages.INITIAL.options.ACCEPT", HoverTipFactory.FromRelic<ForgottenSoul>())
		});
	}

	private async Task Confront()
	{
		await CardPileCmd.AddCurseToDeck<Decay>(base.Owner);
		CardModel cardModel = (await CardSelectCmd.FromDeckForEnchantment(base.Owner, ModelDb.Enchantment<SoulsPower>(), 1, new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, 1))).FirstOrDefault();
		if (cardModel != null)
		{
			CardCmd.Enchant<SoulsPower>(cardModel, 1m);
			NCardEnchantVfx nCardEnchantVfx = NCardEnchantVfx.Create(cardModel);
			if (nCardEnchantVfx != null)
			{
				NRun.Instance?.GlobalUi.CardPreviewContainer.AddChildSafely(nCardEnchantVfx);
			}
		}
		SetEventFinished(L10NLookup("GRAVE_OF_THE_FORGOTTEN.pages.CONFRONT.description"));
	}

	private async Task Accept()
	{
		await RelicCmd.Obtain<ForgottenSoul>(base.Owner);
		SetEventFinished(L10NLookup("GRAVE_OF_THE_FORGOTTEN.pages.ACCEPT.description"));
	}
}
