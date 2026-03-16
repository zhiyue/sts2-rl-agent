using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class BigDummy : MonsterModel
{
	public override LocString Title => MonsterModel.L10NMonsterLookup("BIG_DUMMY.name");

	protected override string VisualsPath => SceneHelper.GetScenePath("creature_visuals/defect");

	public override int MinInitialHp => 9999;

	public override int MaxInitialHp => 9999;

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("NOTHING", NothingMove, new HiddenIntent());
		moveState.FollowUpState = moveState;
		list.Add(moveState);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private Task NothingMove(IReadOnlyList<Creature> targets)
	{
		return Task.CompletedTask;
	}
}
