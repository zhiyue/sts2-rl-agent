using MegaCrit.Sts2.Core.Nodes.Animation;

namespace MegaCrit.Sts2.Core.Models.Monsters;

/// <summary>
/// Monster class exists to connect this to the correct monster visual scene.
/// All logic lives in the <see cref="T:MegaCrit.Sts2.Core.Models.Monsters.DecimillipedeSegment" />.
/// </summary>
public sealed class DecimillipedeSegmentFront : DecimillipedeSegment
{
	public override void SegmentAttack()
	{
		base.Creature.GetCreatureNode()?.GetSpecialNode<NDecimillipedeSegmentDriver>("%Visuals/SegmentDriver")?.AttackShake();
	}
}
