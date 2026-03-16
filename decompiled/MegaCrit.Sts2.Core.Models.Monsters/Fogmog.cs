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
using MegaCrit.Sts2.Core.MonsterMoves;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class Fogmog : MonsterModel
{
	private const string _summonTrigger = "Summon";

	private const string _sporesSfx = "event:/sfx/enemy/enemy_attacks/fogmog/fogmog_summon";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 78, 74);

	public override int MaxInitialHp => MinInitialHp;

	private int SwipeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 9, 8);

	private int HeadbuttDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 16, 14);

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Plant;

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("ILLUSION_MOVE", IllusionMove, new SummonIntent());
		MoveState moveState2 = new MoveState("SWIPE_MOVE", SwipeMove, new SingleAttackIntent(SwipeDamage), new BuffIntent());
		MoveState moveState3 = new MoveState("SWIPE_RANDOM_MOVE", SwipeMove, new SingleAttackIntent(SwipeDamage), new BuffIntent());
		MoveState moveState4 = new MoveState("HEADBUTT_MOVE", HeadbuttMove, new SingleAttackIntent(HeadbuttDamage));
		RandomBranchState randomBranchState = new RandomBranchState("BRANCH");
		randomBranchState.AddBranch(moveState3, MoveRepeatType.CannotRepeat, () => 0.4f);
		randomBranchState.AddBranch(moveState4, MoveRepeatType.CannotRepeat, () => 0.6f);
		moveState.FollowUpState = moveState2;
		moveState2.FollowUpState = randomBranchState;
		moveState3.FollowUpState = moveState4;
		moveState4.FollowUpState = moveState2;
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		list.Add(randomBranchState);
		list.Add(moveState4);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task IllusionMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/fogmog/fogmog_summon");
		await CreatureCmd.TriggerAnim(base.Creature, "Summon", 0.75f);
		await CreatureCmd.Add<EyeWithTeeth>(base.CombatState, "illusion");
	}

	private async Task SwipeMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(SwipeDamage).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
		await PowerCmd.Apply<StrengthPower>(base.Creature, 1m, base.Creature, null);
	}

	private async Task HeadbuttMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(HeadbuttDamage).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("summon");
		AnimState animState3 = new AnimState("attack");
		AnimState animState4 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		animState2.NextState = animState;
		animState3.NextState = animState;
		animState4.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState4);
		creatureAnimator.AddAnyState("Summon", animState2);
		creatureAnimator.AddAnyState("Attack", animState3);
		return creatureAnimator;
	}
}
