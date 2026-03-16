using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
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

public sealed class WaterloggedScriptorium : EventModel
{
	private const int _spawnGoldRequirement = 65;

	private const string _pricklySpongeGoldKey = "PricklySpongeGold";

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[4]
	{
		new MaxHpVar(6m),
		new GoldVar(65),
		new GoldVar("PricklySpongeGold", 155),
		new CardsVar(2)
	});

	public override bool IsAllowed(RunState runState)
	{
		return runState.Players.All((Player p) => p.Gold >= 65);
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		int num = 1;
		List<EventOption> list = new List<EventOption>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<EventOption> span = CollectionsMarshal.AsSpan(list);
		int index = 0;
		span[index] = new EventOption(this, BloodyInk, "WATERLOGGED_SCRIPTORIUM.pages.INITIAL.options.BLOODY_INK");
		List<EventOption> list2 = list;
		if (base.Owner.Gold >= base.DynamicVars.Gold.IntValue)
		{
			list2.Add(new EventOption(this, TentacleQuill, "WATERLOGGED_SCRIPTORIUM.pages.INITIAL.options.TENTACLE_QUILL", HoverTipFactory.FromEnchantment<Steady>()));
		}
		else
		{
			list2.Add(new EventOption(this, null, "WATERLOGGED_SCRIPTORIUM.pages.INITIAL.options.TENTACLE_QUILL_LOCKED"));
		}
		if (base.Owner.Gold >= base.DynamicVars["PricklySpongeGold"].IntValue)
		{
			list2.Add(new EventOption(this, PricklySponge, "WATERLOGGED_SCRIPTORIUM.pages.INITIAL.options.PRICKLY_SPONGE", HoverTipFactory.FromEnchantment<Steady>()));
		}
		else
		{
			list2.Add(new EventOption(this, null, "WATERLOGGED_SCRIPTORIUM.pages.INITIAL.options.PRICKLY_SPONGE_LOCKED"));
		}
		return list2;
	}

	private async Task PricklySponge()
	{
		await PlayerCmd.LoseGold(base.DynamicVars["PricklySpongeGold"].BaseValue, base.Owner, GoldLossType.Spent);
		CardSelectorPrefs prefs = new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, base.DynamicVars.Cards.IntValue);
		Steady enchantment = ModelDb.Enchantment<Steady>();
		foreach (CardModel item in await CardSelectCmd.FromDeckForEnchantment(base.Owner, enchantment, 1, prefs))
		{
			CardCmd.Enchant<Steady>(item, 1m);
			NCardEnchantVfx nCardEnchantVfx = NCardEnchantVfx.Create(item);
			if (nCardEnchantVfx != null)
			{
				NRun.Instance?.GlobalUi.CardPreviewContainer.AddChildSafely(nCardEnchantVfx);
			}
		}
		SetEventFinished(L10NLookup("WATERLOGGED_SCRIPTORIUM.pages.PRICKLY_SPONGE.description"));
	}

	private async Task TentacleQuill()
	{
		await PlayerCmd.LoseGold(base.DynamicVars.Gold.BaseValue, base.Owner, GoldLossType.Spent);
		CardModel cardModel = (await CardSelectCmd.FromDeckForEnchantment(prefs: new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, 1), player: base.Owner, enchantment: ModelDb.Enchantment<Steady>(), amount: 1)).FirstOrDefault();
		if (cardModel != null)
		{
			CardCmd.Enchant<Steady>(cardModel, 1m);
			NCardEnchantVfx nCardEnchantVfx = NCardEnchantVfx.Create(cardModel);
			if (nCardEnchantVfx != null)
			{
				NRun.Instance?.GlobalUi.CardPreviewContainer.AddChildSafely(nCardEnchantVfx);
			}
		}
		SetEventFinished(L10NLookup("WATERLOGGED_SCRIPTORIUM.pages.TENTACLE_QUILL.description"));
	}

	private async Task BloodyInk()
	{
		await CreatureCmd.GainMaxHp(base.Owner.Creature, base.DynamicVars.MaxHp.BaseValue);
		SetEventFinished(L10NLookup("WATERLOGGED_SCRIPTORIUM.pages.BLOODY_INK.description"));
	}
}
