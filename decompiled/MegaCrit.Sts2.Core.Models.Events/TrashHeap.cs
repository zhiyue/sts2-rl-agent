using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class TrashHeap : EventModel
{
	private static RelicModel[] Relics => new RelicModel[5]
	{
		ModelDb.Relic<DarkstonePeriapt>(),
		ModelDb.Relic<DreamCatcher>(),
		ModelDb.Relic<HandDrill>(),
		ModelDb.Relic<MawBank>(),
		ModelDb.Relic<TheBoot>()
	};

	private static CardModel[] Cards => new CardModel[10]
	{
		ModelDb.Card<Caltrops>(),
		ModelDb.Card<Clash>(),
		ModelDb.Card<Distraction>(),
		ModelDb.Card<DualWield>(),
		ModelDb.Card<Entrench>(),
		ModelDb.Card<HelloWorld>(),
		ModelDb.Card<Outmaneuver>(),
		ModelDb.Card<Rebound>(),
		ModelDb.Card<RipAndTear>(),
		ModelDb.Card<Stack>()
	};

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new HpLossVar(8m),
		new GoldVar(100)
	});

	public override bool IsAllowed(RunState runState)
	{
		return runState.Players.All((Player player) => player.Creature.CurrentHp > 5);
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[2]
		{
			new EventOption(this, DiveIn, "TRASH_HEAP.pages.INITIAL.options.DIVE_IN").ThatDoesDamage(base.DynamicVars.HpLoss.IntValue),
			new EventOption(this, Grab, "TRASH_HEAP.pages.INITIAL.options.GRAB")
		});
	}

	private async Task DiveIn()
	{
		await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), base.Owner.Creature, base.DynamicVars.HpLoss.IntValue, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
		RelicModel relicModel = base.Rng.NextItem(Relics);
		await RelicCmd.Obtain(relicModel.ToMutable(), base.Owner);
		SetEventFinished(L10NLookup("TRASH_HEAP.pages.DIVE_IN.description"));
	}

	private async Task Grab()
	{
		await PlayerCmd.GainGold(base.DynamicVars.Gold.BaseValue, base.Owner);
		CardModel card = base.Owner.RunState.CreateCard(base.Rng.NextItem(Cards), base.Owner);
		CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck), 2f);
		SetEventFinished(L10NLookup("TRASH_HEAP.pages.GRAB.description"));
	}
}
