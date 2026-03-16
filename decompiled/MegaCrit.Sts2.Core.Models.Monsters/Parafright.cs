using System.Collections.Generic;
using System.Linq;
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

public sealed class Parafright : MonsterModel
{
	public const string healSfx = "event:/sfx/enemy/enemy_attacks/obscura/obscura_hologram_heal";

	protected override string AttackSfx => "event:/sfx/enemy/enemy_attacks/obscura/obscura_hologram_attack";

	public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/obscura/obscura_hologram_die";

	public override int MinInitialHp => 21;

	public override int MaxInitialHp => MinInitialHp;

	private int SlamDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 17, 16);

	public override bool HasDeathSfx => false;

	public override bool ShouldDisappearFromDoom => false;

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<IllusionPower>(base.Creature, 1m, base.Creature, null);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("SLAM_MOVE", SlamMove, new SingleAttackIntent(SlamDamage));
		moveState.FollowUpState = moveState;
		list.Add(moveState);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task SlamMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(SlamDamage).FromMonster(this).WithAttackerAnim("Attack", 0.15f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState nextState = new AnimState("idle_loop", isLooping: true);
		AnimState animState = new AnimState("attack");
		AnimState state = new AnimState("die");
		AnimState animState2 = new AnimState("hurt");
		AnimState animState3 = new AnimState("hurt_stunned");
		AnimState nextState2 = new AnimState("stunned_loop", isLooping: true);
		AnimState animState4 = new AnimState("spawn");
		AnimState animState5 = new AnimState("wake_up");
		AnimState animState6 = new AnimState("stun");
		animState4.NextState = nextState;
		animState.NextState = nextState;
		animState2.NextState = nextState;
		animState5.NextState = nextState;
		animState6.NextState = nextState2;
		animState3.NextState = nextState2;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState4, controller);
		creatureAnimator.AddAnyState("Attack", animState);
		creatureAnimator.AddAnyState("Hit", animState2, () => !base.Creature.GetPower<IllusionPower>().IsReviving);
		creatureAnimator.AddAnyState("Hit", animState3, () => base.Creature.GetPower<IllusionPower>().IsReviving);
		creatureAnimator.AddAnyState("StunTrigger", animState6);
		creatureAnimator.AddAnyState("WakeUpTrigger", animState5);
		creatureAnimator.AddAnyState("Dead", state, () => !base.CombatState.GetTeammatesOf(base.Creature).Any((Creature t) => t != null && t.IsPrimaryEnemy && t.IsAlive));
		return creatureAnimator;
	}
}
