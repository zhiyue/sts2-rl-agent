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
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class SelfHelpBook : EventModel
{
	private const string _readTheBackDescriptionKey = "SELF_HELP_BOOK.pages.READ_THE_BACK.description";

	private const string _readPassageDescriptionKey = "SELF_HELP_BOOK.pages.READ_PASSAGE.description";

	private const string _readEntireBookDescriptionKey = "SELF_HELP_BOOK.pages.READ_ENTIRE_BOOK.description";

	private const int _sharpAmount = 2;

	private const int _nimbleAmount = 2;

	private const int _swiftAmount = 2;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[6]
	{
		new StringVar("Enchantment1", ModelDb.Enchantment<Sharp>().Title.GetFormattedText()),
		new StringVar("Enchantment2", ModelDb.Enchantment<Nimble>().Title.GetFormattedText()),
		new StringVar("Enchantment3", ModelDb.Enchantment<Swift>().Title.GetFormattedText()),
		new IntVar("Enchantment1Amount", 2m),
		new IntVar("Enchantment2Amount", 2m),
		new IntVar("Enchantment3Amount", 2m)
	});

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		List<EventOption> list = new List<EventOption>();
		bool flag = PlayerHasCardsAvailable<Sharp>(base.Owner, CardType.Attack);
		bool flag2 = PlayerHasCardsAvailable<Nimble>(base.Owner, CardType.Skill);
		bool flag3 = PlayerHasCardsAvailable<Swift>(base.Owner, CardType.Power);
		if (flag || flag2 || flag3)
		{
			if (flag)
			{
				list.Add(new EventOption(this, ReadTheBack, "SELF_HELP_BOOK.pages.INITIAL.options.READ_THE_BACK", HoverTipFactory.FromEnchantment<Sharp>(2)));
			}
			else
			{
				list.Add(new EventOption(this, null, "SELF_HELP_BOOK.pages.INITIAL.options.READ_THE_BACK_LOCKED"));
			}
			if (flag2)
			{
				list.Add(new EventOption(this, ReadPassage, "SELF_HELP_BOOK.pages.INITIAL.options.READ_PASSAGE", HoverTipFactory.FromEnchantment<Nimble>(2)));
			}
			else
			{
				list.Add(new EventOption(this, null, "SELF_HELP_BOOK.pages.INITIAL.options.READ_PASSAGE_LOCKED"));
			}
			if (flag3)
			{
				list.Add(new EventOption(this, ReadEntireBook, "SELF_HELP_BOOK.pages.INITIAL.options.READ_ENTIRE_BOOK", HoverTipFactory.FromEnchantment<Swift>(2)));
			}
			else
			{
				list.Add(new EventOption(this, null, "SELF_HELP_BOOK.pages.INITIAL.options.READ_ENTIRE_BOOK_LOCKED"));
			}
		}
		else
		{
			list.Add(new EventOption(this, SkipBook, "SELF_HELP_BOOK.pages.INITIAL.options.NO_OPTIONS"));
		}
		return list;
	}

	private async Task ReadTheBack()
	{
		await SelectAndEnchant<Sharp>(2, CardType.Attack, L10NLookup("SELF_HELP_BOOK.pages.READ_THE_BACK.description"));
	}

	private async Task ReadPassage()
	{
		await SelectAndEnchant<Nimble>(2, CardType.Skill, L10NLookup("SELF_HELP_BOOK.pages.READ_PASSAGE.description"));
	}

	private async Task ReadEntireBook()
	{
		await SelectAndEnchant<Swift>(2, CardType.Power, L10NLookup("SELF_HELP_BOOK.pages.READ_ENTIRE_BOOK.description"));
	}

	private bool PlayerHasCardsAvailable<T>(Player player, CardType typeRestriction) where T : EnchantmentModel
	{
		EnchantmentModel enchantment = ModelDb.Enchantment<T>();
		return PileType.Deck.GetPile(player).Cards.FirstOrDefault((CardModel c) => DeckFilter(c, enchantment, typeRestriction)) != null;
	}

	private async Task SelectAndEnchant<T>(int amount, CardType typeRestriction, LocString finalDescription) where T : EnchantmentModel
	{
		CardSelectorPrefs prefs = new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, 1);
		EnchantmentModel enchantment = ModelDb.Enchantment<T>();
		CardModel cardModel = (await CardSelectCmd.FromDeckForEnchantment(base.Owner, enchantment, amount, (CardModel? c) => c.Type == typeRestriction, prefs)).FirstOrDefault();
		if (cardModel != null)
		{
			await ApplyEnchantment<T>(cardModel, amount);
		}
		SetEventFinished(finalDescription);
	}

	private bool DeckFilter(CardModel card, EnchantmentModel enchantment, CardType type)
	{
		if (card.Pile.Type == PileType.Deck && card.Type == type)
		{
			return enchantment.CanEnchant(card);
		}
		return false;
	}

	private Task ApplyEnchantment<T>(CardModel card, int amount) where T : EnchantmentModel
	{
		CardCmd.Enchant<T>(card, amount);
		NCardEnchantVfx nCardEnchantVfx = NCardEnchantVfx.Create(card);
		if (nCardEnchantVfx != null)
		{
			NRun.Instance?.GlobalUi.CardPreviewContainer.AddChildSafely(nCardEnchantVfx);
		}
		return Task.CompletedTask;
	}

	private Task SkipBook()
	{
		SetEventFinished(L10NLookup("SELF_HELP_BOOK.pages.NO_OPTIONS.description"));
		return Task.CompletedTask;
	}
}
