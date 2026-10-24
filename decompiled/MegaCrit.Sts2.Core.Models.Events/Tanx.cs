using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Relics;

namespace MegaCrit.Sts2.Core.Models.Events;

public class Tanx : AncientEventModel
{
	private const string _sfxLaugh = "event:/sfx/npcs/tanx/tanx_laugh";

	private const string _sfxRoar = "event:/sfx/npcs/tanx/tanx_roar";

	private const string _sfxCuriosity = "event:/sfx/npcs/tanx/tanx_curiosity";

	private const int _triBoomerangCount = 3;

	public override Color ButtonColor => new Color(0.05f, 0.02f, 0f, 0.5f);

	public override Color DialogueColor => new Color("731717");

	public override IEnumerable<EventOption> AllPossibleOptions => BaseOptionPool.Append(TriBoomerangOption);

	private IEnumerable<EventOption> BaseOptionPool => new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[9]
	{
		RelicOption<Claws>(),
		RelicOption<Crossbow>(),
		RelicOption<IronClub>(),
		RelicOption<MeatCleaver>(),
		RelicOption<Sai>(),
		RelicOption<SpikedGauntlets>(),
		RelicOption<TanxsWhistle>(),
		RelicOption<ThrowingAxe>(),
		RelicOption<WarHammer>()
	});

	private EventOption TriBoomerangOption => RelicOption<TriBoomerang>();

	protected override AncientDialogueSet DefineDialogues()
	{
		AncientDialogueSet ancientDialogueSet = new AncientDialogueSet();
		ancientDialogueSet.FirstVisitEverDialogue = new AncientDialogue("event:/sfx/npcs/tanx/tanx_laugh");
		ancientDialogueSet.CharacterDialogues = new Dictionary<string, IReadOnlyList<AncientDialogue>>
		{
			[AncientEventModel.CharKey<Ironclad>()] = new global::_003C_003Ez__ReadOnlyArray<AncientDialogue>(new AncientDialogue[3]
			{
				new AncientDialogue("event:/sfx/npcs/tanx/tanx_roar")
				{
					VisitIndex = 0
				},
				new AncientDialogue("event:/sfx/npcs/tanx/tanx_laugh")
				{
					VisitIndex = 1
				},
				new AncientDialogue("event:/sfx/npcs/tanx/tanx_roar", "", "event:/sfx/npcs/tanx/tanx_laugh")
				{
					VisitIndex = 4
				}
			}),
			[AncientEventModel.CharKey<Silent>()] = new global::_003C_003Ez__ReadOnlyArray<AncientDialogue>(new AncientDialogue[3]
			{
				new AncientDialogue("event:/sfx/npcs/tanx/tanx_roar")
				{
					VisitIndex = 0
				},
				new AncientDialogue("event:/sfx/npcs/tanx/tanx_curiosity")
				{
					VisitIndex = 1
				},
				new AncientDialogue("event:/sfx/npcs/tanx/tanx_roar", "", "event:/sfx/npcs/tanx/tanx_curiosity")
				{
					VisitIndex = 4
				}
			}),
			[AncientEventModel.CharKey<Defect>()] = new global::_003C_003Ez__ReadOnlyArray<AncientDialogue>(new AncientDialogue[3]
			{
				new AncientDialogue("event:/sfx/npcs/tanx/tanx_curiosity")
				{
					VisitIndex = 0
				},
				new AncientDialogue("event:/sfx/npcs/tanx/tanx_laugh")
				{
					VisitIndex = 1
				},
				new AncientDialogue("event:/sfx/npcs/tanx/tanx_roar", "", "event:/sfx/npcs/tanx/tanx_curiosity")
				{
					VisitIndex = 4
				}
			}),
			[AncientEventModel.CharKey<Necrobinder>()] = new global::_003C_003Ez__ReadOnlyArray<AncientDialogue>(new AncientDialogue[3]
			{
				new AncientDialogue("event:/sfx/npcs/tanx/tanx_roar", "", "event:/sfx/npcs/tanx/tanx_curiosity")
				{
					VisitIndex = 0
				},
				new AncientDialogue("event:/sfx/npcs/tanx/tanx_roar")
				{
					VisitIndex = 1
				},
				new AncientDialogue("event:/sfx/npcs/tanx/tanx_roar", "", "event:/sfx/npcs/tanx/tanx_laugh")
				{
					VisitIndex = 4
				}
			}),
			[AncientEventModel.CharKey<Regent>()] = new global::_003C_003Ez__ReadOnlyArray<AncientDialogue>(new AncientDialogue[3]
			{
				new AncientDialogue("event:/sfx/npcs/tanx/tanx_curiosity", "", "event:/sfx/npcs/tanx/tanx_laugh")
				{
					VisitIndex = 0
				},
				new AncientDialogue("event:/sfx/npcs/tanx/tanx_roar")
				{
					VisitIndex = 1
				},
				new AncientDialogue("event:/sfx/npcs/tanx/tanx_roar", "", "event:/sfx/npcs/tanx/tanx_roar")
				{
					VisitIndex = 4
				}
			})
		};
		ancientDialogueSet.AgnosticDialogues = new global::_003C_003Ez__ReadOnlyArray<AncientDialogue>(new AncientDialogue[2]
		{
			new AncientDialogue("event:/sfx/npcs/tanx/tanx_roar"),
			new AncientDialogue("event:/sfx/npcs/tanx/tanx_laugh")
		});
		return ancientDialogueSet;
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		List<EventOption> list = BaseOptionPool.ToList();
		if (base.Owner.Deck.Cards.Count((CardModel c) => ModelDb.Enchantment<Instinct>().CanEnchant(c)) >= 3)
		{
			list.Add(TriBoomerangOption);
		}
		return list.UnstableShuffle(base.Rng).Take(3).ToList();
	}
}
