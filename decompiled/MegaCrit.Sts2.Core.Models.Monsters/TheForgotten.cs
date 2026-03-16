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

public sealed class TheForgotten : MonsterModel
{
	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 111, 106);

	public override int MaxInitialHp => MinInitialHp;

	private int DreadDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 17, 15);

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Stone;

	public override async Task AfterAddedToRoom()
	{
		await PowerCmd.Apply<PossessSpeedPower>(base.Creature, 1m, null, null);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("MIASMA", MiasmaMove, new DebuffIntent(), new DefendIntent(), new BuffIntent());
		MoveState moveState2 = (MoveState)(moveState.FollowUpState = new MoveState("DREAD", DreadMove, new SingleAttackIntent(DreadDamage)));
		moveState2.FollowUpState = moveState;
		list.Add(moveState);
		list.Add(moveState2);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task MiasmaMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play(CastSfx);
		await CreatureCmd.TriggerAnim(base.Creature, "Cast", 0.5f);
		await PowerCmd.Apply<DexterityPower>(targets, -2m, base.Creature, null);
		await CreatureCmd.GainBlock(base.Creature, 8m, ValueProp.Move, null);
		await PowerCmd.Apply<DexterityPower>(base.Creature, 2m, base.Creature, null);
	}

	private async Task DreadMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(DreadDamage).FromMonster(this).WithAttackerAnim("Attack", 0.15f)
			.OnlyPlayAnimOnce()
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("debuff");
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
