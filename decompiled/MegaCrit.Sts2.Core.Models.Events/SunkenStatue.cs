using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class SunkenStatue : EventModel
{
	private const string _relicKey = "Relic";

	private const string _hpLossKey = "HpLoss";

	private const int _goldVariance = 10;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[3]
	{
		new StringVar("Relic", ModelDb.Relic<SwordOfStone>().Title.GetFormattedText()),
		new GoldVar(111),
		new DynamicVar("HpLoss", 7m)
	});

	public override void CalculateVars()
	{
		base.DynamicVars.Gold.BaseValue += (decimal)base.Rng.NextInt(-10, 11);
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return new global::_003C_003Ez__ReadOnlyArray<EventOption>(new EventOption[2]
		{
			new EventOption(this, GrabSword, "SUNKEN_STATUE.pages.INITIAL.options.GRAB_SWORD", HoverTipFactory.FromRelic<SwordOfStone>()),
			new EventOption(this, DiveIntoWater, "SUNKEN_STATUE.pages.INITIAL.options.DIVE_INTO_WATER").ThatDoesDamage(base.DynamicVars["HpLoss"].BaseValue)
		});
	}

	private async Task GrabSword()
	{
		await RelicCmd.Obtain<SwordOfStone>(base.Owner);
		SetEventFinished(L10NLookup("SUNKEN_STATUE.pages.GRAB_SWORD.description"));
	}

	private async Task DiveIntoWater()
	{
		await PlayerCmd.GainGold(base.DynamicVars.Gold.BaseValue, base.Owner);
		await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), base.Owner.Creature, base.DynamicVars["HpLoss"].BaseValue, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
		SetEventFinished(L10NLookup("SUNKEN_STATUE.pages.DIVE_INTO_WATER.description"));
	}
}
