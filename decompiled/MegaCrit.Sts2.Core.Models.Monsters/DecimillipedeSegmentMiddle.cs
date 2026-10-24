using MegaCrit.Sts2.Core.Nodes.Animation;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace MegaCrit.Sts2.Core.Models.Monsters;

/// <summary>
/// Monster class exists to connect this to the correct monster visual scene.
/// All logic lives in the <see cref="T:MegaCrit.Sts2.Core.Models.Monsters.DecimillipedeSegment" />.
/// </summary>
public sealed class DecimillipedeSegmentMiddle : DecimillipedeSegment
{
	public override void SegmentAttack()
	{
		NCreature creatureNode = base.Creature.GetCreatureNode();
		creatureNode?.GetSpecialNode<NDecimillipedeSegmentDriver>("%Visuals/RightSegmentDriver")?.AttackShake();
		creatureNode?.GetSpecialNode<NDecimillipedeSegmentDriver>("%Visuals/LeftSegmentDriver")?.AttackShake();
	}
}
