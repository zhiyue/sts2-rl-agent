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

public sealed class FossilStalker : MonsterModel
{
	private const string _attackDoubleTrigger = "AttackDouble";

	private const string _attackBuff = "event:/sfx/enemy/enemy_attacks/fossil_stalker/fossil_stalker_attack_buff";

	private const string _attackDouble = "event:/sfx/enemy/enemy_attacks/fossil_stalker/fossil_stalker_attack_double";

	private const string _attackSingle = "event:/sfx/enemy/enemy_attacks/fossil_stalker/fossil_stalker_attack_single";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 54, 51);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 56, 53);

	private int TackleDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 11, 9);

	private int LatchDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 14, 12);

	private int LashDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);

	private int LashRepeat => 2;

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Stone;

	public override string HurtSfx => "event:/sfx/enemy/enemy_attacks/fossil_stalker/fossil_stalker_hurt";

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<SuckPower>(base.Creature, 3m, base.Creature, null);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("TACKLE_MOVE", TackleMove, new SingleAttackIntent(TackleDamage), new DebuffIntent());
		MoveState moveState2 = new MoveState("LATCH_MOVE", LatchMove, new SingleAttackIntent(LatchDamage));
		MoveState moveState3 = new MoveState("LASH_MOVE", LashAttack, new MultiAttackIntent(LashDamage, LashRepeat));
		RandomBranchState randomBranchState = (RandomBranchState)(moveState3.FollowUpState = (moveState.FollowUpState = (moveState2.FollowUpState = new RandomBranchState("RAND"))));
		randomBranchState.AddBranch(moveState2, 2);
		randomBranchState.AddBranch(moveState, 2);
		randomBranchState.AddBranch(moveState3, 2);
		list.Add(randomBranchState);
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		return new MonsterMoveStateMachine(list, moveState2);
	}

	private async Task TackleMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(TackleDamage).FromMonster(this).WithAttackerAnim("Cast", 0.35f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/fossil_stalker/fossil_stalker_attack_buff")
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
		await PowerCmd.Apply<FrailPower>(targets, 1m, base.Creature, null);
	}

	private async Task LatchMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(LatchDamage).FromMonster(this).WithAttackerAnim("Attack", 0.2f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/fossil_stalker/fossil_stalker_attack_single")
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
	}

	private async Task LashAttack(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(LashDamage).WithHitCount(LashRepeat).FromMonster(this)
			.WithAttackerAnim("AttackDouble", 0.2f)
			.OnlyPlayAnimOnce()
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/fossil_stalker/fossil_stalker_attack_double")
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("debuff");
		AnimState animState3 = new AnimState("attack");
		AnimState animState4 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		AnimState animState5 = new AnimState("attack_double");
		animState2.NextState = animState;
		animState3.NextState = animState;
		animState4.NextState = animState;
		animState5.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Idle", animState);
		creatureAnimator.AddAnyState("Cast", animState2);
		creatureAnimator.AddAnyState("Attack", animState3);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState4);
		creatureAnimator.AddAnyState("AttackDouble", animState5);
		return creatureAnimator;
	}
}
