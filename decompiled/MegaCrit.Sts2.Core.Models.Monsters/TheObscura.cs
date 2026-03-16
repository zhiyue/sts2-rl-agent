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
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class TheObscura : MonsterModel
{
	private const string _summonTrigger = "Summon";

	private const string _summonSfx = "event:/sfx/enemy/enemy_attacks/obscura/obscura_summon";

	private const string _buffSfx = "event:/sfx/enemy/enemy_attacks/obscura/obscura_buff";

	private bool _hasSummoned;

	protected override string AttackSfx => "event:/sfx/enemy/enemy_attacks/obscura/obscura_attack";

	public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/obscura/obscura_die";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 129, 123);

	public override int MaxInitialHp => MinInitialHp;

	private int PiercingGazeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 11, 10);

	private int HardeningStrikeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);

	private int HardeningStrikeBlock => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);

	private bool HasSummoned
	{
		get
		{
			return _hasSummoned;
		}
		set
		{
			AssertMutable();
			_hasSummoned = value;
		}
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("ILLUSION_MOVE", IllusionMove, new SummonIntent());
		MoveState moveState2 = new MoveState("PIERCING_GAZE_MOVE", PiercingGazeMove, new SingleAttackIntent(PiercingGazeDamage));
		MoveState moveState3 = new MoveState("SAIL_MOVE", WailMove, new BuffIntent());
		MoveState moveState4 = new MoveState("HARDENING_STRIKE_MOVE", HardeningStrikeMove, new SingleAttackIntent(HardeningStrikeDamage), new DefendIntent());
		RandomBranchState randomBranchState = (RandomBranchState)(moveState4.FollowUpState = (moveState3.FollowUpState = (moveState2.FollowUpState = (moveState.FollowUpState = new RandomBranchState("RAND")))));
		randomBranchState.AddBranch(moveState2, MoveRepeatType.CannotRepeat);
		randomBranchState.AddBranch(moveState3, MoveRepeatType.CannotRepeat);
		randomBranchState.AddBranch(moveState4, MoveRepeatType.CannotRepeat);
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		list.Add(moveState4);
		list.Add(randomBranchState);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task IllusionMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/obscura/obscura_summon");
		await CreatureCmd.TriggerAnim(base.Creature, "Summon", 0.15f);
		await CreatureCmd.Add<Parafright>(base.CombatState, "illusion");
		HasSummoned = true;
	}

	private async Task PiercingGazeMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(PiercingGazeDamage).FromMonster(this).WithAttackerFx(null, AttackSfx)
			.WithAttackerAnim("Attack", 0.3f)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task WailMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/obscura/obscura_buff");
		await CreatureCmd.TriggerAnim(base.Creature, "Cast", 0.7f);
		await PowerCmd.Apply<StrengthPower>(base.Creature.CombatState.GetTeammatesOf(base.Creature), 3m, base.Creature, null);
	}

	private async Task HardeningStrikeMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(HardeningStrikeDamage).FromMonster(this).WithAttackerFx(null, AttackSfx)
			.WithAttackerAnim("Attack", 0.3f)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
		await CreatureCmd.GainBlock(base.Creature, HardeningStrikeBlock, ValueProp.Move, null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState nextState = new AnimState("idle_loop", isLooping: true);
		AnimState animState = new AnimState("attack");
		AnimState state = new AnimState("die");
		AnimState animState2 = new AnimState("hurt");
		AnimState animState3 = new AnimState("cast");
		AnimState animState4 = new AnimState("cast_intro");
		AnimState animState5 = new AnimState("hurt_intro");
		AnimState state2 = new AnimState("die_intro");
		AnimState animState6 = new AnimState("intro_loop", isLooping: true);
		animState.NextState = nextState;
		animState2.NextState = nextState;
		animState3.NextState = nextState;
		animState4.NextState = nextState;
		animState5.NextState = animState6;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState6, controller);
		creatureAnimator.AddAnyState("Attack", animState);
		creatureAnimator.AddAnyState("Hit", animState2, () => HasSummoned);
		creatureAnimator.AddAnyState("Hit", animState5, () => !HasSummoned);
		creatureAnimator.AddAnyState("Dead", state, () => HasSummoned);
		creatureAnimator.AddAnyState("Dead", state2, () => !HasSummoned);
		creatureAnimator.AddAnyState("Cast", animState3);
		creatureAnimator.AddAnyState("Summon", animState4);
		return creatureAnimator;
	}
}
