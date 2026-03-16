using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class BattleFriendV3 : MonsterModel
{
	public override int MinInitialHp => 300;

	public override int MaxInitialHp => 300;

	public override void SetupSkins(NCreatureVisuals visuals)
	{
		MegaSkeleton skeleton = visuals.SpineBody.GetSkeleton();
		MegaSkeletonDataResource data = skeleton.GetData();
		skeleton.SetSkin(data.FindSkin("v3"));
		skeleton.SetSlotsToSetupPose();
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		MoveState moveState = new MoveState("NOTHING_MOVE", (IReadOnlyList<Creature> _) => Task.CompletedTask);
		moveState.FollowUpState = moveState;
		return new MonsterMoveStateMachine(new global::_003C_003Ez__ReadOnlySingleElementList<MonsterState>(moveState), moveState);
	}

	public override async Task AfterAddedToRoom()
	{
		await PowerCmd.Apply<BattlewornDummyTimeLimitPower>(base.Creature, 3m, null, null);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("hurt");
		AnimState animState3 = new AnimState("die");
		AnimState nextState = new AnimState("die_loop", isLooping: true);
		animState2.NextState = animState;
		animState3.NextState = nextState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Idle", animState);
		creatureAnimator.AddAnyState("Dead", animState3);
		creatureAnimator.AddAnyState("Hit", animState2);
		return creatureAnimator;
	}
}
