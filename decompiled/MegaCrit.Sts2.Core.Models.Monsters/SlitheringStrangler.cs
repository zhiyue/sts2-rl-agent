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
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class SlitheringStrangler : MonsterModel
{
	private const string _attackDefendTrigger = "AttackDefendTrigger";

	private const string _attackHeadbuttSfx = "event:/sfx/enemy/enemy_attacks/slithering_strangler/slithering_strangler_attack_headbutt";

	private const string _attackTailSfx = "event:/sfx/enemy/enemy_attacks/slithering_strangler/slithering_strangler_tail";

	private const string _castSfx = "event:/sfx/enemy/enemy_attacks/slithering_strangler/slithering_strangler_cast";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 54, 53);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 56, 55);

	private int ThwackDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);

	private int LashDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 13, 12);

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Plant;

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("CONSTRICT", ConstrictMove, new DebuffIntent());
		MoveState moveState2 = new MoveState("TWACK", ThwackMove, new SingleAttackIntent(ThwackDamage), new DefendIntent());
		MoveState moveState3 = new MoveState("LASH", LashMove, new SingleAttackIntent(LashDamage));
		RandomBranchState randomBranchState = (RandomBranchState)(moveState.FollowUpState = new RandomBranchState("rand"));
		moveState2.FollowUpState = moveState;
		moveState3.FollowUpState = moveState;
		randomBranchState.AddBranch(moveState2, MoveRepeatType.CanRepeatForever);
		randomBranchState.AddBranch(moveState3, MoveRepeatType.CanRepeatForever);
		list.Add(randomBranchState);
		list.Add(moveState2);
		list.Add(moveState);
		list.Add(moveState3);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task ConstrictMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/slithering_strangler/slithering_strangler_cast");
		await CreatureCmd.TriggerAnim(base.Creature, "Cast", 0.6f);
		await PowerCmd.Apply<ConstrictPower>(targets, 3m, base.Creature, null);
	}

	private async Task ThwackMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(ThwackDamage).FromMonster(this).WithAttackerAnim("AttackDefendTrigger", 0.2f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/slithering_strangler/slithering_strangler_attack_headbutt")
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
		await CreatureCmd.GainBlock(base.Creature, 5m, ValueProp.Move, null);
	}

	private async Task LashMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(LashDamage).FromMonster(this).WithAttackerAnim("Attack", 0.2f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/slithering_strangler/slithering_strangler_tail")
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("constrict");
		AnimState animState3 = new AnimState("attack_defend");
		AnimState animState4 = new AnimState("attack");
		AnimState animState5 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		animState2.NextState = animState;
		animState4.NextState = animState;
		animState5.NextState = animState;
		animState3.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Cast", animState2);
		creatureAnimator.AddAnyState("Attack", animState4);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState5);
		creatureAnimator.AddAnyState("AttackDefendTrigger", animState5);
		return creatureAnimator;
	}
}
