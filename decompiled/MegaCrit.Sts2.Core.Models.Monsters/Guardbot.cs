using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class Guardbot : MonsterModel
{
	public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 17, 16);

	public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 21, 20);

	public override DamageSfxType TakeDamageSfxType => DamageSfxType.Armor;

	public override Task AfterAddedToRoom()
	{
		base.AfterAddedToRoom();
		if (TestMode.IsOff)
		{
			NCreature creatureNode = NCombatRoom.Instance.GetCreatureNode(base.Creature);
			FabricatorNormal.SetBotFallPosition(creatureNode);
		}
		return Task.CompletedTask;
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> list = new List<MonsterState>();
		MoveState moveState = new MoveState("GUARD_MOVE", GuardMove, new DefendIntent());
		moveState.FollowUpState = moveState;
		list.Add(moveState);
		return new MonsterMoveStateMachine(list, moveState);
	}

	private async Task GuardMove(IReadOnlyList<Creature> targets)
	{
		SfxCmd.Play(CastSfx);
		await CreatureCmd.TriggerAnim(base.Creature, "Cast", 0.6f);
		List<Creature> list = base.Creature.CombatState.Enemies.Where((Creature c) => c.Monster is Fabricator).ToList();
		foreach (Creature item in list)
		{
			await CreatureCmd.GainBlock(item, 15m, ValueProp.Unpowered, null);
		}
	}
}
