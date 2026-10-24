using MegaCrit.Sts2.Core.Entities.Relics;

namespace MegaCrit.Sts2.Core.Models.Relics;

/// <summary>
/// Represents a relic that has been removed from the game. Mostly used for the run history.
/// </summary>
public sealed class DeprecatedRelic : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.None;

	public override bool IsStackable => true;
}
