using MegaCrit.Sts2.Core.Entities.Relics;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class DeprecatedRelic : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.None;

	public override bool IsStackable => true;
}
