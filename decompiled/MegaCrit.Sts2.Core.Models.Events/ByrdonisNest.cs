using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class ByrdonisNest : EventModel
{
	private const string _cardKey = "Card";

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new MaxHpVar(7m),
		new StringVar("Card", ModelDb.Card<ByrdonisEgg>().Title)
	});

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[2]
		{
			new EventOption(this, Eat, "BYRDONIS_NEST.pages.INITIAL.options.EAT"),
			new EventOption(this, Take, "BYRDONIS_NEST.pages.INITIAL.options.TAKE", HoverTipFactory.FromCardWithCardHoverTips<ByrdonisEgg>())
		});
	}

	public override bool IsAllowed(RunState runState)
	{
		return runState.Players.All((Player p) => !p.HasEventPet());
	}

	private async Task Eat()
	{
		await CreatureCmd.GainMaxHp(base.Owner.Creature, base.DynamicVars.MaxHp.BaseValue);
		SetEventFinished(L10NLookup("BYRDONIS_NEST.pages.EAT.description"));
	}

	private async Task Take()
	{
		CardModel card = base.Owner.RunState.CreateCard<ByrdonisEgg>(base.Owner);
		CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck), 2f);
		SetEventFinished(L10NLookup("BYRDONIS_NEST.pages.TAKE.description"));
	}
}
