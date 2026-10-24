using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Rewards;

namespace MegaCrit.Sts2.Core.Models.Events.Mocks;

public class MockEventModel : EventModel
{
	public bool isShared;

	public int? optionChosen;

	public List<EventOption>? initialOptions;

	public override bool IsShared => isShared;

	public string OptionKey => base.Id.Entry + ".pages.INITIAL.options.TEST";

	private List<EventOption> DefaultInitialOptions
	{
		get
		{
			int num = 2;
			List<EventOption> list = new List<EventOption>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<EventOption> span = CollectionsMarshal.AsSpan(list);
			int num2 = 0;
			span[num2] = new EventOption(this, delegate
			{
				optionChosen = 0;
				return Task.CompletedTask;
			}, OptionKey);
			num2++;
			span[num2] = new EventOption(this, delegate
			{
				optionChosen = 1;
				return Task.CompletedTask;
			}, OptionKey);
			return list;
		}
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return initialOptions ?? DefaultInitialOptions;
	}

	public new void EnterCombatWithoutExitingEvent<T>(IReadOnlyList<Reward> extraRewards, bool shouldResumeAfterCombat) where T : EncounterModel
	{
		base.EnterCombatWithoutExitingEvent<T>(extraRewards, shouldResumeAfterCombat);
	}

	public void SetEventState(IEnumerable<EventOption> options)
	{
		SetEventState(L10NLookup(""), options);
	}

	public void SetEventFinished()
	{
		SetEventFinished(L10NLookup(""));
	}
}
