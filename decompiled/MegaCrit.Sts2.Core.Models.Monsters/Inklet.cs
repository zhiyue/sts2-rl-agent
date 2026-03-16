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

public sealed class Inklet : MonsterModel
{
	private const string _attackTripleTrigger = "TRIPLE_ATTACK";

	private const int _whirlwindRepeat = 3;

	private bool _middleInklet;

	private const string _attackTripleSfx = "event:/sfx/enemy/enemy_attacks/inklet/inklet_attack_triple";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 12, 11);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 18, 17);

	private int JabDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);

	private int WhirlwindDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);

	private int PiercingGazeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 11, 10);

	public bool MiddleInklet
	{
		get
		{
			return _middleInklet;
		}
		set
		{
			AssertMutable();
			_middleInklet = value;
		}
	}

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Magic;

	public override string HurtSfx => "event:/sfx/enemy/enemy_attacks/inklet/inklet_hurt";

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<SlipperyPower>(base.Creature, 1m, base.Creature, null);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("JAB_MOVE", JabMove, new SingleAttackIntent(JabDamage));
		MoveState moveState2 = new MoveState("WHIRLWIND_MOVE", WhirlwindMove, new MultiAttackIntent(WhirlwindDamage, 3));
		MoveState moveState3 = new MoveState("PIERCING_GAZE_MOVE", PiercingGazeMove, new SingleAttackIntent(PiercingGazeDamage));
		RandomBranchState randomBranchState = new RandomBranchState("INIT_RAND");
		RandomBranchState randomBranchState2 = (RandomBranchState)(moveState2.FollowUpState = (moveState3.FollowUpState = (moveState.FollowUpState = new RandomBranchState("RAND"))));
		randomBranchState.AddBranch(moveState, 2, 1f);
		randomBranchState.AddBranch(moveState2, MoveRepeatType.CannotRepeat, 1f);
		randomBranchState2.AddBranch(moveState3, MoveRepeatType.CannotRepeat, 1f);
		randomBranchState2.AddBranch(moveState2, MoveRepeatType.CannotRepeat, 1f);
		moveState.FollowUpState = randomBranchState2;
		moveState2.FollowUpState = moveState;
		moveState3.FollowUpState = moveState;
		list.Add(moveState);
		list.Add(moveState3);
		list.Add(moveState2);
		list.Add(randomBranchState2);
		MoveState initialState = (_middleInklet ? moveState2 : moveState);
		return new MonsterMoveStateMachine(list, initialState);
	}

	private async Task JabMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(JabDamage).FromMonster(this).WithAttackerAnim("Attack", 0.75f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
	}

	private async Task WhirlwindMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(WhirlwindDamage).WithHitCount(3).FromMonster(this)
			.WithAttackerAnim("TRIPLE_ATTACK", 0.3f)
			.OnlyPlayAnimOnce()
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/inklet/inklet_attack_triple")
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
	}

	private async Task PiercingGazeMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(PiercingGazeDamage).FromMonster(this).WithAttackerAnim("Attack", 0.75f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("cast");
		AnimState animState3 = new AnimState("attack");
		AnimState animState4 = new AnimState("attack_fast");
		AnimState animState5 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		animState2.NextState = animState;
		animState3.NextState = animState;
		animState5.NextState = animState;
		animState4.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Cast", animState2);
		creatureAnimator.AddAnyState("Attack", animState3);
		creatureAnimator.AddAnyState("TRIPLE_ATTACK", animState4);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState5);
		return creatureAnimator;
	}
}
