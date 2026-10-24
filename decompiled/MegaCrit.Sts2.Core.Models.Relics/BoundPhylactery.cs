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

	/// <summary>
	/// We use AfterEnergyResetLate instead of <see cref="M:MegaCrit.Sts2.Core.Hooks.Hook.AfterEnergyReset(MegaCrit.Sts2.Core.Combat.ICombatState,MegaCrit.Sts2.Core.Entities.Players.Player)" /> or
	/// <see cref="M:MegaCrit.Sts2.Core.Hooks.Hook.BeforeSideTurnStart(MegaCrit.Sts2.Core.Combat.ICombatState,MegaCrit.Sts2.Core.Combat.CombatSide,System.Collections.Generic.IReadOnlyList{MegaCrit.Sts2.Core.Entities.Creatures.Creature})" /> because we want to allow effects that check for Osty's existence (like
	/// <see cref="T:MegaCrit.Sts2.Core.Models.Cards.Friendship" />) to run before we summon Osty.
	/// </summary>
	public override async Task AfterEnergyResetLate(Player player)
	{
		if (player == base.Owner && base.Owner.PlayerCombatState.TurnNumber != 1)
		{
			await SummonPet();
		}
	}

	private async Task SummonPet()
	{
		await OstyCmd.Summon(new ThrowingPlayerChoiceContext(), base.Owner, base.DynamicVars.Summon.BaseValue, this);
	}
}
