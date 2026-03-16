using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class LivingShield : MonsterModel
{
	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 65, 55);

	public override int MaxInitialHp => MinInitialHp;

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Armor;

	public override bool HasDeathSfx => false;

	private int ShieldSlamDamage => 6;

	private int SmashDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 18, 16);

	private int EnrageStr => 3;

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<RampartPower>(base.Creature, 25m, base.Creature, null);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("SHIELD_SLAM_MOVE", ShieldSlamMove, new SingleAttackIntent(ShieldSlamDamage));
		ConditionalBranchState conditionalBranchState = new ConditionalBranchState("SHIELD_SLAM_BRANCH");
		MoveState moveState2 = new MoveState("SMASH_MOVE", SmashMove, new SingleAttackIntent(SmashDamage), new BuffIntent());
		moveState.FollowUpState = conditionalBranchState;
		conditionalBranchState.AddState(moveState, () => GetAllyCount() > 0);
		conditionalBranchState.AddState(moveState2, () => GetAllyCount() == 0);
		moveState2.FollowUpState = moveState2;
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(conditionalBranchState);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task ShieldSlamMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(ShieldSlamDamage).FromMonster(this).WithAttackerAnim("Attack", 0.3f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task SmashMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(SmashDamage).FromMonster(this).WithAttackerAnim("Attack", 0.3f)
			.WithAttackerFx(null, AttackSfx)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
		await PowerCmd.Apply<StrengthPower>(base.Creature, EnrageStr, base.Creature, null);
	}

	private int GetAllyCount()
	{
		return base.Creature.CombatState.GetTeammatesOf(base.Creature).Count((Creature c) => c.IsAlive && c != base.Creature);
	}
}
