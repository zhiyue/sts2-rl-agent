using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Relics;

namespace MegaCrit.Sts2.Core.Models.Events;

public class Nonupeipe : AncientEventModel
{
	private const string _sfxEeked = "event:/sfx/npcs/nonupeipe/nonupeipe_eeked";

	private const string _sfxWelcome = "event:/sfx/npcs/nonupeipe/nonupeipe_welcome";

	private const string _sfxGrossedOut = "event:/sfx/npcs/nonupeipe/nonupeipe_grossed_out";

	private const string _sfxGiggle = "event:/sfx/npcs/nonupeipe/nonupeipe_giggle";

	public override Color ButtonColor => new Color(0f, 0.1f, 0.16f, 0.75f);

	public override Color DialogueColor => new Color("0A494D");

	public override IEnumerable<EventOption> AllPossibleOptions => OptionPool.Concat(new global::_003C_003Ez__ReadOnlySingleElementList<EventOption>(BeautifulBraceletEventOption));

	protected override IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

	private IEnumerable<EventOption> OptionPool => new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[9]
	{
		RelicOption<BlessedAntler>(),
		RelicOption<BrilliantScarf>(),
		RelicOption<DelicateFrond>(),
		RelicOption<DiamondDiadem>(),
		RelicOption<FurCoat>(),
		RelicOption<Glitter>(),
		RelicOption<JewelryBox>(),
		RelicOption<LoomingFruit>(),
		RelicOption<SignetRing>()
	});

	private EventOption BeautifulBraceletEventOption => RelicOption<BeautifulBracelet>();

	protected override Color EventButtonColor => new Color("000000BF");

	protected override AncientDialogueSet DefineDialogues()
	{
		AncientDialogueSet ancientDialogueSet = new AncientDialogueSet();
		ancientDialogueSet.FirstVisitEverDialogue = new AncientDialogue("event:/sfx/npcs/nonupeipe/nonupeipe_welcome");
		ancientDialogueSet.CharacterDialogues = new Dictionary<string, IReadOnlyList<AncientDialogue>>
		{
			[AncientEventModel.CharKey<Ironclad>()] = new global::_003C_003Ez__ReadOnlyArray<AncientDialogue>(new AncientDialogue[3]
			{
				new AncientDialogue("event:/sfx/npcs/nonupeipe/nonupeipe_welcome")
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
				new AncientDialogue("event:/sfx/npcs/nonupeipe/nonupeipe_grossed_out", "", "event:/sfx/npcs/nonupeipe/nonupeipe_welcome")
				{
					VisitIndex = 0
				},
				new AncientDialogue("event:/sfx/npcs/nonupeipe/nonupeipe_giggle")
				{
					VisitIndex = 1
				},
				new AncientDialogue("event:/sfx/npcs/nonupeipe/nonupeipe_eeked", "", "event:/sfx/npcs/nonupeipe/nonupeipe_eeked")
				{
					VisitIndex = 4
				}
			}),
			[AncientEventModel.CharKey<Defect>()] = new global::_003C_003Ez__ReadOnlyArray<AncientDialogue>(new AncientDialogue[3]
			{
				new AncientDialogue("event:/sfx/npcs/nonupeipe/nonupeipe_welcome")
				{
					VisitIndex = 0
				},
				new AncientDialogue("event:/sfx/npcs/nonupeipe/nonupeipe_welcome")
				{
					VisitIndex = 1
				},
				new AncientDialogue("event:/sfx/npcs/nonupeipe/nonupeipe_giggle", "")
				{
					VisitIndex = 4
				}
			}),
			[AncientEventModel.CharKey<Necrobinder>()] = new global::_003C_003Ez__ReadOnlyArray<AncientDialogue>(new AncientDialogue[3]
			{
				new AncientDialogue("", "event:/sfx/npcs/nonupeipe/nonupeipe_eeked")
				{
					VisitIndex = 0
				},
				new AncientDialogue("event:/sfx/npcs/nonupeipe/nonupeipe_welcome")
				{
					VisitIndex = 1
				},
				new AncientDialogue("event:/sfx/npcs/nonupeipe/nonupeipe_grossed_out", "")
				{
					VisitIndex = 4
				}
			}),
			[AncientEventModel.CharKey<Regent>()] = new global::_003C_003Ez__ReadOnlyArray<AncientDialogue>(new AncientDialogue[3]
			{
				new AncientDialogue("", "event:/sfx/npcs/nonupeipe/nonupeipe_eeked")
				{
					VisitIndex = 0
				},
				new AncientDialogue("event:/sfx/npcs/nonupeipe/nonupeipe_welcome")
				{
					VisitIndex = 1
				},
				new AncientDialogue("event:/sfx/npcs/nonupeipe/nonupeipe_grossed_out", "")
				{
					VisitIndex = 4
				}
			})
		};
		ancientDialogueSet.AgnosticDialogues = new global::_003C_003Ez__ReadOnlyArray<AncientDialogue>(new AncientDialogue[2]
		{
			new AncientDialogue("event:/sfx/npcs/nonupeipe/nonupeipe_welcome"),
			new AncientDialogue("event:/sfx/npcs/nonupeipe/nonupeipe_eeked")
		});
		return ancientDialogueSet;
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		List<EventOption> list = OptionPool.ToList();
		if (base.Owner.Deck.Cards.Count(ModelDb.Enchantment<Swift>().CanEnchant) >= 4)
		{
			list.Add(BeautifulBraceletEventOption);
		}
		list.UnstableShuffle(base.Rng);
		return list.Take(3).ToList();
	}
}
