using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class SpiralingWhirlpool : EventModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new HealVar(0m));

	public override void CalculateVars()
	{
		base.DynamicVars.Heal.BaseValue = ((base.Owner != null) ? ((decimal)base.Owner.Creature.MaxHp * 0.33m) : 0m);
	}

	public override bool IsAllowed(RunState runState)
	{
		return runState.Players.All((Player p) => p.Deck.Cards.Any(ModelDb.Enchantment<Spiral>().CanEnchant));
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[2]
		{
			new EventOption(this, ObserveTheSpiral, "SPIRALING_WHIRLPOOL.pages.INITIAL.options.OBSERVE", HoverTipFactory.FromEnchantment<Spiral>()),
			new EventOption(this, Drink, "SPIRALING_WHIRLPOOL.pages.INITIAL.options.DRINK")
		});
	}

	private async Task ObserveTheSpiral()
	{
		CardModel cardModel = (await CardSelectCmd.FromDeckForEnchantment(base.Owner, ModelDb.Enchantment<Spiral>(), 1, (CardModel? c) => ModelDb.Enchantment<Spiral>().CanEnchant(c), new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, 1))).FirstOrDefault();
		if (cardModel != null)
		{
			CardCmd.Enchant<Spiral>(cardModel, 1m);
			NCardEnchantVfx nCardEnchantVfx = NCardEnchantVfx.Create(cardModel);
			if (nCardEnchantVfx != null)
			{
				NRun.Instance?.GlobalUi.CardPreviewContainer.AddChildSafely(nCardEnchantVfx);
			}
		}
		SetEventFinished(L10NLookup("SPIRALING_WHIRLPOOL.pages.OBSERVE.description"));
	}

	private async Task Drink()
	{
		await CreatureCmd.Heal(base.Owner.Creature, base.DynamicVars.Heal.IntValue);
		SetEventFinished(L10NLookup("SPIRALING_WHIRLPOOL.pages.DRINK.description"));
	}
}
