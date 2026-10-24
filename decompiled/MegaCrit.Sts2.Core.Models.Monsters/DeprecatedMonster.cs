using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MegaCrit.Sts2.Core.Models.Monsters;

/// <summary>
/// Represents a monster that has been removed from the game. Mostly used for the run history.
/// </summary>
public sealed class DeprecatedMonster : MonsterModel
{
	public override int MinInitialHp => 0;

	public override int MaxInitialHp => 0;

	public override bool HasDeathSfx => false;

	public override IEnumerable<string> AssetPaths => Array.Empty<string>();

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		MoveState moveState = new MoveState("STUB", (IReadOnlyList<Creature> _) => Task.CompletedTask, new HiddenIntent());
		moveState.FollowUpState = moveState;
		return new MonsterMoveStateMachine(new global::_003C_003Ez__ReadOnlySingleElementList<MonsterState>(moveState), moveState);
	}
}
