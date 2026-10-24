using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class SkulkingColony : MonsterModel
{
	private const string _attackDoubleTrigger = "AttackDouble";

	private const string _attackBuffTrigger = "AttackBuff";

	private const string _attackHeavyTrigger = "AttackHeavy";

	private const string _kickSfx = "event:/sfx/enemy/enemy_attacks/skulking_colony/skulking_colony_kick";

	private const string _spinSfx = "event:/sfx/enemy/enemy_attacks/skulking_colony/skulking_colony_spin";

	private const string _slapSfx = "event:/sfx/enemy/enemy_attacks/skulking_colony/skulking_colony_slap";

	private const string _thrustSfx = "event:/sfx/enemy/enemy_attacks/skulking_colony/skulking_colony_thrust";

	protected override bool HasPhobiaSpineSkin => true;

	public override string HurtSfx => "event:/sfx/enemy/enemy_attacks/skulking_colony/skulking_colony_hurt";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 80, 75);

	public override int MaxInitialHp => MinInitialHp;

	private int InertiaDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 11, 9);

	private int ZoomDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 16, 14);

	private int PiercingStabsDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);

	private int PiercingStabsRepeat => 2;

	private int InertiaStrengthGain => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 2);

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Stone;

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<HardenedShellPower>(new ThrowingPlayerChoiceContext(), base.Creature, 20m, base.Creature, null);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("ZOOM_MOVE", ZoomMove, new SingleAttackIntent(ZoomDamage));
		MoveState moveState2 = new MoveState("ZOOM_MOVE_2", ZoomMove, new SingleAttackIntent(ZoomDamage));
		MoveState moveState3 = new MoveState("INERTIA_MOVE", InertiaMove, new SingleAttackIntent(InertiaDamage), new BuffIntent());
		MoveState moveState4 = new MoveState("PIERCING_STABS_MOVE", PiercingStabsMove, new MultiAttackIntent(PiercingStabsDamage, PiercingStabsRepeat));
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

	private async Task InertiaMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(InertiaDamage).FromMonster(this).WithAttackerAnim("AttackBuff", 0.5f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/skulking_colony/skulking_colony_thrust")
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
		await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), base.Creature, InertiaStrengthGain, base.Creature, null);
	}

	private async Task PiercingStabsMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(PiercingStabsDamage).WithHitCount(PiercingStabsRepeat).OnlyPlayAnimOnce()
			.FromMonster(this)
			.WithAttackerAnim("AttackDouble", 0.45f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/skulking_colony/skulking_colony_spin")
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task ZoomMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(ZoomDamage).FromMonster(this).WithAttackerAnim("AttackHeavy", 0.25f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/skulking_colony/skulking_colony_kick")
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("cast");
		AnimState animState3 = new AnimState("attack_buff");
		AnimState animState4 = new AnimState("attack_heavy");
		AnimState animState5 = new AnimState("attack_double");
		AnimState animState6 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		animState2.NextState = animState;
		animState6.NextState = animState;
		animState3.NextState = animState;
		animState4.NextState = animState;
		animState5.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Idle", animState);
		creatureAnimator.AddAnyState("Cast", animState2);
		creatureAnimator.AddAnyState("AttackBuff", animState3);
		creatureAnimator.AddAnyState("AttackHeavy", animState4);
		creatureAnimator.AddAnyState("AttackDouble", animState5);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState6);
		return creatureAnimator;
	}
}
