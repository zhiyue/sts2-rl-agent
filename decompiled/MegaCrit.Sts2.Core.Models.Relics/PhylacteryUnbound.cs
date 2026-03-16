using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class PhylacteryUnbound : RelicModel
{
	private const string _startOfCombatKey = "StartOfCombat";

	private const string _startOfTurnKey = "StartOfTurn";

	public override RelicRarity Rarity => RelicRarity.Starter;

	public override bool SpawnsPets => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new SummonVar("StartOfCombat", 5m),
		new SummonVar("StartOfTurn", 2m)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.Static(StaticHoverTip.SummonStatic));

	public override async Task BeforeCombatStart()
	{
		await OstyCmd.Summon(new ThrowingPlayerChoiceContext(), base.Owner, base.DynamicVars["StartOfCombat"].BaseValue, this);
	}

	public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
	{
		if (side == CombatSide.Player)
		{
			await OstyCmd.Summon(new ThrowingPlayerChoiceContext(), base.Owner, base.DynamicVars["StartOfTurn"].BaseValue, this);
		}
	}
}
