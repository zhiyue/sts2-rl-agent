using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class RanwidTheElder : EventModel
{
	private const string _potionChoiceKey = "RANWID_THE_ELDER.pages.INITIAL.options.POTION";

	private const string _potionKey = "Potion";

	private const string _relicChoiceKey = "RANWID_THE_ELDER.pages.INITIAL.options.RELIC";

	private const string _relicKey = "Relic";

	private LocString PotionChoiceTitle => new LocString("events", "RANWID_THE_ELDER.pages.INITIAL.options.POTION.title");

	private LocString PotionChoiceDescription => new LocString("events", "RANWID_THE_ELDER.pages.INITIAL.options.POTION.description");

	private LocString RelicChoiceTitle => new LocString("events", "RANWID_THE_ELDER.pages.INITIAL.options.RELIC.title");

	private LocString RelicChoiceDescription => new LocString("events", "RANWID_THE_ELDER.pages.INITIAL.options.RELIC.description");

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[3]
	{
		new GoldVar(100),
		new StringVar("Potion", "Potion"),
		new StringVar("Relic", "Relic")
	});

	protected override Task BeforeEventStarted()
	{
		base.Owner.CanRemovePotions = false;
		return Task.CompletedTask;
	}

	protected override void OnEventFinished()
	{
		base.Owner.CanRemovePotions = true;
	}

	public override bool IsAllowed(RunState runState)
	{
		if (runState.CurrentActIndex == 0)
		{
			return false;
		}
		if (runState.Players.Any((Player p) => !GetValidRelics(p).Any()))
		{
			return false;
		}
		if (runState.Players.Any((Player p) => p.Gold < 100))
		{
			return false;
		}
		if (runState.Players.Any((Player p) => !p.Potions.Any()))
		{
			return false;
		}
		return true;
	}

	private IEnumerable<RelicModel> GetValidRelics(Player player)
	{
		return player.Relics.Where((RelicModel r) => r.IsTradable);
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		List<EventOption> list = new List<EventOption>();
		PotionModel potion = base.Rng.NextItem(base.Owner.Potions);
		if (potion != null)
		{
			((StringVar)base.DynamicVars["Potion"]).StringValue = potion.Title.GetFormattedText();
			list.Add(new EventOption(this, async delegate
			{
				await GivePotion(potion);
			}, PotionChoiceTitle, PotionChoiceDescription, "RANWID_THE_ELDER.pages.INITIAL.options.POTION", potion.HoverTips).ThatHasDynamicTitle());
		}
		else
		{
			list.Add(new EventOption(this, null, "RANWID_THE_ELDER.pages.INITIAL.options.POTION_LOCKED"));
		}
		list.Add(new EventOption(this, GiveGold, "RANWID_THE_ELDER.pages.INITIAL.options.GOLD"));
		RelicModel relic = base.Rng.NextItem(base.Owner.Relics.Where((RelicModel r) => r.IsTradable));
		if (relic != null)
		{
			((StringVar)base.DynamicVars["Relic"]).StringValue = relic.Title.GetFormattedText();
			list.Add(new EventOption(this, async delegate
			{
				await GiveRelic(relic);
			}, RelicChoiceTitle, RelicChoiceDescription, "RANWID_THE_ELDER.pages.INITIAL.options.RELIC", relic.HoverTips).ThatHasDynamicTitle());
		}
		else
		{
			list.Add(new EventOption(this, null, "RANWID_THE_ELDER.pages.INITIAL.options.RELIC_LOCKED"));
		}
		return list;
	}

	private async Task GivePotion(PotionModel potion)
	{
		await PotionCmd.Discard(potion);
		RelicModel relic = RelicFactory.PullNextRelicFromFront(base.Owner).ToMutable();
		await RelicCmd.Obtain(relic, base.Owner);
		SetEventFinished(L10NLookup("RANWID_THE_ELDER.pages.POTION.description"));
	}

	private async Task GiveGold()
	{
		await PlayerCmd.LoseGold(base.DynamicVars.Gold.IntValue, base.Owner, GoldLossType.Spent);
		RelicModel relic = RelicFactory.PullNextRelicFromFront(base.Owner).ToMutable();
		await RelicCmd.Obtain(relic, base.Owner);
		SetEventFinished(L10NLookup("RANWID_THE_ELDER.pages.GOLD.description"));
	}

	private async Task GiveRelic(RelicModel relic)
	{
		await RelicCmd.Remove(relic);
		for (int i = 0; i < 2; i++)
		{
			await RelicCmd.Obtain(RelicFactory.PullNextRelicFromFront(base.Owner).ToMutable(), base.Owner);
		}
		SetEventFinished(L10NLookup("RANWID_THE_ELDER.pages.RELIC.description"));
	}
}
