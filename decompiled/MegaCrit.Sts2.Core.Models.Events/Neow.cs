using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Relics;

namespace MegaCrit.Sts2.Core.Models.Events;

public class Neow : AncientEventModel
{
	private const string _cursedChoiceDoneDescriptionOverride = "NEOW.pages.DONE.CURSED.description";

	private const string _positiveChoiceDoneDescriptionOverride = "NEOW.pages.DONE.POSITIVE.description";

	private const string _sfxSleepy = "event:/sfx/npcs/neow/neow_sleepy";

	private const string _sfxWelcome = "event:/sfx/npcs/neow/neow_welcome";

	private const string _sfxCurious = "event:/sfx/npcs/neow/neow_curious";

	private List<EventOption>? _modifierOptions;

	public override string AmbientBgm => "event:/sfx/ambience/act1_neow";

	public override Color ButtonColor => new Color(0f, 0.1f, 0.2f, 0.5f);

	public override Color DialogueColor => new Color("28454F");

	public override LocString InitialDescription
	{
		get
		{
			Player? owner = base.Owner;
			if (owner != null && owner.RunState.Modifiers.Count <= 0)
			{
				return base.InitialDescription;
			}
			return L10NLookup(base.Id.Entry + ".EVENT.description");
		}
	}

	public override IEnumerable<EventOption> AllPossibleOptions
	{
		get
		{
			List<EventOption> list = new List<EventOption>();
			list.AddRange(CurseOptions);
			list.AddRange(PositiveOptions);
			list.Add(LavaRockOption);
			list.Add(NeowsTalismanOption);
			list.Add(NutritiousOysterOption);
			list.Add(PomanderOption);
			list.Add(SmallCapsuleOption);
			list.Add(StoneHumidifierOption);
			return new _003C_003Ez__ReadOnlyList<EventOption>(list);
		}
	}

