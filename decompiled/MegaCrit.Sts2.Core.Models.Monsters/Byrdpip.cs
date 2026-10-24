using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class Byrdpip : MonsterModel
{
	public override int MinInitialHp => 9999;

	public override int MaxInitialHp => 9999;

	public override bool IsHealthBarVisible => false;

	public override void SetupSkins(MegaSprite spine, MegaSkeleton skeleton)
	{
		string skinName = ((!base.IsMutable) ? MegaCrit.Sts2.Core.Models.Relics.Byrdpip.SkinOptions[0] : base.Creature.PetOwner.GetRelic<MegaCrit.Sts2.Core.Models.Relics.Byrdpip>().Skin);
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
}
