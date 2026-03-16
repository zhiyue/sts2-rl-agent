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

public class FlailKnight : MonsterModel
{
	private const string _flailAttackTrigger = "FlailAttack";

	private const string _ramAttackTrigger = "RamAttack";

	private const string _breakerAttackTrigger = "BreakerAttack";

	private const string _flailSfx = "event:/sfx/enemy/enemy_attacks/flail_knight/flail_knight_flail";

	private const string _chantSfx = "event:/sfx/enemy/enemy_attacks/flail_knight/flail_knight_war_chant";

	private const string _ramSfx = "event:/sfx/enemy/enemy_attacks/flail_knight/flail_knight_ram";

	private const int _flailRepeat = 2;

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 108, 101);

	public override int MaxInitialHp => MinInitialHp;

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Armor;

	private int FlailDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 9);

	private int RamDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 17, 15);

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("WAR_CHANT", WarChantMove, new BuffIntent());
		MoveState moveState2 = new MoveState("FLAIL_MOVE", FlailMove, new MultiAttackIntent(FlailDamage, 2));
		MoveState moveState3 = new MoveState("RAM_MOVE", RamMove, new SingleAttackIntent(RamDamage));
		RandomBranchState randomBranchState = (RandomBranchState)(moveState3.FollowUpState = (moveState2.FollowUpState = (moveState.FollowUpState = new RandomBranchState("RAND"))));
		randomBranchState.AddBranch(moveState, MoveRepeatType.CannotRepeat);
		randomBranchState.AddBranch(moveState2, 2);
		randomBranchState.AddBranch(moveState3, 2);
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		list.Add(randomBranchState);
		return new MonsterMoveStateMachine(list, moveState3);
	}

	private async Task WarChantMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/flail_knight/flail_knight_war_chant");
		await CreatureCmd.TriggerAnim(base.Creature, "BreakerAttack", 0.5f);
		await PowerCmd.Apply<StrengthPower>(base.Creature, 3m, base.Creature, null);
	}

	public async Task FlailMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(FlailDamage).WithHitCount(2).FromMonster(this)
			.OnlyPlayAnimOnce()
			.WithAttackerAnim("FlailAttack", 0.5f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/flail_knight/flail_knight_flail")
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
	}

	private async Task RamMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(RamDamage).FromMonster(this).WithAttackerAnim("RamAttack", 0.5f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/flail_knight/flail_knight_ram")
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("cast");
		AnimState animState3 = new AnimState("attack_flail");
		AnimState animState4 = new AnimState("attack_ram");
		AnimState animState5 = new AnimState("attack_breaker");
		AnimState animState6 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		animState2.NextState = animState;
		animState3.NextState = animState;
		animState4.NextState = animState;
		animState5.NextState = animState;
		animState6.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState6);
		creatureAnimator.AddAnyState("Cast", animState2);
		creatureAnimator.AddAnyState("FlailAttack", animState3);
		creatureAnimator.AddAnyState("RamAttack", animState4);
		creatureAnimator.AddAnyState("BreakerAttack", animState5);
		return creatureAnimator;
	}
}
