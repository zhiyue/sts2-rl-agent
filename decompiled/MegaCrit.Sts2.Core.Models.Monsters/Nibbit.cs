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

public sealed class Nibbit : MonsterModel
{
	private bool _isFront;

	private bool _isAlone;

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 44, 42);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 48, 46);

	private int ButtDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 13, 12);

	private int SliceBlock => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 5, 5);

	private int SliceDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 6);

	public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/nibbit/nibbit_die";

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Slime;

	public bool IsFront
	{
		get
		{
			return _isFront;
		}
		set
		{
			AssertMutable();
			_isFront = value;
		}
	}

	public bool IsAlone
	{
		get
		{
			return _isAlone;
		}
		set
		{
			AssertMutable();
			_isAlone = value;
		}
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("BUTT_MOVE", ButtMove, new SingleAttackIntent(ButtDamage));
		MoveState moveState2 = new MoveState("SLICE_MOVE", SliceMove, new SingleAttackIntent(SliceDamage), new DefendIntent());
		MoveState moveState3 = new MoveState("HISS_MOVE", HissMove, new BuffIntent());
		ConditionalBranchState conditionalBranchState = new ConditionalBranchState("INIT_MOVE");
		if (_isAlone)
		{
			conditionalBranchState.AddState(moveState, () => ((Nibbit)base.Creature.Monster).IsAlone);
		}
		else
		{
			conditionalBranchState.AddState(moveState3, () => !((Nibbit)base.Creature.Monster).IsFront);
			conditionalBranchState.AddState(moveState2, () => ((Nibbit)base.Creature.Monster).IsFront);
		}
		moveState2.FollowUpState = moveState3;
		moveState.FollowUpState = moveState2;
		moveState3.FollowUpState = moveState;
		list.Add(conditionalBranchState);
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		return new MonsterMoveStateMachine(list, conditionalBranchState);
	}

	private async Task ButtMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(ButtDamage).FromMonster(this).WithAttackerAnim("Attack", 0.15f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task SliceMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(SliceDamage).FromMonster(this).WithAttackerAnim("Attack", 0.15f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
		await CreatureCmd.GainBlock(base.Creature, SliceBlock, ValueProp.Move, null);
	}

	private async Task HissMove(IReadOnlyList<Creature> targets)
	{
		await CreatureCmd.TriggerAnim(base.Creature, "Cast", 0.6f);
		await PowerCmd.Apply<StrengthPower>(base.Creature, 2m, base.Creature, null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("hiss");
		AnimState animState3 = new AnimState("attack");
		AnimState animState4 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		animState2.NextState = animState;
		animState3.NextState = animState;
		animState4.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Cast", animState2);
		creatureAnimator.AddAnyState("Attack", animState3);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState4);
		return creatureAnimator;
	}
}
