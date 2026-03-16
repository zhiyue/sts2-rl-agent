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
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class InfestedPrism : MonsterModel
{
	private const string _buffSfx = "event:/sfx/enemy/enemy_attacks/infested_prisms/infested_prisms_buff";

	private const string _attackDefendSfx = "event:/sfx/enemy/enemy_attacks/infested_prisms/infested_prisms_attack_defend";

	private const string _attackSpinSfx = "event:/sfx/enemy/enemy_attacks/infested_prisms/infested_prisms_attack_spin";

	private const string _attackBlockTrigger = "AttackBlock";

	private const string _attackDoubleTrigger = "AttackDouble";

	public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/infested_prisms/infested_prisms_die";

	protected override string AttackSfx => "event:/sfx/enemy/enemy_attacks/infested_prisms/infested_prisms_attack";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 215, 200);

	public override int MaxInitialHp => MinInitialHp;

	private int JabDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 24, 22);

	private int PulsatePowerAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 5, 4);

	private int PulsateBlock => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 22, 20);

	private int RadiateDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 18, 16);

	private int RadiateBlock => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 18, 16);

	private int WhirlwindDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 9);

	private int WhirlwindRepeat => 3;

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Stone;

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<VitalSparkPower>(base.Creature, 1m, base.Creature, null);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("JAB_MOVE", JabMove, new SingleAttackIntent(JabDamage));
		MoveState moveState2 = new MoveState("RADIATE_MOVE", RadiateMove, new SingleAttackIntent(RadiateDamage), new DefendIntent());
		MoveState moveState3 = new MoveState("WHIRLWIND_MOVE", WhirlwindMove, new MultiAttackIntent(WhirlwindDamage, WhirlwindRepeat));
		MoveState moveState4 = new MoveState("PULSATE_MOVE", PulsateMove, new BuffIntent(), new DefendIntent());
		moveState.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState3;
		moveState3.FollowUpState = moveState4;
		moveState4.FollowUpState = moveState;
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		list.Add(moveState4);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task JabMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(JabDamage).FromMonster(this).WithAttackerAnim("Attack", 0.1f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task RadiateMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(RadiateDamage).FromMonster(this).WithAttackerAnim("AttackBlock", 0.25f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/infested_prisms/infested_prisms_attack_defend")
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
		await CreatureCmd.GainBlock(base.Creature, RadiateBlock, ValueProp.Move, null);
	}

	private async Task WhirlwindMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(WhirlwindDamage).WithHitCount(WhirlwindRepeat).FromMonster(this)
			.WithAttackerAnim("AttackDouble", 0.2f)
			.OnlyPlayAnimOnce()
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/infested_prisms/infested_prisms_attack_spin")
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task PulsateMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/infested_prisms/infested_prisms_buff");
		await CreatureCmd.TriggerAnim(base.Creature, "Cast", 0.6f);
		await CreatureCmd.GainBlock(base.Creature, PulsateBlock, ValueProp.Move, null);
		await PowerCmd.Apply<StrengthPower>(base.Creature, PulsatePowerAmount, base.Creature, null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("buff");
		AnimState animState3 = new AnimState("attack");
		AnimState animState4 = new AnimState("attack_block");
		AnimState animState5 = new AnimState("attack_double");
		AnimState animState6 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		animState2.NextState = animState;
		animState3.NextState = animState;
		animState6.NextState = animState;
		animState4.NextState = animState;
		animState5.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Cast", animState2);
		creatureAnimator.AddAnyState("Attack", animState3);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState6);
		creatureAnimator.AddAnyState("AttackBlock", animState4);
		creatureAnimator.AddAnyState("AttackDouble", animState5);
		return creatureAnimator;
	}
}
