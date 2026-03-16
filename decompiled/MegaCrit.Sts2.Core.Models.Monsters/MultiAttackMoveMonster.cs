using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class MultiAttackMoveMonster : MonsterModel
{
	public const int repeat = 5;

	public override LocString Title => MonsterModel.L10NMonsterLookup("BIG_DUMMY.name");

	public override int MinInitialHp => 999;

	public override int MaxInitialHp => 999;

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("POKE", PokeMove, new MultiAttackIntent(1, 5));
		moveState.FollowUpState = moveState;
		list.Add(moveState);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task PokeMove(IReadOnlyList<Creature> targets)
	{
		await DamageCmd.Attack(1m).WithHitCount(5).FromMonster(this)
			.Execute(null);
	}
}
