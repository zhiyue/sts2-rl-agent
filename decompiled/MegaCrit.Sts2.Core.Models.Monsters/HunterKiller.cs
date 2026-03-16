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

public sealed class HunterKiller : MonsterModel
{
	private const string _tripleAttackTrigger = "TripleAttack";

	private const int _punctureRepeat = 3;

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 126, 121);

	public override int MaxInitialHp => MinInitialHp;

	private int BiteDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 19, 17);

	private int PunctureDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);

	public override string TakeDamageSfx => "event:/sfx/enemy/enemy_attacks/hunter_killer/hunter_killer_hurt";

	public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/hunter_killer/hunter_killer_die";

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("TENDERIZING_GOOP_MOVE", GoopMove, new DebuffIntent());
		MoveState moveState2 = new MoveState("BITE_MOVE", BiteMove, new SingleAttackIntent(BiteDamage));
		MoveState moveState3 = new MoveState("PUNCTURE_MOVE", PunctureMove, new MultiAttackIntent(PunctureDamage, 3));
		RandomBranchState randomBranchState = (RandomBranchState)(moveState3.FollowUpState = (moveState2.FollowUpState = (moveState.FollowUpState = new RandomBranchState("RAND"))));
		randomBranchState.AddBranch(moveState2, MoveRepeatType.CannotRepeat);
		randomBranchState.AddBranch(moveState3, 2);
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		list.Add(randomBranchState);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task GoopMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play(CastSfx);
		await CreatureCmd.TriggerAnim(base.Creature, "Cast", 0.4f);
		await PowerCmd.Apply<TenderPower>(targets, 1m, base.Creature, null);
	}

	private async Task BiteMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(BiteDamage).FromMonster(this).WithAttackerAnim("Attack", 0.3f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_bite")
			.Execute(null);
	}

	private async Task PunctureMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(PunctureDamage).WithHitCount(3).OnlyPlayAnimOnce()
			.FromMonster(this)
			.WithAttackerAnim("TripleAttack", 0.3f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("cast");
		AnimState animState3 = new AnimState("attack");
		AnimState animState4 = new AnimState("attack_triple");
		AnimState animState5 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		animState2.NextState = animState;
		animState3.NextState = animState;
		animState5.NextState = animState;
		animState4.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Cast", animState2);
		creatureAnimator.AddAnyState("Attack", animState3);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState5);
		creatureAnimator.AddAnyState("TripleAttack", animState4);
		return creatureAnimator;
	}
}
