using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Godot;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Relics;

namespace MegaCrit.Sts2.Core.Models.Events;

public class Darv : AncientEventModel
{
	private struct ValidRelicSet
	{
		public readonly Func<Player, bool> filter;

		public readonly RelicModel[] relics;

		public ValidRelicSet(Func<Player, bool> filter, RelicModel[] relics)
		{
			this.filter = filter;
			this.relics = relics;
		}

		public ValidRelicSet(RelicModel[] relics)
		{
			filter = (Player _) => true;
			this.relics = relics;
		}
	}

	private const string _sfxExcited = "event:/sfx/npcs/darv/darv_excited";

	private const string _sfxOuttaTheWay = "event:/sfx/npcs/darv/darv_outta_the_way";

	private const string _sfxFear = "event:/sfx/npcs/darv/darv_fear";

	private const string _sfxPain = "event:/sfx/npcs/darv/darv_pain";

	private const string _sfxEndeared = "event:/sfx/npcs/darv/darv_endeared";

	private const string _sfxIntroduction = "event:/sfx/npcs/darv/darv_introduction";

	private static readonly List<ValidRelicSet> _validRelicSets;

	public override Color ButtonColor => new Color(0.06f, 0f, 0.08f, 0.5f);

	public override Color DialogueColor => new Color("512E66");

	public override IEnumerable<EventOption> AllPossibleOptions => (from r in _validRelicSets.SelectMany((ValidRelicSet s) => s.relics)
		select RelicOption(r.ToMutable())).Concat(new global::_003C_003Ez__ReadOnlySingleElementList<EventOption>(RelicOption<DustyTome>()));

	protected override AncientDialogueSet DefineDialogues()
	{
		AncientDialogueSet ancientDialogueSet = new AncientDialogueSet();
		ancientDialogueSet.FirstVisitEverDialogue = new AncientDialogue("event:/sfx/npcs/darv/darv_introduction");
		ancientDialogueSet.CharacterDialogues = new Dictionary<string, IReadOnlyList<AncientDialogue>>
		{
			[AncientEventModel.CharKey<Ironclad>()] = new global::_003C_003Ez__ReadOnlyArray<AncientDialogue>(new AncientDialogue[3]
			{
				new AncientDialogue("event:/sfx/npcs/darv/darv_introduction")
				{
					VisitIndex = 0
				},
				new AncientDialogue("event:/sfx/npcs/darv/darv_endeared")
				{
					VisitIndex = 1
				},
				new AncientDialogue("", "", "")
				{
					VisitIndex = 4
				}
			}),
			[AncientEventModel.CharKey<Silent>()] = new global::_003C_003Ez__ReadOnlyArray<AncientDialogue>(new AncientDialogue[3]
			{
				new AncientDialogue("event:/sfx/npcs/darv/darv_introduction")
				{
					VisitIndex = 0
				},
				new AncientDialogue("event:/sfx/npcs/darv/darv_excited")
				{
					VisitIndex = 1
				},
				new AncientDialogue("event:/sfx/npcs/darv/darv_pain", "", "event:/sfx/npcs/darv/darv_outta_the_way")
				{
					VisitIndex = 4
				}
			}),
			[AncientEventModel.CharKey<Defect>()] = new global::_003C_003Ez__ReadOnlyArray<AncientDialogue>(new AncientDialogue[3]
			{
				new AncientDialogue("event:/sfx/npcs/darv/darv_introduction", "")
				{
					VisitIndex = 0
				},
				new AncientDialogue("event:/sfx/npcs/darv/darv_endeared")
				{
					VisitIndex = 1
				},
				new AncientDialogue("event:/sfx/npcs/darv/darv_fear", "", "event:/sfx/npcs/darv/darv_fear")
				{
					VisitIndex = 4
				}
			}),
			[AncientEventModel.CharKey<Necrobinder>()] = new global::_003C_003Ez__ReadOnlyArray<AncientDialogue>(new AncientDialogue[3]
			{
				new AncientDialogue("event:/sfx/npcs/darv/darv_introduction", "", "event:/sfx/npcs/darv/darv_excited")
				{
					VisitIndex = 0
				},
				new AncientDialogue("event:/sfx/npcs/darv/darv_endeared")
				{
					VisitIndex = 1
				},
				new AncientDialogue("event:/sfx/npcs/darv/darv_excited", "", "event:/sfx/npcs/darv/darv_fear")
				{
					VisitIndex = 4
				}
			}),
			[AncientEventModel.CharKey<Regent>()] = new global::_003C_003Ez__ReadOnlyArray<AncientDialogue>(new AncientDialogue[3]
			{
				new AncientDialogue("event:/sfx/npcs/darv/darv_introduction", "", "event:/sfx/npcs/darv/darv_excited")
				{
					VisitIndex = 0
				},
				new AncientDialogue("event:/sfx/npcs/darv/darv_introduction")
				{
					VisitIndex = 1
				},
				new AncientDialogue("event:/sfx/npcs/darv/darv_excited", "", "event:/sfx/npcs/darv/darv_pain")
				{
					VisitIndex = 4
				}
			})
		};
		ancientDialogueSet.AgnosticDialogues = new global::_003C_003Ez__ReadOnlyArray<AncientDialogue>(new AncientDialogue[2]
		{
			new AncientDialogue("event:/sfx/npcs/darv/darv_excited"),
			new AncientDialogue("event:/sfx/npcs/darv/darv_outta_the_way")
		});
		return ancientDialogueSet;
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		List<EventOption> source = (from rs in _validRelicSets
			where rs.filter(base.Owner)
			select RelicOption(base.Rng.NextItem(rs.relics).ToMutable())).ToList().UnstableShuffle(base.Rng);
		List<EventOption> list;
		if (base.Rng.NextBool())
		{
			list = source.Take(2).ToList();
			DustyTome dustyTome = (DustyTome)ModelDb.Relic<DustyTome>().ToMutable();
			if (base.Owner != null)
			{
				dustyTome.SetupForPlayer(base.Owner);
			}
			list.Add(RelicOption(dustyTome));
		}
		else
		{
			list = source.Take(3).ToList();
		}
		return list;
	}

