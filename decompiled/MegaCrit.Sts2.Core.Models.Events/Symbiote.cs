using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class Symbiote : EventModel
{
	private const string _enchantmentKey = "Enchantment";

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new StringVar("Enchantment", ModelDb.Enchantment<Corrupted>().Title.GetFormattedText()),
		new CardsVar(1)
	});

	public override bool IsAllowed(RunState runState)
	{
		return runState.CurrentActIndex > 0;
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		CardPile pile = PileType.Deck.GetPile(base.Owner);
		EventOption eventOption = ((!pile.Cards.Any(CanEnchant)) ? new EventOption(this, null, "SYMBIOTE.pages.INITIAL.options.APPROACH_LOCKED") : new EventOption(this, Approach, "SYMBIOTE.pages.INITIAL.options.APPROACH", HoverTipFactory.FromEnchantment<Corrupted>()));
		EventOption eventOption2 = new EventOption(this, KillWithFire, "SYMBIOTE.pages.INITIAL.options.KILL_WITH_FIRE", HoverTipFactory.Static(StaticHoverTip.Transform));
		int num = 2;
		List<EventOption> list = new List<EventOption>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<EventOption> span = CollectionsMarshal.AsSpan(list);
		int num2 = 0;
		span[num2] = eventOption;
		num2++;
		span[num2] = eventOption2;
		return list;
	}

	private async Task Approach()
	{
		CardModel cardModel = (await CardSelectCmd.FromDeckForEnchantment(prefs: new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, 1), player: base.Owner, enchantment: ModelDb.Enchantment<Corrupted>(), amount: 1)).FirstOrDefault();
		if (cardModel != null)
		{
			CardCmd.Enchant<Corrupted>(cardModel, 1m);
			NCardEnchantVfx nCardEnchantVfx = NCardEnchantVfx.Create(cardModel);
			if (nCardEnchantVfx != null)
			{
				NRun.Instance?.GlobalUi.CardPreviewContainer.AddChildSafely(nCardEnchantVfx);
			}
		}
		SetEventFinished(L10NLookup("SYMBIOTE.pages.APPROACH.description"));
	}

	private async Task KillWithFire()
	{
		List<CardModel> list = (await CardSelectCmd.FromDeckForTransformation(prefs: new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, base.DynamicVars.Cards.IntValue), player: base.Owner)).ToList();
		foreach (CardModel item in list)
		{
			await CardCmd.TransformToRandom(item, base.Rng, CardPreviewStyle.EventLayout);
		}
		SetEventFinished(L10NLookup("SYMBIOTE.pages.KILL_WITH_FIRE.description"));
	}

	private static bool CanEnchant(CardModel card)
	{
		return ModelDb.Enchantment<Corrupted>().CanEnchant(card);
	}
}
