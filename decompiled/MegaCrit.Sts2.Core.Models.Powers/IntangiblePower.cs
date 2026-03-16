using System;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;
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
		return Math.Min(GetDamageCap(dealer), amount);
	}

	public override Task AfterModifyingHpLostAfterOsty()
	{
		Flash();
		return Task.CompletedTask;
	}

	public override decimal ModifyDamageCap(Creature? target, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target != base.Owner)
		{
			return decimal.MaxValue;
		}
		return GetDamageCap(dealer);
	}

	public override Task AfterModifyingDamageAmount(CardModel? cardSource)
	{
		Flash();
		return Task.CompletedTask;
	}

	public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side == CombatSide.Enemy)
		{
			await PowerCmd.TickDownDuration(this);
		}
	}

	private int GetDamageCap(Creature? dealer)
	{
		Player player = dealer?.Player ?? dealer?.PetOwner;
		if (player == null || !player.Relics.Any((RelicModel r) => r is TheBoot))
		{
			return 1;
		}
		return 5;
	}
}
