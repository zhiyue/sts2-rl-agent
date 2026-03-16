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
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class WoodCarvings : EventModel
{
	private const string _birdCardKey = "BirdCard";

	private const string _snakeEnchantmentKey = "SnakeEnchantment";

	private const string _toricCardKey = "ToricCard";

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[3]
	{
		new StringVar("BirdCard", ModelDb.Card<Peck>().Title),
		new StringVar("SnakeEnchantment", ModelDb.Enchantment<Slither>().Title.GetFormattedText()),
		new StringVar("ToricCard", ModelDb.Card<ToricToughness>().Title)
	});

	public override bool IsAllowed(RunState runState)
	{
		return runState.Players.All((Player p) => CardPile.Get(PileType.Deck, p).Cards.Any((CardModel c) => c != null && c.Rarity == CardRarity.Basic && c.IsRemovable));
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		IReadOnlyList<CardModel> cards = PileType.Deck.GetPile(base.Owner).Cards;
		EventOption eventOption = ((!cards.Any((CardModel c) => ModelDb.Enchantment<Slither>().CanEnchant(c))) ? new EventOption(this, null, "WOOD_CARVINGS.pages.INITIAL.options.SNAKE_LOCKED") : new EventOption(this, Snake, "WOOD_CARVINGS.pages.INITIAL.options.SNAKE", HoverTipFactory.FromEnchantment<Slither>()));
		return new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[3]
		{
			new EventOption(this, Bird, "WOOD_CARVINGS.pages.INITIAL.options.BIRD", HoverTipFactory.FromCardWithCardHoverTips<Peck>()),
			eventOption,
			new EventOption(this, Torus, "WOOD_CARVINGS.pages.INITIAL.options.TORUS", HoverTipFactory.FromCardWithCardHoverTips<ToricToughness>())
		});
	}

	private async Task Bird()
	{
		CardModel cardModel = (await CardSelectCmd.FromDeckGeneric(base.Owner, new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 1), (CardModel c) => c.IsTransformable && c.Rarity == CardRarity.Basic)).FirstOrDefault();
		if (cardModel != null)
		{
			await CardCmd.TransformTo<Peck>(cardModel, CardPreviewStyle.EventLayout);
		}
		SetEventFinished(L10NLookup("WOOD_CARVINGS.pages.BIRD.description"));
	}

	private async Task Snake()
	{
		CardModel cardModel = (await CardSelectCmd.FromDeckForEnchantment(base.Owner, ModelDb.Enchantment<Slither>(), 1, new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, 1))).FirstOrDefault();
		if (cardModel != null)
		{
			CardCmd.Enchant<Slither>(cardModel, 1m);
			NCardEnchantVfx nCardEnchantVfx = NCardEnchantVfx.Create(cardModel);
			if (nCardEnchantVfx != null)
			{
				NRun.Instance?.GlobalUi.CardPreviewContainer.AddChildSafely(nCardEnchantVfx);
			}
		}
		SetEventFinished(L10NLookup("WOOD_CARVINGS.pages.SNAKE.description"));
	}

	private async Task Torus()
	{
		CardModel cardModel = (await CardSelectCmd.FromDeckGeneric(base.Owner, new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 1), (CardModel c) => c != null && c.IsTransformable && c.Rarity == CardRarity.Basic)).FirstOrDefault();
		if (cardModel != null)
		{
			await CardCmd.TransformTo<ToricToughness>(cardModel, CardPreviewStyle.EventLayout);
		}
		SetEventFinished(L10NLookup("WOOD_CARVINGS.pages.TORUS.description"));
	}
}
