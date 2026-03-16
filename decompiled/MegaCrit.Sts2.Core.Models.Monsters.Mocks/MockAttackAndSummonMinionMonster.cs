using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MegaCrit.Sts2.Core.Models.Monsters.Mocks;

public sealed class MockAttackAndSummonMinionMonster : MonsterModel
{
	protected override string VisualsPath => SceneHelper.GetScenePath("creature_visuals/defect");

	public override int MinInitialHp => 10;

	public override int MaxInitialHp => 10;

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("ATTACK_AND_SUMMON_MINION", AttackAndSummonMinionMove, new SingleAttackIntent(1));
		moveState.FollowUpState = moveState;
		list.Add(moveState);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task AttackAndSummonMinionMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(1m).FromMonster(this).Execute(null);
		await PowerCmd.Apply<MinionPower>(await CreatureCmd.Add<BigDummy>(base.CombatState), 1m, base.Creature, null);
	}
}
