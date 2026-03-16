using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class DebilitatePower : PowerModel
{
	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
	{
		HoverTipFactory.FromPower<VulnerablePower>(),
		HoverTipFactory.FromPower<WeakPower>()
	});

	public decimal ModifyVulnerableMultiplier(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target != base.Owner)
		{
			return amount;
		}
		if (!props.IsPoweredAttack())
		{
			return amount;
		}
		return amount + (amount - 1m);
	}

	public decimal ModifyWeakMultiplier(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (dealer != base.Owner)
		{
			return amount;
		}
		if (!props.IsPoweredAttack())
		{
			return amount;
		}
		return amount - (1m - amount);
	}

	public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side == base.Owner.Side)
		{
			await PowerCmd.Decrement(this);
		}
	}
}
