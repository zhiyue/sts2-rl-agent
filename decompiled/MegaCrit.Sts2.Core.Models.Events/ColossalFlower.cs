using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class ColossalFlower : EventModel
{
	private static readonly string[] _prizeKeys = new string[3] { "Prize1", "Prize2", "Prize3" };

	private static readonly int[] _prizeCosts = new int[3] { 35, 75, 135 };

	private static readonly int[] _prizeDamage = new int[3] { 5, 6, 7 };

	private int _numberOfDigs;

	private int NumberOfDigs
	{
		get
		{
			return _numberOfDigs;
		}
		set
		{
			AssertMutable();
			_numberOfDigs = value;
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[3]
	{
		new GoldVar(_prizeKeys[0], _prizeCosts[0]),
		new GoldVar(_prizeKeys[1], _prizeCosts[1]),
		new GoldVar(_prizeKeys[2], _prizeCosts[2])
	});

	public override bool IsAllowed(RunState runState)
	{
		return runState.Players.All((Player p) => p.Creature.CurrentHp >= 19);
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[2]
		{
			new EventOption(this, ExtractCurrentPrize, $"COLOSSAL_FLOWER.pages.INITIAL.options.EXTRACT_CURRENT_PRIZE_{NumberOfDigs + 1}"),
			new EventOption(this, ReachDeeper, $"COLOSSAL_FLOWER.pages.INITIAL.options.REACH_DEEPER_{NumberOfDigs + 1}").ThatDoesDamage(_prizeDamage[NumberOfDigs])
		});
	}

	private async Task ReachDeeper()
	{
		await DealReachDeeperDamage();
		NumberOfDigs++;
		if (NumberOfDigs < 2)
		{
			SetEventState(L10NLookup($"COLOSSAL_FLOWER.pages.REACH_DEEPER_{NumberOfDigs}.description"), new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[2]
			{
				new EventOption(this, ExtractCurrentPrize, $"COLOSSAL_FLOWER.pages.REACH_DEEPER_{NumberOfDigs}.options.EXTRACT_CURRENT_PRIZE_{NumberOfDigs + 1}"),
				new EventOption(this, ReachDeeper, $"COLOSSAL_FLOWER.pages.REACH_DEEPER_{NumberOfDigs}.options.REACH_DEEPER_{NumberOfDigs + 1}").ThatDoesDamage(_prizeDamage[NumberOfDigs])
			}));
		}
		else
		{
			SetEventState(L10NLookup("COLOSSAL_FLOWER.pages.REACH_DEEPER_2.description"), new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[2]
			{
				new EventOption(this, ExtractInstead, "COLOSSAL_FLOWER.pages.REACH_DEEPER_2.options.EXTRACT_INSTEAD"),
				new EventOption(this, ObtainPollinousCore, "COLOSSAL_FLOWER.pages.REACH_DEEPER_2.options.POLLINOUS_CORE", HoverTipFactory.FromRelic<PollinousCore>()).ThatDoesDamage(_prizeDamage[NumberOfDigs])
			}));
		}
	}

	private async Task ExtractCurrentPrize()
	{
		await PlayerCmd.GainGold(_prizeCosts[NumberOfDigs], base.Owner);
		SetEventFinished(L10NLookup("COLOSSAL_FLOWER.pages.EXTRACT_CURRENT_PRIZE.description"));
	}

	private async Task ExtractInstead()
	{
		await PlayerCmd.GainGold(_prizeCosts[NumberOfDigs], base.Owner);
		SetEventFinished(L10NLookup("COLOSSAL_FLOWER.pages.EXTRACT_INSTEAD.description"));
	}

	private async Task ObtainPollinousCore()
	{
		await DealReachDeeperDamage();
		await RelicCmd.Obtain<PollinousCore>(base.Owner);
		SetEventFinished(L10NLookup("COLOSSAL_FLOWER.pages.POLLINOUS_CORE.description"));
	}

	private async Task DealReachDeeperDamage()
	{
		await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), base.Owner.Creature, _prizeDamage[NumberOfDigs], ValueProp.Unblockable | ValueProp.Unpowered, null, null);
	}
}
