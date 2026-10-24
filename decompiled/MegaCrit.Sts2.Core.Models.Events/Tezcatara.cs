using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Godot;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Relics;

namespace MegaCrit.Sts2.Core.Models.Events;

public class Tezcatara : AncientEventModel
{
	/// <summary>
	/// This ancient's "any character" dialogue is friendly, but they don't trust the <see cref="T:MegaCrit.Sts2.Core.Models.Characters.Defect" />, so we
	/// blacklist them so we don't show these friendly dialogue options for them.
	/// </summary>
	public override IEnumerable<CharacterModel> AnyCharacterDialogueBlacklist => new global::_003C_003Ez__ReadOnlySingleElementList<CharacterModel>(ModelDb.Character<Defect>());

	public override Color ButtonColor => new Color(0.08f, 0.04f, 0f, 0.75f);

	public override Color DialogueColor => new Color("33251E");

	public override IEnumerable<EventOption> AllPossibleOptions
	{
		get
		{
			List<EventOption>[] obj = new List<EventOption>[4] { OptionPool1, OptionPool2, OptionPool3, null };
			int num = 1;
			List<EventOption> list = new List<EventOption>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<EventOption> span = CollectionsMarshal.AsSpan(list);
			int index = 0;
			span[index] = NutritiousSoupOption;
			obj[3] = list;
			return obj.SelectMany((List<EventOption> x) => x);
		}
	}

	private EventOption NutritiousSoupOption => RelicOption<NutritiousSoup>();

	private List<EventOption> OptionPool1
	{
		get
		{
			int num = 2;
			List<EventOption> list = new List<EventOption>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<EventOption> span = CollectionsMarshal.AsSpan(list);
			int num2 = 0;
			span[num2] = RelicOption<VeryHotCocoa>();
			num2++;
			span[num2] = RelicOption<YummyCookie>();
			return list;
		}
	}

	private List<EventOption> OptionPool2
	{
		get
		{
			int num = 3;
			List<EventOption> list = new List<EventOption>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<EventOption> span = CollectionsMarshal.AsSpan(list);
			int num2 = 0;
			span[num2] = RelicOption<BiiigHug>();
			num2++;
			span[num2] = RelicOption<Storybook>();
			num2++;
			span[num2] = RelicOption<ToastyMittens>();
			return list;
		}
	}

	private List<EventOption> OptionPool3
	{
		get
		{
			int num = 4;
			List<EventOption> list = new List<EventOption>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<EventOption> span = CollectionsMarshal.AsSpan(list);
			int num2 = 0;
			span[num2] = RelicOption<GoldenCompass>();
			num2++;
			span[num2] = RelicOption<PumpkinCandle>();
			num2++;
			span[num2] = RelicOption<ToyBox>();
			num2++;
			span[num2] = RelicOption<SealOfGold>();
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
		ancientDialogueSet.AgnosticDialogues = new global::_003C_003Ez__ReadOnlyArray<AncientDialogue>(new AncientDialogue[2]
		{
			new AncientDialogue(""),
			new AncientDialogue("")
		});
		return ancientDialogueSet;
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		List<EventOption> list = OptionPool1.ToList();
		if (base.Owner.Deck.Cards.Any((CardModel c) => c.Tags.Contains(CardTag.Strike) && c.Rarity == CardRarity.Basic))
		{
			list.Add(NutritiousSoupOption);
		}
		EventOption eventOption = base.Rng.NextItem(list);
		EventOption eventOption2 = base.Rng.NextItem(OptionPool2);
		EventOption eventOption3 = base.Rng.NextItem(OptionPool3);
		return new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[3] { eventOption, eventOption2, eventOption3 });
	}
}
