using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Relics;

namespace MegaCrit.Sts2.Core.Models.Events;

public class Vakuu : AncientEventModel
{
	public override Color ButtonColor => new Color(0.05f, 0.06f, 0.12f, 0.8f);

	public override Color DialogueColor => new Color("3C1931");

	public override IEnumerable<EventOption> AllPossibleOptions => Pool1.Concat(Pool2).Concat(Pool3);

	private IEnumerable<EventOption> Pool1 => new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[3]
	{
		RelicOption<BloodSoakedRose>(),
		RelicOption<WhisperingEarring>(),
		RelicOption<Fiddle>()
	});

	private IEnumerable<EventOption> Pool2 => new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[3]
	{
		RelicOption<PreservedFog>(),
		RelicOption<SereTalon>(),
		RelicOption<DistinguishedCape>().ThatDoesDamage(9m)
	});

	private IEnumerable<EventOption> Pool3 => new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[4]
	{
		RelicOption<ChoicesParadox>(),
		RelicOption<MusicBox>(),
		RelicOption<LordsParasol>(),
		RelicOption<JeweledMask>()
	});

	protected override AncientDialogueSet DefineDialogues()
	{
		AncientDialogueSet ancientDialogueSet = new AncientDialogueSet();
		ancientDialogueSet.FirstVisitEverDialogue = new AncientDialogue("");
		ancientDialogueSet.CharacterDialogues = new Dictionary<string, IReadOnlyList<AncientDialogue>>
		{
			[AncientEventModel.CharKey<Ironclad>()] = new global::_003C_003Ez__ReadOnlyArray<AncientDialogue>(new AncientDialogue[3]
			{
				new AncientDialogue("", "", "")
				{
					VisitIndex = 0
				},
				new AncientDialogue("")
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
				new AncientDialogue("", "", "")
				{
					VisitIndex = 0
				},
				new AncientDialogue("")
				{
					VisitIndex = 1
				},
				new AncientDialogue("", "", "")
				{
					VisitIndex = 4
				}
			}),
			[AncientEventModel.CharKey<Defect>()] = new global::_003C_003Ez__ReadOnlyArray<AncientDialogue>(new AncientDialogue[3]
			{
				new AncientDialogue("")
				{
					VisitIndex = 0
				},
				new AncientDialogue("")
				{
					VisitIndex = 1
				},
				new AncientDialogue("", "", "")
				{
					VisitIndex = 4
				}
			}),
			[AncientEventModel.CharKey<Necrobinder>()] = new global::_003C_003Ez__ReadOnlyArray<AncientDialogue>(new AncientDialogue[3]
			{
				new AncientDialogue("", "")
				{
					VisitIndex = 0
				},
				new AncientDialogue("")
				{
					VisitIndex = 1
				},
				new AncientDialogue("", "", "")
				{
					VisitIndex = 4
				}
			}),
			[AncientEventModel.CharKey<Regent>()] = new global::_003C_003Ez__ReadOnlyArray<AncientDialogue>(new AncientDialogue[3]
			{
				new AncientDialogue("", "", "")
				{
					VisitIndex = 0
				},
				new AncientDialogue("")
				{
					VisitIndex = 1
				},
				new AncientDialogue("", "", "")
				{
					VisitIndex = 4
				}
			})
		};
		ancientDialogueSet.AgnosticDialogues = new global::_003C_003Ez__ReadOnlyArray<AncientDialogue>(new AncientDialogue[3]
		{
			new AncientDialogue(""),
			new AncientDialogue(""),
			new AncientDialogue("")
		});
		return ancientDialogueSet;
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		List<EventOption> list = Pool1.ToList();
		List<EventOption> list2 = Pool2.ToList();
		List<EventOption> list3 = Pool3.ToList();
		list.UnstableShuffle(base.Rng);
		list2.UnstableShuffle(base.Rng);
		list3.UnstableShuffle(base.Rng);
		return new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[3]
		{
			list[0],
			list2[0],
			list3[0]
		});
	}
}
