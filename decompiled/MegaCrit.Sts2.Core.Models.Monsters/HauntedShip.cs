using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class HauntedShip : MonsterModel
{
	private const string _attackTripleTrigger = "AttackTriple";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 67, 63);

	public override int MaxInitialHp => MinInitialHp;

	private int RammingSpeedDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 11, 10);

	private int RammingSpeedStatusCount => 2;

	private int SwipeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 14, 13);

	private int StompDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 5, 4);

	private int StompRepeat => 3;

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.ArmorBig;

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("RAMMING_SPEED_MOVE", RammingSpeedMove, new SingleAttackIntent(RammingSpeedDamage), new StatusIntent(RammingSpeedStatusCount));
		MoveState moveState2 = new MoveState("SWIPE_MOVE", SwipeMove, new SingleAttackIntent(SwipeDamage));
		MoveState moveState3 = new MoveState("STOMP_MOVE", StompMove, new MultiAttackIntent(StompDamage, StompRepeat));
		MoveState moveState4 = new MoveState("HAUNT_MOVE", HauntMove, new DebuffIntent());
		RandomBranchState randomBranchState = (RandomBranchState)(moveState4.FollowUpState = (moveState3.FollowUpState = (moveState2.FollowUpState = (moveState.FollowUpState = new RandomBranchState("RAND")))));
		randomBranchState.AddBranch(moveState, MoveRepeatType.CannotRepeat, () => (base.CombatState.RoundNumber % 2 != 0) ? 1 : 0);
		randomBranchState.AddBranch(moveState2, MoveRepeatType.CannotRepeat, () => (base.CombatState.RoundNumber % 2 != 0) ? 1 : 0);
		randomBranchState.AddBranch(moveState3, MoveRepeatType.CannotRepeat, () => (base.CombatState.RoundNumber % 2 != 0) ? 1 : 0);
		randomBranchState.AddBranch(moveState4, MoveRepeatType.UseOnlyOnce, () => (base.CombatState.RoundNumber % 2 == 0) ? 1 : 0);
		list.Add(randomBranchState);
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		list.Add(moveState4);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task RammingSpeedMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(RammingSpeedDamage).FromMonster(this).WithAttackerAnim("Attack", 0.15f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
		await CardPileCmd.AddToCombatAndPreview<Wound>(targets, PileType.Discard, RammingSpeedStatusCount, addedByPlayer: false);
	}

	private async Task SwipeMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(SwipeDamage).FromMonster(this).WithAttackerAnim("Attack", 0.15f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task StompMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(StompDamage).WithHitCount(StompRepeat).FromMonster(this)
			.WithAttackerAnim("AttackTriple", 0.15f)
			.OnlyPlayAnimOnce()
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task HauntMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play(CastSfx);
		await CreatureCmd.TriggerAnim(base.Creature, "Cast", 0f);
		await Cmd.Wait(0.6f);
		VfxCmd.PlayOnCreatureCenter(base.Creature, "vfx/vfx_spooky_scream");
		await Cmd.CustomScaledWait(0.2f, 0.5f);
		await PowerCmd.Apply<WeakPower>(targets, 2m, base.Creature, null);
		await PowerCmd.Apply<FrailPower>(targets, 2m, base.Creature, null);
		await PowerCmd.Apply<VulnerablePower>(targets, 2m, base.Creature, null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("debuff");
		AnimState animState3 = new AnimState("attack_triple");
		AnimState animState4 = new AnimState("attack");
		AnimState animState5 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		animState2.NextState = animState;
		animState4.NextState = animState;
		animState3.NextState = animState;
		animState5.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Idle", animState);
		creatureAnimator.AddAnyState("Cast", animState2);
		creatureAnimator.AddAnyState("Attack", animState4);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState5);
		creatureAnimator.AddAnyState("AttackTriple", animState3);
		return creatureAnimator;
	}
}