	private IEnumerable<EventOption> PositiveOptions => new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[14]
	{
		RelicOption<ArcaneScroll>("INITIAL", "NEOW.pages.DONE.POSITIVE.description"),
		RelicOption<BoomingConch>("INITIAL", "NEOW.pages.DONE.POSITIVE.description"),
		RelicOption<FishingRod>("INITIAL", "NEOW.pages.DONE.POSITIVE.description"),
		RelicOption<GoldenPearl>("INITIAL", "NEOW.pages.DONE.POSITIVE.description"),
		RelicOption<Kaleidoscope>("INITIAL", "NEOW.pages.DONE.POSITIVE.description"),
		RelicOption<LeadPaperweight>("INITIAL", "NEOW.pages.DONE.POSITIVE.description"),
		RelicOption<LostCoffer>("INITIAL", "NEOW.pages.DONE.POSITIVE.description"),
		RelicOption<MassiveScroll>("INITIAL", "NEOW.pages.DONE.POSITIVE.description"),
		RelicOption<NeowsTorment>("INITIAL", "NEOW.pages.DONE.POSITIVE.description"),
		RelicOption<NewLeaf>("INITIAL", "NEOW.pages.DONE.POSITIVE.description"),
		RelicOption<PhialHolster>("INITIAL", "NEOW.pages.DONE.POSITIVE.description"),
		RelicOption<PreciseScissors>("INITIAL", "NEOW.pages.DONE.POSITIVE.description"),
		RelicOption<ScrollBoxes>("INITIAL", "NEOW.pages.DONE.POSITIVE.description"),
		RelicOption<WingedBoots>("INITIAL", "NEOW.pages.DONE.POSITIVE.description")
	});

	private EventOption LavaRockOption => RelicOption<LavaRock>("INITIAL", "NEOW.pages.DONE.POSITIVE.description");

	private EventOption NeowsTalismanOption => RelicOption<NeowsTalisman>("INITIAL", "NEOW.pages.DONE.POSITIVE.description");

	private EventOption NutritiousOysterOption => RelicOption<NutritiousOyster>("INITIAL", "NEOW.pages.DONE.POSITIVE.description");

	private EventOption PomanderOption => RelicOption<Pomander>("INITIAL", "NEOW.pages.DONE.POSITIVE.description");

	private EventOption SmallCapsuleOption => RelicOption<SmallCapsule>("INITIAL", "NEOW.pages.DONE.POSITIVE.description");

	private EventOption StoneHumidifierOption => RelicOption<StoneHumidifier>("INITIAL", "NEOW.pages.DONE.POSITIVE.description");

	private IEnumerable<EventOption> CurseOptions => new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[8]
	{
		RelicOption<CursedPearl>("INITIAL", "NEOW.pages.DONE.CURSED.description"),
		RelicOption<HeftyTablet>("INITIAL", "NEOW.pages.DONE.CURSED.description"),
		RelicOption<LargeCapsule>("INITIAL", "NEOW.pages.DONE.CURSED.description"),
		RelicOption<LeafyPoultice>("INITIAL", "NEOW.pages.DONE.CURSED.description"),
		RelicOption<NeowsBones>("INITIAL", "NEOW.pages.DONE.POSITIVE.description"),
		RelicOption<PrecariousShears>("INITIAL", "NEOW.pages.DONE.CURSED.description"),
		RelicOption<SilkenTress>("INITIAL", "NEOW.pages.DONE.CURSED.description"),
		RelicOption<SilverCrucible>("INITIAL", "NEOW.pages.DONE.CURSED.description")
	});

	private List<EventOption> ModifierOptions
	{
		get
		{
			AssertMutable();
			if (_modifierOptions == null)
			{
				_modifierOptions = new List<EventOption>();
			}
			return _modifierOptions;
		}
	}

	protected override AncientDialogueSet DefineDialogues()
	{
		AncientDialogueSet ancientDialogueSet = new AncientDialogueSet();
		ancientDialogueSet.FirstVisitEverDialogue = new AncientDialogue("event:/sfx/npcs/neow/neow_welcome");
		ancientDialogueSet.CharacterDialogues = new Dictionary<string, IReadOnlyList<AncientDialogue>>
		{
			[AncientEventModel.CharKey<Ironclad>()] = new global::_003C_003Ez__ReadOnlyArray<AncientDialogue>(new AncientDialogue[3]
			{
				new AncientDialogue("event:/sfx/npcs/neow/neow_welcome")
				{
					VisitIndex = 0
				},
				new AncientDialogue("event:/sfx/npcs/neow/neow_curious")
				{
					VisitIndex = 1
				},
				new AncientDialogue("event:/sfx/npcs/neow/neow_sleepy", "event:/sfx/npcs/neow/neow_sleepy", "event:/sfx/npcs/neow/neow_sleepy")
				{
					VisitIndex = 4
				}
			}),
			[AncientEventModel.CharKey<Silent>()] = new global::_003C_003Ez__ReadOnlyArray<AncientDialogue>(new AncientDialogue[3]
			{
				new AncientDialogue("event:/sfx/npcs/neow/neow_curious")
				{
					VisitIndex = 0
				},
				new AncientDialogue("event:/sfx/npcs/neow/neow_sleepy")
				{
					VisitIndex = 1
				},
				new AncientDialogue("event:/sfx/npcs/neow/neow_sleepy", "", "")
				{
					VisitIndex = 4
				}
			}),
			[AncientEventModel.CharKey<Defect>()] = new global::_003C_003Ez__ReadOnlyArray<AncientDialogue>(new AncientDialogue[3]
			{
				new AncientDialogue("event:/sfx/npcs/neow/neow_curious")
				{
					VisitIndex = 0
				},
				new AncientDialogue("event:/sfx/npcs/neow/neow_sleepy")
				{
					VisitIndex = 1
				},
				new AncientDialogue("event:/sfx/npcs/neow/neow_sleepy", "", "event:/sfx/npcs/neow/neow_sleepy")
				{
					VisitIndex = 4
				}
			}),
			[AncientEventModel.CharKey<Necrobinder>()] = new global::_003C_003Ez__ReadOnlyArray<AncientDialogue>(new AncientDialogue[3]
			{
				new AncientDialogue("event:/sfx/npcs/neow/neow_welcome", "", "event:/sfx/npcs/neow/neow_sleepy")
				{
					VisitIndex = 0
				},
				new AncientDialogue("event:/sfx/npcs/neow/neow_sleepy")
				{
					VisitIndex = 1
				},
				new AncientDialogue("", "event:/sfx/npcs/neow/neow_sleepy")
				{
					VisitIndex = 4
				}
			}),
			[AncientEventModel.CharKey<Regent>()] = new global::_003C_003Ez__ReadOnlyArray<AncientDialogue>(new AncientDialogue[3]
			{
				new AncientDialogue("", "event:/sfx/npcs/neow/neow_sleepy")
				{
					VisitIndex = 0
				},
				new AncientDialogue("event:/sfx/npcs/neow/neow_curious")
				{
					VisitIndex = 1
				},
				new AncientDialogue("", "event:/sfx/npcs/neow/neow_sleepy", "")
				{
					VisitIndex = 4
				}
			})
		};
		ancientDialogueSet.AgnosticDialogues = new global::_003C_003Ez__ReadOnlyArray<AncientDialogue>(new AncientDialogue[5]
		{
			new AncientDialogue("event:/sfx/npcs/neow/neow_welcome"),
			new AncientDialogue("event:/sfx/npcs/neow/neow_welcome"),
			new AncientDialogue("event:/sfx/npcs/neow/neow_welcome"),
			new AncientDialogue("event:/sfx/npcs/neow/neow_welcome"),
			new AncientDialogue("")
		});
		return ancientDialogueSet;
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		if (base.Owner.RunState.Modifiers.Count <= 0)
		{
			List<EventOption> list = CurseOptions.ToList();
			list.RemoveAll(delegate(EventOption r)
			{
				RelicModel? relic = r.Relic;
				return relic != null && !relic.IsAllowedAtNeow(base.Owner);
			});
			EventOption eventOption = base.Rng.NextItem(list);
			List<EventOption> list2 = PositiveOptions.ToList();
			if (eventOption.Relic is CursedPearl)
			{
				list2.RemoveAll((EventOption o) => o.Relic is GoldenPearl);
			}
			if (eventOption.Relic is HeftyTablet)
			{
				list2.RemoveAll((EventOption o) => o.Relic is ArcaneScroll);
			}
			if (eventOption.Relic is LeafyPoultice)
			{
				list2.RemoveAll((EventOption o) => o.Relic is NewLeaf);
			}
			if (eventOption.Relic is PrecariousShears)
			{
				list2.RemoveAll((EventOption o) => o.Relic is PreciseScissors);
			}
			if (!(eventOption.Relic is LargeCapsule))
			{
				if (base.Rng.NextBool())
				{
					list2.Add(LavaRockOption);
				}
				else
				{
					list2.Add(SmallCapsuleOption);
				}
			}
			if (base.Rng.NextBool())
			{
				list2.Add(NutritiousOysterOption);
			}
			else
			{
				list2.Add(StoneHumidifierOption);
			}
			if (base.Rng.NextBool())
			{
				list2.Add(NeowsTalismanOption);
			}
			else
			{
				list2.Add(PomanderOption);
			}
			list2.RemoveAll(delegate(EventOption r)
			{
				RelicModel? relic = r.Relic;
				return relic != null && !relic.IsAllowedAtNeow(base.Owner);
			});
			List<EventOption> list3 = new List<EventOption>();
			list3.AddRange(list2.ToList().UnstableShuffle(base.Rng).Take(2));
			list3.Add(eventOption);
			return new _003C_003Ez__ReadOnlyList<EventOption>(list3);
		}
		foreach (ModifierModel modifier in base.Owner.RunState.Modifiers)
		{
			Func<Task> neowOption = modifier.GenerateNeowOption(this);
			if (neowOption != null)
			{
				int optionIndex = ModifierOptions.Count;
				ModifierOptions.Add(new EventOption(this, () => OnModifierOptionSelected(neowOption, optionIndex), modifier.NeowOptionTitle, modifier.NeowOptionDescription, modifier.Id.Entry, modifier.HoverTips.ToArray()));
			}
		}
		if (ModifierOptions.Count > 0)
		{
			return new global::_003C_003Ez__ReadOnlySingleElementList<EventOption>(ModifierOptions[0]);
		}
		return Array.Empty<EventOption>();
	}

	private async Task OnModifierOptionSelected(Func<Task> modifierFunc, int index)
	{
		await modifierFunc();
		if (index + 1 >= ModifierOptions.Count)
		{
			SetEventFinished(L10NLookup(base.Id.Entry + ".pages.DONE.description"));
		}
		else
		{
			SetEventState(InitialDescription, new global::_003C_003Ez__ReadOnlySingleElementList<EventOption>(ModifierOptions[index + 1]));
		}
	}
}
