using MegaCrit.Sts2.Core.Nodes.Animation;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace MegaCrit.Sts2.Core.Models.Monsters;

public sealed class DecimillipedeSegmentMiddle : DecimillipedeSegment
{
	protected override void SegmentAttack()
	{
		NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(base.Creature);
		nCreature?.GetSpecialNode<NDecimillipedeSegmentDriver>("%Visuals/RightSegmentDriver")?.AttackShake();
		nCreature?.GetSpecialNode<NDecimillipedeSegmentDriver>("%Visuals/LeftSegmentDriver")?.AttackShake();
	}
}
