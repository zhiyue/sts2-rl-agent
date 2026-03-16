using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Rewards;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class TheLanternKey : EventModel
{
	public override EventLayoutType LayoutType => EventLayoutType.Combat;

	public override EncounterModel CanonicalEncounter => ModelDb.Encounter<MysteriousKnightEventEncounter>();

	public override bool IsShared => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new GoldVar(100));

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[2]
		{
			new EventOption(this, ReturnTheKey, "THE_LANTERN_KEY.pages.INITIAL.options.RETURN_THE_KEY"),
			new EventOption(this, KeepTheKey, "THE_LANTERN_KEY.pages.INITIAL.options.KEEP_THE_KEY")
		});
	}

	private async Task ReturnTheKey()
	{
		await PlayerCmd.GainGold(base.DynamicVars.Gold.BaseValue, base.Owner);
		SetEventFinished(L10NLookup("THE_LANTERN_KEY.pages.DONE.options.RETURN_THE_KEY.description"));
	}

	private Task KeepTheKey()
	{
		SetEventState(L10NLookup("THE_LANTERN_KEY.pages.KEEP_THE_KEY.description"), new global::_003C_003Ez__ReadOnlySingleElementList<EventOption>(new EventOption(this, Fight, "THE_LANTERN_KEY.pages.KEEP_THE_KEY.options.FIGHT")));
		return Task.CompletedTask;
	}

	private Task Fight()
	{
		EnterCombatWithoutExitingEvent<MysteriousKnightEventEncounter>(new global::_003C_003Ez__ReadOnlySingleElementList<Reward>(new SpecialCardReward(base.Owner.RunState.CreateCard<LanternKey>(base.Owner), base.Owner)), shouldResumeAfterCombat: false);
		return Task.CompletedTask;
	}
}
