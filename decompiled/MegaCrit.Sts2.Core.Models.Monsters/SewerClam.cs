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

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class SewerClam : MonsterModel
{
	private const string _buffSfx = "event:/sfx/enemy/enemy_attacks/sewer_clam/sewer_clam_buff";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 58, 56);

	public override int MaxInitialHp => MinInitialHp;

	private int JetDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 11, 10);

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Stone;

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		int valueIfAscension = AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 9, 8);
		await PowerCmd.Apply<PlatingPower>(base.Creature, valueIfAscension, base.Creature, null);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("PRESSURIZE_MOVE", PressurizeMove, new BuffIntent());
		MoveState moveState2 = (MoveState)(moveState.FollowUpState = new MoveState("JET_MOVE", JetMove, new SingleAttackIntent(JetDamage)));
		moveState2.FollowUpState = moveState;
		list.Add(moveState);
		list.Add(moveState2);
		return new MonsterMoveStateMachine(list, moveState2);
	}

	private async Task PressurizeMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/sewer_clam/sewer_clam_buff");
		await CreatureCmd.TriggerAnim(base.Creature, "Cast", 1f);
		await PowerCmd.Apply<StrengthPower>(base.Creature, 4m, base.Creature, null);
	}

	private async Task JetMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(JetDamage).FromMonster(this).WithAttackerAnim("Attack", 0.45f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("buff");
		AnimState animState3 = new AnimState("attack");
		AnimState animState4 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		animState2.NextState = animState;
		animState3.NextState = animState;
		animState4.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Idle", animState);
		creatureAnimator.AddAnyState("Cast", animState2);
		creatureAnimator.AddAnyState("Attack", animState3);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState4);
		return creatureAnimator;
	}
}
