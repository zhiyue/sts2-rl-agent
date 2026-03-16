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

public sealed class Entomancer : MonsterModel
{
	private const string _rangedAttackMove = "attack_ranged";

	private const string _attackRangedSfx = "event:/sfx/enemy/enemy_attacks/entomancer/entomancer_attack_ranged";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 155, 145);

	public override int MaxInitialHp => MinInitialHp;

	private int SpearMoveDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 20, 18);

	private int BeesRepeat => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);

	private int BeesDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 3);

	public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/entomancer/entomancer_die";

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<PersonalHivePower>(base.Creature, 1m, base.Creature, null);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("PHEROMONE_SPIT_MOVE", SpitMove, new BuffIntent());
		MoveState moveState2 = new MoveState("BEES_MOVE", BeesMove, new MultiAttackIntent(BeesDamage, BeesRepeat));
		MoveState moveState3 = (MoveState)(moveState2.FollowUpState = new MoveState("SPEAR_MOVE", SpearMove, new SingleAttackIntent(SpearMoveDamage)));
		moveState3.FollowUpState = moveState;
		moveState.FollowUpState = moveState2;
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		return new MonsterMoveStateMachine(list, moveState2);
	}

	private async Task SpitMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play(CastSfx);
		await CreatureCmd.TriggerAnim(base.Creature, "Cast", 0.5f);
		PersonalHivePower personalHivePower = base.Creature.Powers.OfType<PersonalHivePower>().First();
		if (personalHivePower.Amount < 3)
		{
			await PowerCmd.Apply<PersonalHivePower>(base.Creature, 1m, base.Creature, null);
			await PowerCmd.Apply<StrengthPower>(base.Creature, 1m, base.Creature, null);
		}
		else
		{
			await PowerCmd.Apply<StrengthPower>(base.Creature, 2m, base.Creature, null);
		}
	}

	private async Task BeesMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(BeesDamage).WithHitCount(BeesRepeat).FromMonster(this)
			.WithAttackerAnim("attack_ranged", 0.3f)
			.OnlyPlayAnimOnce()
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/entomancer/entomancer_attack_ranged")
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task SpearMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(SpearMoveDamage).FromMonster(this).WithAttackerAnim("Attack", 0.25f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/entomancer/entomancer_attack_ranged")
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("cast");
		AnimState animState3 = new AnimState("attack");
		AnimState animState4 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		AnimState animState5 = new AnimState("attack_ranged");
		animState2.NextState = animState;
		animState3.NextState = animState;
		animState5.NextState = animState;
		animState4.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Cast", animState2);
		creatureAnimator.AddAnyState("Attack", animState3);
		creatureAnimator.AddAnyState("attack_ranged", animState5);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState4);
		return creatureAnimator;
	}
}
