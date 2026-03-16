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

public sealed class SludgeSpinner : MonsterModel
{
	private const string _dashAttackSfx = "event:/sfx/enemy/enemy_attacks/sludge_spinner/sludge_spinner_attack_dash";

	private const string _spinAttackSfx = "event:/sfx/enemy/enemy_attacks/sludge_spinner/sludge_spinner_attack_spin";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 41, 37);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 42, 39);

	private int OilSprayDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 9, 8);

	private int SlamDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 12, 11);

	private int RageDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Stone;

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("OIL_SPRAY_MOVE", OilSprayMove, new SingleAttackIntent(OilSprayDamage), new DebuffIntent());
		MoveState moveState2 = new MoveState("SLAM_MOVE", SlamMove, new SingleAttackIntent(SlamDamage));
		MoveState moveState3 = new MoveState("RAGE_MOVE", RageMove, new SingleAttackIntent(RageDamage), new BuffIntent());
		RandomBranchState randomBranchState = (RandomBranchState)(moveState3.FollowUpState = (moveState2.FollowUpState = (moveState.FollowUpState = new RandomBranchState("RAND"))));
		randomBranchState.AddBranch(moveState, MoveRepeatType.CannotRepeat);
		randomBranchState.AddBranch(moveState2, MoveRepeatType.CannotRepeat);
		randomBranchState.AddBranch(moveState3, MoveRepeatType.CannotRepeat);
		list.Add(randomBranchState);
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task OilSprayMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(OilSprayDamage).FromMonster(this).WithAttackerAnim("Cast", 0.5f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/sludge_spinner/sludge_spinner_attack_spin")
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
		await PowerCmd.Apply<WeakPower>(targets, 1m, base.Creature, null);
	}

	private async Task SlamMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(SlamDamage).FromMonster(this).WithAttackerAnim("Attack", 0.15f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/sludge_spinner/sludge_spinner_attack_dash")
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
	}

	private async Task RageMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(RageDamage).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/sludge_spinner/sludge_spinner_attack_dash")
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
		await PowerCmd.Apply<StrengthPower>(base.Creature, 3m, base.Creature, null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("slam");
		AnimState animState3 = new AnimState("spray");
		AnimState animState4 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		animState2.NextState = animState;
		animState3.NextState = animState;
		animState4.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Attack", animState2);
		creatureAnimator.AddAnyState("Cast", animState3);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState4);
		return creatureAnimator;
	}
}
