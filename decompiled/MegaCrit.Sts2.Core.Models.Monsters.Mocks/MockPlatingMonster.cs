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

public sealed class MockPlatingMonster : MonsterModel
{
	private int _platingAmount;

	public override LocString Title => MonsterModel.L10NMonsterLookup("BIG_DUMMY.name");

	protected override string VisualsPath => SceneHelper.GetScenePath("creature_visuals/defect");

	public override int MinInitialHp => 9999;

	public override int MaxInitialHp => 9999;

	public int PlatingAmount
	{
		get
		{
			return _platingAmount;
		}
		set
		{
			AssertMutable();
			_platingAmount = value;
		}
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("NOTHING", NothingMove, new HiddenIntent());
		moveState.FollowUpState = moveState;
		list.Add(moveState);
		return new MonsterMoveStateMachine(list, moveState);
	}

	public override async Task AfterAddedToRoom()
	{
		await base.AfterAddedToRoom();
		await PowerCmd.Apply<PlatingPower>(base.Creature, PlatingAmount, base.Creature, null);
	}

	private Task NothingMove(IReadOnlyList<Creature> targets)
	{
		return Task.CompletedTask;
	}
}
