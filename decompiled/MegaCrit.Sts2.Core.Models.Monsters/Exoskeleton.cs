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

public sealed class Exoskeleton : MonsterModel
{
	private const string _buffTrigger = "Buff";

	private const int _buffAmount = 2;

	private const string _heavyAttackTrigger = "HeavyAttack";

	private const string _attackHeavySfx = "event:/sfx/enemy/enemy_attacks/roaches/roaches_attack_heavy";

	private const string _buffSfx = "event:/sfx/enemy/enemy_attacks/roaches/roaches_buff";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 25, 24);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 29, 28);

	private int SkitterDamage => 1;

	private int SkitterRepeats => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);

	private int MandiblesDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 9, 8);

	protected override string AttackSfx => "event:/sfx/enemy/enemy_attacks/roaches/roaches_attack";

	public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/roaches/roaches_die";

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Insect;

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<HardToKillPower>(base.Creature, 9m, base.Creature, null);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("SKITTER_MOVE", SkitterMove, new MultiAttackIntent(SkitterDamage, SkitterRepeats));
		MoveState moveState2 = new MoveState("MANDIBLE_MOVE", MandiblesMove, new SingleAttackIntent(MandiblesDamage));
		MoveState moveState3 = new MoveState("ENRAGE_MOVE", EnrageMove, new BuffIntent());
		RandomBranchState randomBranchState = new RandomBranchState("RAND");
		randomBranchState.AddBranch(moveState, MoveRepeatType.CannotRepeat, 1f);
		randomBranchState.AddBranch(moveState2, MoveRepeatType.CannotRepeat, 1f);
		ConditionalBranchState conditionalBranchState = new ConditionalBranchState("INIT_MOVE");
		conditionalBranchState.AddState(moveState, () => base.Creature.SlotName == "first");
		conditionalBranchState.AddState(moveState2, () => base.Creature.SlotName == "second");
		conditionalBranchState.AddState(moveState3, () => base.Creature.SlotName == "third");
		conditionalBranchState.AddState(randomBranchState, () => base.Creature.SlotName == "fourth");
		moveState.FollowUpState = randomBranchState;
		moveState2.FollowUpState = moveState3;
		moveState3.FollowUpState = randomBranchState;
		list.Add(conditionalBranchState);
		list.Add(randomBranchState);
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		return new MonsterMoveStateMachine(list, conditionalBranchState);
	}

	private async Task SkitterMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(SkitterDamage).WithHitCount(SkitterRepeats).FromMonster(this)
			.OnlyPlayAnimOnce()
			.WithAttackerAnim("Attack", 0.15f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task MandiblesMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(MandiblesDamage).FromMonster(this).WithAttackerAnim("HeavyAttack", 0.3f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/roaches/roaches_attack_heavy")
			.WithHitFx("vfx/vfx_bite")
			.Execute(null);
	}

	private async Task EnrageMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/roaches/roaches_buff");
		await CreatureCmd.TriggerAnim(base.Creature, "Buff", 0.3f);
		await PowerCmd.Apply<StrengthPower>(base.Creature, 2m, base.Creature, null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState state = new AnimState("die");
		AnimState animState2 = new AnimState("hurt");
		AnimState animState3 = new AnimState("cast");
		AnimState animState4 = new AnimState("buff");
		AnimState animState5 = new AnimState("attack");
		AnimState animState6 = new AnimState("attack_heavy");
		animState5.NextState = animState;
		animState3.NextState = animState;
		animState4.NextState = animState;
		animState2.NextState = animState;
		animState6.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Hit", animState2);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Cast", animState3);
		creatureAnimator.AddAnyState("Attack", animState5);
		creatureAnimator.AddAnyState("HeavyAttack", animState6);
		creatureAnimator.AddAnyState("Buff", animState4);
		return creatureAnimator;
	}
}
