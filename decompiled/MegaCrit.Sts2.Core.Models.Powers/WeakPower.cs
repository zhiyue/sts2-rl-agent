using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class WeakPower : PowerModel
{
	private const string _damageDecrease = "DamageDecrease";

	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("DamageDecrease", 0.75m));

	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (dealer != base.Owner)
		{
			return 1m;
		}
		if (!props.IsPoweredAttack())
		{
			return 1m;
		}
		decimal num = base.DynamicVars["DamageDecrease"].BaseValue;
		PaperKrane paperKrane = target?.Player?.GetRelic<PaperKrane>();
		if (paperKrane != null)
		{
			num = paperKrane.ModifyWeakMultiplier(target, num, props, dealer, cardSource);
		}
		DebilitatePower power = dealer.GetPower<DebilitatePower>();
		if (power != null)
		{
			num = power.ModifyWeakMultiplier(dealer, num, props, dealer, cardSource);
		}
		return num;
	}

	public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side == CombatSide.Enemy)
		{
			await PowerCmd.TickDownDuration(this);
		}
	}
}
