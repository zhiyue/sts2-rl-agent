using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class Byrdonis : MonsterModel
{
	private const string _angryTrigger = "Angry";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 99, 91);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 99, 94);

	private static int PeckDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);

	private static int PeckRepeat => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 3);

	private static int SwoopDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 18, 16);

	public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/byrdonis/byrdonis_die";

	public override string TakeDamageSfx => "event:/sfx/enemy/enemy_attacks/byrdonis/byrdonis_hurt";

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<TerritorialPower>(base.Creature, 1m, base.Creature, null);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("PECK_MOVE", PeckMove, new MultiAttackIntent(PeckDamage, PeckRepeat));
		MoveState moveState2 = new MoveState("SWOOP_MOVE", SwoopMove, new SingleAttackIntent(SwoopDamage));
		moveState2.FollowUpState = moveState;
		moveState.FollowUpState = moveState2;
		list.Add(moveState2);
		list.Add(moveState);
		return new MonsterMoveStateMachine(list, moveState2);
	}

	private async Task PeckMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(PeckDamage).WithHitCount(PeckRepeat).FromMonster(this)
			.WithAttackerAnim("Attack", 0.4f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task SwoopMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(SwoopDamage).FromMonster(this).WithAttackerAnim("Attack", 0.4f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("hurt");
		AnimState animState3 = new AnimState("attack");
		AnimState state = new AnimState("die");
		AnimState animState4 = new AnimState("get_angry");
		animState2.NextState = animState;
		animState3.NextState = animState;
		animState4.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Angry", animState4);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState2);
		creatureAnimator.AddAnyState("Attack", animState3);
		return creatureAnimator;
	}
}
