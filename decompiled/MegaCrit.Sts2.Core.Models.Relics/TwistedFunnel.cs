using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class TwistedFunnel : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Uncommon;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new PowerVar<PoisonPower>(4m));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<PoisonPower>());

	public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
	{
		if (side != base.Owner.Creature.Side || combatState.RoundNumber > 1)
		{
			return;
		}
		Flash();
		foreach (Creature hittableEnemy in base.Owner.Creature.CombatState.HittableEnemies)
		{
			NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NSmokePuffVfx.Create(hittableEnemy, NSmokePuffVfx.SmokePuffColor.Green));
		}
		await Cmd.CustomScaledWait(0.2f, 0.4f);
		foreach (Creature hittableEnemy2 in base.Owner.Creature.CombatState.HittableEnemies)
		{
			await PowerCmd.Apply<PoisonPower>(hittableEnemy2, base.DynamicVars["PoisonPower"].IntValue, base.Owner.Creature, null);
		}
	}
}
