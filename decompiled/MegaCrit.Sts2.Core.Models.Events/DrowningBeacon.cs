using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class DrowningBeacon : EventModel
{
	private const string _potionKey = "Potion";

	private const string _relicKey = "Relic";

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[3]
	{
		new HpLossVar(13m),
		new StringVar("Potion", ModelDb.Potion<GlowwaterPotion>().Title.GetFormattedText()),
		new StringVar("Relic", ModelDb.Relic<FresnelLens>().Title.GetFormattedText())
	});

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[2]
		{
			new EventOption(this, BottleOption, "DROWNING_BEACON.pages.INITIAL.options.BOTTLE", HoverTipFactory.FromPotion(ModelDb.Potion<GlowwaterPotion>())),
			new EventOption(this, ClimbOption, "DROWNING_BEACON.pages.INITIAL.options.CLIMB", HoverTipFactory.FromRelic<FresnelLens>())
		});
	}

	private async Task BottleOption()
	{
		await RewardsCmd.OfferCustom(base.Owner, new List<Reward>(1)
		{
			new PotionReward(ModelDb.Potion<GlowwaterPotion>().ToMutable(), base.Owner)
		});
		SetEventFinished(L10NLookup("DROWNING_BEACON.pages.BOTTLE.description"));
	}

	private async Task ClimbOption()
	{
		await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), base.Owner.Creature, base.DynamicVars.HpLoss.BaseValue, isFromCard: false);
		await RelicCmd.Obtain(ModelDb.Relic<FresnelLens>().ToMutable(), base.Owner);
		SetEventFinished(L10NLookup("DROWNING_BEACON.pages.CLIMB.description"));
	}
}
