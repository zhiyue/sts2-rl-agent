using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class IntangiblePower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (!CombatManager.Instance.IsInProgress)
		{
			return amount;
		}
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

	public override Task AfterModifyingHpLostAfterOsty()
	{
		Flash();
		return Task.CompletedTask;
	}

	/// <summary>
	/// Caps damage received at 1.
	/// Note: the HP loss logic is already handled by <see cref="M:MegaCrit.Sts2.Core.Models.Powers.IntangiblePower.ModifyHpLostAfterOsty(MegaCrit.Sts2.Core.Entities.Creatures.Creature,System.Decimal,MegaCrit.Sts2.Core.ValueProps.ValueProp,MegaCrit.Sts2.Core.Entities.Creatures.Creature,MegaCrit.Sts2.Core.Models.CardModel)" />, the duplicated logic here is
	/// for block loss and preview logic on targeted attacks.
	/// </summary>
	public override decimal ModifyDamageCap(Creature? target, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target != base.Owner)
		{
			return decimal.MaxValue;
		}
		return 1m;
	}

	/// <summary>
	/// Note: the HP loss logic is already handled by <see cref="M:MegaCrit.Sts2.Core.Models.Powers.IntangiblePower.AfterModifyingHpLostAfterOsty" />, the duplicated logic
	/// here is for block loss and preview logic on targeted attacks.
	/// </summary>
	public override Task AfterModifyingDamageAmount(CardModel? cardSource)
	{
		Flash();
		return Task.CompletedTask;
	}

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (side == CombatSide.Enemy)
		{
			await PowerCmd.Decrement(this);
		}
	}
}
