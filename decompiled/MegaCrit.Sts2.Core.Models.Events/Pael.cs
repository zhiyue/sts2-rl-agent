using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Godot;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Relics;

namespace MegaCrit.Sts2.Core.Models.Events;

public class Pael : AncientEventModel
{
	public override IEnumerable<EventOption> AllPossibleOptions => OptionPool1.Concat(OptionPool2).Concat(OptionPool3).Concat(new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[4] { PaelsClawOption, PaelsToothOption, PaelsLegionOption, PaelsGrowthOption }));

	public override Color ButtonColor => new Color(0.03f, 0.08f, 0f, 0.75f);

	public override Color DialogueColor => new Color("332C29");

	private EventOption PaelsClawOption => RelicOption<PaelsClaw>();

	private EventOption PaelsToothOption => RelicOption<PaelsTooth>();

	private EventOption PaelsGrowthOption => RelicOption<PaelsGrowth>();

	private EventOption PaelsLegionOption => RelicOption<PaelsLegion>();

	private IEnumerable<EventOption> OptionPool1 => new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[3]
	{
		RelicOption<PaelsFlesh>(),
		RelicOption<PaelsHorn>(),
		RelicOption<PaelsTears>()
	});

	private List<EventOption> OptionPool2
	{
		get
		{
			int num = 1;
			List<EventOption> list = new List<EventOption>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<EventOption> span = CollectionsMarshal.AsSpan(list);
			int index = 0;
			span[index] = RelicOption<PaelsWing>();
			return list;
		}
	}

	private List<EventOption> OptionPool3
	{
		get
		{
			int num = 2;
			List<EventOption> list = new List<EventOption>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<EventOption> span = CollectionsMarshal.AsSpan(list);
			int num2 = 0;
			span[num2] = RelicOption<PaelsEye>();
			num2++;
			span[num2] = RelicOption<PaelsBlood>();
			return list;
		}
	}

	protected override AncientDialogueSet DefineDialogues()
	{
		AncientDialogueSet ancientDialogueSet = new AncientDialogueSet();
		ancientDialogueSet.FirstVisitEverDialogue = new AncientDialogue("");
		ancientDialogueSet.CharacterDialogues = new Dictionary<string, IReadOnlyList<AncientDialogue>>
		{
			[AncientEventModel.CharKey<Ironclad>()] = new global::_003C_003Ez__ReadOnlyArray<AncientDialogue>(new AncientDialogue[3]
			{
				new AncientDialogue("")
				{
					VisitIndex = 0
				},
				new AncientDialogue("")
				{
					VisitIndex = 1
				},
				new AncientDialogue("")
				{
					VisitIndex = 4
				}
			}),
			[AncientEventModel.CharKey<Silent>()] = new global::_003C_003Ez__ReadOnlyArray<AncientDialogue>(new AncientDialogue[3]
			{
				new AncientDialogue("")
				{
					VisitIndex = 0
				},
				new AncientDialogue("")
				{
					VisitIndex = 1
				},
				new AncientDialogue("")
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
				new AncientDialogue("")
				{
					VisitIndex = 4
				}
			}),
			[AncientEventModel.CharKey<Necrobinder>()] = new global::_003C_003Ez__ReadOnlyArray<AncientDialogue>(new AncientDialogue[3]
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
			[AncientEventModel.CharKey<Regent>()] = new global::_003C_003Ez__ReadOnlyArray<AncientDialogue>(new AncientDialogue[3]
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
			})
		};
		ancientDialogueSet.AgnosticDialogues = new global::_003C_003Ez__ReadOnlyArray<AncientDialogue>(new AncientDialogue[2]
		{
			new AncientDialogue(""),
			new AncientDialogue("")
		});
		return ancientDialogueSet;
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		EventOption eventOption = base.Rng.NextItem(OptionPool1);
		List<EventOption> list = OptionPool2.ToList();
		IReadOnlyList<CardModel> cards = base.Owner.Deck.Cards;
		if (cards.Count((CardModel c) => ModelDb.Enchantment<Goopy>().CanEnchant(c)) >= 3)
		{
			list.Add(PaelsClawOption);
		}
		if (cards.Count((CardModel c) => c.IsRemovable) >= 5)
		{
			list.Add(PaelsToothOption);
		}
		list.AddRange(list);
		list.Add(PaelsGrowthOption);
		EventOption eventOption2 = base.Rng.NextItem(list);
		List<EventOption> list2 = OptionPool3.ToList();
		if (!base.Owner.HasEventPet())
		{
			list2.Add(PaelsLegionOption);
		}
		EventOption eventOption3 = base.Rng.NextItem(list2);
		return new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[3] { eventOption, eventOption2, eventOption3 });
	}
}
