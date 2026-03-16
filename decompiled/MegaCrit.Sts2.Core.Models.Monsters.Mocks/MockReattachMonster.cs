using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MegaCrit.Sts2.Core.Models.Monsters.Mocks;

public sealed class MockReattachMonster : MonsterModel
{
	private MoveState _deadState;

	public override LocString Title => MonsterModel.L10NMonsterLookup("BIG_DUMMY.name");

	public override int MinInitialHp => 1;

	public override int MaxInitialHp => 1;

	public MoveState DeadState
	{
		get
		{
			return _deadState;
		}
		private set
		{
			AssertMutable();
			_deadState = value;
		}
	}

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<ReattachPower>(base.Creature, 1m, base.Creature, null);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("NOTHING", NothingMove, new HiddenIntent());
		moveState.FollowUpState = moveState;
		MoveState moveState2 = new MoveState("REATTACH_MOVE", ReattachMove, new HealIntent())
		{
			MustPerformOnceBeforeTransitioning = true,
			FollowUpState = moveState
		};
		DeadState = new MoveState("DEAD_MOVE", NothingMove)
		{
			FollowUpState = moveState2
		};
		list.Add(moveState);
		list.Add(moveState2);
		list.Add(DeadState);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private Task NothingMove(IReadOnlyList<Creature> targets)
	{
		return Task.CompletedTask;
	}

	private async Task ReattachMove(IReadOnlyList<Creature> targets)
	{
		await base.Creature.GetPower<ReattachPower>().DoReattach();
	}
}
