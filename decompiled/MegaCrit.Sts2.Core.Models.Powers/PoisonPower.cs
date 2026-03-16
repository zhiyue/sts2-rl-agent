using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class PoisonPower : PowerModel
{
	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override Color AmountLabelColor => PowerModel._normalAmountLabelColor;

	private int TriggerCount
	{
		get
		{
			IEnumerable<Creature> source = from c in base.Owner.CombatState.GetOpponentsOf(base.Owner)
				where c.IsAlive
				select c;
			return Math.Min(base.Amount, 1 + source.Sum((Creature a) => a.GetPowerAmount<AccelerantPower>()));
		}
	}

	public int CalculateTotalDamageNextTurn()
	{
		decimal num = default(decimal);
		int num2 = Math.Min(base.Amount, TriggerCount);
		for (int i = 0; i < num2; i++)
		{
			decimal damage = base.Amount - i;
			damage = Hook.ModifyDamage(base.Owner.CombatState.RunState, base.Owner.CombatState, base.Owner, null, damage, ValueProp.Unblockable | ValueProp.Unpowered, null, ModifyDamageHookType.All, CardPreviewMode.None, out IEnumerable<AbstractModel> _);
			num += damage;
		}
		return (int)num;
	}

	public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
	{
		if (side != base.Owner.Side)
		{
			return;
		}
		int iterations = TriggerCount;
		for (int i = 0; i < iterations; i++)
		{
			await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), base.Owner, base.Amount, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
			if (base.Owner.IsAlive)
			{
				await PowerCmd.Decrement(this);
			}
			else
			{
				await Cmd.CustomScaledWait(0.1f, 0.25f);
			}
		}
	}
}
