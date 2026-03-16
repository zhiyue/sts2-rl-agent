using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class Mawler : MonsterModel
{
	private const int _clawRepeat = 2;

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 76, 72);

	public override int MaxInitialHp => MinInitialHp;

	private int RipAndTearDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 16, 14);

	private int ClawDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 5, 4);

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("RIP_AND_TEAR_MOVE", RipAndTearMove, new SingleAttackIntent(RipAndTearDamage));
		MoveState moveState2 = new MoveState("ROAR_MOVE", RoarMove, new DebuffIntent());
		MoveState moveState3 = new MoveState("CLAW_MOVE", ClawMove, new MultiAttackIntent(ClawDamage, 2));
		RandomBranchState randomBranchState = (RandomBranchState)(moveState3.FollowUpState = (moveState2.FollowUpState = (moveState.FollowUpState = new RandomBranchState("RAND"))));
		randomBranchState.AddBranch(moveState, MoveRepeatType.CannotRepeat, 1f);
		randomBranchState.AddBranch(moveState2, MoveRepeatType.UseOnlyOnce, 1f);
		randomBranchState.AddBranch(moveState3, MoveRepeatType.CannotRepeat, 1f);
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		list.Add(randomBranchState);
		return new MonsterMoveStateMachine(list, moveState3);
	}

	private async Task RipAndTearMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(RipAndTearDamage).FromMonster(this).WithAttackerAnim("Attack", 0.35f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task RoarMove(IReadOnlyList<Creature> targets)
	{
		await CreatureCmd.TriggerAnim(base.Creature, "Cast", 0.5f);
		await PowerCmd.Apply<VulnerablePower>(targets, 3m, base.Creature, null);
	}

	private async Task ClawMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(ClawDamage).WithHitCount(2).FromMonster(this)
			.WithAttackerAnim("Attack", 0.35f)
			.OnlyPlayAnimOnce()
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("roar");
		AnimState animState3 = new AnimState("attack");
		AnimState animState4 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		animState2.NextState = animState;
		animState3.NextState = animState;
		animState4.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Idle", animState);
		creatureAnimator.AddAnyState("Cast", animState2);
		creatureAnimator.AddAnyState("Attack", animState3);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState4);
		return creatureAnimator;
	}
}
