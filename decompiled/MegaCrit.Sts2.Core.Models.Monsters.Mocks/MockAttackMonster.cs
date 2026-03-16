using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MegaCrit.Sts2.Core.Models.Monsters.Mocks;

public sealed class MockAttackMonster : MonsterModel
{
	protected override string VisualsPath => SceneHelper.GetScenePath("creature_visuals/defect");

	public override int MinInitialHp => 9999;

	public override int MaxInitialHp => 9999;

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("ATTACK", AttackMove, new SingleAttackIntent(1));
		moveState.FollowUpState = moveState;
		list.Add(moveState);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task AttackMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(1m).FromMonster(this).Execute(null);
	}
}
