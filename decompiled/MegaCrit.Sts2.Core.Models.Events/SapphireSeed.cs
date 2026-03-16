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
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class SapphireSeed : EventModel
{
	private const string _enchantmentKey = "Enchantment";

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new StringVar("Enchantment", ModelDb.Enchantment<Sown>().Title.GetFormattedText()),
		new HealVar(9m)
	});

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[2]
		{
			new EventOption(this, Eat, "SAPPHIRE_SEED.pages.INITIAL.options.EAT"),
			new EventOption(this, Plant, "SAPPHIRE_SEED.pages.INITIAL.options.PLANT", HoverTipFactory.FromEnchantment<Sown>())
		});
	}

	private async Task Eat()
	{
		await CreatureCmd.Heal(base.Owner.Creature, base.DynamicVars.Heal.IntValue);
		CardModel cardModel = (await CardSelectCmd.FromDeckForUpgrade(base.Owner, new CardSelectorPrefs(CardSelectorPrefs.UpgradeSelectionPrompt, 1))).FirstOrDefault();
		if (cardModel != null)
		{
			CardCmd.Upgrade(cardModel);
		}
		SetEventFinished(L10NLookup("SAPPHIRE_SEED.pages.EAT.description"));
	}

	private async Task Plant()
	{
		EnchantmentModel sown = ModelDb.Enchantment<Sown>();
		List<CardModel> cards = PileType.Deck.GetPile(base.Owner).Cards.Where((CardModel c) => sown.CanEnchant(c)).ToList();
		CardModel cardModel = (await CardSelectCmd.FromDeckForEnchantment(prefs: new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, 1), cards: cards, enchantment: sown, amount: 1)).FirstOrDefault();
		if (cardModel != null)
		{
			CardCmd.Enchant<Sown>(cardModel, 1m);
			NCardEnchantVfx nCardEnchantVfx = NCardEnchantVfx.Create(cardModel);
			if (nCardEnchantVfx != null)
			{
				NRun.Instance?.GlobalUi.CardPreviewContainer.AddChildSafely(nCardEnchantVfx);
			}
		}
		SetEventFinished(L10NLookup("SAPPHIRE_SEED.pages.PLANT.description"));
	}
}
