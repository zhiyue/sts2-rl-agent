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

public sealed class VineShambler : MonsterModel
{
	private const string _vineShamblerVfxPath = "vfx/monsters/vine_shambler_vines/vine_shambler_vines_vfx";

	private const int _swipeRepeat = 2;

	private const string _swipeTrigger = "SwipePower";

	private const string _vinesTrigger = "Vines";

	private const string _chompTrigger = "Chomp";

	private const string _chomp = "event:/sfx/enemy/enemy_attacks/vine_shambler/vine_shambler_chomp";

	private const string _defensiveSwipe = "event:/sfx/enemy/enemy_attacks/vine_shambler/vine_shambler_defensive_swipe";

	private const string _graspingVines = "event:/sfx/enemy/enemy_attacks/vine_shambler/vine_shambler_cast";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 64, 61);

	public override int MaxInitialHp => MinInitialHp;

	private int GraspingVinesDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 9, 8);

	private int SwipeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);

	private int ChompDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 18, 16);

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("GRASPING_VINES_MOVE", GraspingVinesMove, new SingleAttackIntent(GraspingVinesDamage), new CardDebuffIntent());
		MoveState moveState2 = new MoveState("SWIPE_MOVE", SwipeMove, new MultiAttackIntent(SwipeDamage, 2));
		MoveState moveState3 = new MoveState("CHOMP_MOVE", ChompMove, new SingleAttackIntent(ChompDamage));
		moveState2.FollowUpState = moveState;
		moveState.FollowUpState = moveState3;
		moveState3.FollowUpState = moveState2;
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		return new MonsterMoveStateMachine(list, moveState2);
	}

	private async Task GraspingVinesMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(GraspingVinesDamage).FromMonster(this).WithAttackerAnim("Vines", 0.5f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/vine_shambler/vine_shambler_cast")
			.WithHitFx("vfx/monsters/vine_shambler_vines/vine_shambler_vines_vfx")
			.SpawningHitVfxOnEachCreature()
			.WithHitVfxSpawnedAtBase()
			.Execute(null);
		await PowerCmd.Apply<TangledPower>(targets, 1m, base.Creature, null);
	}

	private async Task SwipeMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(SwipeDamage).WithHitCount(2).FromMonster(this)
			.OnlyPlayAnimOnce()
			.WithAttackerAnim("SwipePower", 0.4f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/vine_shambler/vine_shambler_defensive_swipe")
			.WithHitFx("vfx/vfx_scratch")
			.Execute(null);
	}

	private async Task ChompMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(ChompDamage).FromMonster(this).WithAttackerAnim("Chomp", 0.4f)
			.WithAttackerFx(null, "event:/sfx/enemy/enemy_attacks/vine_shambler/vine_shambler_chomp")
			.WithHitFx("vfx/vfx_bite")
			.Execute(null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("cast");
		AnimState animState3 = new AnimState("attack_chomp");
		AnimState animState4 = new AnimState("attack_swipe");
		AnimState animState5 = new AnimState("attack_vines");
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
		creatureAnimator.AddAnyState("Chomp", animState3);
		creatureAnimator.AddAnyState("SwipePower", animState4);
		creatureAnimator.AddAnyState("Vines", animState5);
		return creatureAnimator;
	}
}
