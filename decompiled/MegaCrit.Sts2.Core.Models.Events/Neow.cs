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

	public override IEnumerable<EventOption> AllPossibleOptions => PositiveOptions.Concat(CurseOptions).Concat(new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[7] { ClericOption, BundleOption, EmpowerOption, ToughnessOption, SafetyOption, PatienceOption, ScavengerOption }));

	private IEnumerable<EventOption> PositiveOptions => new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[9]
	{
		RelicOption<ArcaneScroll>("INITIAL", "NEOW.pages.DONE.POSITIVE.description"),
		RelicOption<BoomingConch>("INITIAL", "NEOW.pages.DONE.POSITIVE.description"),
		RelicOption<Pomander>("INITIAL", "NEOW.pages.DONE.POSITIVE.description"),
		RelicOption<GoldenPearl>("INITIAL", "NEOW.pages.DONE.POSITIVE.description"),
		RelicOption<LeadPaperweight>("INITIAL", "NEOW.pages.DONE.POSITIVE.description"),
		RelicOption<NewLeaf>("INITIAL", "NEOW.pages.DONE.POSITIVE.description"),
		RelicOption<NeowsTorment>("INITIAL", "NEOW.pages.DONE.POSITIVE.description"),
		RelicOption<PreciseScissors>("INITIAL", "NEOW.pages.DONE.POSITIVE.description"),
		RelicOption<LostCoffer>("INITIAL", "NEOW.pages.DONE.POSITIVE.description")
	});

	private EventOption ToughnessOption => RelicOption<NutritiousOyster>("INITIAL", "NEOW.pages.DONE.POSITIVE.description");

	private EventOption SafetyOption => RelicOption<StoneHumidifier>("INITIAL", "NEOW.pages.DONE.POSITIVE.description");

	private EventOption ClericOption => RelicOption<MassiveScroll>("INITIAL", "NEOW.pages.DONE.POSITIVE.description");

	private EventOption PatienceOption => RelicOption<LavaRock>("INITIAL", "NEOW.pages.DONE.POSITIVE.description");

	private EventOption ScavengerOption => RelicOption<SmallCapsule>("INITIAL", "NEOW.pages.DONE.POSITIVE.description");

	private EventOption EmpowerOption => RelicOption<SilverCrucible>("INITIAL", "NEOW.pages.DONE.CURSED.description");

	private IEnumerable<EventOption> CurseOptions => new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[4]
	{
		RelicOption<CursedPearl>("INITIAL", "NEOW.pages.DONE.POSITIVE.description"),
		RelicOption<LargeCapsule>("INITIAL", "NEOW.pages.DONE.CURSED.description"),
		RelicOption<LeafyPoultice>("INITIAL", "NEOW.pages.DONE.CURSED.description"),
		RelicOption<PrecariousShears>("INITIAL", "NEOW.pages.DONE.CURSED.description")
	});

	private EventOption BundleOption => RelicOption<ScrollBoxes>("INITIAL", "NEOW.pages.DONE.CURSED.description");

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
			if (ScrollBoxes.CanGenerateBundles(base.Owner))
			{
				list.Add(BundleOption);
			}
			if (base.Owner.RunState.Players.Count == 1)
			{
				list.Add(EmpowerOption);
			}
			EventOption eventOption = base.Rng.NextItem(list);
			List<EventOption> list2 = PositiveOptions.ToList();
			if (eventOption.Relic is CursedPearl)
			{
				list2.RemoveAll((EventOption o) => o.Relic is GoldenPearl);
			}
			if (eventOption.Relic is PrecariousShears)
			{
				list2.RemoveAll((EventOption o) => o.Relic is PreciseScissors);
			}
			if (eventOption.Relic is LeafyPoultice)
			{
				list2.RemoveAll((EventOption o) => o.Relic is NewLeaf);
			}
			if (base.Owner.RunState.Players.Count > 1)
			{
				list2.Add(ClericOption);
			}
			if (base.Rng.NextBool())
			{
				list2.Add(ToughnessOption);
			}
			else
			{
				list2.Add(SafetyOption);
			}
			if (!(eventOption.Relic is LargeCapsule))
			{
				if (base.Rng.NextBool())
				{
					list2.Add(PatienceOption);
				}
				else
				{
					list2.Add(ScavengerOption);
				}
			}
			List<EventOption> list3 = list2.ToList().UnstableShuffle(base.Rng).Take(2)
				.ToList();
			list3.Add(eventOption);
			return list3;
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
