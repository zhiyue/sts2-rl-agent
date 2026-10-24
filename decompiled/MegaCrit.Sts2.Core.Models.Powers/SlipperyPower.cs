using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class SlipperyPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override bool ShouldScaleInMultiplayer => true;

	public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target != base.Owner)
		{
			return amount;
		}
		if (amount < 1m)
		{
			return amount;
		}
		return 1m;
	}

	public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target == base.Owner && result.UnblockedDamage >= 1)
		{
			Flash();
			await PowerCmd.Decrement(this);
		}
	}

	public override decimal GetScaledAmountForMultiplayer(ICombatState combatState, Creature? applier, decimal amount, Creature target, CardModel? cardSource)
	{
		return amount * (decimal)combatState.Players.Count;
	}
}
