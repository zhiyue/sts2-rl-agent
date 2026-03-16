using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class TheBombPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override bool IsInstanced => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DamageVar(40m, ValueProp.Unpowered));

	public void SetDamage(decimal damage)
	{
		AssertMutable();
		base.DynamicVars.Damage.BaseValue = damage;
	}

	public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side != base.Owner.Side)
		{
			return;
		}
		if (base.Amount > 1)
		{
			await PowerCmd.Decrement(this);
			return;
		}
		Flash();
		await Cmd.CustomScaledWait(0.2f, 0.4f);
		foreach (Creature hittableEnemy in base.CombatState.HittableEnemies)
		{
			NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NFireSmokePuffVfx.Create(hittableEnemy));
		}
		await Cmd.CustomScaledWait(0.2f, 0.4f);
		await CreatureCmd.Damage(choiceContext, base.CombatState.HittableEnemies, base.DynamicVars.Damage, base.Owner);
		await PowerCmd.Remove(this);
	}
}
