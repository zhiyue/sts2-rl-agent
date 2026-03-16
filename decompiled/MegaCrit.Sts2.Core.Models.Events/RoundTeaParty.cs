using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class RoundTeaParty : EventModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DamageVar(11m, ValueProp.Unblockable | ValueProp.Unpowered),
		new StringVar("Relic", ModelDb.Relic<RoyalPoison>().Title.GetFormattedText())
	});

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[2]
		{
			new EventOption(this, EnjoyTea, "ROUND_TEA_PARTY.pages.INITIAL.options.ENJOY_TEA", HoverTipFactory.FromRelic<RoyalPoison>()),
			new EventOption(this, PickFight, "ROUND_TEA_PARTY.pages.INITIAL.options.PICK_FIGHT")
		});
	}

	private async Task EnjoyTea()
	{
		Creature targetCreature = base.Owner.Creature;
		await RelicCmd.Obtain<RoyalPoison>(base.Owner);
		await CreatureCmd.Heal(targetCreature, targetCreature.MaxHp - targetCreature.CurrentHp);
		SetEventFinished(L10NLookup("ROUND_TEA_PARTY.pages.ENJOY_TEA.description"));
	}

	private Task PickFight()
	{
		SetEventState(L10NLookup("ROUND_TEA_PARTY.pages.PICK_FIGHT.description"), new global::_003C_003Ez__ReadOnlySingleElementList<EventOption>(new EventOption(this, ContinueFight, "ROUND_TEA_PARTY.pages.PICK_FIGHT.options.CONTINUE_FIGHT").ThatWontSaveToChoiceHistory()));
		return Task.CompletedTask;
	}

	private async Task ContinueFight()
	{
		await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), base.Owner.Creature, base.DynamicVars.Damage, null, null);
		await RelicCmd.Obtain(RelicFactory.PullNextRelicFromFront(base.Owner).ToMutable(), base.Owner);
		SetEventFinished(L10NLookup("ROUND_TEA_PARTY.pages.CONTINUE_FIGHT.description"));
	}
}
