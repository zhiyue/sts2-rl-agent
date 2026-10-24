using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class AsleepPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target == base.Owner && result.UnblockedDamage != 0)
		{
			if (base.Owner.HasPower<PlatingPower>())
			{
				await PowerCmd.Remove(base.Owner.GetPower<PlatingPower>());
			}
			LagavulinMatriarch monster = (LagavulinMatriarch)base.Owner.Monster;
			SfxCmd.Play("event:/sfx/enemy/enemy_attacks/lagavulin_matriarch/lagavulin_matriarch_awaken");
			await CreatureCmd.TriggerAnim(base.Owner, "Wake", 0.6f);
			monster.IsAwake = true;
			await CreatureCmd.Stun(base.Owner, monster.WakeUpMove, "SLASH_MOVE");
			await PowerCmd.Remove(this);
		}
	}

	public override async Task BeforeSideTurnEndVeryEarly(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (participants.Contains(base.Owner) && base.Amount <= 1 && base.Owner.HasPower<PlatingPower>())
		{
			await PowerCmd.Remove(base.Owner.GetPower<PlatingPower>());
		}
	}

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (participants.Contains(base.Owner))
		{
			await PowerCmd.Decrement(this);
			if (base.Amount <= 0)
			{
				LagavulinMatriarch lagavulinMatriarch = (LagavulinMatriarch)base.Owner.Monster;
				await lagavulinMatriarch.WakeUpMove(Array.Empty<Creature>());
			}
		}
	}
}
