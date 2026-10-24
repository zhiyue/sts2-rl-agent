using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class PaelsLegion : MonsterModel
{
	public const string blockTrigger = "BlockTrigger";

	public const string sleepTrigger = "SleepTrigger";

	public const string wakeUpTrigger = "WakeUpTrigger";

	public override int MinInitialHp => 9999;

	public override int MaxInitialHp => 9999;

	public override bool IsHealthBarVisible => false;

	public override void SetupSkins(MegaSprite spine, MegaSkeleton skeleton)
	{
		string skinName = ((!base.IsMutable) ? MegaCrit.Sts2.Core.Models.Relics.PaelsLegion.SkinOptions[0] : base.Creature.PetOwner.GetRelic<MegaCrit.Sts2.Core.Models.Relics.PaelsLegion>().Skin);
		MegaSkeletonDataResource data = skeleton.GetData();
		skeleton.SetSkin(data.FindSkin(skinName));
		skeleton.SetSlotsToSetupPose();
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		MoveState moveState = new MoveState("NOTHING_MOVE", (IReadOnlyList<Creature> _) => Task.CompletedTask);
		moveState.FollowUpState = moveState;
		return new MonsterMoveStateMachine(new global::_003C_003Ez__ReadOnlySingleElementList<MonsterState>(moveState), moveState);
	}

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		AnimState animState = new AnimState("idle_loop", isLooping: true);
		AnimState animState2 = new AnimState("block");
		AnimState nextState = new AnimState("block_loop");
		AnimState state = new AnimState("sleep");
		AnimState animState3 = new AnimState("wake_up");
		animState3.NextState = animState;
		animState2.NextState = nextState;
		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Idle", animState);
		creatureAnimator.AddAnyState("BlockTrigger", animState2);
		creatureAnimator.AddAnyState("SleepTrigger", state);
		creatureAnimator.AddAnyState("WakeUpTrigger", animState3);
		return creatureAnimator;
	}
}
