using System.Collections.Generic;
using System.Linq;
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

public sealed class Ovicopter : MonsterModel
{
	private const string _layMove = "lay";

	private const string _idleLoop = "event:/sfx/enemy/enemy_attacks/egg_layer/egg_layer_idle_loop";

	private const string _laySfx = "event:/sfx/enemy/enemy_attacks/egg_layer/egg_layer_lay";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 126, 124);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 132, 130);

	private int SmashDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 17, 16);

	private int TenderizerDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);

	private int NutritionalPasteStrengthAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);

	public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/egg_layer/egg_layer_die";

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Slime;

	private bool CanLay => base.Creature.CombatState.GetTeammatesOf(base.Creature).Count((Creature c) => c.IsAlive) <= 3;

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		SfxCmd.PlayLoop("event:/sfx/enemy/enemy_attacks/egg_layer/egg_layer_idle_loop");
	}

	public override void BeforeRemovedFromRoom()
	{
		SfxCmd.StopLoop("event:/sfx/enemy/enemy_attacks/egg_layer/egg_layer_idle_loop");
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("LAY_EGGS_MOVE", LayEggsMove, new SummonIntent());
		MoveState moveState2 = new MoveState("SMASH_MOVE", SmashMove, new SingleAttackIntent(SmashDamage));
		MoveState moveState3 = new MoveState("TENDERIZER_MOVE", TenderizerMove, new SingleAttackIntent(TenderizerDamage), new DebuffIntent());
		MoveState moveState4 = new MoveState("NUTRITIONAL_PASTE_MOVE", NutritionalPasteMove, new BuffIntent());
		ConditionalBranchState conditionalBranchState = new ConditionalBranchState("SUMMON_BRANCH_STATE");
		moveState.FollowUpState = moveState2;
		moveState4.FollowUpState = moveState2;
		moveState2.FollowUpState = moveState3;
		moveState3.FollowUpState = conditionalBranchState;
		conditionalBranchState.AddState(moveState, () => CanLay);
		conditionalBranchState.AddState(moveState4, () => !CanLay);
		list.Add(moveState4);
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		list.Add(conditionalBranchState);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task LayEggsMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/egg_layer/egg_layer_lay");
		await CreatureCmd.TriggerAnim(base.Creature, "lay", 1f);
		for (int i = 0; i < 3; i++)
		{
			string slotName = base.CombatState.Encounter.Slots.LastOrDefault((string s) => base.CombatState.Enemies.All((Creature c) => c.SlotName != s), string.Empty);
			await PowerCmd.Apply<MinionPower>(await CreatureCmd.Add<ToughEgg>(base.CombatState, slotName), 1m, base.Creature, null);
		}
	}

	private async Task NutritionalPasteMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_attacks/egg_layer/egg_layer_lay");
		await CreatureCmd.TriggerAnim(base.Creature, "lay", 1f);
		await PowerCmd.Apply<StrengthPower>(base.Creature, NutritionalPasteStrengthAmount, base.Creature, null);
	}

	private async Task SmashMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(SmashDamage).FromMonster(this).WithAttackerAnim("Attack", 0.3f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task TenderizerMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(TenderizerDamage).FromMonster(this).WithAttackerAnim("Attack", 0.3f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
		await PowerCmd.Apply<VulnerablePower>(targets, 2m, base.Creature, null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("cast");
		AnimState animState3 = new AnimState("attack");
		AnimState animState4 = new AnimState("hurt");
		AnimState state = new AnimState("die");
		AnimState animState5 = new AnimState("lay");
		animState2.NextState = animState;
		animState3.NextState = animState;
		animState5.NextState = animState;
		animState4.NextState = animState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Dead", state);
		creatureAnimator.AddAnyState("Hit", animState4);
		creatureAnimator.AddAnyState("Cast", animState2);
		creatureAnimator.AddAnyState("Attack", animState3);
		creatureAnimator.AddAnyState("lay", animState5);
		return creatureAnimator;
	}
}
