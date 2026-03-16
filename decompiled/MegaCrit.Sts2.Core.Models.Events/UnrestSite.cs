using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class UnrestSite : EventModel
{
	private const string _maxHpLossKey = "MaxHpLoss";

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new HealVar(0m),
		new DynamicVar("MaxHpLoss", 8m)
	});

	public override bool IsAllowed(RunState runState)
	{
		return runState.Players.All((Player p) => (decimal)p.Creature.CurrentHp <= (decimal)p.Creature.MaxHp * 0.70m);
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[2]
		{
			new EventOption(this, Rest, "UNREST_SITE.pages.INITIAL.options.REST", HoverTipFactory.FromCardWithCardHoverTips<PoorSleep>()),
			new EventOption(this, Kill, "UNREST_SITE.pages.INITIAL.options.KILL")
		});
	}

	public override void CalculateVars()
	{
		base.DynamicVars.Heal.BaseValue = base.Owner.Creature.MaxHp - base.Owner.Creature.CurrentHp;
	}

	private async Task Rest()
	{
		await CreatureCmd.Heal(base.Owner.Creature, base.DynamicVars.Heal.BaseValue);
		await CardPileCmd.AddCursesToDeck(new global::_003C_003Ez__ReadOnlySingleElementList<CardModel>(ModelDb.Card<PoorSleep>()), base.Owner);
		SetEventFinished(L10NLookup("UNREST_SITE.pages.REST.description"));
	}

	private async Task Kill()
	{
		await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), base.Owner.Creature, base.DynamicVars["MaxHpLoss"].BaseValue, isFromCard: false);
		RelicModel relic = RelicFactory.PullNextRelicFromFront(base.Owner).ToMutable();
		await RelicCmd.Obtain(relic, base.Owner);
		SetEventFinished(L10NLookup("UNREST_SITE.pages.KILL.description"));
	}
}
