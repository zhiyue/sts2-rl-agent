using System;
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

public sealed class PunchConstruct : MonsterModel
{
	private const string _attackDoubleTrigger = "DoubleAttack";

	private bool _startsWithStrongPunch;

	private int _startingHpReduction;

	private const string _attackSingleSfx = "event:/sfx/enemy/enemy_attacks/punch_construct/punch_construct_attack_single";

	private const string _attackDoubleSfx = "event:/sfx/enemy/enemy_attacks/punch_construct/punch_construct_attack_double";

	private const string _buffSfx = "event:/sfx/enemy/enemy_attacks/punch_construct/punch_construct_buff";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 60, 55);

	public override int MaxInitialHp => MinInitialHp;

	private int StrongPunchDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 16, 14);

	private int FastPunchDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 5);

	private int FastPunchRepeat => 2;

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Armor;

	public bool StartsWithStrongPunch
	{
		get
		{
			return _startsWithStrongPunch;
		}
		set
		{
			AssertMutable();
			_startsWithStrongPunch = value;
		}
	}

	public int StartingHpReduction
	{
		get
		{
			return _startingHpReduction;
		}
		set
		{
			AssertMutable();
			_startingHpReduction = value;
		}
	}

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<ArtifactPower>(base.Creature, 1m, base.Creature, null);
		if (StartingHpReduction > 0)
		{
			base.Creature.SetCurrentHpInternal(Math.Max(1, base.Creature.CurrentHp - StartingHpReduction));
		}
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("READY_MOVE", ReadyMove, new DefendIntent());
		MoveState moveState2 = new MoveState("STRONG_PUNCH_MOVE", StrongPunchMove, new SingleAttackIntent(StrongPunchDamage));
		MoveState moveState3 = new MoveState("FAST_PUNCH_MOVE", FastPunchMove, new MultiAttackIntent(FastPunchDamage, FastPunchRepeat), new DebuffIntent());
		moveState.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState3;
		moveState3.FollowUpState = moveState;
		list.Add(moveState);
		list.Add(moveState3);
		list.Add(moveState2);
		return new MonsterMoveStateMachine(list, StartsWithStrongPunch ? moveState2 : moveState);
	}

	private async Task ReadyMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/punch_construct/punch_construct_buff");
		await CreatureCmd.TriggerAnim(base.Creature, "Cast", 0.8f);
		await CreatureCmd.GainBlock(base.Creature, 10m, ValueProp.Move, null);
	}

	private async Task StrongPunchMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(StrongPunchDamage).FromMonster(this).WithAttackerAnim("Attack", 0.25f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/punch_construct/punch_construct_attack_single")
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
	}

	private async Task FastPunchMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(FastPunchDamage).WithHitCount(FastPunchRepeat).FromMonster(this)
			.WithAttackerAnim("DoubleAttack", 0.2f)
			.OnlyPlayAnimOnce()
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/punch_construct/punch_construct_attack_double")
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
		await PowerCmd.Apply<WeakPower>(targets, 1m, base.Creature, null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("attack_double");
		AnimState animState3 = new AnimState("block");
		AnimState animState4 = new AnimState("attack");
		AnimState animState5 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		animState3.NextState = animState;
		animState4.NextState = animState;
		animState5.NextState = animState;
		animState2.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Cast", animState3);
		creatureAnimator.AddAnyState("Attack", animState4);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState5);
		creatureAnimator.AddAnyState("DoubleAttack", animState2);
		return creatureAnimator;
	}
}
