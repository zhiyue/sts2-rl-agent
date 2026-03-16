using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class FlutterPower : PowerModel
{
	private const string _damageDecreaseKey = "DamageDecrease";

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override bool ShouldScaleInMultiplayer => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("DamageDecrease", 50m));

	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target != base.Owner)
		{
			return 1m;
		}
		if (!props.IsPoweredAttack())
		{
			return 1m;
		}
		return base.DynamicVars["DamageDecrease"].BaseValue / 100m;
	}

	public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target == base.Owner && result.UnblockedDamage != 0 && props.IsPoweredAttack())
		{
			await PowerCmd.Decrement(this);
			if (base.Amount <= 0)
			{
				await CreatureCmd.TriggerAnim(base.Owner, "StunTrigger", 0.6f);
				string nextState = base.Owner.Monster.MoveStateMachine.StateLog.Last().GetNextState(base.Owner, base.Owner.Monster.RunRng.MonsterAi);
				await CreatureCmd.Stun(base.Owner, StunnedMove, nextState);
				((ThievingHopper)base.Owner.Monster).IsHovering = false;
				SfxCmd.StopLoop("event:/sfx/enemy/enemy_attacks/thieving_hopper/thieving_hopper_hover_loop");
				Flash();
				await Cmd.Wait(0.25f);
			}
		}
	}

	private Task StunnedMove(IReadOnlyList<Creature> targets)
	{
		return Task.CompletedTask;
	}
}
