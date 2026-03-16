using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class TurretOperator : MonsterModel
{
	private const string _buffSfx = "event:/sfx/enemy/enemy_attacks/turret_operator/turret_operator_buff";

	private const int _fireRepeat = 5;

	private const string _crankTrigger = "Crank";

	public override string HurtSfx => "event:/sfx/enemy/enemy_attacks/turret_operator/turret_operator_hurt";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 51, 41);

	public override int MaxInitialHp => MinInitialHp;

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Fur;

	private int FireDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("UNLOAD_MOVE_1", UnloadMove, new MultiAttackIntent(FireDamage, 5));
		MoveState moveState2 = new MoveState("UNLOAD_MOVE_2", UnloadMove, new MultiAttackIntent(FireDamage, 5));
		MoveState moveState3 = new MoveState("RELOAD_MOVE", ReloadMove, new BuffIntent());
		moveState.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState3;
		moveState3.FollowUpState = moveState;
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task ReloadMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/turret_operator/turret_operator_buff");
		await CreatureCmd.TriggerAnim(base.Creature, "Crank", 0.4f);
		await PowerCmd.Apply<StrengthPower>(base.Creature, 1m, base.Creature, null);
	}

	private async Task UnloadMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(FireDamage).WithHitCount(5).FromMonster(this)
			.WithAttackerAnim("Attack", 0.4f)
			.OnlyPlayAnimOnce()
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("crank");
		AnimState animState3 = new AnimState("attack");
		AnimState animState4 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		animState2.NextState = animState;
		animState3.NextState = animState;
		animState4.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Crank", animState2);
		creatureAnimator.AddAnyState("Attack", animState3);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState4);
		return creatureAnimator;
	}
}
