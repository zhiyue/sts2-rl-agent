using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class BoundPhylactery : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Starter;

	public override bool SpawnsPets => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new SummonVar(1m));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.Static(StaticHoverTip.SummonDynamic, base.DynamicVars.Summon));

	public override async Task BeforeCombatStart()
	{
		await SummonPet();
	}

	public override async Task AfterEnergyResetLate(Player player)
	{
		if (player == base.Owner && player.Creature.CombatState.RoundNumber != 1)
		{
			await SummonPet();
		}
	}

	private async Task SummonPet()
	{
		await OstyCmd.Summon(new ThrowingPlayerChoiceContext(), base.Owner, base.DynamicVars.Summon.BaseValue, this);
	}
}
