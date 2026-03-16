using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MegaCrit.Sts2.Core.Models.Monsters.Mocks;

public sealed class MockIntangibleMonster : MonsterModel
{
	public override LocString Title => MonsterModel.L10NMonsterLookup("BIG_DUMMY.name");

	protected override string VisualsPath => SceneHelper.GetScenePath("creature_visuals/defect");

	public override int MinInitialHp => 9999;

	public override int MaxInitialHp => 9999;

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("INTANGIBLE", IntangibleMove, new HiddenIntent());
		MoveState moveState2 = (MoveState)(moveState.FollowUpState = new MoveState("NOTHING", NothingMove, new HiddenIntent()));
		moveState2.FollowUpState = moveState;
		list.Add(moveState);
		list.Add(moveState2);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task IntangibleMove(IReadOnlyList<Creature> targets)
	{
		await PowerCmd.Apply<IntangiblePower>(base.Creature, 2m, base.Creature, null);
	}

	private Task NothingMove(IReadOnlyList<Creature> targets)
	{
		return Task.CompletedTask;
	}
}