	static Darv()
	{
		int num = 9;
		List<ValidRelicSet> list = new List<ValidRelicSet>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<ValidRelicSet> span = CollectionsMarshal.AsSpan(list);
		int num2 = 0;
		span[num2] = new ValidRelicSet(new RelicModel[1] { ModelDb.Relic<Astrolabe>() });
		num2++;
		span[num2] = new ValidRelicSet(new RelicModel[1] { ModelDb.Relic<BlackStar>() });
		num2++;
		span[num2] = new ValidRelicSet(new RelicModel[1] { ModelDb.Relic<CallingBell>() });
		num2++;
		span[num2] = new ValidRelicSet(new RelicModel[1] { ModelDb.Relic<EmptyCage>() });
		num2++;
		span[num2] = new ValidRelicSet((Player owner) => !owner.RunState.Modifiers.Any((ModifierModel m) => m.ClearsPlayerDeck), new RelicModel[1] { ModelDb.Relic<PandorasBox>() });
		num2++;
		span[num2] = new ValidRelicSet(new RelicModel[1] { ModelDb.Relic<RunicPyramid>() });
		num2++;
		span[num2] = new ValidRelicSet(new RelicModel[1] { ModelDb.Relic<SneckoEye>() });
		num2++;
		span[num2] = new ValidRelicSet((Player owner) => owner.RunState.CurrentActIndex == 1, new RelicModel[2]
		{
			ModelDb.Relic<Ectoplasm>(),
			ModelDb.Relic<Sozu>()
		});
		num2++;
		span[num2] = new ValidRelicSet((Player owner) => owner.RunState.CurrentActIndex == 2, new RelicModel[2]
		{
			ModelDb.Relic<PhilosophersStone>(),
			ModelDb.Relic<VelvetChoker>()
		});
		_validRelicSets = list;
	}
}
