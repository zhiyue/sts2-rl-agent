using MegaCrit.Sts2.Core.Nodes.Animation;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class DecimillipedeSegmentBack : DecimillipedeSegment
{
	protected override void SegmentAttack()
	{
		(NCombatRoom.Instance?.GetCreatureNode(base.Creature))?.GetSpecialNode<NDecimillipedeSegmentDriver>("%Visuals/SegmentDriver")?.AttackShake();
	}
}
