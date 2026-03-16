using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class Fabricator : MonsterModel
{
	public static readonly HashSet<MonsterModel> aggroSpawns = new HashSet<MonsterModel>
	{
		ModelDb.Monster<Zapbot>(),
		ModelDb.Monster<Stabbot>()
	};

	public static readonly HashSet<MonsterModel> defenseSpawns = new HashSet<MonsterModel>
	{
		ModelDb.Monster<Guardbot>(),
		ModelDb.Monster<Noisebot>()
	};

	private MonsterModel? _lastSpawned;

	public override string HurtSfx => "event:/sfx/enemy/enemy_attacks/fabricator/fabricator_hurt";

	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 155, 150);

	public override int MaxInitialHp => MinInitialHp;

	private int FabricatingStrikeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 21, 18);

	private int DisintegrateDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 13, 11);

	public override bool ShouldFadeAfterDeath => false;

	private bool CanFabricate => base.Creature.CombatState.GetTeammatesOf(base.Creature).Count((Creature c) => c.IsAlive) < 4;

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("FABRICATE_MOVE", FabricateMove, new SummonIntent());
		MoveState moveState2 = new MoveState("FABRICATING_STRIKE_MOVE", FabricatingStrikeMove, new SingleAttackIntent(FabricatingStrikeDamage), new SummonIntent());
		MoveState moveState3 = new MoveState("DISINTEGRATE_MOVE", DisintegrateMove, new SingleAttackIntent(DisintegrateDamage));
		RandomBranchState randomBranchState = new RandomBranchState("RAND");
		randomBranchState.AddBranch(moveState, MoveRepeatType.CanRepeatForever, () => 1f);
		randomBranchState.AddBranch(moveState2, MoveRepeatType.CanRepeatForever, () => 1f);
		ConditionalBranchState conditionalBranchState = new ConditionalBranchState("fabricateBranch");
		conditionalBranchState.AddState(randomBranchState, () => CanFabricate);
		conditionalBranchState.AddState(moveState3, () => !CanFabricate);
		moveState.FollowUpState = conditionalBranchState;
		moveState3.FollowUpState = conditionalBranchState;
		moveState2.FollowUpState = conditionalBranchState;
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(moveState3);
		list.Add(conditionalBranchState);
		list.Add(randomBranchState);
		return new MonsterMoveStateMachine(list, conditionalBranchState);
	}

	private async Task FabricateMove(IReadOnlyList<Creature> targets)
	{
		await SpawnDefensiveBot();
		await SpawnAggroBot();
	}

	private async Task FabricatingStrikeMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(FabricatingStrikeDamage).FromMonster(this).WithAttackerAnim("Attack", 0.6f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
		await SpawnAggroBot();
	}

	private async Task DisintegrateMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(DisintegrateDamage).FromMonster(this).WithAttackerAnim("Attack", 0.6f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task SpawnDefensiveBot()
	{
		await SpawnBot(defenseSpawns);
	}

	private async Task SpawnAggroBot()
	{
		await SpawnBot(aggroSpawns);
	}

	private async Task SpawnBot(IEnumerable<MonsterModel> options)
	{
		List<MonsterModel> items = options.Where((MonsterModel m) => m != _lastSpawned).ToList();
		await PowerCmd.Apply<MinionPower>(await CreatureCmd.Add((_lastSpawned = base.RunRng.MonsterAi.NextItem(items)).ToMutable(), base.CombatState, CombatSide.Enemy, base.CombatState.Encounter.GetNextSlot(base.CombatState)), 1m, base.Creature, null);
	}
}
