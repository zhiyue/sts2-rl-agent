using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class DollRoom : EventModel
{
	private struct DollChoice : IComparable<DollChoice>
	{
		public RelicModel relic;

		public string descriptionKey;

		public int CompareTo(DollChoice other)
		{
			return relic.CompareTo(other.relic);
		}
	}

	private const string _takeTimeHpLossKey = "TakeTimeHpLoss";

	private const string _examineHpLossKey = "ExamineHpLoss";

	private int? _ambienceHandle;

	private static readonly DollChoice[] _dolls = new DollChoice[3]
	{
		new DollChoice
		{
			relic = ModelDb.Relic<DaughterOfTheWind>(),
			descriptionKey = "DOLL_ROOM.pages.DAUGHTER_OF_WIND.description"
		},
		new DollChoice
		{
			relic = ModelDb.Relic<MrStruggles>(),
			descriptionKey = "DOLL_ROOM.pages.MR_STRUGGLES.description"
		},
		new DollChoice
		{
			relic = ModelDb.Relic<BingBong>(),
			descriptionKey = "DOLL_ROOM.pages.FABLE.description"
		}
	};

	private int? AmbienceHandle
	{
		get
		{
			return _ambienceHandle;
		}
		set
		{
			AssertMutable();
			_ambienceHandle = value;
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DamageVar("TakeTimeHpLoss", 5m, ValueProp.Unblockable | ValueProp.Unpowered),
		new DamageVar("ExamineHpLoss", 15m, ValueProp.Unblockable | ValueProp.Unpowered)
	});

	public override bool IsAllowed(RunState runState)
	{
		return runState.CurrentActIndex == 1;
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[3]
		{
			new EventOption(this, ChooseRandom, "DOLL_ROOM.pages.INITIAL.options.RANDOM"),
			new EventOption(this, TakeSomeTime, "DOLL_ROOM.pages.INITIAL.options.TAKE_SOME_TIME").ThatDoesDamage(base.DynamicVars["TakeTimeHpLoss"].BaseValue),
			new EventOption(this, Examine, "DOLL_ROOM.pages.INITIAL.options.EXAMINE").ThatDoesDamage(base.DynamicVars["ExamineHpLoss"].BaseValue)
		});
	}

	protected override Task BeforeEventStarted()
	{
		if (LocalContext.IsMe(base.Owner) && TestMode.IsOff)
		{
			AmbienceHandle = NDebugAudioManager.Instance.Play("doll_room_amb.mp3");
		}
		return Task.CompletedTask;
	}

	private async Task ChooseRandom()
	{
		await ChooseDollAndShowDescription(base.Owner.RunState.Rng.Niche.NextItem(_dolls));
	}

	private async Task TakeSomeTime()
	{
		await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), base.Owner.Creature, (DamageVar)base.DynamicVars["TakeTimeHpLoss"], null, null);
		IEnumerable<DollChoice> enumerable = _dolls.ToList().StableShuffle(base.Rng).Take(2);
		List<EventOption> list = new List<EventOption>();
		foreach (DollChoice item in enumerable)
		{
			list.Add(OptionFromChoice(item));
		}
		SetEventState(L10NLookup("DOLL_ROOM.pages.TAKE_SOME_TIME.description"), list);
	}

	private async Task Examine()
	{
		await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), base.Owner.Creature, (DamageVar)base.DynamicVars["ExamineHpLoss"], null, null);
		IEnumerable<DollChoice> enumerable = _dolls.ToList().StableShuffle(base.Rng);
		List<EventOption> list = new List<EventOption>();
		foreach (DollChoice item in enumerable)
		{
			list.Add(OptionFromChoice(item));
		}
		SetEventState(L10NLookup("DOLL_ROOM.pages.EXAMINE.description"), list);
	}

	private EventOption OptionFromChoice(DollChoice choice)
	{
		LocString title = choice.relic.Title;
		LocString locString = L10NLookup("DOLL_ROOM.pages.TAKE.options.TAKE.description");
		locString.Add("RelicName", choice.relic.Title);
		return new EventOption(this, Func, title, locString, choice.relic.Title.GetRawText(), HoverTipFactory.FromRelic(choice.relic)).WithOverridenHistoryName(choice.relic.Title);
		Task Func()
		{
			return ChooseDollAndShowDescription(choice);
		}
	}

	private async Task ChooseDollAndShowDescription(DollChoice choice)
	{
		StopAudio();
		await RelicCmd.Obtain(choice.relic.ToMutable(), base.Owner);
		SetEventFinished(L10NLookup(choice.descriptionKey));
	}

	protected override void OnEventFinished()
	{
		StopAudio();
	}

	private void StopAudio()
	{
		if (AmbienceHandle.HasValue)
		{
			NDebugAudioManager.Instance.Stop(AmbienceHandle.Value);
			AmbienceHandle = null;
		}
	}
}
