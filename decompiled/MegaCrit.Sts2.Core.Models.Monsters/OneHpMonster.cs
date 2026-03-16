using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class OneHpMonster : MonsterModel
{
	public override LocString Title => MonsterModel.L10NMonsterLookup("BIG_DUMMY.name");

	public override int MinInitialHp => 1;

	public override int MaxInitialHp => 1;

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
