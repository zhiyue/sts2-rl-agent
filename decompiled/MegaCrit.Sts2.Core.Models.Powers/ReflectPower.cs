using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class ReflectPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target == base.Owner && result.BlockedDamage > 0 && props.IsPoweredAttack() && dealer != null)
		{
			await CreatureCmd.Damage(choiceContext, dealer, result.BlockedDamage, ValueProp.Unpowered, base.Owner, null);
		}
	}

	public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
	{
		if (side == base.Owner.Side)
		{
			await PowerCmd.Decrement(this);
		}
	}
}
