using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class SlumberPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target == base.Owner && result.UnblockedDamage != 0)
		{
			await PowerCmd.Decrement(this);
			if (base.Amount <= 0)
			{
				SlumberingBeetle slumberingBeetle = (SlumberingBeetle)base.Owner.Monster;
				await CreatureCmd.Stun(base.Owner, slumberingBeetle.WakeUpMove, "ROLL_OUT_MOVE");
			}
		}
	}

	public override Task AfterRemoved(Creature oldOwner)
	{
		SfxCmd.StopLoop("event:/sfx/enemy/enemy_attacks/slumbering_beetle/slumbering_beetle_sleep_loop");
		return Task.CompletedTask;
	}

	public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side == base.Owner.Side)
		{
			await PowerCmd.Decrement(this);
			if (base.Amount <= 0)
			{
				SlumberingBeetle slumberingBeetle = (SlumberingBeetle)base.Owner.Monster;
				await slumberingBeetle.WakeUpMove(Array.Empty<Creature>());
			}
		}
	}
}
